using UnityEngine;
using System.Collections;
using ArduinoCommunicator;
using System;
using System.IO;
using Assets.ArduinoCommunicator;
using System.Reflection;

public class NewBehaviourScript : MonoArduinoBehaviour
{
    void UpdateA()
    {
        transform.Translate(0.001f * (arduino.analogRead(0) - 512), 0, 0);
        arduino.digitalWrite(13, !arduino.digitalRead(13));
    }
    void StartA()
    {
        arduino.pinMode(13, PinMode.OUTPUT);
    }
}
