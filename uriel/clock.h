/*
 * DS1302 RTC Module Example
 * For MH Real Time Clock modules with CLK, DAT, RST pins
 */

#include "DS1302Unix.h"

const int kCePin   = 8;  // RST pin
const int kIoPin   = 7;  // DAT pin
const int kSclkPin = 6;  // CLK pin

#define CUSTOM_EPOCH 1747602000UL  //19.05.2025 00:00

DS1302Unix rtc(kCePin, kIoPin, kSclkPin);

uint32_t tick()  {
  return rtc.getTime();
} 

void clockSetup() {
  rtc.begin();
}
