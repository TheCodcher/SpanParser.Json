using System;
using System.Text;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace SpanParser
{
    namespace Json
    {
        /// <summary>
        /// Allows to parse
        /// </summary>
#pragma warning disable CS0660 // Тип определяет оператор == или оператор !=, но не переопределяет Object.Equals(object o)
#pragma warning disable CS0661 // Тип определяет оператор == или оператор !=, но не переопределяет Object.GetHashCode()
        public readonly ref struct JSpan
#pragma warning restore CS0661 // Тип определяет оператор == или оператор !=, но не переопределяет Object.GetHashCode()
#pragma warning restore CS0660 // Тип определяет оператор == или оператор !=, но не переопределяет Object.Equals(object o)
        {
            private readonly ReadOnlySpan<char> SourceSet;
            private readonly IJsonMemoryContext MemoryContext;

            private readonly JArrayNode Array;
            private readonly JObjectNode Object;

            /// <summary>
            /// false if Source(jsonCharSpan) is empty, otherwise true
            /// </summary>
            public bool IsEmpty => Array.ValueType == JType.None && Object.ValueType == JType.None || SourceSet.IsEmpty;
            /// <summary>
            /// true if JSpan includes json array, otherwise false.
            /// you can get the value by index
            /// </summary>
            public bool IsArray =>
                Array.ValueType == JType.Array ||
                Object.ValueType == JType.Array;
            /// <summary>
            /// true if JSpan includes json object, otherwise false.
            /// you can get the value by key
            /// </summary>
            public bool IsObject =>
                Array.ValueType == JType.Object ||
                Object.ValueType == JType.Object;

            /// <summary>
            /// true if JSpan includes any value, otherwise false.
            /// you can get the value by ToString method
            /// </summary>
            public bool IsValue =>
                Array.ValueType == JType.Value ||
                Object.ValueType == JType.Value;

            /// <summary>
            /// true if JSpan includes true, false, null or digit, otherwise false.
            /// you can get the value by ToString or AsJsonString method
            /// </summary>
            public bool IsMaterial =>
                Array.ValueType == JType.Material ||
                Object.ValueType == JType.Material;

            private bool ObjectHasValue => Object.ValueType != JType.None;
            private bool ArrayHasValue => Array.ValueType != JType.None;

            internal JSpan(ReadOnlySpan<char> jsonCharSpan, IJsonMemoryContext context)
            {
                SourceSet = jsonCharSpan;
                MemoryContext = context;
                Array = default;
                Object = default;
            }
            internal JSpan(ReadOnlySpan<char> jsonCharSpan, IJsonMemoryContext context, JArrayNode array) : this(jsonCharSpan, context)
            {
                Array = array;
            }
            internal JSpan(ReadOnlySpan<char> jsonCharSpan, IJsonMemoryContext context, JObjectNode obj) : this(jsonCharSpan, context)
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
            public static JSpan Parse(ReadOnlySpan<char> jsonCharSpan, IJsonMemoryContext context)
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
                if (!IsObject) return default;
                var valueRef = ObjectHasValue ? Object.Value : Array.Value;
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
                if (!IsArray) return default;
                var valueRef = ObjectHasValue ? Object.Value : Array.Value;
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
                if (!IsArray) 
                    return default;
                return new JSpanEnumerator(this);
            }

            private bool GetValue(out int startIndx, out int endIndx)
            {
                if (ArrayHasValue)
                {
                    startIndx = Array.ValueStartIndx;
                    endIndx = Array.ValueEndIndx;
                    return true;
                }
                if (ObjectHasValue)
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
            /// use to get the value contained in the JSpan or contained json
            /// </summary>
            /// <returns>value, json or empty string</returns>
            public override string ToString()
            {
                if (IsMaterial || !IsValue) return ToJsonString();
                if (GetValue(out var startIndx, out var endIndx))
                {
                    startIndx++;

                    var builder = new StringBuilder(endIndx - startIndx);
                    int indx = startIndx;
                    while (indx < endIndx)
                    {
                        var found = SourceSet[indx..endIndx].IndexOf('\\');

                        if (found == -1)
                        {
                            builder.Append(SourceSet[indx..endIndx]);
                            break;
                        }
                        else
                        {
                            found += indx;
                            builder.Append(SourceSet[indx..found]);
                            var tempChar = GetBackslashConstruction(ref found, SourceSet);
                            builder.Append(tempChar);
                            indx = found + 1;
                        }
                    }

                    return builder.ToString();
                }
                return string.Empty;
            }

            private char GetBackslashConstruction(ref int indx, ReadOnlySpan<char> source)
            {
                indx++;
                var nextSymb = source[indx];
                switch (nextSymb)
                {
                    case 'a': return '\a';
                    case 'b': return '\b';
                    case 'r': return '\r';
                    case 'n': return '\n';
                    case 't': return '\t';
                    case 'f': return '\f';
                    case 'u':
                        indx++;
                        var res = (char)int.Parse(source[indx..(indx + 4)], System.Globalization.NumberStyles.HexNumber);
                        indx += 3;
                        return res;
                    case '\\': return '\\';
                    default: return nextSymb;
                }
            }

            /// <summary>
            /// use to get the value contained in the JSpan or contained json
            /// </summary>
            /// <returns>value, json or empty span</returns>
            public ReadOnlySpan<char> AsSpan()
            {
                if (IsMaterial || !IsValue) return AsJsonSpan();
                if (GetValue(out var startIndx, out var endIndx))
                {
                    return SourceSet[(startIndx + 1)..endIndx];
                }
                else
                    return ReadOnlySpan<char>.Empty;
            }

            /// <summary>
            /// use to get the value contained in the JSpan in json form
            /// (with opening and closing elements defining the json object)
            /// </summary>
            /// <returns>value or empty string</returns>
            public string ToJsonString()
            {
                if (GetValue(out var startIndx, out var endIndx))
                {
                    return new string(SourceSet[startIndx..(endIndx + 1)]);
                }
                else
                    return string.Empty;
            }

            /// <summary>
            /// use to get the value contained in the JSpan in json form
            /// (with opening and closing elements defining the json object)
            /// </summary>
            /// <returns>value or empty span</returns>
            public ReadOnlySpan<char> AsJsonSpan()
            {
                if (GetValue(out var startIndx, out var endIndx))
                {
                    return SourceSet[startIndx..(endIndx + 1)];
                }
                else
                    return ReadOnlySpan<char>.Empty;
            }

            /// <summary>
            /// use to deserialize
            /// </summary>
            /// <typeparam name="T">the type to which the JSpan(json object) will be deserialized</typeparam>
            /// <returns>object ot T-default value</returns>
            public T Deserialize<T>()
            {
                var set = AsJsonSpan();
                if (!set.IsEmpty)
                {
                    var encoding = Encoding.UTF8;
                    Span<byte> buffer = stackalloc byte[encoding.GetByteCount(set)];
                    encoding.GetBytes(set, buffer);
                    return JsonSerializer.Deserialize<T>(buffer);
                }
                else
                    return default;
            }

            /// <summary>
            /// JSpans are compared by values and by references to included ReadOnlySpan of char and IJsonMemoryContext
            /// </summary>
            public static bool operator ==(JSpan item1, JSpan item2)
            {
                return
                    item1.MemoryContext == item2.MemoryContext &&
                    item1.Object.Equality(item2.Object) &&
                    item1.Array.Equality(item2.Array) &&
                    item1.SourceSet == item2.SourceSet;
            }
            /// <summary>
            /// JSpans are compared by values and by references to included ReadOnlySpan of char and IJsonMemoryContext
            /// </summary>
            public static bool operator !=(JSpan item1, JSpan item2)
            {
                return
                    item1.MemoryContext != item2.MemoryContext ||
                    !item1.Object.Equality(item2.Object) ||
                    !item1.Array.Equality(item2.Array) ||
                    item1.SourceSet != item2.SourceSet;
            }
        }
    }
}
