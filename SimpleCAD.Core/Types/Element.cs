using System;

namespace SimpleCAD.Core.Types
{
    public abstract class Element : ICloneable
    {
        public abstract object Clone();
    }
}