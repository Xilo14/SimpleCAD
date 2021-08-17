using System.Collections.Generic;
using SimpleCAD.Core.Types;

namespace SimpleCAD.Core.Interfaces
{
    public interface ITracer
    {
        public List<TracingScheme> Snapshots { get; }
        public TracingScheme Result { get; }
        public TracingScheme StartScheme { get; set; }

        public TracingScheme TraceAllSteps();
        public TracingScheme TraceStep();
    }
}