using System;
using System.Collections.Generic;
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
        SerialCommunicator sc;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                sc = new SerialCommunicator("COM22", 115200, System.IO.Ports.Parity.Even, 8, System.IO.Ports.StopBits.Two);
                sc.ArduinoStateReceived += Sc_ArduinoStateReceived;
                sc.Start();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Sc_ArduinoStateReceived(object sender, ArduinoUpdateEventArgs e)
        {
            var sc = sender as SerialCommunicator;
            Dispatcher.Invoke(() =>
            {
                if (e.State.IsValid)
                    textBlock.Text = e.State.ToString();
            });

        }
    }
}
