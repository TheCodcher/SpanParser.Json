using NUnit.Framework;
using SpanParser.Json;
using System.IO;
using System;

namespace JsonSpanParserTests
{
    public class ParseTest
    {
        JsonMemoryContext context;

        const string FilePath = "";

        char[] json;
        [SetUp]
        public void Setup()
        {
            context = new JsonMemoryContext();
            json = File.ReadAllText(FilePath).ToCharArray();
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