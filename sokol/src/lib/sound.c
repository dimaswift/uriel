#include "sound.h"
#include <math.h>
#include <stdlib.h>
#include <string.h>
#include "../sokol/sokol_audio.h"

static SoundSource sources[MAX_SOURCES] = {0};

void sine_wave_callback(float* buffer, const int num_frames, const int num_channels, void* user_data) {
    SineWave* wave = user_data;
    const float two_pi = 2.0f * M_PI;

    for (int i = 0; i < num_frames; i++) {
        float sample = sinf(wave->phase) * wave->amplitude;
        for (int c = 0; c < num_channels; c++) {
            buffer[i * num_channels + c] = sample;
        }
        wave->phase += (two_pi * wave->frequency) / SAMPLE_RATE;
        if (wave->phase > two_pi) wave->phase -= two_pi;
    }
}

void generate_sine_clip(float* buffer, const int size, const float frequency) {
    const float two_pi = 2.0f * M_PI;
    for (int i = 0; i < size; i++) {
        const float t = (float)i / SAMPLE_RATE;
        buffer[i] = sinf(two_pi * frequency * t);
    }
}

int add_sound_clip(const float* clip_buffer, const int clip_size, const float volume) {
    for (int i = 0; i < MAX_SOURCES; i++) {
        if (!sources[i].playing) {
            sources[i].playing = true;
            sources[i].clip_buffer = clip_buffer;
            sources[i].clip_size = clip_size;
            sources[i].clip_position = 0;
            sources[i].volume = volume;
            sources[i].duration = 10;
            return i;
        }
    }
    return -1;
}

bool is_sound_source_playing(const SoundStreamCallback callback) {
    for (int i = 0; i < MAX_SOURCES; i++) {
        if (sources[i].callback == callback && sources[i].playing) {
            return true;
        }
    }
    return false;
}

bool remove_sound_source(const SoundStreamCallback callback) {
    for (int i = 0; i < MAX_SOURCES; i++) {
        if (sources[i].callback == callback) {
            sources[i].playing = false;
            sources[i].callback = NULL;
            sources[i].user_data = NULL;
            return true;
        }
    }
    return false;
}

int add_sound_source(const SoundStreamCallback callback, void *user_data) {
    for (int i = 0; i < MAX_SOURCES; i++) {
        if (!sources[i].playing) {
            sources[i].playing = true;
            sources[i].callback = callback;
            sources[i].user_data = user_data;
            return i;
        }
    }
    return -1;
}

void noise_callback(float* buffer, const int num_frames, const int num_channels, void* user_data) {
    const float amplitude = *(float*)user_data;

    for (int i = 0; i < num_frames; i++) {
        float sample = ((float)rand() / RAND_MAX) * 2.0f - 1.0f;
        sample *= amplitude;

        for (int c = 0; c < num_channels; c++) {
            buffer[i * num_channels + c] = sample;
        }
    }
}

static void stream_cb(float* buffer, const int num_frames, const int num_channels) {
    memset(buffer, 0, num_frames * num_channels * sizeof(float)); // Start with silence

    for (int i = 0; i < MAX_SOURCES; i++) {
        if (sources[i].playing) {
            float temp_buffer[num_frames * num_channels];
            memset(temp_buffer, 0, sizeof(temp_buffer));

            if (sources[i].callback) {
                // Procedural sound generation
                sources[i].callback(temp_buffer, num_frames, num_channels, sources[i].user_data);

            } else if (sources[i].clip_buffer) {
                // Sound clip playback
                for (int frame = 0; frame < num_frames; frame++) {
                    if (sources[i].clip_position >= sources[i].clip_size) {
                        sources[i].playing = false; // Stop when clip finishes
                        break;
                    }

                    const float sample = sources[i].clip_buffer[sources[i].clip_position++] * sources[i].volume;
                    for (int ch = 0; ch < num_channels; ch++) {
                        temp_buffer[frame * num_channels + ch] = sample;
                    }
                }
            }

            // Mix temp_buffer into main buffer
            for (int j = 0; j < num_frames * num_channels; j++) {
                buffer[j] += temp_buffer[j];
            }
        }
    }

    // Prevent clipping: Clamp samples to [-1.0, 1.0]
    for (int i = 0; i < num_frames * num_channels; i++) {
        if (buffer[i] > 1.0f) buffer[i] = 1.0f;
        if (buffer[i] < -1.0f) buffer[i] = -1.0f;
    }
}

void sound_setup() {
    saudio_setup(&(saudio_desc){
        .stream_cb = stream_cb,
        .num_channels = 1,
        .sample_rate = SAMPLE_RATE,
        .buffer_frames = 1024
    });
}
