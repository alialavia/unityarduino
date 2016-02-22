/*
  Arduino remote controller 
*/

#ifndef UNITYCONNECTOR_H
#define UNITYCONNECTOR_H

#include "Arduino.h"

class UnityConnector
{

  public:
    void connect();
    static void readCommands(void);

  private:
    
    static const int DIGITAL_PINS = 14;
    static const int ANALOG_PINS = 6;
    static const byte BOF = 0xFF;
    static const byte SIZE = 0;

    static const byte PIN_MODE = 0xF0;
    static const byte DIGITAL_WRITE = 0xF1;
    static const byte ANALOG_WRITE = 0xF2;
    static const byte DIGITAL_READ = 0xF3;
    static const byte ANALOG_READ = 0xF4;
    
    static const byte ACK = 0xFE;
    static const byte NACK = 0xFD;
    
    static byte Crc8(const byte *data, int len);
    static byte mCRC(const byte *data, int len);
    static byte mlow7(int i);
    static byte mhigh7(int i);
    static void sendMsg(byte command, byte _msb, byte _lsb);

};

extern UnityConnector Unity;

#endif