
cmake_minimum_required(VERSION 3.20)
project(uriel)
set(CMAKE_C_STANDARD 99)
set(CMAKE_CXX_STANDARD 11)

# Linux -pthread shenanigans
if (CMAKE_SYSTEM_NAME STREQUAL Linux)
    set(THREADS_PREFER_PTHREAD_FLAG ON)
    find_package(Threads REQUIRED)
endif()


#=== LIBRARY: cimgui + Dear ImGui
add_library(cimgui STATIC
        cimgui/cimgui.cpp
        cimgui/cimgui.h
        cimgui/imgui.cpp
        cimgui/imgui.h
        cimgui/imgui_widgets.cpp
        cimgui/imgui_draw.cpp
        cimgui/imgui_tables.cpp
        cimgui/imgui_demo.cpp)
target_include_directories(cimgui INTERFACE cimgui)



add_library(lib STATIC
        lib/utils.c
        lib/utils.h
        lib/ephemeris.c
        lib/ephemeris.h
        lib/ephemeris.c)

target_include_directories(lib INTERFACE lib)

#=== LIBRARY: tinyfiledialogs
add_library(tinyfiledialogs STATIC
        vendor/tinyfiledialogs/tinyfiledialogs.c
        vendor/tinyfiledialogs/tinyfiledialogs.h)
target_include_directories(tinyfiledialogs INTERFACE vendor/tinyfiledialogs)

# Platform-specific linking for tinyfiledialogs
if(CMAKE_SYSTEM_NAME STREQUAL Windows)
    target_link_libraries(tinyfiledialogs PUBLIC ole32 comdlg32)
elseif(CMAKE_SYSTEM_NAME STREQUAL Darwin)
    target_link_libraries(tinyfiledialogs PUBLIC "-framework Cocoa")
elseif(CMAKE_SYSTEM_NAME STREQUAL Linux)
    # For Linux, tinyfiledialogs can use zenity, kdialog, or basic console
    # No additional linking required for basic functionality
endif()

#=== LIBRARY: sokol
set(SOKOL_HEADERS
        sokol/sokol_gfx.h
        sokol/sokol_app.h
        sokol/sokol_imgui.h
        sokol/sokol_glue.h
        sokol/sokol_audio.h
        sokol/sokol_gl.h
        sokol/sokol_time.h
        sokol/sokol_color.h
        sokol/sokol_fetch.h
)
if(CMAKE_SYSTEM_NAME STREQUAL Darwin)
    add_library(sokol STATIC sokol/sokol.c ${SOKOL_HEADERS})
    target_compile_options(sokol PRIVATE -x objective-c)
    target_link_libraries(sokol PUBLIC "-framework AudioToolbox"
            "-framework CoreAudio"
            "-framework QuartzCore"
            "-framework Cocoa"
            "-framework MetalKit"
            "-framework Metal")
else()
    add_library(sokol STATIC sokol/sokol.c ${SOKOL_HEADERS})
    if (CMAKE_SYSTEM_NAME STREQUAL Linux)
        target_link_libraries(sokol INTERFACE X11 Xi Xcursor GL dl m)
        target_link_libraries(sokol PUBLIC Threads::Threads)
    endif()
endif()
target_link_libraries(sokol PUBLIC cimgui)
target_include_directories(sokol INTERFACE sokol)

#=== SHADER HEADERS: Include all .h files from shaders/compiled
file(GLOB_RECURSE SHADER_HEADERS "shaders/compiled/*.h")
if(SHADER_HEADERS)
    message(STATUS "Including shader headers: ${SHADER_HEADERS}")
    add_custom_target(shaders DEPENDS ${SHADER_HEADERS})
else()
    message(WARNING "No shader headers found in shaders/compiled")
endif()

#=== EXECUTABLE: main
if(CMAKE_SYSTEM_NAME STREQUAL Windows)
    add_executable(main WIN32 main.c)
    set_property(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} PROPERTY VS_STARTUP_PROJECT main)
    target_compile_definitions(main PRIVATE USE_GMP)
else()
    add_executable(main main.c
            lib/sound.c
            ${SHADER_HEADERS}
    )
endif()

# Link libraries to main executable
target_link_libraries(main sokol tinyfiledialogs lib)

# Emscripten-specific linker options
if (CMAKE_SYSTEM_NAME STREQUAL Emscripten)
    set(CMAKE_EXECUTABLE_SUFFIX ".html")
    target_link_options(main PRIVATE --shell-file ../sokol/shell.html)
    target_link_options(main PRIVATE -sUSE_WEBGL2=1)
    add_link_options("SHELL:-s USE_PTHREADS=1")
    target_link_options(main PRIVATE -sNO_FILESYSTEM=0 -pthread -sINITIAL_MEMORY=33554432 -sASSERTIONS=1 -sALLOW_MEMORY_GROWTH -DSOKOL_GLES3 -sMALLOC=emmalloc --closure=1)
    add_compile_options("SHELL:-s USE_PTHREADS=1")
    add_compile_definitions(SOKOL_GLES3)
else ()
    target_compile_definitions(main PRIVATE USE_GMP)
    add_compile_definitions(SOKOL_METAL)
endif()

# explicitly strip dead code
if (CMAKE_C_COMPILER_ID MATCHES "Clang" AND NOT CMAKE_SYSTEM_NAME STREQUAL Emscripten)
    target_link_options(main PRIVATE LINKER:-dead_strip)
endif()

# Add shaders as a dependency for main
add_dependencies(main shaders)

# this hack removes the xxx-CMakeForceLinker.cxx dummy file
set_target_properties(main PROPERTIES LINKER_LANGUAGE C)
