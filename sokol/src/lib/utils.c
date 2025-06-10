//
// Created by Dmitry Popov on 09.06.2025.
//

#include "utils.h"

/**
 * Replace the extension of a file path
 * @param path Original file path
 * @param new_ext New extension (with or without leading dot)
 * @param buffer Output buffer for the new path
 * @param buffer_size Size of the output buffer
 * @return true on success, false on failure (buffer too small)
 */
bool replace_extension(const char* path, const char* new_ext, char* buffer, size_t buffer_size) {
    if (!path || !new_ext || !buffer || buffer_size == 0) {
        return false;
    }

    // Find the last dot and last path separator
    const char* last_dot = strrchr(path, '.');
    const char* last_slash = strrchr(path, '/');
    const char* last_backslash = strrchr(path, '\\');

    // Get the actual last path separator (handle both / and \)
    const char* last_separator = NULL;
    if (last_slash && last_backslash) {
        last_separator = (last_slash > last_backslash) ? last_slash : last_backslash;
    } else if (last_slash) {
        last_separator = last_slash;
    } else if (last_backslash) {
        last_separator = last_backslash;
    }

    // Determine if we have a valid extension to replace
    // Extension is valid if:
    // 1. There's a dot
    // 2. The dot comes after the last path separator (or there's no separator)
    // 3. The dot is not the first character of the filename (hidden files)
    bool has_extension = false;
    size_t base_length = strlen(path);

    if (last_dot && (last_separator == NULL || last_dot > last_separator)) {
        // Make sure it's not a hidden file (dot at start of filename)
        const char* filename_start = last_separator ? last_separator + 1 : path;
        if (last_dot > filename_start) {
            has_extension = true;
            base_length = last_dot - path;
        }
    }

    // Prepare the new extension (ensure it starts with a dot)
    const char* ext_to_add = new_ext;
    char ext_buffer[256];
    if (new_ext[0] != '.') {
        snprintf(ext_buffer, sizeof(ext_buffer), ".%s", new_ext);
        ext_to_add = ext_buffer;
    }

    // Check if the result will fit in the buffer
    size_t new_length = base_length + strlen(ext_to_add);
    if (new_length >= buffer_size) {
        return false;
    }

    // Copy the base path (without extension) and add new extension
    strncpy(buffer, path, base_length);
    buffer[base_length] = '\0';
    strcat(buffer, ext_to_add);

    return true;
}

/**
 * In-place version that modifies the original string
 * @param path Path to modify (must have enough space for new extension)
 * @param path_size Maximum size of the path buffer
 * @param new_ext New extension (with or without leading dot)
 * @return true on success, false on failure
 */
bool replace_extension_inplace(char* path, size_t path_size, const char* new_ext) {
    return replace_extension(path, new_ext, path, path_size);
}

/**
 * Get just the extension from a path
 * @param path File path
 * @return pointer to extension (including dot), or NULL if no extension
 */
const char* get_extension(const char* path) {
    if (!path) return NULL;

    const char* last_dot = strrchr(path, '.');
    const char* last_slash = strrchr(path, '/');
    const char* last_backslash = strrchr(path, '\\');

    const char* last_separator = NULL;
    if (last_slash && last_backslash) {
        last_separator = (last_slash > last_backslash) ? last_slash : last_backslash;
    } else if (last_slash) {
        last_separator = last_slash;
    } else if (last_backslash) {
        last_separator = last_backslash;
    }

    if (last_dot && (last_separator == NULL || last_dot > last_separator)) {
        const char* filename_start = last_separator ? last_separator + 1 : path;
        if (last_dot > filename_start) {
            return last_dot;
        }
    }

    return NULL;
}

/**
 * Remove extension from path
 * @param path Original path
 * @param buffer Output buffer
 * @param buffer_size Size of output buffer
 * @return true on success, false on failure
 */
bool remove_extension(const char* path, char* buffer, size_t buffer_size) {
    if (!path || !buffer || buffer_size == 0) {
        return false;
    }

    const char* ext = get_extension(path);
    size_t base_length = ext ? (size_t)(ext - path) : strlen(path);

    if (base_length >= buffer_size) {
        return false;
    }

    strncpy(buffer, path, base_length);
    buffer[base_length] = '\0';

    return true;
}