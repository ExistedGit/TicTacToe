using GameLibrary;
using MessageLibrary;
using System;
using System.Net;
using System.Linq;
namespace Server
{
    public delegate void RoomEventHandler(GameRoom room);
    public delegate void PlayerEventHandler(GameRoom sender, Player player);
    public class GameRoom
    {
        public event RoomEventHandler PlayerLeft;

        public static Random rng = new Random();
        public Player Player1
        {
            get => player1;
            set
            {
                player1 = value;
                if (value == null)
                {
                    player1 = Player2;
                    Player2 = null;
                }
            }
        }
        public Player Player2 { get; set; }
        public Cell[,] Cells { get; private set; }
        public bool Full { get => Player1 != null && Player2 != null; }
        public uint Id { get; private set; } = IdCounter++;
        private static uint IdCounter = 0;
        public GameRoom()
        {
        }
        private void RefillCells()
        {
            Cells = new Cell[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    Cells[i, j] = new Cell(i + 1, j + 1);
        }
        const bool DEBUG = true;
        private Player WhoseTurn;
        public void StartGame(bool addEvents = false)
        {
            RefillCells();
            bool turn = Convert.ToBoolean(rng.Next(0, 2));
            if (DEBUG) turn = DEBUG;
            WhoseTurn = turn ? Player1 : Player2;
            if (addEvents)
            {
                Player1.Client.Disconnected += (client) =>
                {
                    Player1 = null;
                    PlayerLeft?.Invoke(this);
                };
                Player2.Client.Disconnected += (client) =>
                {
                    Player2 = null;
                    PlayerLeft?.Invoke(this);
                };
            }
            Player1.Client.SendAsync(new StartGameMessage(Player2.UserName, Id, turn, CellState.Cross));
            Player2.Client.SendAsync(new StartGameMessage(Player1.UserName, Id, !turn, CellState.Circle));
        }

        private bool HorizontalCheck(Cell cell)
        {
            for (int i = 0; i < 3; i++)
                if (Cells[cell.Y - 1, i].State != cell.State)
                    return false;

            return true;
        }
        private bool VerticalCheck(Cell cell)
        {
            for (int i = 0; i < 3; i++)
                if (Cells[i, cell.X - 1].State != cell.State)
                    return false;
            return true;
        }
        private bool DiagonalCheck(Cell cell)
        {

            // Находится ли клетка на диагонали
            int x = cell.X - 1, y = cell.Y - 1;
            if (x == 1 ^ y == 1)
                return false;
            bool counter = true;
            // Правая диагональ 0/2 1/1 2/0
            for (int i = 0, j = 2; i < 3 && counter; i++, j--)
                if (Cells[j, i].State != cell.State)
                    counter = false;
            if (counter) return counter;
            // Левая диагональ 2/2 1/1 0/0
            for (int i = 2, j = 2; i > 0; i--, j--)
                if (Cells[j, i].State != cell.State)
                    return false;
            return true;
        }
        private bool CheckWin(Cell cell) => HorizontalCheck(cell) || VerticalCheck(cell) || DiagonalCheck(cell);

        public event PlayerEventHandler FindNewRoom;
        public event RoomEventHandler TryFindEnemy;
        private bool Player1Restart, Player2Restart;
        private Player player1;

        public void MessageReceived(TcpClientWrap client, Message msg)
        {
            if (msg is GameInfoMessage)
            {
                GameInfoMessage gameInfo = msg as GameInfoMessage;
                if (gameInfo.Id == Id)
                {
                    Cell cell = gameInfo.UpdatedCell;
                    Console.WriteLine(
                        "[ROOM " + Id.ToString() + "]: " +
                        "USER " + WhoseTurn.UserName + " changed cell " +
                        gameInfo.UpdatedCell.X + " " + gameInfo.UpdatedCell.Y + " "
                        + cell.State.ToString());
                    Cells[cell.Y - 1, cell.X - 1] = cell;
                    if (CheckWin(cell))
                    {
                        WhoseTurn.Client.SendAsync(new GameInfoMessage(null, Id, GameResult.Win));
                        WhoseTurn = WhoseTurn == Player1 ? Player2 : Player1;
                        WhoseTurn.Client.SendAsync(new GameInfoMessage(cell, Id, GameResult.Lose));
                        WhoseTurn = null;
                    }
                    else if (Cells.Cast<Cell>().All(c => c.State != CellState.Empty))
                    {
                        WhoseTurn.Client.SendAsync(new GameInfoMessage(null, Id, GameResult.Draw));
                        WhoseTurn = WhoseTurn == Player1 ? Player2 : Player1;
                        Player2.Client.SendAsync(new GameInfoMessage(cell, Id, GameResult.Draw));
                        WhoseTurn = null;
                    }
                    else
                    {
                        WhoseTurn = WhoseTurn == Player1 ? Player2 : Player1;
                        WhoseTurn.Client.SendAsync(new GameInfoMessage(cell, Id, GameResult.None));
                    }
                }
            }
            else if (msg is RestartGameMessage)
            {
                RestartGameMessage restart = msg as RestartGameMessage;
                Player player = Player1.Client.Tcp.Client.RemoteEndPoint.ToString().Equals(client.Tcp.Client.RemoteEndPoint.ToString()) ? Player1 : Player2;

                RefillCells();
                if (Player1Restart && Player2Restart)
                    Player1Restart = Player2Restart = false;
                if (restart.NewGame)
                {
                    if (restart.NewEnemy)
                    {
                        if (player == Player1)
                            Player1 = null;
                        else
                            Player2 = null;
                        FindNewRoom?.Invoke(this, player);

                        PlayerLeft?.Invoke(this);
                        TryFindEnemy?.Invoke(this);
                    }
                    else if (Player1Restart && Player2Restart)
                        StartGame(false);   
                    else
                    {
                        if (player == Player1)
                            Player1Restart = true;
                        else
                            Player2Restart = true;
                    }
                }
                else
                {
                    if (player == Player1)
                        Player1 = null;
                    else
                        Player2 = null;

                    PlayerLeft?.Invoke(this);
                    player.Client.Send(new UserDisconnectMessage());
                }
            }
        }

    }

}
