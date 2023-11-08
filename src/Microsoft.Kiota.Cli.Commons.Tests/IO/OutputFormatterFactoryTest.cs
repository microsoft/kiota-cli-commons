using System;
using Microsoft.Kiota.Cli.Commons.IO;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.IO;

public class OutputFormatterFactoryTest
{
    public class GetFormatterFunction_Should
    {
        [Theory]
        [InlineData((FormatterType)20)]
        public void ThrowException_On_Invalid_Enum_FormatterType(FormatterType formatterType)
        {
            var factory = new OutputFormatterFactory();

            Assert.Throws<ArgumentOutOfRangeException>(() => factory.GetFormatter(formatterType));
        }

        [Theory]
        [InlineData("invalid")]
        public void ThrowException_On_Invalid_String_FormatterType(string formatterType)
        {
            var factory = new OutputFormatterFactory();

            Assert.Throws<ArgumentOutOfRangeException>(() => factory.GetFormatter(formatterType));
        }

        [Theory]
        [InlineData(FormatterType.RAW_JSON, typeof(JsonOutputFormatter))]
        [InlineData(FormatterType.JSON, typeof(JsonOutputFormatter))]
        [InlineData(FormatterType.TABLE, typeof(TableOutputFormatter))]
        [InlineData(FormatterType.TEXT, typeof(TextOutputFormatter))]
        [InlineData(FormatterType.NONE, typeof(NoneOutputFormatter))]
        public void Return_OutputFormatter_On_FormatterType(FormatterType formatterType, Type expectedType)
        {
            var factory = new OutputFormatterFactory();

            var formatter = factory.GetFormatter(formatterType);

            Assert.NotNull(formatter);
            Assert.IsType(expectedType, formatter);
        }

        [Theory]
        [InlineData("raw_json", typeof(JsonOutputFormatter))]
        [InlineData("RAW_JSON", typeof(JsonOutputFormatter))]
        [InlineData("JSON", typeof(JsonOutputFormatter))]
        [InlineData("table", typeof(TableOutputFormatter))]
        [InlineData("TABLE", typeof(TableOutputFormatter))]
        [InlineData("text", typeof(TextOutputFormatter))]
        [InlineData("TEXT", typeof(TextOutputFormatter))]
        [InlineData("none", typeof(NoneOutputFormatter))]
        [InlineData("NONE", typeof(NoneOutputFormatter))]
        public void Return_OutputFormatter_On_FormatterType_String(string formatterType, Type expectedType)
        {
            var factory = new OutputFormatterFactory();

            var formatter = factory.GetFormatter(formatterType);

            Assert.NotNull(formatter);
            Assert.Equal(expectedType, formatter.GetType());
        }
    }
}
