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

            arduino = new Arduino(BoardName.UNO, false); 
            try
            {
                arduino.pinMode(13, PinMode.OUTPUT);
                arduino.pinMode(12, PinMode.OUTPUT);

                arduino.pinMode(7, PinMode.INPUT_PULLUP);
                //arduino.pinMode(12, PinMode.OUTPUT);
                arduino.sendCommands();
                arduino.getStates();
                arduino.ArduinoStateReceived += Arduino_ArduinoStateReceived;            
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
                if (e.State.IsValid)
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
            arduino.sendCommands();
            arduino.getStates();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            arduino.Close();
        }
    }
}
