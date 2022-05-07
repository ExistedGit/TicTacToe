using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameLibrary
{
    public enum CellState
    {
        Empty,
        Cross,
        Circle
    }

    [Serializable]
    public class Cell : INotifyPropertyChanged
    {
        private CellState state;

        public int X, Y;
        public CellState State
        {
            get => state;
            set
            {
                state = value;
                OnPropertyChanged();
            }
        }
        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            State = CellState.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
