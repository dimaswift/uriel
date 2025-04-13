// ========== CONFIGURABLE PARAMETERS ==========
#define GRID_SIZE 128             // number of cells per row/column
#define FIELD_SIZE 128.0          // virtual resolution
#define NUM_SOURCES 1            // number of oscillators
#define SOURCE_SPEED 5.5         // time influence
#define FREQ_MULT 100.0           // distance scaling
#define HUE_RANGE 0.833          // rainbow cutoff
#define CELL_BORDER 0.00         // 0.0 = full square, 0.5 = no square

// ========== HSV TO RGB ==========
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

// ========== INTERFERENCE FIELD ==========
float interference(vec2 uv, float t) {
    float sum = 0.0;
    for (int i = 0; i < NUM_SOURCES; i++) {
        float a = float(i) / float(NUM_SOURCES) * 6.28318530718;
        vec2 source = 0.5 + 0.4 * vec2(cos(a), sin(a));
        float d = distance(uv, source);
        sum += sin(d * FREQ_MULT - t * SOURCE_SPEED + float(i));
    }
    return sum;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    // Normalize UV with square aspect
    vec2 resolution = vec2(min(iResolution.x, iResolution.y));
    vec2 uv = fragCoord.xy / resolution;
    vec2 cellUV = floor(uv * float(GRID_SIZE)) / float(GRID_SIZE);

    float stepSize = 1.0 / FIELD_SIZE;
    float accum = 0.0;
    int samplesPerCell = int(FIELD_SIZE / float(GRID_SIZE));

    // Integrate over higher-resolution field
    for (int y = 0; y < samplesPerCell; y++) {
        for (int x = 0; x < samplesPerCell; x++) {
            vec2 p = cellUV + vec2(x, y) * stepSize;
            accum += interference(p, iTime);
        }
    }

    accum /= float(samplesPerCell * samplesPerCell);
    float normalized = 0.5 + 0.5 * accum / float(NUM_SOURCES);
    vec3 color = hsv2rgb(normalized * HUE_RANGE, 1.0, 1.0);

    // Draw square cells with borders
    vec2 gridFrac = fract(uv * float(GRID_SIZE));
    float mask = step(CELL_BORDER, gridFrac.x) * step(CELL_BORDER, gridFrac.y) *
                 step(gridFrac.x, 1.0 - CELL_BORDER) * step(gridFrac.y, 1.0 - CELL_BORDER);

    fragColor = vec4(color * mask, 1.0);
}
