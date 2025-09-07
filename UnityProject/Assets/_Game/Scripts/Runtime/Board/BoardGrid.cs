using System;
using UnityEngine;
using _Game.Runtime.Core;

namespace _Game.Runtime.Board
{
    public sealed class BoardGrid
    {
        public GridSize Size { get; }
        public float CellSize { get; }

        public int PlaceableRowCount => Size.Rows / 2;

        private readonly bool[,] _occupied; // [row, col]

        public BoardGrid(int rows, int cols, float cellSize)
        {
            if (cellSize <= 0f) throw new ArgumentOutOfRangeException(nameof(cellSize));
            Size = new GridSize(rows, cols);
            CellSize = cellSize;
            _occupied = new bool[Size.Rows, Size.Cols];
        }

        public bool InBounds(Cell c) =>
            c.Row >= 0 && c.Row < Size.Rows && c.Col >= 0 && c.Col < Size.Cols;

        public bool IsOccupied(Cell c)
        {
            if (!InBounds(c)) return false;
            return _occupied[c.Row, c.Col];
        }

        public bool TryOccupy(Cell c)
        {
            if (!InBounds(c) || _occupied[c.Row, c.Col]) return false;
            _occupied[c.Row, c.Col] = true;
            return true;
        }

        public void Free(Cell c)
        {
            if (!InBounds(c)) return;
            _occupied[c.Row, c.Col] = false;
        }

        public bool IsDefensePlacementAllowed(Cell c)
        {
            return InBounds(c) && c.Row < PlaceableRowCount && !_occupied[c.Row, c.Col];
        }

        public Vector3 CellToLocalCenter(Cell c)
        {
            if (!InBounds(c)) throw new ArgumentOutOfRangeException(nameof(c));
            var x = (c.Col + 0.5f) * CellSize;
            var z = (c.Row + 0.5f) * CellSize;
            return new Vector3(x, 0f, z);
        }

       
        public bool TryLocalToCell(Vector3 local, out Cell cell)
        {
            int col = Mathf.FloorToInt(local.x / CellSize);
            int row = Mathf.FloorToInt(local.z / CellSize);
            cell = new Cell(row, col);
            return InBounds(cell);
        }
    }
}
