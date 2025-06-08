/*
 * DS1302Unix.h - Custom Unix timestamp library for DS1302 RTC
 * Stores Unix timestamp directly in RAM and increments as seconds counter
 */

 #ifndef DS1302Unix_h
 #define DS1302Unix_h
 
 #include <Arduino.h>
 
 class DS1302Unix {
   public:
     // Constructor
     DS1302Unix(uint8_t ce_pin, uint8_t io_pin, uint8_t sclk_pin);
     
     // Initialize the module
     void begin();
     
     // Set the Unix timestamp
     void setTime(uint32_t unix_time);
     
     // Get the current Unix timestamp
     uint32_t getTime();
     
     // Convert Unix time to human readable format (store in buffer)
     void formatTime(uint32_t unix_time, char* buffer);
     
     // Get components from Unix time
     void getTimeComponents(uint32_t unix_time, uint16_t &year, uint8_t &month, 
                           uint8_t &day, uint8_t &hour, uint8_t &minute, uint8_t &second,
                           uint8_t &weekday);
   
   private:
     uint8_t _ce_pin;
     uint8_t _io_pin;
     uint8_t _sclk_pin;
     
     // Helper functions to communicate with the DS1302
     void _writeOut(uint8_t value);
     uint8_t _readIn();
     void _start();
     void _stop();
     
     // Read/write RAM functions
     void _writeRam(uint8_t address, uint8_t value);
     uint8_t _readRam(uint8_t address);
     
     // Helper function to determine if a year is a leap year
     bool _isLeapYear(uint16_t year);
     
     // RAM addresses for timestamp (using first 4 bytes of RAM)
     static const uint8_t RAM_ADDR_UNIX_0 = 0; // Least significant byte
     static const uint8_t RAM_ADDR_UNIX_1 = 1;
     static const uint8_t RAM_ADDR_UNIX_2 = 2;
     static const uint8_t RAM_ADDR_UNIX_3 = 3; // Most significant byte
 };
 
 #endif