using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            /*
            var sp = new MonoSerialPort("COM18", 9600, Parity.None, 8, StopBits.One );
            sp.DataReceived += Sp_DataReceived;
            sp.Open();*/
            
            //var a = System.IO.Ports.SerialPort.GetPortNames();
            arduino = new Arduino(BoardName.UNO, new MonoSerialPort("COM18", 115200, Parity.None, 8, StopBits.One)); 
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
        /*
        private void Sp_DataReceived(object sender, MonoSerialDataReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.EventType == System.IO.Ports.SerialData.Chars)
                {
                    byte[] buffer = new byte[1];
                    (sender as MonoSerialPort).Read(buffer, 0, 1);
                    textBlock.Text += Encoding.UTF8.GetString(buffer);
                    Thread.Sleep(1);
                }
            });
        }
        */
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            arduino.Close();
        }
    }
}
