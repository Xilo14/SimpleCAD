using System.Collections.Generic;

namespace SimpleCAD.Core.Types
{
    public class ConductorElement : Element
    {
        public enum ConductorSide
        {
            Top, Right, Bottom, Left
        }
        public List<List<ConductorSide>> ConductorWires { get; set; } = new List<List<ConductorSide>>();
        public ConductorElement()
        {

        }

        public override object Clone()
        {

            var newWires = new List<List<ConductorSide>>();
            foreach (var wire in ConductorWires)
            {
                var newWire = new List<ConductorSide>();
                foreach (var side in wire)
                {
                    newWire.Add(side);
                }
                newWires.Add(newWire);
            }
            return new ConductorElement()
            {
                ConductorWires = newWires
            };
        }
    }
}