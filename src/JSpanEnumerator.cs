using System;
using System.Collections.Generic;
using System.Text;

namespace SpanParser
{
    namespace Json
    {
        /// <summary>
        /// an object that provides access to a json-array, similar to an IEnumerator
        /// </summary>
        public ref struct JSpanEnumerator
        {
            private readonly JSpan baseObj;
            int iterator;
            int count;

            internal JSpanEnumerator(JSpan baseObj)
            {
                this.baseObj = baseObj;
                Current = default;
                iterator = 0;
                count = -1;
            }
            /// <summary>
            /// assigns the Сurrent to the next element of the enumerator 
            /// </summary>
            /// <returns>true, if there are objects in the enumerator</returns>
            public bool MoveNext()
            {
                if (iterator == -1) return false;
                Current = baseObj[iterator];
                if (Current.IsEmpty)
                {
                    iterator = -1;
                    Current = default;
                    return false;
                }
                iterator++;
                return true;
            }
            /// <summary>
            /// similar to MoveNext, 
            /// allows you to iterate over objects from the last to the first
            /// </summary>
            /// <returns>true, if there are objects in the enumerator</returns>
            public bool MovePrevious()
            {
                if (iterator == -1) return false;
                Current = baseObj[iterator];
                if (Current.IsEmpty)
                {
                    iterator = -1;
                    Current = default;
                    return false;
                }
                iterator--;
                return true;
            }
            /// <summary>
            /// allows you to get the last element of the enumeration 
            /// </summary>
            /// <returns>default if enumeration is empty</returns>
            public JSpan Last()
            {
                return baseObj[Count() - 1];
            }
            /// <summary>
            /// allows you to get the first element of the enumeration 
            /// </summary>
            /// <returns>default if enumeration is empty</returns>
            public JSpan First()
            {
                return baseObj[0];
            }
            /// <summary>
            /// allows you to get the length of the enumeration
            /// </summary>
            /// <returns></returns>
            public int Count()
            {
                if (count == -1)
                {
                    var iter = iterator;
                    var counter = 0;
                    ResetToFirst();
                    while (MoveNext())
                    {
                        counter++;
                    }
                    iterator = iter;
                    count = counter;
                }
                return count;
            }
            /// <summary>
            /// moves the iterator to the first item. 
            /// </summary>
            public void ResetToFirst()
            {
                iterator = 0;
            }
            /// <summary>
            /// moves the iterator to the last item. 
            /// Use before using the MovePrevious
            /// </summary>
            public void ResetToLast()
            {
                iterator = Count() - 1;
            }
            /// <summary>
            /// contains the current enumeration object or default of JSpan
            /// </summary>
            public JSpan Current { get; private set; }
        }
    }
}
