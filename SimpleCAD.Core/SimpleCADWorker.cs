namespace SimpleCAD.Core
{
    public class SimpleCADWorker
    {
        public TracersCatalog Tracers { get; }
        public ConvertersCatalog Converters { get; }
        public SimpleCADWorker()
        {
            Tracers = new();
            Converters = new();
        }
    }
}