using Gy39Sensor.Core;
using Gy39Sensor.Demo.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Threading;

namespace Gy39Sensor.Demo.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Gy39 _gy39;
        private readonly SensorData _sensorData;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            _sensorData = new SensorData()
            {
                Humidity = "N/A", Atmosphere = "N/A", Brightness = "N/A", Temperature = "N/A"
            };

            DataContext = _sensorData;
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;

            // Add all available ports
            foreach(var port in SerialPort.GetPortNames())
            {
                PortComboBox.Items.Add(port);
            }
        }

        private void OpenPortButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _gy39 = new Gy39(PortComboBox.SelectedValue.ToString());         
            } 
            catch(Exception exception)
            {
                MessageBox.Show("Failed to open the port, reason: " + exception.Message);
            }

            OpenPortButton.IsEnabled = false;
            ClosePortButton.IsEnabled = true;
            _timer.Start();
            MessageBox.Show("Port opened.");
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            var weather = _gy39.QueryWeather();

            DataContext = new SensorData()
            {
                Brightness = _gy39.QueryBrightness().Lux.ToString(CultureInfo.CurrentCulture),
                Temperature = weather.Temperature.ToString(CultureInfo.CurrentCulture),
                Humidity = weather.Humidity.ToString(CultureInfo.CurrentUICulture),
                Atmosphere = weather.Atmosphere.ToString(CultureInfo.CurrentUICulture)
            };
        }

        private void ClosePortButton_Click(object sender, RoutedEventArgs e)
        {
            _gy39.Dispose();
            _timer.Stop();

            OpenPortButton.IsEnabled = true;
            ClosePortButton.IsEnabled = false;
        }
    }
}
