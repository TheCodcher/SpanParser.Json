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
            public static readonly char BACKSLASH = '\\';
            public static readonly char DIGIT_DOT = '.';

            public static readonly char[] TRUE_WORD = "true".ToCharArray();
            public static readonly char[] FALSE_WORD = "false".ToCharArray();
            public static readonly char[] NULL_WORD = "null".ToCharArray();

            public static readonly JSONSeparator OBJECT = new JSONSeparator('{', '}', JType.Object);
            public static readonly JSONSeparator ARRAY = new JSONSeparator('[', ']', JType.Array);
            public static readonly JSONSeparator VALUE = new JSONSeparator(VALUE_AND_KEY, VALUE_AND_KEY, JType.Value);

            public static readonly ImmutableHashSet<char> MANAGE_CONSTRUCTS = new[] { '\r', ' ', '\n', '\a', '\b', '\f', '\t' }.ToImmutableHashSet();
            public static readonly ImmutableHashSet<char> MATERIAL_BEGANS_SIMBOLS = new[] { '-', 't', 'f', 'n' }.ToImmutableHashSet();
            public static readonly ImmutableHashSet<char> NUMERICS = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }.ToImmutableHashSet();

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
                    if (MANAGE_CONSTRUCTS.Contains(symbol)) continue;
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
                    if (MANAGE_CONSTRUCTS.Contains(symbol)) continue;

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
                    if (MANAGE_CONSTRUCTS.Contains(symbol)) continue;
                    if (MATERIAL_BEGANS_SIMBOLS.Contains(symbol) || NUMERICS.Contains(symbol))
                    {
                        type = JType.Material;
                        return i;
                    }

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

                if (source[found - 1] == BACKSLASH && GetBackSlashCount(source, found - 1) % 2 == 1)
                    return GetCloseValueSeparatorIndx(source, found + 1);

                return found;
            }
            private static int GetBackSlashCount(ReadOnlySpan<char> source, int firstAMONGUSIndx)
            {
                int counter = 0;
                for (int i = firstAMONGUSIndx; i >= 0; i--)
                {
                    if (source[i] == BACKSLASH)
                    {
                        counter++;
                    }
                    else
                    {
                        break;
                    }
                }
                return counter;
            }
            public static int GetMaterialValueEndIndx(this ReadOnlySpan<char> source, int searchstartIndx)
            {
                var simb = source[searchstartIndx];

                int CheckWord(char[] word, ReadOnlySpan<char> src)
                {
                    for (int i = 1; i < word.Length; i++)
                    {
                        if (src[searchstartIndx + i] == word[i]) continue;
                        return -1;
                    }
                    return searchstartIndx + word.Length - 1;
                }

                if (simb == NULL_WORD[0])
                {
                    return CheckWord(NULL_WORD, source);
                }

                if (simb == TRUE_WORD[0])
                {
                    return CheckWord(TRUE_WORD, source);
                }

                if (simb == FALSE_WORD[0])
                {
                    return CheckWord(FALSE_WORD, source);
                }

                if (simb == '-' || NUMERICS.Contains(simb))
                {
                    var dotFlag = false;
                    for (int i = searchstartIndx + 1; i < source.Length; i++)
                    {
                        simb = source[i];
                        if (NUMERICS.Contains(simb)) continue;
                        if (simb == DIGIT_DOT)
                        {
                            if (dotFlag)
                            {
                                return -1;
                            }
                            else
                            {
                                dotFlag = true;
                            }
                            continue;
                        }
                        return i - 1;
                    }
                }
                return -1;
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
