using System;
using System.Collections.Generic;
using System.Text;

namespace JsonSpanParser
{
    internal enum JType : byte
    {
        None = 0,
        Value,
        Array,
        Object
    }
}
