/*
  Arduino remote controller 
*/

#include "Arduino.h"
#include "UnityConnector.h"

byte UnityConnector::Crc8(const byte *data, int len)
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

byte UnityConnector::mCRC(const byte *data, int len)
{
  return Crc8(data, len) & 0x7F;
}


byte UnityConnector::mlow7(int i)
{
  return (byte) (i % 128);
}

byte UnityConnector::mhigh7(int i)
{
  return i / 128;
}
void UnityConnector::sendMsg(byte command, byte _msb, byte _lsb)
{
    /* make sure that _lsb and _msb don't exceed 0x80 
       because upper hald are reserved for control commands */
    
    byte buffer[4] = {command, _msb, _lsb, 0};
    buffer[3] = mCRC(buffer, 3);
    Serial.write(buffer, 4);
}

void UnityConnector::readCommands(void)
{    
    byte pin = 0, value = 0;    
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

void UnityConnector::connect()
{
  // Set timer
  OCR0A = 0xAF;
  TIMSK0 |= _BV(OCIE0A);

  Serial.begin(115200, SERIAL_8E2);
  
  //Timer1.initialize(5000); // usec
  //Timer1.attachInterrupt();
}

// called every milliseconds
SIGNAL(TIMER0_COMPA_vect) 
{
  UnityConnector::readCommands();
}

UnityConnector Unity;
