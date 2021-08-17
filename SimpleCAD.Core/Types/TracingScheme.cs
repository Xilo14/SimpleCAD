using System;
using System.Collections.Generic;

namespace SimpleCAD.Core.Types
{
    public class TracingScheme : ICloneable
    {
        public CellGraph Graph { get; set; } = new();
        public List<List<Cell>> Wires { get; set; } = new();

        public void ClearStuff()
        {
            Graph.ClearStuff();
        }
        public object Clone()
        {
            var sch = new TracingScheme() {
                Graph = (CellGraph)Graph.Clone(),
               
            };

            var newWires = new List<List<Cell>>();
            foreach (var wire in Wires)
            {
                var newWire = new List<Cell>();
                foreach (var cell in wire)
                {
                    newWire.Add(Graph.GetCell((uint)cell.RowIndex,(uint)cell.ColumnIndex));
                }
                newWires.Add(newWire);
            }
            
            sch.Wires = newWires;
            
            return sch;
        }
    }
}