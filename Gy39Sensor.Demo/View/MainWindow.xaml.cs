using Gy39Sensor.Core;
using Gy39Sensor.Demo.ViewModel;
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

namespace Gy39Sensor.Demo.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Gy39 _gy39;
        private SensorData _sensorData;

        public MainWindow()
        {
            InitializeComponent();
            _sensorData = new SensorData()
            {
                Humidity = "N/A", Atmosphere = "N/A", Brightness = "N/A", Temperature = "N/A"
            };

            DataContext = _sensorData;

            // Add all available ports
            foreach(var port in SerialPort.GetPortNames())
            {
                PortComboBox.Items.Add(port);
            }
        }

        private void OpenPortButton_Click(object sender, RoutedEventArgs e)
        {
            _gy39 = new Gy39(PortComboBox.SelectedValue.ToString());
            _gy39.Start();

            _gy39.SensorDataReceived += OnSensorDataReceived;
        }

        private void OnSensorDataReceived(object sender, Core.Model.SensorDataReceivedEventArgs e)
        {
            _sensorData.Brightness = e.Brightness.Lux.ToString();
            _sensorData.Temperature = e.Weather.Temperature.ToString();
            _sensorData.Humidity = e.Weather.Temperature.ToString();
            _sensorData.Atmosphere = e.Weather.Atmosphere.ToString();
        }

        private void ClosePortButton_Click(object sender, RoutedEventArgs e)
        {
            _gy39.Stop();
        }
    }
}
