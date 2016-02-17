using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using ArduinoCommunicator;

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

            Timer t = new Timer(10);
            t.Elapsed += T_Elapsed;
            Timer t2 = new Timer(200);
            t2.Elapsed += T2_Elapsed;
            arduino = new Arduino(BoardName.UNO);
            try
            {
                arduino.pinMode(13, PinMode.OUTPUT);
                arduino.pinMode(12, PinMode.OUTPUT);
                arduino.pinMode(7, PinMode.INPUT_PULLUP);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            t.Start();
            t2.Start();
        }

        private void T2_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                textBlock.Text = (100.0 * (float)error / (float)success).ToString();
            }));
        }

        int success = 1;
        int error = 0;
        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Dispatcher.Invoke(new Action(() => {
            //  textBlock.Text = 
            try
            {
                arduino.analogWrite(6, (byte)(arduino.analogReadEx(0)/4));
                
                success++;
            }
            catch (Exception ex)
            {
                error++;
                var sp = arduino.SerialPort;
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

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            arduino.Close();
        }

        #endregion Private Methods

        #region Private Fields

        //SerialCommunicator sc;
        private Arduino arduino;

        #endregion Private Fields
    }
}