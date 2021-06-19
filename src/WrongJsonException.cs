using System;
using System.Collections.Generic;
using System.Text;

namespace SpanParser
{
    namespace Json
    {
        public class WrongJsonException : InvalidOperationException
        {
            public WrongJsonException() : base() { }

#nullable enable
            public WrongJsonException(string? message) : base(message) { }
            public WrongJsonException(string? message, Exception innerException) : base(message, innerException) { }
#nullable disable
        }
    }
}
