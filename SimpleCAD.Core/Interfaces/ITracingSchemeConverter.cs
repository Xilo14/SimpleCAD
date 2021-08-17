namespace SimpleCAD.Core.Interfaces
{
	public interface ITracingSchemeConverter<out T>
	{
        public T Convert();
	}
}