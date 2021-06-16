namespace SpanParser
{
    namespace Json
    {
        /// <summary>
        /// basic IMemoryContext implementation
        /// </summary>
        public class JsonMemoryContext : IJsonMemoryContext
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
            /// <summary>
            /// in this implementation allows you to release memory
            /// and reset buffer to initial state
            /// </summary>
            public void Collect()
            {
                (ObjectRefMemory as PoolArray<JObjectNode>).Clear();
                (ArrayRefMemory as PoolArray<JArrayNode>).Clear();
            }

            /// <summary>
            /// in this implementation allows you 
            /// to сompress the used memory to the filled length 
            /// </summary>
            public void Compress()
            {
                (ObjectRefMemory as PoolArray<JObjectNode>).Compress();
                (ArrayRefMemory as PoolArray<JArrayNode>).Compress();
            }
        }
    }
}
