using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLibrary
{
    public struct Cell
    {
        public enum CellState
        {
            Empty,
            Cross,
            Circle
        }
        int X, Y;
        CellState State;
        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            State = CellState.Empty;
        }
    }
}
