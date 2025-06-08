// URIEL Core: Arduino Prototype v1.0
// Harmonic Light + Sound Creature

#include <Adafruit_NeoPixel.h>
#include <Servo.h>
#define SCREEN_WIDTH 128  // OLED width
#define SCREEN_HEIGHT 64  // OLED height
#define OLED_RESET    -1  // No reset pin
 
#define LED_PIN     7
#define NUM_LEDS    64
#define MATRIX_WIDTH 8
#define MATRIX_HEIGHT 8
#define BUZZER_PIN  9
#define POT_PIN     A0
#define SERVO_PIN 8
#define MATRIX_SIZE 27
#define VFIELD_SIZE 15  // Virtual harmonic field resolution
#define MAX_COMMAND_LENGTH 32
char commandBuffer[MAX_COMMAND_LENGTH];
int bufferIndex = 0;

Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);
Servo myservo;

typedef struct {
  float x;
  float y;
  float z;
} float3;

typedef struct  
{
    int iterations;
    float frequency;
    float amplitude;
    float phase;
    float radius;
    float density;
} Photon;

typedef struct 
{
    float time;
    float frequency;
    float phase;
    float amplitude;
} Modulation;

Photon photon = {
  .iterations = 1,
  .frequency = 33.0,
  .amplitude = 30.0,
  .phase = 1.0,
  .radius = 19.0,
  .density = 2.16
};

Modulation mod = {
  .time = 0.0,
  .frequency = 0.0,
  .phase = 1.0,
  .amplitude = 10.0
};

static float3 octa[6];
int currentPosition = 90;  // Start at center position
unsigned long lastMoveTime = 0;
const long moveInterval = 250;  // Move 1 degree every 50ms

// Deadzone for potentiometer (to prevent drift)
const int deadZoneCenter = 512;  // Center of potentiometer range (0-1023)
const int deadZoneWidth = 200;   // Width of deadzone (no movement zone)

void draw(Photon p, Modulation m);

float3 f3(float x, float y, float z) {
  return float3 {
    .x = x,
    .y = y,
    .z = z
  };
}

void getRGB(uint32_t color, uint8_t *r, uint8_t *g, uint8_t *b) {
  *r = (color >> 16) & 0xFF;  // Extract red
  *g = (color >> 8) & 0xFF;   // Extract green
  *b = color & 0xFF;          // Extract blue
}

uint32_t rgb2color(uint8_t r, uint8_t g, uint8_t b) {
  return ((uint32_t)r << 16) | ((uint32_t)g << 8) | b;
}

void printStatus() {
  Serial.println(F("=== Current Settings ==="));
  Serial.print(F("iterations=")); Serial.println(photon.iterations);
  Serial.print(F("frequency=")); Serial.println(photon.frequency);
  Serial.print(F("amplitude=")); Serial.println(photon.amplitude);
  Serial.print(F("phase=")); Serial.println(photon.phase);
  Serial.print(F("radius=")); Serial.println(photon.radius);
  Serial.print(F("density=")); Serial.println(photon.density);
  Serial.println(F("--- Modulation ---"));
  Serial.print(F("mod_time=")); Serial.println(mod.time);
  Serial.print(F("mod_freq=")); Serial.println(mod.frequency);
  Serial.print(F("mod_phase=")); Serial.println(mod.phase);
  Serial.print(F("mod_amp=")); Serial.println(mod.amplitude);
  Serial.print(F("brightness=")); Serial.println(strip.getBrightness());
}

void checkSerialCommands() {
  while (Serial.available() > 0) {
    char c = Serial.read();
    
    // End of command
    if (c == '\n' || c == '\r') {
      if (bufferIndex > 0) {
        commandBuffer[bufferIndex] = '\0';
        processCommand(commandBuffer);
        bufferIndex = 0;
      }
    }
    // Add to buffer if not full
    else if (bufferIndex < MAX_COMMAND_LENGTH - 1) {
      commandBuffer[bufferIndex++] = c;
    }
  }
}

bool processCommand(const char* command) {
  char param[16] = {0};
  char valueStr[16] = {0};
  
  // Find the equals sign
  const char* equals = strchr(command, '=');
  if (!equals) {
    Serial.println(F("Invalid command format. Use parameter=value"));
    return false;
  }
  
  // Extract parameter name and value
  int paramLen = equals - command;
  if (paramLen >= sizeof(param)) paramLen = sizeof(param) - 1;
  strncpy(param, command, paramLen);
  param[paramLen] = '\0';
  
  // Extract value
  strncpy(valueStr, equals + 1, sizeof(valueStr) - 1);
  float value = atof(valueStr);
  
  // Process the command
  bool validCommand = true;
  
  if (strcmp(param, "iterations") == 0) {
    photon.iterations = (int)value;
  } 
  else if (strcmp(param, "frequency") == 0) {
    photon.frequency = value;
  }
  else if (strcmp(param, "amplitude") == 0) {
    photon.amplitude = value;
  }
  else if (strcmp(param, "phase") == 0) {
    photon.phase = value;
  }
  else if (strcmp(param, "radius") == 0) {
    photon.radius = value;
  }
  else if (strcmp(param, "density") == 0) {
    photon.density = value;
  }
  // Modulation parameters
  else if (strcmp(param, "mod_time") == 0) {
    mod.time = value;
  }
  else if (strcmp(param, "mod_freq") == 0) {
    mod.frequency = value;
  }
  else if (strcmp(param, "mod_phase") == 0) {
    mod.phase = value;
  }
  else if (strcmp(param, "mod_amp") == 0) {
    mod.amplitude = value;
  }
  else if (strcmp(param, "brightness") == 0) {
    int brightness = constrain((int)value, 0, 255);
    strip.setBrightness(brightness);
    strip.show();
  }
  else {
    Serial.print(F("Unknown parameter: "));
    Serial.println(param);
    validCommand = false;
  }
  
  if (validCommand) {
    Serial.print(F("Updated "));
    Serial.print(param);
    Serial.print(F(" to "));
    Serial.println(value);
    
    // Immediately redraw with new settings
    draw(photon, mod);
  }
  
  return validCommand;
}


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

float minF(float a, float b) {
  return a < b ? a : b;
}

float maxF(float a, float b) {
  return a > b ? a : b;
}

float clamp(float v, float min, float max) {
  return maxF(min, minF(v, max));
}

float3 scale(float3 p, float v) {
  p.x *= v;
  p.y *= v;
  p.z *= v;
  return p;
}

float distance(float3 a, float3 b) {
    float dx = b.x - a.x;
    float dy = b.y - a.y;
    float dz = b.z - a.z;
    return sqrtf(dx*dx + dy*dy + dz*dz);
}
float sampleField(const float3 pos, const float3 vertex,
    const Photon photon, const int size, Modulation m)
{
    const float dist = clamp(distance(pos, vertex) * (1.0 / max(0.01, photon.radius * PI)), 0.0, 1.0);
    const float freq = photon.frequency + (m.time * m.frequency);
    const float phase = photon.phase + m.time * m.phase;
    const float amp = photon.amplitude;
    return sin(dist * freq + phase) * amp;
}

int getLEDIndex(int x, int y) {
    if (x % 2 == 0) {
        // Even columns: top to bottom
        return x * MATRIX_HEIGHT + y;
    } else {
        // Odd columns: bottom to top
        return x * MATRIX_HEIGHT + (MATRIX_HEIGHT - 1 - y);
    }
}



void draw(Photon p, Modulation m) {
  for (int y = 0; y < MATRIX_HEIGHT; y++) {
    for (int x = 0; x < MATRIX_WIDTH; x++) {
      float density = 0;
      float3 samplePoint;
      samplePoint.x = 2.0 * ((float) x / (MATRIX_WIDTH - 1) - 0.5);
      samplePoint.y = 2.0 * ((float) y / (MATRIX_WIDTH - 1) - 0.5);

      samplePoint.z = (sin(mod.time) / PI);


      for(int i = 0; i < 6; i++) {
        
          float3 vertex = octa[i];
          density += sampleField(samplePoint, scale(vertex, p.density), p, MATRIX_SIZE, m);
         
      }
      // for(int mx = -1; mx <= 1; mx++) {
      //   for(int my = -1; my <= 1; my++) {
      //     for(int mz = -1; mz <= 1; mz++) {
              
      //     }
      //   }
      // }
      int idx = getLEDIndex(x,y);
      float v = density * mod.amplitude;

      uint32_t color = hsv2rgb(v, 1, 1);
      uint8_t r,g,b;

      getRGB(color, &r, &g, &b);

      if (r > 100) {
            color = 0;
        }
     

      strip.setPixelColor(idx, color);
    }
  }
  strip.show();
}


void setup() {
  
  Serial.begin(9600);
  myservo.attach(SERVO_PIN);
  octa[0] = f3(1.0f, 0.0f, 0.0f);
  octa[1] = f3(-1.0f, 0.0f, 0.0f);
  octa[2] = f3(0.0f, 1.0f, 0.0f);
  octa[3] = f3(0.0f, -1.0f, 0.0f);
  octa[4] = f3(0.0f, 0.0f, 1.0f);
  octa[5] = f3(0.0f, 0.0f, -1.0f);
  pinMode(LED_PIN, OUTPUT);
  strip.begin();
  strip.setBrightness(200);
  strip.show();
  pinMode(BUZZER_PIN, OUTPUT);
  

}



void loop() {
  
  if (Serial.available() > 0) {
    // Read the incoming angle value
    int angle = Serial.parseInt();
    
    // Clear remaining data in buffer
    while (Serial.available() > 0) {
      Serial.read();
    }
    
    // Validate input and move servo
    if (angle >= 0 && angle <= 180) {
      Serial.print("Moving to: ");
      Serial.print(angle);
      Serial.println(" degrees");
      
      myservo.write(angle);
    } else {
      Serial.println("Invalid angle. Enter a value between 0 and 180.");
    }
  }

  checkSerialCommands();
  int potValue = analogRead(1);
  
  // Determine direction based on pot value
  int direction = 0; // 0 = no movement, -1 = CCW, 1 = CW
  
  // Calculate distance from center
  int distFromCenter = potValue - deadZoneCenter;
  
  // Determine direction based on potentiometer position
  if (abs(distFromCenter) < deadZoneWidth/2) {
    // Within deadzone - no movement
    direction = 0;
  } else if (distFromCenter < 0) {
    // Left of center - counterclockwise
    direction = 1;
  } else {
    // Right of center - clockwise
    direction = -1;
  }
  
  // Check if it's time to move the servo
  unsigned long currentTime = millis();
  if (currentTime - lastMoveTime >= moveInterval) {
    lastMoveTime = currentTime;  // Reset move timer
    
    if (direction != 0) {
      // Update position in the specified direction
      currentPosition += direction;
      
      // Ensure position stays within valid range
      currentPosition = constrain(currentPosition, 0, 180);
      
      // Move servo to new position
      myservo.write(currentPosition);
      
      // Print current position and direction
      Serial.print("Position: ");
      Serial.print(currentPosition);
      Serial.print(" | Direction: ");
      Serial.print(direction > 0 ? "CW" : "CCW");
      Serial.print(" | Pot Value: ");
      Serial.println(potValue);
    }
  }
  
  // Update time-based modulation
  mod.time += 0.00001;
  
  // Draw the current pattern
  draw(photon, mod);
  
  delay(2);  // 20fps update rate
}
