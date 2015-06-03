using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Test.WP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly StreamSocket clientSocket;
        //private bool closing = false;
        private bool connected;
        private HostName serverHost;
        //private string serverHostnameString;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            clientSocket = new StreamSocket();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (connected)
            {
                statusBox.Text += "Already Connected";
                return;
            }

            serverHost = new HostName(serverHostname.Text);

            await clientSocket.ConnectAsync(serverHost, serverPort.Text);
            connected = true;
            statusBox.Text += "Connected!\n";
            ReadAsync();
        }

        private async void ReadAsync()
        {
            while (connected)
            {
                var buffer = new byte[50].AsBuffer();

                await clientSocket.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);
                var result = buffer.ToArray();
                statusBox.Text += string.Format("Read {0} bytes from server\n", result.Length);
            }
        }
    }
}
