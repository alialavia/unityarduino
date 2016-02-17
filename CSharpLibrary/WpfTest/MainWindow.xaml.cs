using System;
using System.Windows;
using ArduinoCommunicator;

namespace WpfTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //SerialCommunicator sc;
        Arduino arduino;
        public MainWindow()
        {
            InitializeComponent();

            arduino = new Arduino(BoardName.UNO, new SerialPortNET("COM18", 115200, Parity.None, 8, StopBits.One));
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
        }

        private void Arduino_ArduinoStateReceived(object sender, ArduinoUpdatesEventArgs e)
        {
            var sc = sender as SerialCommunicator;
            Dispatcher.Invoke(() =>
            {
                if (e.State.IsConnected)
                    textBlock.Text = e.State.ToString();
            });
        }

        bool l = false;
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (l)
            {
                arduino.digitalWrite(13, DigitalValue.High);
            }
            else
            {
                arduino.digitalWrite(13, DigitalValue.Low);
            }
            l = !l;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            arduino.Close();
        }
    }
}
