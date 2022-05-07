using System;
using System.Windows;
using MessageLibrary;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public partial class MainWindow : Window
    {
        public TcpClientWrap Client { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void BTN_ConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            string[] Address = TB_ServerAddres.Text.Split(':');

            if (Address.Length != 2)
            {
                MessageBox.Show("Invalid input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            IPAddress ipAddress;

            if (!IPAddress.TryParse(Address[0], out ipAddress))
            {
                MessageBox.Show("Invalid ip address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return ;
            }

            int port;

            if(int.TryParse(Address[1], out port) && port > 0 && port <= ushort.MaxValue)
            {
                Client = new TcpClientWrap(ipAddress, port);
                Client.Connected += OnClientConnected;
                Client.ConnectAsync();
            }
            else
            {
                MessageBox.Show("Invalid port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnClientConnected(TcpClientWrap client)
        {
            TB_ServerAddres.IsEnabled = false;
            BTN_ConnectToServer.IsEnabled = false;

            TextMessage message = new TextMessage("Путін хуйло!");
            client.SendAsync(message);
        }

    }
}
