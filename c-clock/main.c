
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

// Index entry for fast lookups (8 bytes)
typedef struct {
    uint32_t timestamp;          // Timestamp key
    uint32_t file_offset;        // Byte offset in data file
} index_entry_t;

// Context structure for Arduino usage
typedef struct {
    FILE *data_file;
    ephemeris_header_t header;
    index_entry_t *index;        // Array of index entries
    uint32_t index_size;         // Number of index entries
    uint32_t cache_timestamp;    // Cached record timestamp
    ephemeris_record_t cache_record; // Cached record data
    uint8_t cache_valid;         // Cache validity flag
} ephemeris_context_t;

// Function prototypes
int parse_csv_file(const char *csv_filename, const char *binary_filename);
int create_binary_file(ephemeris_record_t *records, uint32_t count,
                      uint32_t time_step, const char *filename);
int load_ephemeris(const char *filename, ephemeris_context_t *ctx);
void close_ephemeris(ephemeris_context_t *ctx);
int find_record_by_timestamp(ephemeris_context_t *ctx, uint32_t timestamp,
                             ephemeris_record_t *record);
int interpolate_position(ephemeris_context_t *ctx, uint32_t timestamp,
                        ephemeris_record_t *result);
void print_record(const ephemeris_record_t *record, uint32_t timestamp);

// Utility functions
int skip_csv_comments(FILE *file);
float parse_float_field(const char *field);
uint32_t parse_uint_field(const char *field);

int main(int argc, char *argv[]) {
    if (argc != 3) {
        printf("Usage: %s <input.csv> <output.bin>\n", argv[0]);
        printf("Converts ephemeris CSV to efficient binary format for Arduino\n");
        return 1;
    }

    printf("Ephemeris Binary Converter v1.0\n");
    printf("================================\n");

    // Convert CSV to binary
    printf("Converting %s to %s...\n", argv[1], argv[2]);
    if (parse_csv_file(argv[1], argv[2]) != 0) {
        printf("Error: Failed to convert CSV file\n");
        return 1;
    }

    // Test the binary file
    printf("\nTesting binary file...\n");
    ephemeris_context_t ctx;
    if (load_ephemeris(argv[2], &ctx) != 0) {
        printf("Error: Failed to load binary file\n");
        return 1;
    }

    printf("Binary file loaded successfully!\n");
    printf("Records: %u\n", ctx.header.record_count);
    printf("Time range: %u to %u\n", ctx.header.start_timestamp, ctx.header.end_timestamp);
    printf("File size: %u bytes\n",
           (uint32_t)(sizeof(ephemeris_header_t) +
                     ctx.header.record_count * sizeof(ephemeris_record_t) +
                     ctx.index_size * sizeof(index_entry_t)));

    // Test lookups
    printf("\nTesting lookups:\n");
    ephemeris_record_t record;
    uint32_t test_timestamps[] = {
        ctx.header.start_timestamp,
        ctx.header.start_timestamp + 86400,  // +1 day
        (ctx.header.start_timestamp + ctx.header.end_timestamp) / 2,  // middle
        ctx.header.end_timestamp
    };

    for (int i = 0; i < 4; i++) {
        if (find_record_by_timestamp(&ctx, test_timestamps[i], &record) == 0) {
            printf("  ");
            print_record(&record, test_timestamps[i]);
        }
    }

    close_ephemeris(&ctx);
    printf("\nConversion complete!\n");
    return 0;
}

int parse_csv_file(const char *csv_filename, const char *binary_filename) {
    FILE *csv_file = fopen(csv_filename, "r");
    if (!csv_file) {
        printf("Error: Cannot open CSV file %s\n", csv_filename);
        return -1;
    }

    // Skip header comments
    if (skip_csv_comments(csv_file) != 0) {
        fclose(csv_file);
        return -1;
    }

    // Read CSV header line
    char line[MAX_LINE_LENGTH];
    if (!fgets(line, sizeof(line), csv_file)) {
        printf("Error: Cannot read CSV header\n");
        fclose(csv_file);
        return -1;
    }

    // Count records first
    long data_start = ftell(csv_file);
    uint32_t record_count = 0;
    while (fgets(line, sizeof(line), csv_file)) {
        if (strlen(line) > 5) record_count++;  // Skip empty lines
    }

    printf("Found %u records in CSV\n", record_count);

    // Allocate memory for records
    ephemeris_record_t *records = malloc(record_count * sizeof(ephemeris_record_t));
    if (!records) {
        printf("Error: Cannot allocate memory for %u records\n", record_count);
        fclose(csv_file);
        return -1;
    }

    // Reset to data start and parse records
    fseek(csv_file, data_start, SEEK_SET);
    uint32_t parsed_count = 0;
    uint32_t min_timestamp = UINT32_MAX;
    uint32_t max_timestamp = 0;

    while (fgets(line, sizeof(line), csv_file) && parsed_count < record_count) {
        // Parse CSV line: timestamp,phase,distance_km,azimuth_deg,altitude_deg,event,distance_event,closest_planet_id,angular_distance_deg,datetime_utc
        char *fields[10];
        int field_count = 0;

        char *token = strtok(line, ",");
        while (token && field_count < 10) {
            fields[field_count++] = token;
            token = strtok(NULL, ",");
        }

        if (field_count >= 9) {  // Need at least 9 fields
            ephemeris_record_t *rec = &records[parsed_count];

            rec->timestamp = parse_uint_field(fields[0]);
            rec->phase = parse_float_field(fields[1]);
            rec->distance_km = parse_float_field(fields[2]);
            rec->azimuth_deg = parse_float_field(fields[3]);
            rec->altitude_deg = parse_float_field(fields[4]);
            rec->event = (uint8_t)parse_uint_field(fields[5]);
            rec->distance_event = (uint8_t)parse_uint_field(fields[6]);
            rec->closest_planet_id = (uint8_t)parse_uint_field(fields[7]);
            rec->angular_distance_deg = parse_float_field(fields[8]);
            rec->reserved = 0;

            // Track timestamp range
            if (rec->timestamp < min_timestamp) min_timestamp = rec->timestamp;
            if (rec->timestamp > max_timestamp) max_timestamp = rec->timestamp;

            parsed_count++;
        }
    }

    fclose(csv_file);

    printf("Parsed %u records\n", parsed_count);
    printf("Timestamp range: %u to %u\n", min_timestamp, max_timestamp);

    // Estimate time step (use first two regular records)
    uint32_t time_step = 3600;  // Default 1 hour
    if (parsed_count >= 2) {
        time_step = records[1].timestamp - records[0].timestamp;
        printf("Detected time step: %u seconds\n", time_step);
    }

    // Create binary file
    int result = create_binary_file(records, parsed_count, time_step, binary_filename);
    free(records);

    return result;
}

int create_binary_file(ephemeris_record_t *records, uint32_t count,
                      uint32_t time_step, const char *filename) {
    FILE *file = fopen(filename, "wb");
    if (!file) {
        printf("Error: Cannot create binary file %s\n", filename);
        return -1;
    }

    // Create header
    ephemeris_header_t header;
    header.magic = EPHEMERIS_MAGIC;
    header.version = EPHEMERIS_VERSION;
    header.record_count = count;
    header.record_size = sizeof(ephemeris_record_t);
    header.start_timestamp = records[0].timestamp;
    header.end_timestamp = records[count-1].timestamp;
    header.time_step_seconds = time_step;
    header.reserved = 0;

    // Write header
    if (fwrite(&header, sizeof(ephemeris_header_t), 1, file) != 1) {
        printf("Error: Cannot write header\n");
        fclose(file);
        return -1;
    }

    // Write records
    if (fwrite(records, sizeof(ephemeris_record_t), count, file) != count) {
        printf("Error: Cannot write records\n");
        fclose(file);
        return -1;
    }

    // Create and write index (every 10th record for faster seeks)
    uint32_t index_stride = 10;
    uint32_t index_count = (count + index_stride - 1) / index_stride;

    for (uint32_t i = 0; i < count; i += index_stride) {
        index_entry_t entry;
        entry.timestamp = records[i].timestamp;
        entry.file_offset = sizeof(header) + i * sizeof(ephemeris_record_t);

        if (fwrite(&entry, sizeof(entry), 1, file) != 1) {
            printf("Error: Cannot write index entry\n");
            fclose(file);
            return -1;
        }
    }

    fclose(file);
    printf("Binary file created: %u records, %u index entries\n", count, index_count);
    return 0;
}



int load_ephemeris(const char *filename, ephemeris_context_t *ctx) {
    memset(ctx, 0, sizeof(*ctx));

    ctx->data_file = fopen(filename, "rb");
    if (!ctx->data_file) {
        return -1;
    }

    // Read header
    if (fread(&ctx->header, sizeof(ctx->header), 1, ctx->data_file) != 1) {
        fclose(ctx->data_file);
        return -1;
    }

    // Verify magic number
    if (ctx->header.magic != EPHEMERIS_MAGIC) {
        fclose(ctx->data_file);
        return -1;
    }

    // Calculate index size and position
    ctx->index_size = (ctx->header.record_count + 9) / 10;  // Every 10th record
    long index_pos = sizeof(ctx->header) +
                     ctx->header.record_count * sizeof(ephemeris_record_t);

    // Load index
    ctx->index = malloc(ctx->index_size * sizeof(index_entry_t));
    if (!ctx->index) {
        fclose(ctx->data_file);
        return -1;
    }

    fseek(ctx->data_file, index_pos, SEEK_SET);
    if (fread(ctx->index, sizeof(index_entry_t), ctx->index_size, ctx->data_file) != ctx->index_size) {
        free(ctx->index);
        fclose(ctx->data_file);
        return -1;
    }

    ctx->cache_valid = 0;
    return 0;
}

void close_ephemeris(ephemeris_context_t *ctx) {
    if (ctx->data_file) {
        fclose(ctx->data_file);
        ctx->data_file = NULL;
    }
    if (ctx->index) {
        free(ctx->index);
        ctx->index = NULL;
    }
    ctx->cache_valid = 0;
}

int find_record_by_timestamp(ephemeris_context_t *ctx, uint32_t timestamp,
                             ephemeris_record_t *record) {
    // Check cache first
    if (ctx->cache_valid && ctx->cache_timestamp == timestamp) {
        *record = ctx->cache_record;
        return 0;
    }

    // Binary search in index
    int left = 0, right = ctx->index_size - 1;
    int best_index = 0;

    while (left <= right) {
        int mid = (left + right) / 2;
        if (ctx->index[mid].timestamp <= timestamp) {
            best_index = mid;
            left = mid + 1;
        } else {
            right = mid - 1;
        }
    }

    // Search from best index position
    uint32_t start_record = best_index * 10;
    uint32_t end_record = ((best_index + 1) * 10 < ctx->header.record_count) ?
                          (best_index + 1) * 10 : ctx->header.record_count;

    // Linear search in this small range
    for (uint32_t i = start_record; i < end_record; i++) {
        fseek(ctx->data_file, sizeof(ctx->header) + i * sizeof(ephemeris_record_t), SEEK_SET);
        ephemeris_record_t temp_record;

        if (fread(&temp_record, sizeof(temp_record), 1, ctx->data_file) == 1) {
            if (temp_record.timestamp == timestamp) {
                *record = temp_record;
                // Update cache
                ctx->cache_timestamp = timestamp;
                ctx->cache_record = temp_record;
                ctx->cache_valid = 1;
                return 0;
            }
            if (temp_record.timestamp > timestamp) {
                break;  // Past target time
            }
        }
    }

    return -1;  // Not found
}

// Utility functions
int skip_csv_comments(FILE *file) {
    char line[MAX_LINE_LENGTH];
    while (fgets(line, sizeof(line), file)) {
        if (line[0] != '#') {
            // Put back the non-comment line
            fseek(file, -strlen(line), SEEK_CUR);
            return 0;
        }
    }
    return -1;
}

float parse_float_field(const char *field) {
    return (float)atof(field);
}

uint32_t parse_uint_field(const char *field) {
    return (uint32_t)atoi(field);
}

void print_record(const ephemeris_record_t *record, uint32_t timestamp) {
    printf("T:%u Alt:%.1f° Az:%.1f° Dist:%.0fkm Phase:%.3f Event:%u\n",
           timestamp, record->altitude_deg, record->azimuth_deg,
           record->distance_km, record->phase, record->event);
}
