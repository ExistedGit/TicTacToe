using GameLibrary;
using MessageLibrary;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Client
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string currentEnemy;
        private bool isMyTurn;
        private CellState myIcon;
        private bool isFindingEnemy;
        private bool isGameRunning;


        public TcpClientWrap Client { get; set; }
        public ObservableCollection<Cell> Field { get; set; } = new ObservableCollection<Cell>();
        public bool IsGameRunning
        {
            get => isGameRunning;
            set
            {
                isGameRunning = value;
                OnPropertyChanged();
            }
        }
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
        public bool IsFindingEnemy
        {
            get => isFindingEnemy;
            set
            {
                isFindingEnemy = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public CellState MyIcon
        {
            get => myIcon;
            set
            {
                if (value != CellState.Empty)
                    myIcon = value;
                else
                    throw new InvalidEnumArgumentException("Icon don't can be empty");
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

            IsFindingEnemy = false;
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
                Client.MessageReceived += OnMessageReceived;
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
            IsFindingEnemy = true;
            client.SendAsync(message);
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
            IsFindingEnemy = false;
            Client = null;
            TB_ServerAddres.IsEnabled = true;
            BTN_ConnectToServer.IsEnabled = true;
            TB_UserName.IsEnabled = true;

        }

        private void OnMessageReceived(TcpClientWrap client, Message message)
        {
            switch (message.GetType().Name)
            {
                case "StartGameMessage":
                {
                    StartGameMessage info = (StartGameMessage)message;
                    CurrentEnemy = info.EnemyUserName;
                    IsGameRunning = true;
                    IsFindingEnemy = false;
                    IsMyTurn = info.IsYourTurn;
                    RoomId = info.RoomId;
                    MyIcon = info.Cell;
                    Client.ReceiveAsync();
                    break;
                }
                case "GameInfoMessage":
                {
                    GameInfoMessage info = (GameInfoMessage)message;

                    if (info.Result != GameResult.None)
                    {
                        

                        switch (info.Result)
                        { 

                            case GameResult.Lose:
                            {
                                Cell cell = Field.First(c => c.X == info.UpdatedCell.X && c.Y == info.UpdatedCell.Y);
                                cell.State = info.UpdatedCell.State;
                                MessageBox.Show("You lost", "", MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                            }
                            case GameResult.Win:
                            {
                                MessageBox.Show("You won", "", MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                            } 
                            case GameResult.Draw:
                            {
                                if(info.UpdatedCell != null)
                                {
                                    Cell cell = Field.First(c => c.X == info.UpdatedCell.X && c.Y == info.UpdatedCell.Y);
                                    cell.State = info.UpdatedCell.State;
                                }

                              
                                MessageBox.Show("Draw", "", MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                            }
                              
                                
                          
                        }

                        IsGameRunning = false;

                        if (MessageBox.Show("Want to play more?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {

                            bool NewEnemy = false;
                            if(MessageBox.Show("Find new enemy?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                NewEnemy = true;
                        

                            ClearField();

                            IsFindingEnemy = true;

                            RestartGameMessage RestartMessage = new RestartGameMessage(true, NewEnemy); 
                            client.SendAsync(RestartMessage);
                            client.ReceiveAsync();

                        }
                        else
                        {     
                            RestartGameMessage RestartMessage = new RestartGameMessage(false);
                            client.SendAsync(RestartMessage);
                            
                        }
                    }
                    else
                    {
                        IsMyTurn = true;
                        Cell cell = Field.First(c => c.X == info.UpdatedCell.X && c.Y == info.UpdatedCell.Y);
                        cell.State = info.UpdatedCell.State;
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
                    Client.ReceiveAsync();
                    break;
                case "UserConnectMessage":
                    Client.ReceiveAsync();
                    break;
            }
        }

        #endregion Events



        #region CellClick command

        private bool CellClick_CanExecute(object obj)
        {
            return isGameRunning && isMyTurn;
        }

        private void CellClick_Execute(object obj)
        {
            Cell cell = (Cell)obj;

            if(cell.State == CellState.Empty)
            {
                cell.State = MyIcon;

                IsMyTurn = false;
                GameInfoMessage message = new GameInfoMessage(cell, RoomId);
                Client.SendAsync(message);
                
            }
        }

        #endregion

        private void ClearField()
        {
            foreach (var item in Field)
                item.State = CellState.Empty;
            
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
