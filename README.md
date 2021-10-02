# SpanParser.Json<br>
<img src="https://github.com/thecodcher/SpanParser.Json/workflows/SpanParser.Json-Deploy-Package/badge.svg?branch=master">

# Contents

+ [Discription](#Discription)
+ [Getting started](#Getting_started)
+ [Pooling Array](#Pooling_Array)
+ [Linq Api (Exemple)](#Linq_Api)

# <a name="Discription"></a> Discription

For a long time there was a problem of string pooling, since string were allocated and reused completely uncontrollably, it was impossible to allocate memory for storing them and independently control their lifetime. More recently, new types have been added to C#, such as Span\<T>. These structures are designed to work with memory and arrays. In fact, they store a reference to an arbitrary piece of memory. The appearance of these structures made it much easier to optimize the work with strings. For example, Span\<Char> can be placed both in heap and on a stack using a stackalloc keyword. It is on the principles of memory reuse and object allocation for parsing strictly on the stack that this library is built. It allows you to use the usual Linq from NewtonSoft.Json for parsing, however, if used correctly, it will not allocate memory in the heap, which will minimize the load on the GC and optimize your project.

# <a name="Getting_started"></a> Getting started

1. #### Install package from git (Package Manager)
```
Install-Package SpanParser.Json -source https://nuget.pkg.github.com/TheCodcher/index.json
```
*don't forget login with your github token*

2. #### Declare namespace
```cs 
using SpanParser.Json; 
```

3. #### Create your pooling memory or implements interface for custon realization
```cs
IJsonMemoryContext context = new JsonMemoryContext();
```
*remember, JsonMemoryContext isn't concurrent. Monitor access to it while parsing*

4. #### Create parsing by fabric method
```cs
JSpan parseStruct = JSpan.Parse(json, context);
```

5. #### Use api to parse
```cs
if (parseStruct.IsObject)
{
    JSpan jsonObj = parseStruct["key"];
}
if (parseStruct.IsArray)
{
    JSpan jsonArr = parseStruct[0];
}
```
*Just exemple*

6. #### Get result
```cs
string valueContainedJson = parseStruct.ToString();
string jsonString = parseStruct.ToJsonString();
ReadOnlySpan<char> valueContainedJsonWithoutAlloc = parseStruct.AsSpan();
ReadOnlySpan<char> jsonWithoutAlloc = parseStruct.AsJsonSpan();
```
*remember, using AsSpan() or AsSpanJson(), ReadOnlySpan\<char> refers to the same memory that json was written to when calling the factory method for the first time*

7. #### Reuse or clear pooling memory
```cs
var concrateContext = context as JsonMemoryContext;
concrateContext.Collect();
//or
concrateContext.Compress();
//or
concrateContext.Release();
```
*more information about api below*

# <a name="Pooling_Array"></a> Pooling Array
The pooling memory system is represented by two interfaces: IJsonMemoryContext and IMemoryHolder\<JArrayNode>. Its require to implement basic memory work such as writing and reading by index. This is necessary, since not all parsing processes can be stored on a stack. The library provides its own implementation: JsonMemoryContext and PoolArray\<T>, which holds on the heap two non-concurrent arrays of structures required for parsing. These arrays can be reused as soon as the result of parsing is received in order to take off the load from GC.
#### Methods
+ **Collect** - allows you to release memory and reset buffer to initial state
> Create a new buffer in its initial state and allows the GC to collect the old one. 
+ **Compress** - allows you to сompress the used memory to the filled length
> Creates a new buffer the size of the current data, and the old one remains on the GC. This allows the actual size of the occupied memory to be reduced and actualized.
+ **Release** - allows you to overwrite memory without utilizing it and not allocating new
> Does not change the size of the buffer, but only indicates that it can be completely overwritten. It is recommended to use this method between pooling iterations.

# <a name="Linq_Api"></a> Linq Api
### *Exemple*
```cs
IJsonMemoryContext context = new JsonMemoryContext();

const string json = "{ \"ArrayKey\": [ \"first\", \"second\" ], \"ValueKey\": 1.23 }";

JSpan parseStruct = JSpan.Parse(json, context);
string toStringJson = parseStruct.ToJsonString(); // "{ "ArrayKey": [ "first", "second" ], "ValueKey": 1.23 }"
string toString = parseStruct.ToString(); // "{ "ArrayKey": [ "first", "second" ], "ValueKey": 1.23 }"

JSpan arrayParse = parseStruct["ArrayKey"];
string toStringArrJson = arrayParse.ToJsonString(); // [ "first", "second" ]
string toStringArr = arrayParse.ToString(); // [ "first", "second" ]

JSpan firstArray = arrayParse[0];
string firstJson = firstArray.ToJsonString(); // "first"
string firstValue = firstArray.ToString(); // first

string materialJson = parseStruct["ValueKey"].ToJsonString(); // 1.23
string material = parseStruct["ValueKey"].ToString(); // 1.23

JSpanEnumerator jSpanEnumerator = arrayParse.GetEnumerator(); // introduces IEnumerator. don`t use IEnumerable for this structure
while (jSpanEnumerator.MoveNext()) // use for enumeration. don't forget, that JSpanEnumerator present linked list
{
    JSpan jSpanItem = jSpanEnumerator.Current;
    Console.WriteLine(jSpanItem.ToString()); //first and second
}

jSpanEnumerator.ResetToLast(); // set enumerator to last position
while (jSpanEnumerator.MovePrevious()) // use for enumeration from end to beginning
{
    JSpan jSpanItem = jSpanEnumerator.Current;
    Console.WriteLine(jSpanItem.ToString()); // second and first
}

jSpanEnumerator.ResetToFirst(); // set enumerator to first position

int count = jSpanEnumerator.Count(); // 2

JSpan first = jSpanEnumerator.First(); // contain "first"
JSpan last = jSpanEnumerator.Last(); // contain "second"

double value = parseStruct["ValueKey"].Deserialize<double>(); //1.23
```
