#include <TimerOne.h>

const int DIGITAL_PINS = 14;

const int ANALOG_PINS = 6;

const byte BOF = 0xFF;

const byte SIZE = 0;
const int OUT_MESSAGE_LEN = 18;

const byte DIGITAL_WRITE = 0x01;
const byte PIN_MODE = 0x02;
const byte ANALOG_WRITE = 0x03;
const byte ANALOG_READ = 0x04;
const byte DIGITAL_READ = 0x05;

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

int counter = 0;
void sendStates(void)
{
    counter++;
    const int len = 2 + 2 * ANALOG_PINS;
    byte buffer[len];
    int i = 0;
        
    for (i = 0; i < DIGITAL_PINS; i++)          
        bitWrite(buffer[i/8], i % 8, (i<2 ? 0 : digitalRead(i)));       
          
    for (i = 0; i < ANALOG_PINS; i++)
    {
        int val = analogRead(i);
        buffer[2+i*2] = highByte(val);
        buffer[2+i*2 + 1] = lowByte(val);
    }

    /* Beginning of the frame */
    Serial.write(BOF);
    Serial.write(buffer, len);
    Serial.write(Crc8(buffer, len));
 
}
void sendMsg(byte command, byte data0, byte data1)
{
    Serial.write(command);
    Serial.write(data0);
    Serial.write(data1);
    byte buffer[3] = {command, data0, data1};
    Serial.write(Crc8(buffer, 3));
}
void readCommands(void)
{    
    while (Serial.available() >= 4)
    {
       byte command = Serial.read();
       byte pin = Serial.read();
       byte value = Serial.read();
       byte crc = Serial.read();
       byte buffer[3] = {command, pin, value};
       int v = 0;
       if (Crc8(buffer, 3) == crc)
       {
         switch (command) 
         {
             case DIGITAL_WRITE:
               digitalWrite(pin, value);
               break;
             case PIN_MODE:
               pinMode(pin, value);
               break;
             case ANALOG_WRITE:
               analogWrite(pin, value);
               break;
             case ANALOG_READ:
                 v = analogRead(pin);            
                 sendMsg(ANALOG_READ, highByte(v), lowByte(v));
                 break;
             case DIGITAL_READ:                          
                 sendMsg(DIGITAL_READ, 0, digitalRead(pin));
                 break;
             case ACK:                          
               sendMsg(ACK, 0xFF, 0xFF);
               break;
         }
       }
       else
         sendMsg(command, pin, value);
    } 
}

void setup()
{

    // baud: 9600, Databits: 8, Parity: Even, Stopbits: 2  
    //Serial.begin(115200, SERIAL_8E2);
    Serial.begin(115200, SERIAL_8E2);
    /*while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
    }*/
    Timer1.initialize(10000); // usec
    Timer1.attachInterrupt(readCommands);
}

void loop()
{

}
