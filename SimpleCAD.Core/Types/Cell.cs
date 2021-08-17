using System;
using System.Collections.Generic;

namespace SimpleCAD.Core.Types
{
    public class Cell : ICloneable
    {
        public CellGraph CellGraph { get; internal set; }
        internal void SetGraph(CellGraph cellGraph, uint rowIndex, uint columnIndex)
        {
            CellGraph = cellGraph;
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
        }

        public uint? RowIndex { get; set; }
        public uint? ColumnIndex { get; set; }

        public Cell() { }
        public Cell(CellGraph cellGraph, uint rowIndex, uint columnIndex)
        {
            SetGraph(cellGraph, rowIndex, columnIndex);
        }

        public List<Cell> Neightbors => CellGraph?.Neightbors(this);
        public Cell TopNeightbor => CellGraph?.TopNeightbor(this);
        public Cell RightNeightbor => CellGraph?.RightNeightbor(this);
        public Cell LeftNeightbor => CellGraph?.LeftNeightbor(this);
        public Cell BottomNeightbor => CellGraph?.BottomNeightbor(this);

        public Element Element { get; set; }
        public StuffElement StuffElement { get; set; }

        public bool IsFree { get => Element == null && StuffElement == null; }
        public bool IsBusy { get => Element != null || StuffElement != null; }

        public object Clone()
        {
            return new Cell()
            {
                Element = (Element)Element?.Clone(),
                StuffElement = (StuffElement)StuffElement?.Clone(),
            };
        }
    }
}