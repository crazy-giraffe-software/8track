## Description
The I2C port was sort of an add-on for possible 1 of two things:

The first is communication to another micro, in case I want to include this in a larger design. In that case, the SDA and SCL are used. The INT can be used as a general pupose IO for communication to another micro, specifically I was thinking a panic interrupt.

The second is for use with a display, which dictates the pin layout. There are lots of great little I2C OLED display modules that can plug right into pins 1-4. Want to print out the speed or other info?