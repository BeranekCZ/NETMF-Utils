# NETMF-Utils
Library for .net micro framework devices. 

##Contains 

- I2C address scanner
- Class for control OLED display with SSD1306 driver. 
- Class for character LCD display with HD44780 driver. Controled by I2C I/O expander (PCF8574).
- Class for HC-05 bluetooth module (basic AT commands, R/W data, recv. data event, packet mode)
- Class for ESP8266 wifi module 
  - beta, problem with receiving data larger than 1024B
  - only TCP tested
- Class for digital pressure sensors BMP180
- Class for GPS module NEO-6M
- More will be added... 

OLED display video URL: https://youtu.be/ZWVSWinAFTU

# Info

### OLED
Tested only on OLED 128x64. If you have different display check Init method. You can modify Init method according to Adafruit begin method (https://github.com/adafruit/Adafruit_SSD1306/blob/master/Adafruit_SSD1306.cpp)

### Character LCD display
Class can be configured with  binary masks if your I/O expander has pins connected in different order.

### ESP8266 wifi module
Works with espressif firmware version  00200.9.4. Another firmwares not tested. Problem with receiving data larger than 1024B. Only TCP tested.  TODO: Maybe better timeouts for AT commands. 



# Sources

Adafruit:
https://github.com/adafruit/Adafruit_SSD1306,
https://github.com/adafruit/Adafruit-GFX-Library

SSD1306 datasheet:
https://www.adafruit.com/datasheets/SSD1306.pdf

ESP8266 AT commands: 
https://github.com/espressif/esp8266_at/wiki/AT_Description

BMP180 datasheet: 
http://www.adafruit.com/datasheets/BST-BMP180-DS000-09.pdf

