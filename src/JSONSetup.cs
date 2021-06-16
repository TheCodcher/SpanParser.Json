using System;
using System.Collections.Immutable;

namespace SpanParser
{
    namespace Json
    {
        internal static class JSONSetup
        {
            public static readonly char BETWEN_KEY_AND_SENSE = ':';
            public static readonly char BETWEN = ',';
            public static readonly char VALUE_AND_KEY = '"';

            public static readonly JSONSeparator OBJECT = new JSONSeparator('{', '}', JType.Object);
            public static readonly JSONSeparator ARRAY = new JSONSeparator('[', ']', JType.Array);
            public static readonly JSONSeparator VALUE = new JSONSeparator(VALUE_AND_KEY, VALUE_AND_KEY, JType.Value);

            public static readonly ImmutableHashSet<char> MANAGE_CONSTRUCT = new[] { '\r', ' ', '\n', '\a', '\b', '\f', '\t' }.ToImmutableHashSet();

            public static JSONSeparator ChooseSeparate(char OpenSimbol)
            {
                if (OpenSimbol == OBJECT.Open) return OBJECT;
                if (OpenSimbol == ARRAY.Open) return ARRAY;
                if (OpenSimbol == VALUE.Open) return VALUE;

                return null;
                //throw new ArgumentException($"{OpenSimbol} - not registred separator: try with {{ }} [ ] {'"'}");
            }
            public static int GetSeparatorIndx(this ReadOnlySpan<char> source, int searchstartIndx, char sep)
            {
                for (int i = searchstartIndx; i < source.Length; i++)
                {
                    var symbol = source[i];
                    if (symbol == sep) return i;
                    if (MANAGE_CONSTRUCT.Contains(symbol)) continue;
                    return -1;
                }
                return -1;
            }
            public static bool HasNextSense(this ReadOnlySpan<char> source, int searchstartIndx, out int betwenIndx)
            {
                for (int i = searchstartIndx; i < source.Length; i++)
                {
                    var symbol = source[i];
                    if (symbol == BETWEN)
                    {
                        betwenIndx = i;
                        return true;
                    }
                    if (symbol == OBJECT.Close || symbol == ARRAY.Close)
                    {
                        betwenIndx = i;
                        return false;
                    }
                    if (MANAGE_CONSTRUCT.Contains(symbol)) continue;

                    betwenIndx = -1;
                    return false;
                }
                betwenIndx = -1;
                return false;
            }
            public static int GetSenseSeparatorIndx(this ReadOnlySpan<char> source, int searchstartIndx, out JType type)
            {
                for (int i = searchstartIndx; i < source.Length; i++)
                {
                    var symbol = source[i];
                    var sep = ChooseSeparate(symbol);
                    if (sep != null)
                    {
                        type = sep.Type;
                        return i;
                    }
                    if (MANAGE_CONSTRUCT.Contains(symbol)) continue;

                    type = JType.None;
                    return -1;
                }
                type = JType.None;
                return -1;
            }
            public static int GetValueSeparatorIndx(this ReadOnlySpan<char> source, int searchstartIndx, bool open = false)
            {
                return open ? GetOpenValueSeparatorIndx(source, searchstartIndx) : GetCloseValueSeparatorIndx(source, searchstartIndx);
            }
            private static int GetOpenValueSeparatorIndx(ReadOnlySpan<char> source, int searchstartIndx)
            {
                return source.GetSeparatorIndx(searchstartIndx, VALUE_AND_KEY);
            }
            private static int GetCloseValueSeparatorIndx(ReadOnlySpan<char> source, int searchstartIndx)
            {
                var found = source[searchstartIndx..].IndexOf(VALUE_AND_KEY) + searchstartIndx;

                if (found >= source.Length) return -1;

                if (source[found + 1] == VALUE_AND_KEY)
                    return GetCloseValueSeparatorIndx(source, found + 2);

                return found;
            }

        }

        internal class JSONSeparator
        {
            public readonly char Open;
            public readonly char Close;
            public readonly JType Type;
            public JSONSeparator(char open, char close, JType type)
            {
                Open = open;
                Close = close;
                Type = type;
            }
        }
    }
}
