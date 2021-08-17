namespace SimpleCAD.Core.Types
{
    public class ChipElement : Element
    {
        public override object Clone()
        {
            return new ChipElement();
        }
    }
}