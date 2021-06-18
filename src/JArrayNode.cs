using System;

namespace SpanParser
{
    namespace Json
    {
        /// <summary>
        /// an internal object describing a  json-array. Use JSpan for parsing
        /// </summary>
        public readonly struct JArrayNode
        {
            internal readonly JType ValueType;
            internal readonly int Value;
            internal readonly int ValueStartIndx;
            internal readonly int ValueEndIndx;

            internal readonly int SelfIndex;

            internal readonly int Next;

            internal JArrayNode(JType type, int valueRef, int self, int valueStartIndx, int valueEndIndx, int next)
            {
                ValueType = type;
                Value = valueRef;
                ValueStartIndx = valueStartIndx;
                ValueEndIndx = valueEndIndx;

                SelfIndex = self;

                Next = next;
            }

            internal bool Equality(JArrayNode another)
            {
                return
                    ValueType == another.ValueType &&
                    Value == another.Value &&
                    ValueStartIndx == another.ValueStartIndx &&
                    ValueEndIndx == another.ValueEndIndx &
                    SelfIndex == another.SelfIndex &&
                    Next == another.Next;
            }

            internal static JArrayNode Parse(ReadOnlySpan<char> source, int startSearchIndx, int selfIndx, IJsonMemoryContext memoryContext, out int memoryIndx, out int endValueIndx)
            {
                var ValueStartIndx = source.GetSenseSeparatorIndx(startSearchIndx, out var ValueType);

                if (ValueStartIndx == -1)
                {
                    var selff = new JArrayNode(JType.None, -1, 0, 0, 0, -1);
                    endValueIndx = source.GetSeparatorIndx(startSearchIndx, JSONSetup.ARRAY.Close);
                    if (endValueIndx == -1) 
                        throw new InvalidOperationException("wrongJson");
                    memoryIndx = memoryContext.ArrayRefMemory.Add(selff);
                    return selff;
                }
                //"arr":[]

                var asdasda = source[startSearchIndx..];

                if (ValueType == JType.None) 
                    throw new InvalidOperationException("wrongJson");


                var Next = -1;
                var Value = -1;
                var ValueEndIndx = -1;

                if (ValueType == JType.Array)
                {
                    Parse(source, ValueStartIndx + 1, 0, memoryContext, out Value, out ValueEndIndx);
                }
                if (ValueType == JType.Object)
                {
                    JObjectNode.Parse(source, ValueStartIndx + 1, memoryContext, out Value, out ValueEndIndx);
                    ValueEndIndx = source.GetSeparatorIndx(ValueEndIndx + 1, JSONSetup.OBJECT.Close);
                }
                if (ValueType == JType.Value)
                {
                    ValueEndIndx = source.GetValueSeparatorIndx(ValueStartIndx + 1);
                }

                if (ValueType == JType.Material)
                {
                    ValueEndIndx = source.GetMaterialValueEndIndx(ValueStartIndx);
                }

                if (source.HasNextSense(ValueEndIndx + 1, out var tempIndx))
                {
                    Parse(source, tempIndx + 1, selfIndx + 1, memoryContext, out Next, out endValueIndx);
                }
                else
                {
                    endValueIndx = source.GetSeparatorIndx(ValueEndIndx + 1, JSONSetup.ARRAY.Close);
                }

                if (endValueIndx == -1)
                {
                    throw new InvalidOperationException("wrongJson");
                }

                var self = new JArrayNode(ValueType, Value, selfIndx, ValueStartIndx, ValueEndIndx, Next);
                memoryIndx = memoryContext.ArrayRefMemory.Add(self);
                return self;
            }
            internal bool GetByIndex(int indx, IMemoryHolder<JArrayNode> refMemomry, out JArrayNode value)
            {
                if (SelfIndex != indx)
                {
                    if (Next == -1)
                    {
                        value = default;
                        return false;
                    }
                    else
                    {
                        var next = refMemomry.GetByRef(Next);
                        return next.GetByIndex(indx, refMemomry, out value);
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
