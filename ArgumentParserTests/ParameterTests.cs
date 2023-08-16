using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ArgumentParser.Tests
{
    [TestClass()]
    public class ParameterTests
    {
        [TestMethod()]
        public void Name_IsAssignedInConstructor()
        {
            string name = "This is a name";
            Parameter p = Parameter.Named(name, _ => null, new object(), new string[0]);

            Assert.AreEqual(name, p.Name, "Name");
        }

        [TestMethod()]
        public void Aliases_IsAssignedInConstructor()
        {
            string[] aliases = new[] { "a", "b", "c", "d" };
            Parameter p = Parameter.Named("", _ => null, new object(), aliases);

            CollectionAssert.AreEqual(new[] { p.Name }.Concat(aliases).ToArray(), p.Aliases, "Aliases");
        }

        [TestMethod()]
        public void ParseMethod_DefaultsToIdentity()
        {
            Parameter p = Parameter.Named("", null, new object(), new string[0]);

            p.StringValue = "this is a string";
            object output = p.Value<object>();

            Assert.IsInstanceOfType(output, typeof(string), "Value type");
            Assert.AreEqual(p.StringValue, output, "Parsed value");
        }

        [TestMethod()]
        public void Value_CallsParseMethod_WhenStringValueIsNotNull()
        {
            object parseValue = new object();
            bool parseCalled = false;
            Parameter p = Parameter.Named("", _ => { parseCalled = true; return parseValue; }, new object(), new string[0]);

            p.StringValue = "not null";
            object output = p.Value<object>();

            Assert.AreEqual(parseValue, output, "Parsed value");
            Assert.IsTrue(parseCalled, "Parse called");
        }

        [TestMethod()]
        public void Value_IgnoresParseMethodAndUsesDefaultValue_WhenStringValueIsNull()
        {
            object defaultValue = new object();
            bool parseCalled = false;
            Parameter p = Parameter.Named("", _ => { parseCalled = true; return null; }, defaultValue, new string[0]);

            Assert.IsNull(p.StringValue, "StringValue");
            object output = p.Value<object>();

            Assert.AreEqual(defaultValue, output, "Parsed value");
            Assert.IsFalse(parseCalled, "Parse called");
        }

        [TestMethod()]
        public void FlagParameter_HasFixedParseMethod()
        {
            Parameter p = Parameter.Flag("");

            Assert.IsNull(p.StringValue, "StringValue");
            Assert.AreEqual(false, p.Value<object>(), "Null parsed");

            p.StringValue = "Not null";
            Assert.AreEqual(true, p.Value<object>(), "Not null parsed");
        }
    }
}