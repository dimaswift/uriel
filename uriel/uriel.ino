// URIEL Core: Arduino Prototype v1.0
// Harmonic Light + Sound Creature

#include <Adafruit_NeoPixel.h>

#define LED_PIN     6
#define NUM_LEDS    64
#define MATRIX_WIDTH 8
#define MATRIX_HEIGHT 8
#define BUZZER_PIN  9
#define POT_PIN     A0

#define VFIELD_SIZE 64  // Virtual harmonic field resolution

Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);

float field[VFIELD_SIZE][VFIELD_SIZE];
float t = 0.0;
const float freqMult = 1.15;
const int numSources = 3;

struct Source {
  float x, y, phase;
};
Source sources[5];

// === HSV to RGB conversion ===
uint32_t hsv2rgb(float h, float s, float v) {
  h = fmod(h, 1.0);
  float r, g, b;
  float i = floor(h * 6.0);
  float f = h * 6.0 - i;
  float p = v * (1.0 - s);
  float q = v * (1.0 - f * s);
  float t = v * (1.0 - (1.0 - f) * s);
  switch (int(i) % 6) {
    case 0: r = v; g = t; b = p; break;
    case 1: r = q; g = v; b = p; break;
    case 2: r = p; g = v; b = t; break;
    case 3: r = p; g = q; b = v; break;
    case 4: r = t; g = p; b = v; break;
    case 5: r = v; g = p; b = q; break;
  }
  return strip.Color(r * 255, g * 255, b * 255);
}

void setupSources() {
  for (int i = 0; i < numSources; i++) {
    float angle = TWO_PI * i / numSources;
    sources[i].x = 0.5 + 0.4 * cos(angle);
    sources[i].y = 0.5 + 0.4 * sin(angle);
    sources[i].phase = i * 1.5;
  }
}

void generateField() {
  for (int y = 0; y < VFIELD_SIZE; y++) {
    for (int x = 0; x < VFIELD_SIZE; x++) {
      float uvx = float(x) / VFIELD_SIZE;
      float uvy = float(y) / VFIELD_SIZE;
      float val = 0;
      for (int s = 0; s < numSources; s++) {
        float dx = uvx - sources[s].x;
        float dy = uvy - sources[s].y;
        float d = sqrt(dx * dx + dy * dy);
        val += sin(d * VFIELD_SIZE * freqMult - t + sources[s].phase);
      }
      field[y][x] = val / numSources;
    }
  }
}

void downsampleAndDisplay() {
  int block = VFIELD_SIZE / MATRIX_WIDTH;
  for (int y = 0; y < MATRIX_HEIGHT; y++) {
    for (int x = 0; x < MATRIX_WIDTH; x++) {
      float sum = 0;
      for (int dy = 0; dy < block; dy++) {
        for (int dx = 0; dx < block; dx++) {
          sum += field[y * block + dy][x * block + dx];
        }
      }
      float avg = sum / (block * block);
      float hue = (avg * 0.5 + 0.5) * 0.833;
      uint32_t color = hsv2rgb(1, 1.0, 1.0);
      int idx = y * MATRIX_WIDTH + x;
      strip.setPixelColor(idx, color);
    }
  }
  strip.show();
}

void playSoundFromField() {
  for (int y = 0; y < MATRIX_HEIGHT; y++) {
    int sum = 0;
    for (int x = 0; x < MATRIX_WIDTH; x++) {
      int idx = y * MATRIX_WIDTH + x;
      uint32_t c = strip.getPixelColor(idx);
      sum += (c & 0xFF) + ((c >> 8) & 0xFF) + ((c >> 16) & 0xFF);
    }
    int freq = 100 + sum / (MATRIX_WIDTH * 3);
    tone(BUZZER_PIN, freq, 20);
    delay(30);
  }
}

void setup() {
  strip.begin();
  strip.show();
  setupSources();
  pinMode(BUZZER_PIN, OUTPUT);
  Serial.begin(9600);
}

void loop() {
  generateField();
  downsampleAndDisplay();
  playSoundFromField();

  int pot = analogRead(POT_PIN);
  Serial.print("Potentiometer: "); Serial.println(pot);

  t += 0.05;
  delay(50);
}
