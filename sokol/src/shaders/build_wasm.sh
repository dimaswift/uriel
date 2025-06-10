#!/bin/bash

# Input directory containing .glsl files
SOURCE_DIR="${1:-.}"
# Output directory for compiled files
OUTPUT_DIR="${SOURCE_DIR}/compiled"

rm -r -f "$OUTPUT_DIR"

# Create the output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

# Loop through all .glsl files in the source directory
for FILE in "$SOURCE_DIR"/*.glsl; do
    if [ -f "$FILE" ]; then
        BASENAME=$(basename "$FILE" .glsl)           # Extract file name without extension
        OUTPUT="$OUTPUT_DIR/$BASENAME.glsl.h"        # Define output file path

        echo "Processing $FILE..."
        sokol-shdc --input "$FILE" --output "$OUTPUT" --slang glsl300es

        if [ $? -eq 0 ]; then
            echo "Generated: $OUTPUT"
        else
            echo "Error processing: $FILE"
        fi
    fi
done

echo "All .glsl files processed. Outputs are in '$OUTPUT_DIR'."
