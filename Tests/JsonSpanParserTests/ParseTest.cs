using NUnit.Framework;
using SpanParser.Json;
using System.IO;
using System;

namespace JsonSpanParserTests
{
    public class ParseTest
    {
        JsonMemoryContext context;
        char[] json;
        [SetUp]
        public void Setup()
        {
            context = new JsonMemoryContext();
            json = File.ReadAllText(@"C:\Users\Molni\source\repos\JsonSpanParser\Tests\message.json").ToCharArray();
        }

        [Test]
        public void Test1()
        {
            try
            {
                var result = JSpan.Parse(json, context);
            }
            catch(Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Pass();
        }
    }
}