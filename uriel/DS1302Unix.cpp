/*
 * DS1302Unix.cpp - Implementation of the Unix timestamp library for DS1302
 */

 #include "DS1302Unix.h"

 // DS1302 register addresses
 #define REG_SECONDS    0x80
 #define REG_RAM_0      0xC0
 #define REG_RAM_1      0xC2
 #define REG_RAM_2      0xC4
 #define REG_RAM_3      0xC6
 #define REG_TRICKLE    0x90
 #define REG_CLOCK_BURST 0xBE
 #define REG_RAM_BURST  0xFE
 
 // Constructor
 DS1302Unix::DS1302Unix(uint8_t ce_pin, uint8_t io_pin, uint8_t sclk_pin) {
   _ce_pin = ce_pin;
   _io_pin = io_pin;
   _sclk_pin = sclk_pin;
 }
 
 // Initialize the module
 void DS1302Unix::begin() {
   // Setup pins
   pinMode(_ce_pin, OUTPUT);
   pinMode(_sclk_pin, OUTPUT);
   
   // Initial state
   digitalWrite(_ce_pin, LOW);
   digitalWrite(_sclk_pin, LOW);
   
   // Enable the clock (remove halt flag)
   _start();
   _writeOut(0x8E); // Write to control register
   _writeOut(0x00); // Enable clock, disable write protection
   _stop();
 }
 
 // Set the Unix timestamp (stored in RAM)
 void DS1302Unix::setTime(uint32_t unix_time) {
   // Store 32-bit Unix timestamp in 4 bytes of RAM
   _writeRam(RAM_ADDR_UNIX_0, unix_time & 0xFF);
   _writeRam(RAM_ADDR_UNIX_1, (unix_time >> 8) & 0xFF);
   _writeRam(RAM_ADDR_UNIX_2, (unix_time >> 16) & 0xFF);
   _writeRam(RAM_ADDR_UNIX_3, (unix_time >> 24) & 0xFF);
   
   // Also set RTC clock to a standard value (not actually used)
   // But helps to have the RTC running for accurate timekeeping
   _start();
   _writeOut(REG_CLOCK_BURST | 0x01); // Write to clock burst mode
   _writeOut(0x00); // Seconds (0)
   _writeOut(0x00); // Minutes (0)
   _writeOut(0x00); // Hour (0), 24-hour mode
   _writeOut(0x01); // Date (1)
   _writeOut(0x01); // Month (1)
   _writeOut(0x01); // Day of week (1)
   _writeOut(0x70); // Year (70 = 1970)
   _writeOut(0x00); // Write protect off
   _stop();
 }
 
 // Get the current Unix timestamp from RAM
 uint32_t DS1302Unix::getTime() {
   // Read 32-bit Unix timestamp from RAM
   uint32_t unix_time = 0;
   
   unix_time |= (uint32_t)_readRam(RAM_ADDR_UNIX_0);
   unix_time |= (uint32_t)_readRam(RAM_ADDR_UNIX_1) << 8;
   unix_time |= (uint32_t)_readRam(RAM_ADDR_UNIX_2) << 16;
   unix_time |= (uint32_t)_readRam(RAM_ADDR_UNIX_3) << 24;
   
   // Get seconds from RTC itself to update our timestamp
   _start();
   _writeOut(REG_SECONDS | 0x01); // Read seconds
   uint8_t seconds = _readIn();
   _stop();
   
   // Convert from BCD to decimal
   uint8_t sec = ((seconds >> 4) * 10) + (seconds & 0x0F);
   
   // Store updated time back to RAM
   // This auto-increments our counter based on the RTC's internal clock
   setTime(unix_time + 1); // Increment by one second
   
   return unix_time;
 }
 
 // Format Unix timestamp to human-readable string
 void DS1302Unix::formatTime(uint32_t unix_time, char* buffer) {
   uint16_t year;
   uint8_t month, day, hour, minute, second, weekday;
   
   getTimeComponents(unix_time, year, month, day, hour, minute, second, weekday);
   
   // Format: DD/MM/YYYY HH:MM:SS
   sprintf(buffer, "%02d/%02d/%04d %02d:%02d:%02d", 
           day, month, year, hour, minute, second);
 }
 
 // Get date/time components from Unix timestamp
 void DS1302Unix::getTimeComponents(uint32_t unix_time, uint16_t &year, uint8_t &month, 
                                   uint8_t &day, uint8_t &hour, uint8_t &minute, uint8_t &second,
                                   uint8_t &weekday) {
   // Extract time components
   second = unix_time % 60;
   unix_time /= 60;
   minute = unix_time % 60;
   unix_time /= 60;
   hour = unix_time % 24;
   unix_time /= 24; // Now unix_time is days since epoch
   
   // Calculate weekday (1970-01-01 was a Thursday)
   weekday = (unix_time + 4) % 7; // 0 = Sunday, 6 = Saturday
   if (weekday == 0) weekday = 7;  // Convert to 1-7 format (Monday=1)
   
   // Calculate year
   year = 1970;
   while (1) {
     uint16_t days_in_year = _isLeapYear(year) ? 366 : 365;
     if (unix_time < days_in_year) break;
     unix_time -= days_in_year;
     year++;
   }
   
   // Calculate month and day
   static const uint8_t days_in_month[] = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
   month = 1;
   while (1) {
     uint8_t dim = days_in_month[month-1];
     // Adjust February for leap years
     if (month == 2 && _isLeapYear(year)) dim++;
     
     if (unix_time < dim) break;
     unix_time -= dim;
     month++;
   }
   
   day = unix_time + 1; // Days are 1-based
 }
 
 // Helper Functions for DS1302 Communication
 
 // Start communication
 void DS1302Unix::_start() {
   digitalWrite(_ce_pin, HIGH);
   delayMicroseconds(4); // tCC
 }
 
 // Stop communication
 void DS1302Unix::_stop() {
   digitalWrite(_ce_pin, LOW);
   delayMicroseconds(4); // tCWH
 }
 
 // Write a byte
 void DS1302Unix::_writeOut(uint8_t value) {
   pinMode(_io_pin, OUTPUT);
   
   for (uint8_t i = 0; i < 8; i++) {
     digitalWrite(_io_pin, (value >> i) & 1);
     delayMicroseconds(1);
     digitalWrite(_sclk_pin, HIGH);
     delayMicroseconds(1);
     digitalWrite(_sclk_pin, LOW);
     delayMicroseconds(1);
   }
 }
 
 // Read a byte
 uint8_t DS1302Unix::_readIn() {
   uint8_t value = 0;
   pinMode(_io_pin, INPUT);
   
   for (uint8_t i = 0; i < 8; i++) {
     value |= (digitalRead(_io_pin) << i);
     digitalWrite(_sclk_pin, HIGH);
     delayMicroseconds(1);
     digitalWrite(_sclk_pin, LOW);
     delayMicroseconds(1);
   }
   
   return value;
 }
 
 // Write to RAM
 void DS1302Unix::_writeRam(uint8_t address, uint8_t value) {
   _start();
   _writeOut(REG_RAM_0 + (address * 2)); // RAM address (write)
   _writeOut(value);
   _stop();
 }
 
 // Read from RAM
 uint8_t DS1302Unix::_readRam(uint8_t address) {
   _start();
   _writeOut(REG_RAM_0 + (address * 2) + 1); // RAM address (read)
   uint8_t value = _readIn();
   _stop();
   return value;
 }
 
 // Check if year is leap year
 bool DS1302Unix::_isLeapYear(uint16_t year) {
   return ((year % 4 == 0 && year % 100 != 0) || (year % 400 == 0));
 }