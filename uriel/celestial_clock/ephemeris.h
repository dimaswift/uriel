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
#define EPOCH_START 694584840UL
#define BIRTH_OFFSET 26048

typedef struct {
 uint16_t year;
 uint8_t month;
 uint8_t day;
 uint8_t hour;
 uint8_t minute;
 uint8_t second;
} DS1302Time;

/**
 * Data structure representing an astronomical body's ephemeris data
 */
typedef struct {
    uint32_t epoch_seconds;                  // Julian date
    double position[3];         // X, Y, Z position (AU)
    double ra_dec[2];           // RA, Dec (radians)
    double magnitude;           // V-band magnitude
    double phase;               // Phase
    double angular_size;        // Angular size
    double physical_size;       // Physical size
    double albedo;              // Albedo
    double sun_dist;            // Distance from Sun
    double earth_dist;          // Distance from Earth
    double sun_ang_dist;        // Angular distance from Sun
    double theta_edo;           // Elongation parameter
    double ecliptic[3];         // eclLng, eclDist, eclLat
    char constellation[32];     // Constellation name
} EphemerisPoint;

/**
 * Collection of ephemeris data points
 */
typedef struct {
    EphemerisPoint *points;  // Array of data points
    int count;                    // Number of data points in the array
    char object_name[64];         // Name of the astronomical object
} EphemerisData;

// Constants

// DS1302 time structure

// New indexed binary format structures
typedef struct {
    char magic[4];              // "IDX4" magic identifier for indexed format
    uint32_t version;           // Format version (currently 1)
    uint32_t object_count;      // Number of celestial objects
    uint32_t start_time;            // First Date in dataset
    uint32_t end_time;              // Last Date in dataset
    uint32_t time_step;           // Time step between data points (in days)
    uint32_t time_points;       // Number of time points per object
    uint32_t data_start_offset; // Offset where actual data begins
    uint32_t reserved[4];       // Reserved for future use
} IndexedHeader;

// Object metadata (sorted by distance from Sun)
typedef struct {
    char name[MAX_NAME_LENGTH]; // Object name
    uint8_t object_id;          // 0-based ID (0=Mercury, 1=Venus, etc.)
    double avg_sun_distance;    // Average distance from Sun (for sorting)
    uint32_t data_offset;       // Offset to this object's data block
    uint32_t reserved;
} ObjectIndex;

// Compact binary data point (no variable-length strings)
typedef struct {
    uint32_t epoch_seconds;
    float position[3];          // Using float to save space
    float ra_dec[2];
    float magnitude;
    float phase;
    float angular_size;
    float physical_size;
    float albedo;
    float sun_dist;
    float earth_dist;
    float sun_ang_dist;
    float theta_edo;
    float ecliptic[3];
    uint8_t constellation_id;   // ID instead of string (0-87 for constellations)
} CompactPoint;


int de430_load_point_by_time(const char *filename, const DS1302Time *ds_time, 
                             const char *object_name, EphemerisPoint *result);

uint32_t ds1302_to_epoch_seconds(const DS1302Time *ds_time);
uint32_t unix_to_epoch_seconds(uint32_t unix_timestamp);
uint32_t epoch_seconds_to_unix(uint32_t epoch_seconds);

static inline bool is_leap_year(int year) {
    return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
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

    total_seconds -= BIRTH_OFFSET;
    return total_seconds;
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
    uint32_t seconds_in_day = (epoch_seconds + BIRTH_OFFSET) % 86400UL;
    uint32_t total_days = (epoch_seconds + BIRTH_OFFSET) / 86400UL;
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

