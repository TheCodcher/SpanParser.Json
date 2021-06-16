using System;

namespace JsonSpanParser
{
    /// <summary>
    /// basic IMemoryContext implementation
    /// </summary>
    public class MemoryContext : IMemoryContext
    {
        /// <summary>
        /// memory access for storing objects
        /// </summary>
        public IMemoryHolder<JObjectNode> ObjectRefMemory { get; } = new PoolArray<JObjectNode>();
        /// <summary>
        /// memory access for storing objects
        /// </summary>
        public IMemoryHolder<JArrayNode> ArrayRefMemory { get; } = new PoolArray<JArrayNode>();
        /// <summary>
        /// in this implementation allows you to overwrite memory 
        /// without utilizing it and not allocating new
        /// </summary>
        public void Release()
        {
            (ObjectRefMemory as PoolArray<JObjectNode>).Release();
            (ArrayRefMemory as PoolArray<JArrayNode>).Release();
        }
    }
}
