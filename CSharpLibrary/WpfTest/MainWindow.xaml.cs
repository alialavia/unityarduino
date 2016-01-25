using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            var a = SerialPort.GetPortNames();
            arduino = new Arduino(BoardName.UNO, new MonoSerialPort(new System.IO.Ports.SerialPort("COM2", 115200, Parity.Even, 8, StopBits.Two))); 
            try
            {
                arduino.pinMode(13, PinMode.OUTPUT);
                arduino.pinMode(12, PinMode.OUTPUT);

                arduino.ArduinoStateReceived += Arduino_ArduinoStateReceived;            
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Arduino_ArduinoStateReceived(object sender, ArduinoUpdateEventArgs e)
        {
            var sc = sender as SerialCommunicator;
            Dispatcher.Invoke(() =>
            {
                if (e.State.IsValid)
                    textBlock.Text = e.State.ToString();
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (arduino.digitalRead(12) == DigitalValue.Low)
            {
                arduino.digitalWrite(12, DigitalValue.High);
                arduino.digitalWrite(13, DigitalValue.High);
            }
            else
            {
                arduino.digitalWrite(12, DigitalValue.Low);
                arduino.digitalWrite(13, DigitalValue.Low);
            }

        }
    }
}
