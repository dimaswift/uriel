// URIEL: 3D Volumetric Harmonic Field (ShaderToy Style)
// Inspired by electron shells, glyph memory, and sacred interference

#define NUM_SOURCES 8
#define FIELD_DEPTH 256
#define FREQ_MULT 50.0
#define HUE_RANGE 50.233
#define SLICE_SCROLL_SPEED 5.0

// === HSV to RGB ===
vec3 hsv2rgb(float h, float s, float v) {
    h = fract(h);
    float i = floor(h * 6.0);
    float f = h * 6.0 - i;
    float p = v * (1.0 - s);
    float q = v * (1.0 - f * s);
    float t = v * (1.0 - (1.0 - f) * s);
    if(i == 0.0) return vec3(v, t, p);
    else if(i == 1.0) return vec3(q, v, p);
    else if(i == 2.0) return vec3(p, v, t);
    else if(i == 3.0) return vec3(p, q, v);
    else if(i == 4.0) return vec3(t, p, v);
    else return vec3(v, p, q);
}

// === Interference at a given phase slice ===
float interference3D(vec2 uv, float zPhase) {
    float sum = 0.0;
    for (int i = 0; i < NUM_SOURCES; i++) {
        float a = float(i) / float(NUM_SOURCES) * 6.28318;
        vec2 source = 0.5 + 0.5 * vec2(cos(a), sin(a));
        float d = distance(uv, source);
        sum += sin(d * FREQ_MULT - zPhase);
    }
    return sum;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord.xy / iResolution.xy;
    uv = uv * 1.1; // Add margin
    vec2 mouse = iMouse.xy / iResolution.xy;
    float sliceIndex = mod(iTime * SLICE_SCROLL_SPEED, float(FIELD_DEPTH));

    float accum = 0.0;
    for (int z = 0; z < FIELD_DEPTH; z++) {
        float zPhase = float(z) * mouse.x * 0.1;
        float value = interference3D(uv, zPhase + sliceIndex);

        // Pick one projection strategy:
        // accum = max(accum, value);          // Max projection (isoform)
        accum += abs(value);                  // Sum of amplitudes
         //accum += step(0.9, value);          // Binary isosurface
    }

    accum /= float(FIELD_DEPTH);
    float normalized = 0.5 + 0.5 * accum / float(NUM_SOURCES);
    vec3 color = hsv2rgb(normalized * HUE_RANGE + 2.5 * sin(iTime), 1.0, 1.0);

    fragColor = vec4(color, 1.0);
}