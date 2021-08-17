using SimpleCAD.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFDemo
{
    public static class Examples
    {
        public static TracingScheme GetExample10x10()
        {
            var scheme = new TracingScheme()
            {
                Graph = new(10, 10),
            };
            scheme.Wires = new()
            {
                new() { scheme.Graph.GetCell(6, 3), scheme.Graph.GetCell(0, 9) },
                new() { scheme.Graph.GetCell(9, 9), scheme.Graph.GetCell(2, 3) }
            };
            scheme.Graph.GetCell(5, 3).Element = new ChipElement();
            scheme.Graph.GetCell(5, 4).Element = new ChipElement();
            scheme.Graph.GetCell(1, 1).Element = new ChipElement();
            scheme.Graph.GetCell(1, 2).Element = new ChipElement();
            scheme.Graph.GetCell(2, 1).Element = new ChipElement();
            scheme.Graph.GetCell(2, 2).Element = new ChipElement();

            scheme.Graph.GetCell(2, 3).Element = new ConductorElement()
            {
                ConductorWires = new()
                { new() { ConductorElement.ConductorSide.Left } }
            };
            scheme.Graph.GetCell(6, 3).Element = new ConductorElement()
            {
                ConductorWires = new()
                { new() { ConductorElement.ConductorSide.Top } }
            };
            scheme.Graph.GetCell(0, 9).Element = new ConductorElement()
            {
                ConductorWires = new()
                { new() { ConductorElement.ConductorSide.Right } }
            };
            scheme.Graph.GetCell(9, 9).Element = new ConductorElement()
            {
                ConductorWires = new()
                { new() { ConductorElement.ConductorSide.Right } }
            };
            return scheme;
        }
    }
}
