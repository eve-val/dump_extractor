using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ExtractorLib;

namespace ExtractorLibTest
{
    [TestFixture]
    public class YAMLConversionTest
    {
        [Test]
        public void NullConversionTest()
        {
            object testValue = System.DBNull.Value;
            Assert.AreEqual(null, YAMLConversion.ConvertValue((dynamic) testValue));
        }

        [Test]
        public void StringConversionTest()
        {
            object testValue = "<br /> test value \r\n with newline.";
            Assert.AreEqual("\"<br /> test value \\n with newline.\"", YAMLConversion.ConvertValue((dynamic)testValue));
        }

        [Test]
        public void IntConversionTest()
        {
            object testValue = 0;
            Assert.AreEqual("0", YAMLConversion.ConvertValue((dynamic)testValue));
        }

        [Test]
        public void FloatConversionTest()
        {
            object testValue = 0.0;
            Assert.AreEqual("!!float 0", YAMLConversion.ConvertValue((dynamic)testValue));
        }

        [Test]
        public void BoolConversionTest()
        {
            object testValue = true;
            Assert.AreEqual("Yes", YAMLConversion.ConvertValue((dynamic)testValue));
        }
    }
}
