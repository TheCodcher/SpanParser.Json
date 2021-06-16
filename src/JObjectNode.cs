using System;
using System.Collections.Generic;
using System.Text;

namespace SpanParser
{
    namespace Json
    {
        /// <summary>
        /// an internal object describing a  json-object. Use JSpan for parsing
        /// </summary>
        public readonly struct JObjectNode
        {
            internal readonly JType ValueType;
            internal readonly int Value;
            internal readonly int ValueStartIndx;
            internal readonly int ValueEndIndx;

            internal readonly int KeyIndxStart;
            internal readonly int KeyIndxEnd;

            internal readonly int Next;
            internal JObjectNode(JType type, int valueRef, int keyStartIndx, int keyEndIndx, int valueStartIndx, int valueEndIndx, int next)
            {
                ValueType = type;
                Value = valueRef;
                ValueStartIndx = valueStartIndx;
                ValueEndIndx = valueEndIndx;

                KeyIndxStart = keyStartIndx;
                KeyIndxEnd = keyEndIndx;

                Next = next;
            }

            internal static JObjectNode Parse(ReadOnlySpan<char> source, int startSearchIndx, IJsonMemoryContext memoryContext, out int memoryIndx, out int endValueIndx)
            {
                var KeyIndxStart = source.GetValueSeparatorIndx(startSearchIndx, true);
                if (KeyIndxStart == -1)
                {
                    var selff = new JObjectNode(JType.None, -1, 0, 0, 0, 0, -1);
                    endValueIndx = source.GetSeparatorIndx(startSearchIndx, JSONSetup.OBJECT.Close);
                    if (endValueIndx == -1) throw new InvalidOperationException("wrongJson");
                    memoryIndx = memoryContext.ObjectRefMemory.Add(selff);
                    return selff;
                }
                else
                {
                    KeyIndxStart++;
                }
                var KeyIndxEnd = source.GetValueSeparatorIndx(KeyIndxStart);

                if (KeyIndxEnd == -1) throw new InvalidOperationException("wrongJson");

                var tempIndx = source.GetSeparatorIndx(KeyIndxEnd + 1, JSONSetup.BETWEN_KEY_AND_SENSE);
                var ValueStartIndx = source.GetSenseSeparatorIndx(tempIndx + 1, out var ValueType);

                if (ValueType == JType.None) throw new InvalidOperationException("wrongJson");

                var Next = -1;
                var Value = -1;
                var ValueEndIndx = -1;

                if (ValueType == JType.Array)
                {
                    JArrayNode.Parse(source, ValueStartIndx + 1, 0, memoryContext, out Value, out ValueEndIndx);
                }
                if (ValueType == JType.Object)
                {
                    Parse(source, ValueStartIndx + 1, memoryContext, out Value, out ValueEndIndx);
                    ValueEndIndx = source.GetSeparatorIndx(ValueEndIndx + 1, JSONSetup.OBJECT.Close);
                }
                if (ValueType == JType.Value)
                {
                    ValueEndIndx = source.GetValueSeparatorIndx(ValueStartIndx + 1);
                }

                if (source.HasNextSense(ValueEndIndx + 1, out tempIndx))
                {
                    Parse(source, tempIndx + 1, memoryContext, out Next, out endValueIndx);
                }
                else
                {
                    endValueIndx = ValueEndIndx;
                }

                if (endValueIndx == -1)
                {
                    throw new InvalidOperationException("wrongJson");
                }

                var self = new JObjectNode(ValueType, Value, KeyIndxStart, KeyIndxEnd, ValueStartIndx, ValueEndIndx, Next);
                memoryIndx = memoryContext.ObjectRefMemory.Add(self);
                return self;
            }

            internal bool GetByKey(ReadOnlySpan<char> key, ReadOnlySpan<char> source, IMemoryHolder<JObjectNode> refMemomry, out JObjectNode value)
            {
                if (!source[KeyIndxStart..KeyIndxEnd].SequenceEqual(key))
                {
                    if (Next == -1)
                    {
                        value = default;
                        return false;
                    }
                    else
                    {
                        var next = refMemomry.GetByRef(Next);
                        return next.GetByKey(key, source, refMemomry, out value);
                    }
                }
                else
                {
                    value = this;
                    return true;
                }
            }
        }
    }
}
