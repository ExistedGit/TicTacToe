﻿using MessageLibrary;
using System.Net;
using System.Windows;
using GameLibrary;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Client
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string currentEnemy;
        private bool isMyTurn;
        private CellState myIcon;

        public TcpClientWrap Client { get; set; }
        public ObservableCollection<Cell> Field { get; set; } = new ObservableCollection<Cell>();
        public bool isConnected
        {
            get => Client.Tcp.Connected;
        }
        public bool isGamaRunning { get; set; }
        public bool IsMyTurn
        {
            get => isMyTurn;
            set
            {
                isMyTurn = value;
                OnPropertyChanged();
            }
        }
        public Command CellClick { get; set; }
        public string CurrentEnemy
        {
            get => currentEnemy;
            set
            {
                currentEnemy = value;
                OnPropertyChanged();
            }
        }
        public uint RoomId { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        public CellState MyIcon
        {
            get => myIcon;
            set
            {
                if(value != CellState.Empty)
                {
                    myIcon = value;
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            CellClick = new Command(CellClick_CanExecute, CellClick_Execute);
        
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Field.Add(new Cell(i+1, j+1));
                }
            }

            DataContext = this;
        }

       

        #region Events

        private void BTN_ConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            string[] Address = TB_ServerAddres.Text.Split(':');

            if (Address.Length != 2)
            {
                MessageBox.Show("Invalid structure", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(TB_UserName.Text))
            {
                MessageBox.Show("Invalid user name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Client.ConnectFailed += OnConnectedFailed;
                Client.MessageSent += OnMessageSent;
                Client.MessageReceived += OnMessageRecived;
                TB_ServerAddres.IsEnabled = false;
                BTN_ConnectToServer.IsEnabled = false;
                TB_UserName.IsEnabled = false;

                Client.ConnectAsync();
            }
            else
                MessageBox.Show("Invalid port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void OnClientConnected(TcpClientWrap client)
        {
            UserConnectMessage message = new UserConnectMessage(Dispatcher.Invoke(() => TB_UserName.Text));
            client.SendAsync(message);

            client.ReceiveAsync();
        }

        private void OnConnectedFailed(TcpClientWrap client)
        {
            Client = null;

            Dispatcher.Invoke(() =>
            {
                TB_ServerAddres.IsEnabled = true;
                BTN_ConnectToServer.IsEnabled = true;
                TB_UserName.IsEnabled = true;
            });
          
        }

        private void OnClientDisconnected(TcpClientWrap client)
        {
            Client = null;
            TB_ServerAddres.IsEnabled = true;
            BTN_ConnectToServer.IsEnabled = true;
            TB_UserName.IsEnabled = true;

        }

        private void OnMessageRecived(TcpClientWrap client, Message message)
        {

            switch (message.GetType().Name)
            {
                case "StartGameMessage":
                {
                    StartGameMessage info = (StartGameMessage)message;
                    CurrentEnemy = info.EnemyUserName;

                    isGamaRunning = true;
                    IsMyTurn = info.IsYourTurn;
                    RoomId = info.RoomId;
                    break;
                }
                case "GameInfoMessage":
                {
                    GameInfoMessage info = (GameInfoMessage)message;

                    if (info.IsGameOver)
                    {
                        switch (info.IsWinner)
                        {
                            case true:
                                MessageBox.Show("You winn");
                                break;
                            case false:
                                MessageBox.Show("You lose");
                                break;
                        }

                    }
                    else
                    {
                        Cell cell = Field.Where(c => c.X == info.UpdatedCell.X && c.Y == info.UpdatedCell.Y).First();
                        cell.State = info.UpdatedCell.State;

                        IsMyTurn = true;
                    }

                    break;
                }
                   
                    
                 

            }
                       
        }

        public void OnMessageSent(Message message)
        {

            switch (message.GetType().Name)
            {
                case "GameInfoMessage":
                    IsMyTurn = false;
                    break;
            }

       
            
        }

        #endregion Events



        #region CellClick command

        private bool CellClick_CanExecute(object obj)
        {
            return isGamaRunning && isMyTurn;
        }

        private void CellClick_Execute(object obj)
        {
            Cell cell = (Cell)obj;

            if(cell.State != CellState.Empty)
            {
                cell.State = MyIcon;

                GameInfoMessage message = new GameInfoMessage(cell, RoomId);
                Client.SendAsync(message);
                Client.Receive();
            }

        
        }

        #endregion

        private void ClearField()
        {
            foreach (var item in Field)
            {
                item.State = CellState.Empty;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
