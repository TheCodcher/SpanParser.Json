using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace JsonSpanParser
{
    /// <summary>
    /// Allows to parse
    /// </summary>
    public readonly ref struct JSpan
    {
        private readonly ReadOnlySpan<char> SourceSet;
        private readonly IMemoryContext MemoryContext;

        private readonly JArrayNode Array;
        private readonly JObjectNode Object;

        /// <summary>
        /// false if Source(jsonCharSpan) is empty, otherwise true
        /// </summary>
        public bool IsEmpty => !(IsArray || IsObject || IsValue) || SourceSet.IsEmpty;
        /// <summary>
        /// true if JSpan includes json array, otherwise false.
        /// you can get the value by index
        /// </summary>
        public bool IsArray => Array.ValueType != JType.None;
        /// <summary>
        /// true if JSpan includes json object, otherwise false.
        /// you can get the value by key
        /// </summary>
        public bool IsObject => Object.ValueType != JType.None;

        /// <summary>
        /// true if JSpan includes any value, otherwise false.
        /// you can get the value by ToString method
        /// </summary>
        public bool IsValue =>
            IsArray && Array.ValueType == JType.Value ||
            IsObject && Object.ValueType == JType.Value;


        internal JSpan(ReadOnlySpan<char> jsonCharSpan, IMemoryContext context)
        {
            SourceSet = jsonCharSpan;
            MemoryContext = context;
            Array = default;
            Object = default;
        }
        internal JSpan(ReadOnlySpan<char> jsonCharSpan, IMemoryContext context, JArrayNode array) : this(jsonCharSpan, context)
        {
            Array = array;
        }
        internal JSpan(ReadOnlySpan<char> jsonCharSpan, IMemoryContext context, JObjectNode obj) : this(jsonCharSpan, context)
        {
            Object = obj;
        }
        /// <summary>
        /// Use to parse
        /// </summary>
        /// <param name="jsonCharSpan">span incudes json object as json-string</param>
        /// <param name="context">required memory access</param>
        /// <returns>an object similar in functionality to a JObject from Newtonsoft.Json.Linq</returns>
        /// <exception cref="InvalidOperationException">throws if jsonCharSpan include wrong json</exception>
        public static JSpan Parse(ReadOnlySpan<char> jsonCharSpan, IMemoryContext context)
        {
            var foundIndx = JSONSetup.GetSenseSeparatorIndx(jsonCharSpan, 0, out var type);
            if (type == JType.Object)
            {
                JObjectNode.Parse(jsonCharSpan, foundIndx + 1, context, out var objectRef, out var valueEndIndx);
                valueEndIndx = jsonCharSpan.GetSeparatorIndx(valueEndIndx + 1, JSONSetup.OBJECT.Close);
                var Object = new JObjectNode(JType.Object, objectRef, 0, 0, foundIndx, valueEndIndx, -1);
                return new JSpan(jsonCharSpan, context, Object);
            }
            if (type == JType.Array)
            {
                JArrayNode.Parse(jsonCharSpan, foundIndx + 1, 0, context, out var arrayRef, out var valueEndIndx);
                var Array = new JObjectNode(JType.Array, arrayRef, 0, 0, foundIndx, valueEndIndx, -1);
                return new JSpan(jsonCharSpan, context, Array);
            }
            return new JSpan(jsonCharSpan, context);
        }
        /// <summary>
        /// if JSpan is json object use this method to get value by key,
        /// if JSpan not an json object, then the method will return an empty JSpan 
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value or empty JSpan if the key is not found</returns>
        public JSpan this[ReadOnlySpan<char> key] => FindByKey(key);

        private JSpan FindByKey(ReadOnlySpan<char> key)
        {
            if (!(Array.ValueType == JType.Object || Object.ValueType == JType.Object)) return default;
            var valueRef = IsObject ? Object.Value : Array.Value;
            var workflow = MemoryContext.ObjectRefMemory.GetByRef(valueRef);
            if (workflow.GetByKey(key, SourceSet, MemoryContext.ObjectRefMemory, out var obj))
            {
                return new JSpan(SourceSet, MemoryContext, obj);
            }
            return new JSpan(SourceSet, MemoryContext);
        }

        /// <summary>
        /// if JSpan is json array use this method to get value by index,
        /// if JSpan not an json array, then the method will return an empty JSpan 
        /// </summary>
        /// <param name="indx">index</param>
        /// <returns>value or empty JSpan if the key is not found</returns>
        public JSpan this[int indx] => FindByObjectIndex(indx);

        private JSpan FindByObjectIndex(int indx)
        {
            if (!(Array.ValueType == JType.Array || Object.ValueType == JType.Array)) return default;
            var valueRef = IsObject ? Object.Value : Array.Value;
            var workflow = MemoryContext.ArrayRefMemory.GetByRef(valueRef);
            if (workflow.GetByIndex(indx, MemoryContext.ArrayRefMemory, out var obj))
            {
                return new JSpan(SourceSet, MemoryContext, obj);
            }
            return new JSpan(SourceSet, MemoryContext);
        }

        /// <summary>
        /// allows you to get an enumerator of the array contained in the JSpan 
        /// </summary>
        /// <returns>by default if JSpan does not contain an array</returns>
        public JSpanEnumerator GetEnumerator()
        {
            if (!(Array.ValueType == JType.Array || Object.ValueType == JType.Array)) return default;
            return new JSpanEnumerator(this);
        }

        private bool GetValue(out int startIndx, out int endIndx)
        {
            if (IsArray)
            {
                startIndx = Array.ValueStartIndx;
                endIndx = Array.ValueEndIndx;
                return true;
            }
            if (IsObject)
            {
                startIndx = Object.ValueStartIndx;
                endIndx = Object.ValueEndIndx;
                return true;
            }
            startIndx = -1;
            endIndx = -1;
            return false;
        }

        /// <summary>
        /// use to get the value contained in the JSpan
        /// </summary>
        /// <returns>value or empty string</returns>
        public override string ToString()
        {
            if (GetValue(out var startIndx, out var endIndx))
            {
                return new string(SourceSet[(startIndx + 1)..endIndx]);
            }
            else
                return string.Empty;
        }
        /// <summary>
        /// use to get the value contained in the JSpan in json form
        /// (with opening and closing elements defining the json object)
        /// </summary>
        /// <returns>value or empty string</returns>
        public string AsJsonString()
        {
            if (GetValue(out var startIndx, out var endIndx))
            {
                return new string(SourceSet[startIndx..(endIndx + 1)]);
            }
            else
                return string.Empty;
        }
        /// <summary>
        /// use to deserialize
        /// </summary>
        /// <typeparam name="T">the type to which the JSpan(json object) will be deserialized</typeparam>
        /// <returns>object ot T-default value</returns>
        public T Deserialize<T>()
        {
            if (GetValue(out var startIndx, out var endIndx))
            {
                var encoding = Encoding.UTF8;
                var set = SourceSet[startIndx..endIndx];
                Span<byte> buffer = stackalloc byte[encoding.GetByteCount(set)];
                encoding.GetBytes(set, buffer);
                return JsonSerializer.Deserialize<T>(buffer);
            }
            else
                return default;
        }
    }
}
