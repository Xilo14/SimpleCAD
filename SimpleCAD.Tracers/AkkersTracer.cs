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
    public class AkkersTracer : LeeTracer
    {
        public AkkersTracer(TracingScheme scheme) : base(scheme)
        {
        }

        protected override ulong NormalizedWavePropagationIndex
        {
            get
            {
                var x = WavePropagationIndex / 2;
                //x = (ulong)Math.Round((double)x);
                x %= 2;
                x += 1;
                //(ulong)Math.Round((double)WavePropagationIndex / 2) % 2 + 1;
                return x;
            }
        }
        
    }
    public static class AkkersTracerExtension
    {
        public static AkkersTracer Akkers(this TracersCatalog catalog, TracingScheme scheme)
        {
            return new AkkersTracer(scheme);
        }
    }
}
