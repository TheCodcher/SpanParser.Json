using System;
using System.Collections.Generic;
using System.Text;

namespace SpanParser
{
    namespace Json
    {
        internal enum JType : byte
        {
            None = 0,
            Value,
            Array,
            Object
        }
    }
}
