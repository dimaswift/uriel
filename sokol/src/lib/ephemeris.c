//
// Created by Dmitry Popov on 09.06.2025.
//

#include "ephemeris.h"

int eph_parse_csv_file(const char *csv_filename, const char *binary_filename) {
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
    int result = eph_create_binary_file(records, parsed_count, time_step, binary_filename);
    free(records);

    return result;
}

int eph_create_binary_file(ephemeris_record_t *records, uint32_t count,
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

    fclose(file);
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
    ctx->records = malloc(sizeof(ephemeris_record_t) * ctx->header.record_count);

    if (fread(ctx->records, sizeof(ephemeris_record_t), ctx->header.record_count, ctx->data_file) != ctx->header.record_count) {
        fclose(ctx->data_file);
        return -1;
    }

    return 0;
}

void close_ephemeris(ephemeris_context_t *ctx) {
    if (ctx->data_file) {
        fclose(ctx->data_file);
        ctx->data_file = NULL;
    }
    if( ctx->records ) {
        free( ctx->records );
        ctx->records  = NULL;
    }
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
