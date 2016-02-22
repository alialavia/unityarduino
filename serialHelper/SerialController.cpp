#include <TimerOne.h>
#include "Arduino.h"
#include "SerialController.h"
class SerialController
{

private:
  const int DIGITAL_PINS = 14;
  const int ANALOG_PINS = 6;
  const byte BOF = 0xFF;
  const byte SIZE = 0;
  const int OUT_MESSAGE_LEN = 18;
  const byte PIN_MODE = 0xF0;
  const byte DIGITAL_WRITE = 0xF1;
  const byte ANALOG_WRITE = 0xF2;
  const byte DIGITAL_READ = 0xF3;
  const byte ANALOG_READ = 0xF4;
  const byte ACK = 0xFE;
  const byte NACK = 0xFD;
  // BOF SIZE DATA SIZE EOF ...
  byte Crc8(const byte *data, int len)
  {
      //const byte *data = vptr;
      unsigned int crc = 0;
      int i, j;
      for (j = len; j; j--, data++) {
          crc ^= (*data << 8);
          for(i = 8; i; i--) {
              if (crc & 0x8000)
                  crc ^= (0x1070 << 3);
              crc <<= 1;
          }
      }
      return (byte)(crc >> 8);
  }

  byte mCRC(const byte *data, int len)
  {
    return Crc8(data, len) & 0x7F;
  }


  byte mlow7(int i)
  {
    return (byte) (i % 128);
  }

  byte mhigh7(int i)
  {
    return i / 128;
  }
  void sendMsg(byte command, byte _msb, byte _lsb)
  {
      /* make sure that _lsb and _msb don't exceed 0x80 
         because upper hald are reserved for control commands */
      /*
      Serial.write(command);
      Serial.write(_msb);
      Serial.write(_lsb);*/
      byte buffer[4] = {command, _msb, _lsb, 0};
      buffer[3] = mCRC(buffer, 3);
      Serial.write(buffer, 4);
  }
  int counter = 0;
  byte pin = 0, value = 0;
  void readCommands(void)
  {    
      while (Serial.available() >= 4)
      {
         byte command = Serial.read();
         // command works also as a BOF signal.
         if (command < 0xE0)
           continue;
         byte _msb = Serial.read();
         if (_msb >= 0x80)
           continue;
         byte _lsb = Serial.read();
         if (_lsb >= 0x80)
           continue;         
         byte crc = Serial.read();       
         if (crc >= 0x80)
           continue;
         byte buffer[3] = {command, _msb, _lsb};
         int v = 0;
         if (mCRC(buffer, 3) == crc)
         {
           pin = _msb >> 3; // TODO: Bogus
           value = ((_msb & 0x01) << 7) + _lsb;
           switch (command) 
           {
               case DIGITAL_WRITE:
                 digitalWrite(pin, value);
                 sendMsg(DIGITAL_WRITE, pin, value);
                 break;
               case PIN_MODE:
                 pinMode(pin, value);
                 sendMsg(PIN_MODE, pin, value);
                 break;
               case ANALOG_WRITE:
                 analogWrite(pin, value);
                 sendMsg(ANALOG_WRITE, pin, value);
                 break;
               case ANALOG_READ:
                   v = analogRead(pin);            
                   sendMsg(ANALOG_READ, mhigh7(v), mlow7(v));
                   break;
               case DIGITAL_READ:                          
                   sendMsg(DIGITAL_READ, pin, digitalRead(pin));
                   break;
               case ACK:                          
                 sendMsg(ACK, 0xFF, 0xFF);
                 break;
           }
         }
         else
           sendMsg(NACK, 0xFF, 0xFF);
      } 
  }

public:
  static void init()
  {
    Serial.begin(115200, SERIAL_8E2);
    Timer1.initialize(5000); // usec
    Timer1.attachInterrupt(readCommands);
  }
};