//
// Created by Dmitry Popov on 09.06.2025.
//

#ifndef EPHEMERIES_H
#define EPHEMERIES_H

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <math.h>

// Binary file format constants
#define EPHEMERIS_MAGIC 0x45504845  // "EPHE" in hex
#define EPHEMERIS_VERSION 1
#define MAX_LINE_LENGTH 512
#define MAX_FILENAME 256

// Header structure (32 bytes, aligned)

typedef struct {
    uint32_t magic;              // Magic number for file identification
    uint32_t version;            // File format version
    uint32_t record_count;       // Number of records in file
    uint32_t record_size;        // Size of each record in bytes
    uint32_t start_timestamp;    // First timestamp in dataset
    uint32_t end_timestamp;      // Last timestamp in dataset
    uint32_t time_step_seconds;  // Time step between regular records
    uint32_t reserved;           // Reserved for future use
} ephemeris_header_t;

// Compact binary record structure (28 bytes)
typedef struct {
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
} ephemeris_record_t;


// Context structure for Arduino usage
typedef struct {
    FILE *data_file;
    ephemeris_header_t header;
    ephemeris_record_t* records; // Cached record data
} ephemeris_context_t;

// Function prototypes
int eph_parse_csv_file(const char *csv_filename, const char *binary_filename);
int eph_create_binary_file(ephemeris_record_t *records, uint32_t count,
                      uint32_t time_step, const char *filename);
int load_ephemeris(const char *filename, ephemeris_context_t *ctx);
void close_ephemeris(ephemeris_context_t *ctx);

// Utility functions
int skip_csv_comments(FILE *file);
float parse_float_field(const char *field);
uint32_t parse_uint_field(const char *field);

#endif //EPHEMERIES_H
