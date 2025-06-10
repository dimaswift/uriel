//
// Created by Dmitry Popov on 09.06.2025.
//

#ifndef UTILS_H

#define UTILS_H

#include <string.h>
#include <stdio.h>
#include <stdbool.h>

bool replace_extension(const char* path, const char* new_ext, char* buffer, size_t buffer_size);
#endif //UTILS_H
