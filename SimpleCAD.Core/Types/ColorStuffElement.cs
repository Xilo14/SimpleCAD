using System.Drawing;

namespace SimpleCAD.Core.Types
{
    public class ColorStuffElement : StuffElement
    {
        public Color Color { get; set; } = Color.White;
    
        public override object Clone()
        {
            return new ColorStuffElement() { Color = Color };
        }
    }

}