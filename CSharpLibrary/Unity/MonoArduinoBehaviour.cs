using System.IO;
using System.Reflection;
using ArduinoCommunicator;
using UnityEngine;

namespace Assets.ArduinoCommunicator
{
    public class MonoArduinoBehaviour : MonoBehaviour
    {
        #region Public Fields

        public static Arduino arduino;
        private MethodInfo updateMethod;
        private MethodInfo startMethod;

        #endregion Public Fields

        #region Public Methods


        #endregion Public Methods

        #region Private Methods

        // Use this for initialization
        void Start()
        {
            try
            {
                arduino = new Arduino(BoardName.UNO, false);
            }
            catch (IOException ex)
            {
                Debug.LogError(ex.Message);
            }
            startMethod.Invoke(this, null);
            arduino.sendCommands();
        }

        // Update is called once per frame
        void Update()
        {
            arduino.getStates();
            updateMethod.Invoke(this, null);
            arduino.sendCommands();
        }

        #endregion Private Methods

        public MonoArduinoBehaviour()
        {
            updateMethod = this.GetType().GetMethod("UpdateA", BindingFlags.NonPublic | BindingFlags.Instance);
            startMethod = this.GetType().GetMethod("StartA", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}