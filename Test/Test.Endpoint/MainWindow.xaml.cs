using System;
using System.Net;
using System.Windows;
using netSharp.Windows.Connectivity;
using netSharp.Windows.Events;

namespace Test.Endpoint
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private netSharpEndpoint _localEndpoint;
        private bool _endPointActive;
        private bool _isServer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void EndpointControlButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_endPointActive)
            {
                switch (EndpointTypeSelector.Text)
                {
                    case "Server":
                    {
                        var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
                        _localEndpoint = new netSharpEndpoint(true, ipEndPoint, 100);
                        Title = "Server";
                        _isServer = true;
                        break;
                    }
                    case "Client":
                    {
                        _localEndpoint = new netSharpEndpoint(false);
                        Title = "Client";
                        _isServer = false;
                        break;
                    }
                    default:
                    {
                        return;
                    }
                }
                if (_localEndpoint != null)
                {
                    _localEndpoint.SessionCreated += SessionAddedHandler;
                    _localEndpoint.SessionRemoved += SessionRemovedHandler;
                    _localEndpoint.SessionDataRecieved += SessionDataRecievedHandler;
                    _endPointActive = true;
                    EndpointControlButton.Content = "Stop";
                }
            }
            else
            {
                _localEndpoint.Dispose();
                _endPointActive = false;
                EndpointControlButton.Content = "Start";
                Title = "Not Running";
            }
        }

        private void SessionDataRecievedHandler(object sender, ServerEvents e)
        {
            LogTextBox.Text += "Data Recieved!\n";
        }

        private void SessionAddedHandler(object sender, ServerEvents e)
        {
            LogTextBox.Text += "New Session Connected\n";
        }

        private void SessionRemovedHandler(object sender, ServerEvents e)
        {
            LogTextBox.Text += "Session Removed\n";
        }

        private void SendData_Click(object sender, RoutedEventArgs e)
        {
            var sendSize = Convert.ToInt32(dataSizeTb.Text);
            var sendRepeat = Convert.ToInt32(dataRepeatTb.Text);

            while (sendRepeat > 0)
            {
                var test = new byte[sendSize];
                _localEndpoint.SendDataAsync(test);
                sendRepeat--;
            }
        }

        private void connectServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isServer)
            {
                IPAddress remoteEndpointIp = IPAddress.Parse(remoteEndPoint.Text);
                IPEndPoint remoteIpEndpoint = new IPEndPoint(remoteEndpointIp, 3000);
                _localEndpoint.NewSession(remoteIpEndpoint);
            }
        }
    }
}