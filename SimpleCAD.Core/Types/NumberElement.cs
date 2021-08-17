namespace SimpleCAD.Core.Types
{
    public class NumberElement : StuffElement
    {
        public double Number { get; set; } = 0.0;

        public override object Clone()
        {
            return new NumberElement() { Number = Number };
        }
    }
}