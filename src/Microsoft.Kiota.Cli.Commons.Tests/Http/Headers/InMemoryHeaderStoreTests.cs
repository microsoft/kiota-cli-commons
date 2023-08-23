using System;
using System.Linq;
using Microsoft.Kiota.Cli.Commons.Http.Headers;
using Xunit;

namespace Microsoft.Kiota.Cli.Commons.Tests.Http.Headers;

public class InMemoryHeaderStoreTests
{
    public class SetHeadersFunction
    {
        [Fact]
        public void Stores_Single_Header()
        {
            var header = new [] {"sample=header"};
            var store = new InMemoryHeadersStore();
            store.SetHeaders(header);

            Assert.Single(store.GetHeaders());
            Assert.Single(store.GetHeaders().First().Value);
        }

        [Fact]
        public void Stores_Multiple_Headers()
        {
            var header = new [] {"sample=header", "sample2=header2", };
            var store = new InMemoryHeadersStore();
            store.SetHeaders(header);
            
            Assert.NotEmpty(store.GetHeaders());
            Assert.Equal(2, store.GetHeaders().Count());
            Assert.Single(store.GetHeaders().First().Value);
        }

        [Fact]
        public void Stores_Multiple_Headers_With_Matching_Key()
        {
            var header = new [] {"sample=header", "sample=header2", };
            var store = new InMemoryHeadersStore();
            store.SetHeaders(header);
            
            Assert.NotEmpty(store.GetHeaders());
            Assert.Single(store.GetHeaders());
            Assert.Equal(2, store.GetHeaders().First().Value.Count);
        }
        
        [Fact]
        public void Skips_Unparsed_Headers()
        {
            var header = new [] {"sample=", "test", string.Empty, };
            var store = new InMemoryHeadersStore();
            store.SetHeaders(header);
            
            Assert.Empty(store.GetHeaders());
        }
        
        [Fact]
        public void Clears_Existing_Headers()
        {
            var header = new [] {"sample=header", "sample2=header2", };
            var store = new InMemoryHeadersStore();
            store.SetHeaders(header);

            var result = store.SetHeaders(new[] { "sample3=header3" });
            
            Assert.Equal(2, result.Count());
            Assert.Single(store.GetHeaders());
            Assert.Single(store.GetHeaders().First().Value);
            
            result = store.SetHeaders(Array.Empty<string>());
            Assert.Single(result);
            Assert.Empty(store.GetHeaders());
        }
    }
    
    public class AddHeadersFunction
    {
        [Fact]
        public void Adds_To_Existing_Headers()
        {
            var header = new [] {"sample=header", "sample2=header2", };
            var store = new InMemoryHeadersStore();
            store.SetHeaders(header);

            Assert.Equal(2, store.GetHeaders().Count());
            Assert.True(store.AddHeaders(new[] { "sample3=header3" }));
            Assert.Equal(3, store.GetHeaders().Count());
            Assert.False(store.AddHeaders(Array.Empty<string>()));
            Assert.Equal(3, store.GetHeaders().Count());
        }
    }
    
    public class DrainFunction
    {
        [Fact]
        public void Clears_Existing_Headers()
        {
            var header = new [] {"sample=header", "sample2=header2", };
            var store = new InMemoryHeadersStore();
            store.AddHeaders(header);

            Assert.Equal(2, store.GetHeaders().Count());

            var result = store.Drain();

            Assert.Equal(2, result.Count());
            Assert.Empty(store.GetHeaders());
        }
    }
}
