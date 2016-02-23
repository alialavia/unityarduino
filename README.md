# Unity<->Arduino
A unity library for arduino connectivity


# Quick start
## Arduino setup
1. Download [UnitryConnector.zip](https://cdn.rawgit.com/alialavia/unityarduino/master/ArduinoLib/UnityConnector.zip) and import it to your Arduino IDE (Sktech->Import Library...->Add Library...).
2. From Sktech->Import Library... select Unity Connector.
3. Write `Unity.connect();` in `setup()` function.
    So your sketch should look like this:
    ```C
    #include <UnityConnector.h>
    void setup()
    {
      Unity.connect();
    }
    
    void loop()
    {
    
    }
    ```
4. Upload the sketch into Arduino.

## Unity setup
1. Download [ArduinoCommunicatorSPNet.dll](https://cdn.rawgit.com/alialavia/unityarduino/master/ArduinoCommunicatorSPNet.dll) and copy it to your unity Assets folder.
2. Create a C# Script and attach it to a game object. 
3. Now you need to declare a class variable with `Arduino` type and initialize it in your `Start()` method. You can then call Arduino functions on that object. Here is an example:

```CSharp
using UnityEngine;
using ArduinoCommunicator;

public class NewBehaviourScript : MonoBehaviour {

    // Use this for initialization
    Arduino arduino;
    void Start () {
        arduino = new Arduino();
        arduino.pinMode(13, PinMode.OUTPUT);
	}
	
	// Update is called once per frame
	void Update () {
        arduino.digitalWrite(13, !arduino.digitalRead(13));
	}
}

```

That's it! Play around with it, and read the full documentation [here](https://cdn.rawgit.com/alialavia/unityarduino/master/Help/index.html).
