using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleCAD.Core.Types
{
    public class CellGraph : ICloneable
    {
        public ConcurrentBag<Cell> Cells { get; protected set; }
        public Cell[,] CellsMatrix { get; protected set; }
        public uint Height { get; set; }
        public uint Width { get; set; }

        public Cell GetCell(uint RowIndex, uint ColumnIndex) => CellsMatrix[RowIndex, ColumnIndex];
        public CellGraph(uint height = 100, uint width = 100)
        {
            Height = height;
            Width = width;
            CreateClearCells();
        }
        protected CellGraph(uint height, uint width, Cell[,] cellsMatrix)
        {
            CellsMatrix = new Cell[height, width];
            Height = height;
            Width = width; ;
            Cells = new ConcurrentBag<Cell>();
            for (uint i = 0; i < Height; i++)
            {
                for (uint j = 0; j < Width; j++)
                {
                    CellsMatrix[i, j] = (Cell)cellsMatrix[i, j].Clone();
                    CellsMatrix[i, j].SetGraph(this, i, j);
                    Cells.Add(CellsMatrix[i, j]);
                }
            }
        }

        public List<Cell> Neightbors(Cell cell) => cell.CellGraph == this ? new()
        {
            TopNeightbor(cell),
            RightNeightbor(cell),
            LeftNeightbor(cell),
            BottomNeightbor(cell),
        } : null;
        public Cell TopNeightbor(Cell cell)
            => cell.CellGraph == this && ((int)cell.RowIndex - 1) >= 0
            ? CellsMatrix[(int)cell.RowIndex - 1, (int)cell.ColumnIndex] : null;
        public Cell RightNeightbor(Cell cell)
            => cell.CellGraph == this && ((int)cell.ColumnIndex + 1) < Width
            ? CellsMatrix[(int)cell.RowIndex, (int)cell.ColumnIndex + 1] : null;
        public Cell LeftNeightbor(Cell cell)
            => cell.CellGraph == this && ((int)cell.ColumnIndex - 1) >= 0
            ? CellsMatrix[(int)cell.RowIndex, (int)cell.ColumnIndex - 1] : null;
        public Cell BottomNeightbor(Cell cell)
            => cell.CellGraph == this && ((int)cell.RowIndex + 1) < Height
             ? CellsMatrix[(int)cell.RowIndex + 1, (int)cell.ColumnIndex] : null;

        public void ClearStuff()
        {
            foreach (var cell in Cells)
                cell.StuffElement = null;
        }

        protected void CreateClearCells()
        {
            CellsMatrix = new Cell[Height, Width];
            Cells = new ConcurrentBag<Cell>();
            for (uint i = 0; i < Height; i++)
            {
                for (uint j = 0; j < Width; j++)
                {
                    CellsMatrix[i, j] = new(this, i, j);
                    Cells.Add(CellsMatrix[i, j]);
                }
            }

        }

        public object Clone()
        {
            return new CellGraph(Height, Width, CellsMatrix);
        }
    }
}