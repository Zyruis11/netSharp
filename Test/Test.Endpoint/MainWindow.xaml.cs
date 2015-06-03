using System.Net;
using System.Runtime.Remoting.Channels;
using System.Windows;
using netSharp.Server.Connectivity;
using netSharp.Server.Events;

namespace Test.Endpoint
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NsEndpoint _nsEndpoint;
        private bool EndPointActive;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void EndpointControlButton_Click(object sender, RoutedEventArgs e)
        {
            if (!EndPointActive)
            {
                switch (EndpointTypeSelector.Text)
                {
                    case "Server":
                    {
                        var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
                        _nsEndpoint = new NsEndpoint(true, ipEndPoint, 100);
                        break;
                    }
                    case "Client":
                    {
                        _nsEndpoint = new NsEndpoint(false);
                        break;
                    }
                    default :
                    {
                        return;
                    }
                }
                if (_nsEndpoint != null)
                {
                    _nsEndpoint.SessionCreated += SessionAddedHandler;
                    _nsEndpoint.SessionRemoved += SessionRemovedHandler;
                    _nsEndpoint.SessionDataRecieved += SessionDataRecievedHandler;
                    EndPointActive = true;
                    EndpointControlButton.Content = "Stop";
                }
            }
            else
            {
                _nsEndpoint.Dispose();
                EndPointActive = false;
                EndpointControlButton.Content = "Start";
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
            byte[] test = new byte[0];
            _nsEndpoint.SendDataAsync(test);
        }

    }
}