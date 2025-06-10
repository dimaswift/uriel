#ifndef SOUND_H
#define SOUND_H
#include <stdbool.h>
#define SAMPLE_RATE 16200
#define MAX_SOURCES 8

typedef void (*SoundStreamCallback)(float* buffer, int num_frames, int num_channels, void* user_data);

void sine_wave_callback(float* buffer, int num_frames, int num_channels, void* user_data);
void generate_sine_clip(float* buffer, const int size, const float frequency);
void noise_callback(float* buffer, const int num_frames, const int num_channels, void* user_data);
int add_sound_source(const SoundStreamCallback callback, void *user_data);
bool remove_sound_source(const SoundStreamCallback callback) ;
bool is_sound_source_playing(const SoundStreamCallback callback);
int add_sound_clip(const float* clip_buffer, const int clip_size, const float volume);
void sound_setup() ;

typedef struct {
    bool playing;                     // Is this sound source playing?
    SoundStreamCallback callback;     // Custom callback for procedural sounds
    void* user_data;                  // User data for callbacks
    const float* clip_buffer;         // Pointer to pre-generated sound clip data
    int clip_size;                    // Total number of frames in the clip
    int clip_position;                // Current position in the clip
    float volume;                     // Volume of the clip
    float duration;                   // Duration in seconds (for stopping logic)
} SoundSource;

typedef struct {
    float frequency;
    float amplitude;
    float phase;
} SineWave;

#endif