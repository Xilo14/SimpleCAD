using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using SimpleCAD.Core;
using SimpleCAD.Core.Interfaces;
using SimpleCAD.Core.Types;
using Svg;

namespace SimpleCAD.Tracers
{
    public class Mod3Tracer : LeeTracer
    {
        public Mod3Tracer(TracingScheme scheme) : base(scheme)
        {
        }

        protected override ulong NormalizedWavePropagationIndex
        {
            get
            {
                return WavePropagationIndex % 3;
            }
        }
        
    }
    public static class Mod3TracerExtension
    {
        public static Mod3Tracer Mod3(this TracersCatalog catalog, TracingScheme scheme)
        {
            return new Mod3Tracer(scheme);
        }
    }
}
