using System;
using System.Collections.Generic;
using System.Text;

namespace SpanParser
{
    /// <summary>
    /// implements the necessary memory access
    /// </summary>
    /// <typeparam name="T">the type to be located in memory is used in IMemoryContext</typeparam>
    public interface IMemoryHolder<T> where T : unmanaged
    {
        /// <summary>
        /// Allows you to get an object from memory by reference 
        /// </summary>
        /// <param name="indx">reference</param>
        /// <returns>object or default</returns>
        /// <exception cref="IndexOutOfRangeException">When indx less then zero or bigger then length</exception>
        T GetByRef(int indx);

        /// <summary>
        /// Allows to write an object into memory and get a reference to it 
        /// </summary>
        /// <param name="value">object</param>
        /// <returns>reference</returns>
        int Add(T value);
    }

    namespace Json
    {
        /// <summary>
        /// implement at your discretion to provide unmanaged memory for the parser to run without allocations, 
        /// or use ready-made MemoryContext class 
        /// </summary>
        public interface IJsonMemoryContext
        {
            /// <summary>
            /// implements the necessary memory access for storing objects
            /// </summary>
            IMemoryHolder<JObjectNode> ObjectRefMemory { get; }
            /// <summary>
            ///  implements the necessary memory access for storing objects
            /// </summary>
            IMemoryHolder<JArrayNode> ArrayRefMemory { get; }
        }
    }
}
