
#include "ephemeris.h"

#define DATA_MODULE

#ifndef DATA_MODULE

#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128 // OLED display width, in pixels
#define SCREEN_HEIGHT 64 // OLED display height, in pixels

// Declaration for an SSD1306 display connected to I2C (SDA, SCL pins)
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, -1);
#else

#include "EphemerisReader.h"
#include "virtuabotixRTC.h" 

#endif

#define DS1302_CLK_PIN A5 
#define DS1302_DAT_PIN A4 
#define DS1302_RST_PIN 8 
	 
#define CURRENT_SECONDS 0 
#define CURRENT_MINUTES 28  
#define CURRENT_HOURS 18 
#define CURRENT_DAY_OF_WEEK 6 
#define CURRENT_DAY_OF_MONTH 7 
#define CURRENT_MONTH 6 
#define CURRENT_YEAR 2025

// Protocol constants
#define START_MARKER 0xAA
#define END_MARKER 0x55
#define ESCAPE_BYTE 0xCC
#define SYNC_BYTE 0x33    // Additional sync byte
#define PACKET_TIMEOUT 1000  // ms

const byte maxBytes = 64;
byte receivedBytes[maxBytes];
byte numReceived = 0;
boolean newData = false;

#ifdef DATA_MODULE
virtuabotixRTC RTC(DS1302_CLK_PIN, DS1302_DAT_PIN, DS1302_RST_PIN); 

const int SD_CS_PIN = 10;  // SD card chip select pin

EphemerisReader moon;

DS1302Time getTime() {
  DS1302Time t = (DS1302Time) { 
    .year = RTC.year, 
    .month = RTC.month, 
    .day = RTC.dayofmonth, 
    .hour = RTC.hours, 
    .minute = RTC.minutes, 
    .second = RTC.seconds
  };
  return t;
}

uint32_t getCurrentTimestamp() {
    DS1302Time time = getTime();
    return ds1302_to_epoch_seconds(&time);
}

void testLookups() {
    Serial.println(F("\nTesting lookups..."));
    
    uint32_t test_times[] = {
        moon.getStartTime(),
        moon.getStartTime() + 86400,  // +1 day
        moon.getStartTime() + 7*86400,  // +1 week
        moon.getEndTime()
    };
    
    for (int i = 0; i < 4; i++) {
        EphemerisRecord record;
        if (moon.findRecord(test_times[i], &record)) {
            Serial.print(F("Moon "));
            Serial.print(i);
            Serial.print(F(": "));
            moon.printRecord(&record);
        }
    }
}

#else

bool displayReady;

#endif

void setup() {
#ifdef DATA_MODULE
    Serial.begin(9600);
    while (!Serial) { delay(10); }
    //RTC.setDS1302Time(0, 16, 00, 1, 10, 6, 2025);
    // Add small delays between initializations
    delay(100);
    
    // Initialize SD first
    Serial.println("Starting SD...");
    if (!SD.begin(SD_CS_PIN)) {
        Serial.println(F("SD failed"));
        return;
    }
    delay(150);  // Let SD settle
    
    // Load data while SD is stable
    if (!moon.begin("moon.bin")) {
        Serial.println(F("Data load failed"));
        return;
    }
    delay(50);
    
    uint32_t time = getCurrentTimestamp();

    Serial.println(time);
    Serial.print("Unix: ");
    Serial.println(epoch_seconds_to_unix(time));

#else
    Serial.begin(9600);

    if(!display.begin(SSD1306_SWITCHCAPVCC, 0x3C)) { // Address 0x3D for 128x64
        Serial.println(F("SSD1306 allocation failed"));
        for(;;);
    }
    delay(2000);
    display.clearDisplay();

    display.setTextSize(1);
    display.setTextColor(WHITE);
    display.setCursor(0, 10);
    // Display static text
    display.println("Hello, world!");
    display.display(); 
    displayReady = true;
#endif
}

// Calculate simple checksum
uint8_t calculateChecksum(uint8_t* data, uint8_t length) {
    uint16_t sum = 0;
    for (uint8_t i = 0; i < length; i++) {
        sum += data[i];
    }
    return (uint8_t)(sum & 0xFF);
}

#ifdef DATA_MODULE

// TRANSMITTER CODE - Enhanced with better error detection
void transmitRecord(const EphemerisRecord& record) {
    // Clear any pending input to avoid interference
    while (Serial.available()) Serial.read();
    
    uint8_t dataSize = sizeof(EphemerisRecord);
    uint8_t* dataPtr = (uint8_t*)&record;
    
    // Calculate checksum
    uint8_t checksum = calculateChecksum(dataPtr, dataSize);
    
    // Send sync pattern for better alignment
    Serial.write(SYNC_BYTE);
    Serial.write(SYNC_BYTE);
    
    // Send start marker
    Serial.write(START_MARKER);
    
    // Send size
    Serial.write(dataSize);
    
    // Send checksum
    Serial.write(checksum);
    
    // Send the actual data with byte stuffing
    for (int i = 0; i < dataSize; i++) {
        uint8_t dataByte = dataPtr[i];
        
        // Check if data byte conflicts with markers
        if (dataByte == START_MARKER || dataByte == END_MARKER || 
            dataByte == ESCAPE_BYTE || dataByte == SYNC_BYTE) {
            Serial.write(ESCAPE_BYTE);  // Send escape byte first
        }
        Serial.write(dataByte);
    }
    
    // Send end marker
    Serial.write(END_MARKER);
    Serial.flush(); // Ensure data is sent
    
    // Small delay to prevent overwhelming receiver
    delay(50);
}

#else

// RECEIVER CODE - Enhanced with timeout and better error recovery
bool receiveRecord() {
    static bool recvInProgress = false;
    static bool escapeNext = false;
    static uint8_t expectedSize = 0;
    static uint8_t expectedChecksum = 0;
    static uint8_t ndx = 0;
    static unsigned long lastByteTime = 0;
    static uint8_t syncCount = 0;
    
    uint8_t rb;
    unsigned long currentTime = millis();

    // Timeout check - reset if no data received for too long
    if (recvInProgress && (currentTime - lastByteTime > PACKET_TIMEOUT)) {
        recvInProgress = false;
        ndx = 0;
        escapeNext = false;
        syncCount = 0;
        Serial.println("Packet timeout - resetting");
    }

    while (Serial.available() > 0 && newData == false) {
        rb = Serial.read();
        lastByteTime = currentTime;

        if (recvInProgress == true) {
            if (rb == END_MARKER && !escapeNext) {
                // End of transmission
                recvInProgress = false;
                
                // Validate received size and checksum
                if (ndx == expectedSize && expectedSize == sizeof(EphemerisRecord)) {
                    uint8_t actualChecksum = calculateChecksum(receivedBytes, ndx);
                    if (actualChecksum == expectedChecksum) {
                        numReceived = ndx;
                        newData = true;
                        Serial.println("Packet received OK");
                    } else {
                        Serial.print("Checksum error - expected: ");
                        Serial.print(expectedChecksum);
                        Serial.print(", got: ");
                        Serial.println(actualChecksum);
                    }
                } else {
                    Serial.print("Size error - expected: ");
                    Serial.print(expectedSize);
                    Serial.print(", got: ");
                    Serial.println(ndx);
                }
                ndx = 0;
                escapeNext = false;
                syncCount = 0;
            }
            else if (rb == ESCAPE_BYTE && !escapeNext) {
                // Next byte is escaped
                escapeNext = true;
            }
            else {
                // Regular data byte or escaped byte
                if (ndx < maxBytes) {
                    receivedBytes[ndx] = rb;
                    ndx++;
                }
                escapeNext = false;
                
                // Check for buffer overflow
                if (ndx >= maxBytes) {
                    recvInProgress = false;
                    ndx = 0;
                    escapeNext = false;
                    syncCount = 0;
                    Serial.println("Buffer overflow - resetting");
                }
            }
        }
        else {
            // Looking for sync pattern and start marker
            if (rb == SYNC_BYTE) {
                syncCount++;
            } else if (rb == START_MARKER && syncCount >= 2) {
                // Found proper sync pattern + start marker
                recvInProgress = true;
                ndx = 0;
                escapeNext = false;
                syncCount = 0;
                
                // Wait for size and checksum bytes with timeout
                unsigned long waitStart = millis();
                while (Serial.available() < 2 && (millis() - waitStart < 100)) {
                    delay(1);
                }
                
                if (Serial.available() >= 2) {
                    expectedSize = Serial.read();
                    expectedChecksum = Serial.read();
                    
                    // Validate expected size
                    if (expectedSize != sizeof(EphemerisRecord)) {
                        recvInProgress = false;
                        Serial.print("Invalid expected size: ");
                        Serial.println(expectedSize);
                    }
                } else {
                    recvInProgress = false;
                    Serial.println("Timeout waiting for size/checksum");
                }
            } else {
                syncCount = 0;  // Reset sync count if we get unexpected byte
            }
        }
    }
    
    return newData;
}

#endif

void loop() {
#ifndef DATA_MODULE
    if(!displayReady) return;
    
    receiveRecord();

    if(newData) {
        display.setCursor(0, 0);
        display.clearDisplay();
        
        EphemerisRecord* rec = (EphemerisRecord*)(receivedBytes);
        
        // Additional validation
        if(rec && rec->timestamp > 0 && rec->timestamp < 4294967295UL) {
            display.print("Alt: ");
            display.println(rec->altitude_deg, 2);
            display.print("Time: ");
            display.println(rec->timestamp);
            display.print("Phase: ");
            display.println(rec->phase, 3);
            display.print("Status: OK");
            display.display();
        }
        else {
            display.print("Invalid data values");
            display.display();
        }
        newData = false;
    }
    
    delay(50); // Faster polling for better responsiveness
    
#else
    RTC.updateTime(); 
    EphemerisRecord record;
    uint32_t time = getCurrentTimestamp();
    
    if (moon.findRecord(time, &record)) {
        Serial.print("Transmitting - alt: ");
        Serial.println(record.altitude_deg);
        transmitRecord(record);
    }
    else {
        Serial.println("record not found");
    }

    delay(5000);  // Reduced delay for more frequent updates
#endif
}
