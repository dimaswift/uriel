#ifndef EPHEMERIS_H
#define EPHEMERIS_H
#include <stdlib.h>

// Error codes
#define DE430_ERROR_NONE 0
#define DE430_ERROR_COMMAND_FAILED -1
#define DE430_ERROR_MEMORY_ALLOCATION -2
#define DE430_ERROR_PARSE_FAILED -3
#define DE430_ERROR_INVALID_CONFIG -4
#define DE430_ERROR_NOT_FOUND -5
#define DE430_ERROR_INVALID_DATE -6

#define DE430_ERROR_FILE_IO -5
#define DE430_ERROR_JSON_PARSE -6
#define DE430_ERROR_INVALID_CONFIG -7

// Buffer sizes
#define COMMAND_BUFFER_SIZE 4096
#define LINE_BUFFER_SIZE 2048
#define INITIAL_RESULTS_SIZE 1000
#define MAX_OBJECTS 32
#define MAX_NAME_LENGTH 64
#define CONSTELLATION_LENGTH 32
#define EPOCH_START 694566337UL

typedef struct {
 uint16_t year;
 uint8_t month;
 uint8_t day;
 uint8_t hour;
 uint8_t minute;
 uint8_t second;
} DS1302Time;



// Header structure (32 bytes, must match C code)
struct EphemerisHeader {
    uint32_t magic;              // Magic number for file identification
    uint32_t version;            // File format version
    uint32_t record_count;       // Number of records in file
    uint32_t record_size;        // Size of each record in bytes
    uint32_t start_timestamp;    // First timestamp in dataset
    uint32_t end_timestamp;      // Last timestamp in dataset  
    uint32_t time_step_seconds;  // Time step between regular records
    uint32_t reserved;           // Reserved for future use
};
#pragma pack(push, 1)
// Binary record structure (28 bytes, must match C code)
struct EphemerisRecord {
    uint32_t timestamp;          // Seconds since custom epoch
    float phase;                 // Moon phase (0.0-1.0)
    float distance_km;           // Distance from observer in km
    float azimuth_deg;           // Azimuth in degrees
    float altitude_deg;          // Altitude in degrees
    uint8_t event;               // Rise/set/culmination event
    uint8_t distance_event;      // Apogee/perigee event
    uint8_t closest_planet_id;   // ID of closest planet
    uint8_t reserved;            // Padding for alignment
    float angular_distance_deg;  // Angular distance to closest planet
};
#pragma pack(pop)

typedef struct {
    uint16_t magic;          // 0xCELE for "CELEstial"
    uint16_t length;         // Should be sizeof(EphemerisRecord)
    EphemerisRecord data;
    uint16_t checksum;       // Simple checksum for validation
} CelestialPacket;

uint32_t ds1302_to_epoch_seconds(const DS1302Time *ds_time);
uint32_t unix_to_epoch_seconds(uint32_t unix_timestamp);
uint32_t epoch_seconds_to_unix(uint32_t epoch_seconds);
uint16_t calculateChecksum(const EphemerisRecord* rec);

static inline bool is_leap_year(int year) {
    return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
}

uint16_t calculateChecksum(const EphemerisRecord* rec) {
    uint16_t sum = 0;
    uint8_t* bytes = (uint8_t*)rec;
    for (int i = 0; i < sizeof(EphemerisRecord); i++) {
        sum += bytes[i];
    }
    return sum;
}

uint32_t ds1302_to_epoch_seconds(const DS1302Time *ds_time) {
    if (!ds_time || ds_time->year < 1992 || ds_time->month < 1 || ds_time->month > 12 ||
        ds_time->day < 1 || ds_time->day > 31 || ds_time->hour > 23 || 
        ds_time->minute > 59 || ds_time->second > 59) {
        return 0; // Invalid date or before our epoch
    }

    static const uint16_t days_in_month[12] = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
    
    uint32_t total_days = 0;
    
    for (int year = 1992; year < ds_time->year; year++) {
        if (is_leap_year(year)) {
            total_days += 366;
        } else {
            total_days += 365;
        }
    }
    

    for (int month = 1; month < ds_time->month; month++) {
        total_days += days_in_month[month - 1];
      
        if (month == 2 && is_leap_year(ds_time->year)) {
            total_days += 1;
        }
    }

    if (ds_time->year == 1992 && ds_time->month == 1) {
        total_days -= 4; 
    } else if (ds_time->year > 1992 || (ds_time->year == 1992 && ds_time->month > 1)) {
        total_days -= 4;
    }
    
    // Add days in current month (subtract 1 because day 1 = 0 days elapsed)
    total_days += (ds_time->day - 1);
    
    // Convert to total seconds
    uint32_t total_seconds = 
        total_days * 86400UL +           // Days to seconds
        ds_time->hour * 3600UL +         // Hours to seconds
        ds_time->minute * 60UL +         // Minutes to seconds
        ds_time->second;                 // Seconds

    
    return total_seconds + 3260;
}

uint32_t unix_to_epoch_seconds(uint32_t unix_timestamp) {

    if (unix_timestamp < EPOCH_START) {
        return 0; // Before our epoch
    }
    
    return unix_timestamp - EPOCH_START;
}

uint32_t epoch_seconds_to_unix(uint32_t epoch_seconds) {
   
    return epoch_seconds + EPOCH_START;
}

int epoch_seconds_to_ds1302(uint32_t epoch_seconds, DS1302Time *ds_time);

/**
 * Convert epoch seconds back to DS1302 time
 */
int epoch_seconds_to_ds1302(uint32_t epoch_seconds, DS1302Time *ds_time) {
   if (!ds_time) {
        return -1;
    }
    
    // Days in each month (non-leap year)
    static const uint8_t days_in_month[12] = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
    
    // Extract time of day
    uint32_t seconds_in_day = (epoch_seconds) % 86400UL;
    uint32_t total_days = (epoch_seconds) / 86400UL;
    Serial.println(total_days);
    ds_time->hour = seconds_in_day / 3600UL;
    ds_time->minute = (seconds_in_day % 3600UL) / 60UL;
    ds_time->second = seconds_in_day % 60UL;
    
    // Add the 4-day offset (epoch starts on Jan 5, not Jan 1)
    total_days += 4;
    
    // Find the year
    ds_time->year = 1992;
    uint32_t remaining_days = total_days;
    
    while (true) {
        uint32_t days_in_current_year = is_leap_year(ds_time->year) ? 366 : 365;
        
        if (remaining_days < days_in_current_year) {
            break; // Found the year
        }
        
        remaining_days -= days_in_current_year;
        ds_time->year++;
    }
    
    // Find the month and day within the year
    ds_time->month = 1;
    for (int month = 0; month < 12; month++) {
        uint32_t days_in_this_month = days_in_month[month];
        
        // Add extra day for February in leap years
        if (month == 1 && is_leap_year(ds_time->year)) {
            days_in_this_month = 29;
        }
        
        if (remaining_days < days_in_this_month) {
            ds_time->month = month + 1;
            ds_time->day = remaining_days + 1;
            break;
        }
        
        remaining_days -= days_in_this_month;
    }
    
    return 0;
}



#endif

