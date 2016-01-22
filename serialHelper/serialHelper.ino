#include <TimerOne.h>

const int DIGITAL_PINS = 14;

const int ANALOG_PINS = 6;

const byte BOF = 0xFF;
const byte ACK = 0x01;
const byte SIZE = 0;

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


void sendState(void)
{    
    if (Serial.available() > 0 && Serial.read() == ACK)
    {
      const int len = 2 + 2 * ANALOG_PINS;
      byte buffer[len];
      int i = 0;
      for (i = 2; i < DIGITAL_PINS; i++)
          pinMode(i, INPUT_PULLUP);
          
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
}

void setup()
{
    // baud: 9600, Databits: 8, Parity: Even, Stopbits: 2  
    Serial.begin(115200, SERIAL_8E2);
    Timer1.initialize(50000); // usec
    Timer1.attachInterrupt(sendState);
}

void loop()
{

}
