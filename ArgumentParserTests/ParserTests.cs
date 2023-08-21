using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ArgumentParser.Tests
{
    [TestClass()]
    public class ParserTests
    {
        [TestMethod()]
        public void Constructor_ThrowsException_WhenProvidingDuplicateParameters()
        {
            Parameter[] duplicateParameters = new Parameter[]
            {
                Parameter.Flag("verbose", "-v", "--verbose"),
                Parameter.Flag("verbose", "-v", "--verbose"),
            };

            Assert.ThrowsException<ArgumentException>(() => new Parser(duplicateParameters));
        }

        [TestMethod()]
        public void Map_ThrowsException_WhenNotProvidedRequiredParameter()
        {
            Parser p = new Parser(Parameter.Required(""));
            Assert.ThrowsException<InvalidOperationException>(() => p.Map(new string[0]));
        }

        [TestMethod()]
        public void Value_ProvidesDefaultValues_WhenNotMapped()
        {
            Parser p = new Parser(
                Parameter.Flag("s0", "-s", "--switch"),
                Parameter.Named("p0", "pe0", s => s, "-p0", "--parameter0"),
                Parameter.Required("req0", s => int.Parse(s)),
                Parameter.Optional("opt0", 12, s => int.Parse(s)),
                Parameter.Optional("opt1", 0.12, s => double.Parse(s)),
                Parameter.Optional("opt2", "Bla")
            );
            List<string> unmapped = p.Map(new string[] { "255" });

            Assert.AreEqual(false, p.Value<object>("s0"));
            Assert.AreEqual("pe0", p.Value<string>("p0"));
            Assert.AreEqual(255, p.Value<int>("req0"));
            Assert.AreEqual(12, p.Value<int>("opt0"));
            Assert.AreEqual(0.12, p.Value<double>("opt1"));
            Assert.AreEqual("Bla", p.Value<string>("opt2"));
            CollectionAssert.AreEqual(new string[0], unmapped);
        }

        [TestMethod()]
        public void Map_IgnoresDuplicateFlags()
        {
            Parser p = new Parser(Parameter.Flag("switch", "-s", "--switch"));
            p.Map(new string[] { "-s", "--switch" });
            Assert.AreEqual(true, p.Value<bool>("switch"), "-s");
        }

        [TestMethod()]
        public void Map_IgnoresDuplicateNamed()
        {
            Parser p = new Parser(Parameter.Named("n", 0, i => int.Parse(i), "-s", "--switch"));
            p.Map(new string[] { "-s", "1", "--switch", "2" });
            Assert.AreEqual(1, p.Value<int>("-s"), "-s");
        }

        [TestMethod()]
        public void Map_MapsFlagParameter()
        {
            Parser p = new Parser(Parameter.Flag("switch", "-s", "--switch"));
            p.Map(new string[] { "hello world" });
            Assert.AreEqual(false, p.Value<bool>("switch"));
            p.Map(new string[] { "switch" });
            Assert.AreEqual(true, p.Value<bool>("switch"), "-s");
            p.Map(new string[] { "-s" });
            Assert.AreEqual(true, p.Value<bool>("switch"), "-s");
            p.Map(new string[] { "--switch" });
            Assert.AreEqual(true, p.Value<bool>("switch"), "--switch");
        }

        [TestMethod()]
        public void MapStrict_ReturnsFalseAndProvidesText_WhenProvidedUnmappedArguments()
        {
            Assert.IsFalse((new Parser()).MapStrict(new string[] { "-s" }, out string text));
            Assert.IsNotNull(text);
        }

        [TestMethod()]
        public void Value_ParsesValues_WhenMapped()
        {
            Parser p = new Parser(
                Parameter.Flag("s0", "-s", "--switch"),
                Parameter.Named("p0", "pe0", s => s, "-p0", "--parameter0"),
                Parameter.Required("req0", s => int.Parse(s)),
                Parameter.Optional("opt0", 12, s => int.Parse(s)),
                Parameter.Optional("opt1", 0.12, s => double.Parse(s)),
                Parameter.Optional("opt2", "Bla")
            );

            List<string> unmapped = p.Map(new string[] {
                "-s",
                "-p0", "hallo welt",
                "255",
                "55",
                "0.166",
                "auch optional",
                "das da ist schon unmatched",
                "das auch",
            });

            Assert.AreEqual(true, p.Value<bool>("s0"));
            Assert.AreEqual("hallo welt", p.Value<string>("p0"));
            Assert.AreEqual(255, p.Value<int>("req0"));
            Assert.AreEqual(55, p.Value<int>("opt0"));
            Assert.AreEqual(0.166, p.Value<double>("opt1"));
            Assert.AreEqual("auch optional", p.Value<string>("opt2"));
            CollectionAssert.AreEqual(new string[] { "das da ist schon unmatched", "das auch" }, unmapped);
        }
    }
}