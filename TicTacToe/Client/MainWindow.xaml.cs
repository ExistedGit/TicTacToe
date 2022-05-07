using MessageLibrary;
using System.Net;
using System.Windows;
using GameLibrary;
using System.Collections.ObjectModel;

namespace Client
{
    public partial class MainWindow : Window
    {
        public TcpClientWrap Client { get; set; } 
        public ObservableCollection<Cell> Field { get; set; } = new ObservableCollection<Cell>();

        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Field.Add(new Cell(i+1, j+1));
                }
            }


            DataContext = this;
        }

        private void BTN_ConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            string[] Address = TB_ServerAddres.Text.Split(':');

            if (Address.Length != 2)
            {
                MessageBox.Show("Invalid structure", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IPAddress ipAddress;
            int port;

            if (!IPAddress.TryParse(Address[0], out ipAddress)) {
                MessageBox.Show("Invalid ip address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return ;
            }

            

            if(int.TryParse(Address[1], out port) && port > 0 && port <= ushort.MaxValue)
            {
                Client = new TcpClientWrap(ipAddress, port);
                Client.Connected += OnClientConnected;
                Client.Disconnected += OnClientDisconnected;

                Client.ConnectAsync();
            }
            else
                MessageBox.Show("Invalid port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void OnClientConnected(TcpClientWrap client)
        {
            TB_ServerAddres.IsEnabled = false;
            BTN_ConnectToServer.IsEnabled = false;


        }


        private void OnClientDisconnected(TcpClientWrap client)
        {
            Client = null;
            TB_ServerAddres.IsEnabled = true;
            BTN_ConnectToServer.IsEnabled = true;
        }

    }
}
