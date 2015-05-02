# NETMF-Utils
Library for .net micro framework devices. 

##Contains 

- I2C address scanner
- Class for control OLED display with SSD1306 driver. 
- Class for character LCD display with HD44780 driver. Controled by I2C I/O expander (PCF8574).
- More will be added... 

OLED display video URL: https://youtu.be/ZWVSWinAFTU

# Info

Tested only on OLED 128x64. If you have different display check Init method. You can modify Init method according to Adafruit begin method (https://github.com/adafruit/Adafruit_SSD1306/blob/master/Adafruit_SSD1306.cpp)

# Sources

Adafruit:
https://github.com/adafruit/Adafruit_SSD1306,
https://github.com/adafruit/Adafruit-GFX-Library

SSD1306 datasheet:
https://www.adafruit.com/datasheets/SSD1306.pdf

