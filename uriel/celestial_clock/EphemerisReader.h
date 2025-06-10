
// EphemerisReader.h
#ifndef EPHEMERIS_READER_H
#define EPHEMERIS_READER_H
#include "ephemeris.h"
#include <Arduino.h>
#include <SD.h>

// Binary file format constants (must match C converter)
#define EPHEMERIS_MAGIC 0x45504845  // "EPHE" in hex
#define EPHEMERIS_VERSION 1

// Event type constants
#define EVENT_NONE 0
#define EVENT_RISE 1
#define EVENT_SET 2
#define EVENT_CULMINATION 3

#define DISTANCE_EVENT_NONE 0
#define DISTANCE_EVENT_PERIGEE 1
#define DISTANCE_EVENT_APOGEE 2

// Planet ID constants
#define PLANET_MERCURY 1
#define PLANET_VENUS 2
#define PLANET_MARS 4
#define PLANET_JUPITER 5
#define PLANET_SATURN 6
#define PLANET_URANUS 7
#define PLANET_NEPTUNE 8
#define PLANET_SUN 10
#define PLANET_MOON 11



class EphemerisReader {
private:
    File data_file;
    EphemerisHeader header;
    
    // Cache for reducing SD card reads
    uint32_t cache_timestamp;
    EphemerisRecord cache_record;
    bool cache_valid;
    
    // Private methods
    bool readHeader();
    uint32_t calculateFileOffset(uint32_t timestamp);
    bool readRecordAt(uint32_t file_offset, EphemerisRecord *record);
    bool searchNearTimestamp(uint32_t target_timestamp, EphemerisRecord *record, int search_range = 5);
    
public:
    EphemerisReader();
    ~EphemerisReader();
    
    // Main interface methods
    bool begin(const char *filename);
    void end();
    
    // Data access methods
    bool findRecord(uint32_t timestamp, EphemerisRecord *record);
    bool interpolateRecord(uint32_t timestamp, EphemerisRecord *result);
    bool getNextEvent(uint32_t from_timestamp, uint8_t event_type, EphemerisRecord *record);
    
    // Information methods
    uint32_t getStartTime() { return header.start_timestamp; }
    uint32_t getEndTime() { return header.end_timestamp; }
    float getTimeStep() { return header.time_step_seconds; }
    uint32_t getRecordCount() { return header.record_count; }
    
    // Utility methods
    void printRecord(const EphemerisRecord *record);
    const char* getEventName(uint8_t event);
    const char* getDistanceEventName(uint8_t distance_event);
    const char* getPlanetName(uint8_t planet_id);
    
    // Memory usage info (much smaller now!)
    uint16_t getMemoryUsage() { return sizeof(*this); }
};

#endif

EphemerisReader::EphemerisReader() {
    cache_valid = false;
    cache_timestamp = 0;
}

EphemerisReader::~EphemerisReader() {
    end();
}

bool EphemerisReader::begin(const char *filename) {
    // Clean up any previous state
    end();
    
    Serial.print(F("Opening file: "));
    Serial.println(filename);
    
    // Check if file exists
    if (!SD.exists(filename)) {
        Serial.println(F("Error: File does not exist"));
        return false;
    }
    
    // Open the binary file
    data_file = SD.open(filename, FILE_READ);

    int c = 0;
    while(!data_file && c < 10) { 
        c++;
        delay(10);
    }

    if (!data_file) {
        Serial.println(F("Error: Cannot open file"));
        return false;
    }
    
    // Check file size
    uint32_t file_size = data_file.size();
    Serial.print(F("File size: "));
    Serial.print(file_size);
    Serial.println(F(" bytes"));
    
    if (file_size < sizeof(EphemerisHeader)) {
        Serial.print(F("Error: File too small. Expected at least "));
        Serial.print(sizeof(EphemerisHeader));
        Serial.println(F(" bytes for header"));
        data_file.close();
        return false;
    }
    
    // Read and validate header
    if (!readHeader()) {
        data_file.close();
        return false;
    }
    
    Serial.print(F("Ephemeris loaded: "));
    Serial.print(header.record_count);
    Serial.print(F(" records, step: "));
    Serial.print(header.time_step_seconds);
    Serial.print(F("s, RAM: "));
    Serial.print(getMemoryUsage());
    Serial.println(F(" bytes"));
    
    return true;
}

void EphemerisReader::end() {
    if (data_file) {
        data_file.close();
    }
    cache_valid = false;
}

bool EphemerisReader::readHeader() {
    Serial.println(F("Reading header..."));
    
    // Reset file position to start
    data_file.seek(0);
    
    // Read header byte by byte for better debugging
    uint8_t *header_bytes = (uint8_t*)&header;
    uint32_t bytes_read = 0;
    
    for (uint32_t i = 0; i < sizeof(EphemerisHeader); i++) {
        if (data_file.available()) {
            header_bytes[i] = data_file.read();
            bytes_read++;
        } else {
            Serial.print(F("Error: Unexpected end of file at byte "));
            Serial.println(i);
            return false;
        }
    }
    
    Serial.print(F("Header bytes read: "));
    Serial.print(bytes_read);
    Serial.print(F(" of "));
    Serial.println(sizeof(EphemerisHeader));
    
    // Debug: Print first few bytes as hex
    Serial.print(F("First 8 bytes (hex): "));
    for (int i = 0; i < 8 && i < sizeof(EphemerisHeader); i++) {
        if (header_bytes[i] < 0x10) Serial.print(F("0"));
        Serial.print(header_bytes[i], HEX);
        Serial.print(F(" "));
    }
    Serial.println();
    
    // Validate magic number
    Serial.print(F("Magic number: 0x"));
    Serial.print(header.magic, HEX);
    Serial.print(F(" (expected: 0x"));
    Serial.print(EPHEMERIS_MAGIC, HEX);
    Serial.println(F(")"));
    
    if (header.magic != EPHEMERIS_MAGIC) {
        Serial.println(F("Error: Invalid file format (bad magic number)"));
        return false;
    }
    
    // Validate version
    Serial.print(F("Version: "));
    Serial.println(header.version);
    
    if (header.version != EPHEMERIS_VERSION) {
        Serial.print(F("Error: Unsupported version "));
        Serial.println(header.version);
        return false;
    }
    
    // Print header info
    Serial.print(F("Record count: "));
    Serial.println(header.record_count);
    Serial.print(F("Record size: "));
    Serial.println(header.record_size);
    Serial.print(F("Start timestamp: "));
    Serial.println(header.start_timestamp);
    Serial.print(F("End timestamp: "));
    Serial.println(header.end_timestamp);
    Serial.print(F("Time step: "));
    Serial.println(header.time_step_seconds);
    
    // Validate record size
    if (header.record_size != sizeof(EphemerisRecord)) {
        Serial.print(F("Warning: Record size mismatch. File: "));
        Serial.print(header.record_size);
        Serial.print(F(", Expected: "));
        Serial.println(sizeof(EphemerisRecord));
    }
    
    // Calculate expected file size
    uint32_t expected_size = sizeof(EphemerisHeader) + 
                            (header.record_count * header.record_size);  // Index size from C code
    
    Serial.print(F("Expected file size: "));
    Serial.print(expected_size);
    Serial.print(F(", Actual: "));
    Serial.println(data_file.size());
    


    return true;
}

uint32_t EphemerisReader::calculateFileOffset(uint32_t timestamp) {
    // Round timestamp down to nearest time step
    uint32_t aligned_timestamp = timestamp - (timestamp % header.time_step_seconds);
    
    // Calculate how many time steps from start
    if (aligned_timestamp < header.start_timestamp) {
        aligned_timestamp = header.start_timestamp;
    }
    
    uint32_t time_steps = ((aligned_timestamp - header.start_timestamp) / header.time_step_seconds) + 1;
    
    // Calculate file position
    uint32_t file_offset = sizeof(EphemerisHeader) + (time_steps * sizeof(EphemerisRecord));
    
    return file_offset;
}

bool EphemerisReader::readRecordAt(uint32_t file_offset, EphemerisRecord *record) {
    // Check if offset is within file bounds
    uint32_t max_offset = sizeof(EphemerisHeader) + (header.record_count * sizeof(EphemerisRecord));
    if (file_offset >= max_offset) {
        return false;
    }
    
    data_file.seek(file_offset);
    
    // Read record byte by byte for better reliability
    uint8_t *record_bytes = (uint8_t*)record;
    for (uint32_t i = 0; i < sizeof(EphemerisRecord); i++) {
        if (data_file.available()) {
            record_bytes[i] = data_file.read();
        } else {
            return false;
        }
    }
    
    return true;
}

bool EphemerisReader::searchNearTimestamp(uint32_t target_timestamp, EphemerisRecord *record, int search_range) {
    // Start from calculated position
    uint32_t base_offset = calculateFileOffset(target_timestamp);
    
    // Search backwards and forwards from the calculated position
    for (int offset = -search_range; offset <= search_range; offset++) {
        uint32_t search_offset = base_offset + (offset * sizeof(EphemerisRecord));
        
        // Skip invalid offsets
        if (search_offset < sizeof(EphemerisHeader)) continue;
        
        EphemerisRecord temp_record;
        if (readRecordAt(search_offset, &temp_record)) {
            if (temp_record.timestamp == target_timestamp) {
                *record = temp_record;
                return true;
            }
        }
    }
    
    return false;
}

bool EphemerisReader::findRecord(uint32_t timestamp, EphemerisRecord *record) {
    // Check bounds
    if (timestamp < header.start_timestamp || timestamp > header.end_timestamp) {
        Serial.print(F("Timestamp "));
        Serial.print(timestamp);
        Serial.print(F(" out of range ["));
        Serial.print(header.start_timestamp);
        Serial.print(F(", "));
        Serial.print(header.end_timestamp);
        Serial.println(F("]"));
        return false;
    }
    
    // Check cache first
    if (cache_valid && cache_timestamp == timestamp) {
        *record = cache_record;
        return true;
    }
    
    // Try direct calculation first (for regular hourly records)
    uint32_t file_offset = calculateFileOffset(timestamp);
    EphemerisRecord temp_record;
    Serial.print(F("Offset: "));
    Serial.println(file_offset);

    if (readRecordAt(file_offset, &temp_record)) {
        if(timestamp > temp_record.timestamp) {
                Serial.print(F("Timestamp is ahead: "));
                Serial.println(timestamp - temp_record.timestamp);
            }
           
            else {
                 Serial.print(F("Timestamp is behind: "));
                Serial.println(temp_record.timestamp - timestamp);
            }
        *record = temp_record;
            
            // Update cache
            cache_timestamp = timestamp;
            cache_record = temp_record;
            cache_valid = true;
            
            return true;
    }
    
    // If direct calculation didn't work, search nearby (for precise event records)
    if (searchNearTimestamp(timestamp, record, 300)) {
        // Update cache
        cache_timestamp = timestamp;
        cache_record = *record;
        cache_valid = true;
        return true;
    }
    
    return false;
}

bool EphemerisReader::interpolateRecord(uint32_t timestamp, EphemerisRecord *result) {
    // Check bounds
    if (timestamp < header.start_timestamp || timestamp > header.end_timestamp) {
        return false;
    }
    
    // Try exact match first
    if (findRecord(timestamp, result)) {
        return true;
    }
    
    // Find bracketing records for interpolation
    // Calculate the two nearest regular time steps
    uint32_t before_time = timestamp - (timestamp % header.time_step_seconds);
    uint32_t after_time = before_time + header.time_step_seconds;
    
    // Make sure we're within bounds
    if (before_time < header.start_timestamp) {
        before_time = header.start_timestamp;
    }
    if (after_time > header.end_timestamp) {
        after_time = header.end_timestamp;
    }
    
    EphemerisRecord before, after;
    bool found_before = findRecord(before_time, &before);
    bool found_after = findRecord(after_time, &after);
    
    // If we found both bracketing records, interpolate
    if (found_before && found_after && before_time < after_time) {
        float t = (float)(timestamp - before_time) / (float)(after_time - before_time);
        
        result->timestamp = timestamp;
        result->phase = before.phase + t * (after.phase - before.phase);
        result->distance_km = before.distance_km + t * (after.distance_km - before.distance_km);
        result->azimuth_deg = before.azimuth_deg + t * (after.azimuth_deg - before.azimuth_deg);
        result->altitude_deg = before.altitude_deg + t * (after.altitude_deg - before.altitude_deg);
        result->angular_distance_deg = before.angular_distance_deg + t * (after.angular_distance_deg - before.angular_distance_deg);
        
        // Copy non-interpolated fields from the closest record
        if (t < 0.5) {
            result->event = before.event;
            result->distance_event = before.distance_event;
            result->closest_planet_id = before.closest_planet_id;
        } else {
            result->event = after.event;
            result->distance_event = after.distance_event;
            result->closest_planet_id = after.closest_planet_id;
        }
        
        return true;
    }
    
    // If only one record found, use it
    if (found_before) {
        *result = before;
        result->timestamp = timestamp;
        return true;
    }
    
    if (found_after) {
        *result = after;
        result->timestamp = timestamp;
        return true;
    }
    
    return false;
}

bool EphemerisReader::getNextEvent(uint32_t from_timestamp, uint8_t event_type, EphemerisRecord *record) {
    // Search forward from the given timestamp
    uint32_t search_time = from_timestamp;
    uint32_t time_step = header.time_step_seconds;
    uint32_t max_search_steps = header.record_count;  // Limit search to prevent infinite loops
    uint32_t steps_searched = 0;
    
    while (search_time <= header.end_timestamp && steps_searched < max_search_steps) {
        EphemerisRecord temp_record;
        
        if (findRecord(search_time, &temp_record)) {
            if (temp_record.event == event_type || temp_record.distance_event == event_type) {
                *record = temp_record;
                return true;
            }
        }
        
        search_time += time_step;
        steps_searched++;
    }
    
    return false;  // No event found
}

void EphemerisReader::printRecord(const EphemerisRecord *record) {
    Serial.print(F("T:"));
    Serial.print(record->timestamp);
    Serial.print(F(" Alt:"));
    Serial.print(record->altitude_deg, 1);
    Serial.print(F("° Az:"));
    Serial.print(record->azimuth_deg, 1);
    Serial.print(F("° Dist:"));
    Serial.print(record->distance_km, 0);
    Serial.print(F("km Phase:"));
    Serial.print(record->phase, 3);
    
    if (record->event != EVENT_NONE) {
        Serial.print(F(" Event:"));
        Serial.print(getEventName(record->event));
    }
    
    if (record->distance_event != DISTANCE_EVENT_NONE) {
        Serial.print(F(" DistEvent:"));
        Serial.print(getDistanceEventName(record->distance_event));
    }
    
    Serial.print(F(" Closest:"));
    Serial.print(getPlanetName(record->closest_planet_id));
    Serial.print(F("("));
    Serial.print(record->angular_distance_deg, 1);
    Serial.print(F("°)"));
    
    Serial.println();
}

const char* EphemerisReader::getEventName(uint8_t event) {
    switch (event) {
        case EVENT_RISE: return "Rise";
        case EVENT_SET: return "Set";
        case EVENT_CULMINATION: return "Culmination";
        default: return "None";
    }
}

const char* EphemerisReader::getDistanceEventName(uint8_t distance_event) {
    switch (distance_event) {
        case DISTANCE_EVENT_PERIGEE: return "Perigee";
        case DISTANCE_EVENT_APOGEE: return "Apogee";
        default: return "None";
    }
}

const char* EphemerisReader::getPlanetName(uint8_t planet_id) {
    switch (planet_id) {
        case PLANET_MERCURY: return "Mercury";
        case PLANET_VENUS: return "Venus";
        case PLANET_MARS: return "Mars";
        case PLANET_JUPITER: return "Jupiter";
        case PLANET_SATURN: return "Saturn";
        case PLANET_URANUS: return "Uranus";
        case PLANET_NEPTUNE: return "Neptune";
        case PLANET_SUN: return "Sun";
        case PLANET_MOON: return "Moon";
        default: return "Unknown";
    }
}

// Example Arduino sketch with better error handling
/*
#include <SPI.h>
#include <SD.h>
#include "EphemerisReader.h"

const int SD_CS_PIN = 10;  // SD card chip select pin

EphemerisReader moon;

void setup() {
    Serial.begin(9600);
    while (!Serial) { delay(10); }
    
    Serial.println(F("Ephemeris Reader Test (Direct Positioning)"));
    Serial.println(F("=========================================="));
    
    // Initialize SD card
    Serial.println(F("Initializing SD card..."));
    if (!SD.begin(SD_CS_PIN)) {
        Serial.println(F("Error: SD card initialization failed"));
        Serial.println(F("Check:"));
        Serial.println(F("- SD card inserted properly"));
        Serial.println(F("- Wiring connections"));
        Serial.println(F("- CS pin setting"));
        return;
    }
    Serial.println(F("SD card initialized"));
    
    // List files on SD card for debugging
    Serial.println(F("Files on SD card:"));
    File root = SD.open("/");
    while (true) {
        File entry = root.openNextFile();
        if (!entry) break;
        
        Serial.print(F("  "));
        Serial.print(entry.name());
        Serial.print(F(" ("));
        Serial.print(entry.size());
        Serial.println(F(" bytes)"));
        entry.close();
    }
    root.close();
    
    // Try to load Moon ephemeris
    if (!moon.begin("moon.bin")) {
        Serial.println(F("Failed to load moon.bin"));
        Serial.println(F("Make sure you:"));
        Serial.println(F("1. Generated the binary file using the C converter"));
        Serial.println(F("2. Copied moon.bin to SD card root"));
        Serial.println(F("3. File is not corrupted"));
        return;
    }
    
    Serial.println(F("Success! Ready for lookups."));
    testBasicLookup();
}

void loop() {
    // Do nothing - all tests in setup
    delay(1000);
}

void testBasicLookup() {
    Serial.println(F("\nTesting basic lookup..."));
    
    uint32_t test_time = moon.getStartTime();
    EphemerisRecord record;
    
    Serial.print(F("Looking up timestamp: "));
    Serial.println(test_time);
    
    if (moon.findRecord(test_time, &record)) {
        Serial.print(F("Success! Found record: "));
        moon.printRecord(&record);
    } else {
        Serial.println(F("Failed to find record"));
    }
}
*/
