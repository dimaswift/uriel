#include <SPI.h>
#include <Adafruit_GFX.h>
#include <Adafruit_PCD8544.h>
#include "virtuabotixRTC.h" 
#include "ephemeris.h"

#include "EphemerisReader.h"

// Declare LCD object for software SPI
// Adafruit_PCD8544(CLK,DIN,D/C,CE,RST);
Adafruit_PCD8544 display = Adafruit_PCD8544(7, 6, 5, 4, 3);

int rotatetext = 1;

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


void setup()   {
	Serial.begin(9600);
    while (!Serial) { delay(10); }
    
    // Add small delays between initializations
    delay(100);
    
    // Initialize SD first
    Serial.println("Starting SD...");
    if (!SD.begin(SD_CS_PIN)) {
        Serial.println(F("SD failed"));
        return;
    }
    delay(50);  // Let SD settle
    
    // Load data while SD is stable
    if (!moon.begin("moon.bin")) {
        Serial.println(F("Data load failed"));
        return;
    }
    delay(50);
    
    // Now initialize display (software SPI shouldn't interfere)
    Serial.println("Starting display...");
    display.begin();
    delay(10);
    display.setContrast(57);
    display.clearDisplay();
    display.setTextSize(1);
    display.setTextColor(BLACK);
    display.setCursor(0,0);
    display.print("Records: ");
    display.println(moon.getRecordCount());
    display.print("Time step: ");
    display.print(moon.getTimeStep());
    display.display();
    
    Serial.println("All ready!");
    
    // Test lookups
    //testLookups();
    
	 // Set sketch compiling time 
	// RTC.setDS1302Time( 
	//    CURRENT_SECONDS, 
	//    CURRENT_MINUTES, 
	//    CURRENT_HOURS, 
	//    CURRENT_DAY_OF_WEEK, 
	//    CURRENT_DAY_OF_MONTH, 
	//    CURRENT_MONTH, 
	//    CURRENT_YEAR 
	//  ); 



}


void loop() {
    // Get current time (you would get this from RTC)
    // uint32_t current_time = getCurrentTimestamp();
    
    // // Get current moon position
    // EphemerisRecord moon_pos;
    // if (moon.interpolateRecord(current_time, &moon_pos)) {
    //     Serial.print(F("Moon: "));
    //     moon.printRecord(&moon_pos);
    // }
    
    // delay(60000);  // Update every minute
}


// void loop() {

//     delay(1000);
// }