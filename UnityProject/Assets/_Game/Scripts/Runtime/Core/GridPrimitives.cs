using System;

namespace _Game.Runtime.Core
{
    [Serializable]
    public readonly struct GridSize : IEquatable<GridSize>
    {
        public readonly int Rows;
        public readonly int Cols;

        public GridSize(int rows, int cols)
        {
            if (rows <= 0 || cols <= 0) throw new ArgumentOutOfRangeException(nameof(rows), "Rows/Cols must be > 0");
            Rows = rows; Cols = cols;
        }

        public bool Equals(GridSize other) => Rows == other.Rows && Cols == other.Cols;
        public override bool Equals(object obj) => obj is GridSize other && Equals(other);
        public override int GetHashCode() => (Rows * 397) ^ Cols;
        public override string ToString() => $"GridSize(Rows={Rows}, Cols={Cols})";
    }

    [Serializable]
    public readonly struct Cell : IEquatable<Cell>
    {
        public readonly int Row; 
        public readonly int Col; 

        public Cell(int row, int col) { Row = row; Col = col; }

        public bool Equals(Cell other) => Row == other.Row && Col == other.Col;
        public override bool Equals(object obj) => obj is Cell other && Equals(other);
        public override int GetHashCode() => (Row * 397) ^ Col;
        public override string ToString() => $"Cell(Row={Row}, Col={Col})";
    }
}