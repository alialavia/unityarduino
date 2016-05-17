using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using ArduinoCommunicator;
using SerialPortNET;
using UnitySerialPort;

namespace WpfTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
           /* foreach (var k in AndroidSerialPort.EnumerateSerialPorts().Keys)
                MessageBox.Show(k + ":" + AndroidSerialPort.EnumerateSerialPorts()[k]);
            */
            Timer t = new Timer(50);
            t.Elapsed += T_Elapsed;
            Timer t2 = new Timer(200);
            t2.Elapsed += T2_Elapsed;
            arduino1 = new Arduino<UnitySerialPort.AndroidSerialPort>(BoardType.UNO);
            //arduino2 = new Arduino(BoardType.UNO);

            try
            {
                //arduino2.pinMode(13, PinMode.OUTPUT);
                /*arduino.pinMode(12, PinMode.OUTPUT);
                arduino.pinMode(7, PinMode.INPUT_PULLUP);*/
                //arduino.pinMode(12, PinMode.OUTPUT);
                var sp = new SerialPort("COM7", 9600, SerialPortNET.Parity.None, 8, SerialPortNET.StopBits.One);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //t.Start();
            //t2.Start();
        }

        private void T2_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                textBlock.Text = v.ToString();// (100.0 * (float)error / (float)success).ToString();
            }));
        }

        int success = 1;
        int error = 0;
        int v = 0;
        object l = new object();
        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (l) {
                //Dispatcher.Invoke(new Action(() => {
                //  textBlock.Text = 
                //try
                //{
                    v = arduino1.analogRead(0);
                    arduino1.analogWrite(5, (byte)(v / 4));
                    //arduino2.digitalWrite(13, !arduino2.digitalRead(13));
                    success++;
                //}
                /*catch (IOException)
                {
                    error++;
                    var sp = arduino1.SerialPort;
                }*/
            }
            //}));
        }

        #endregion Public Constructors

        #region Private Methods

        private void button_Click(object sender, RoutedEventArgs e)
        {
            /*
            var v = arduino.analogRead(0);
            arduino.digitalWrite(13, !arduino.digitalRead(13));
            */
            arduino1.digitalWrite(13, !arduino1.digitalRead(13));


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            arduino1.Close();
        }

        #endregion Private Methods

        #region Private Fields

        private Arduino<UnitySerialPort.AndroidSerialPort> arduino1;
        //, arduino2;

        #endregion Private Fields
    }
}