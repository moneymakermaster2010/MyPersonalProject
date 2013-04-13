// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Linq;
    using Xunit;

    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void Each_should_iterate_sequence()
        {
            var i = 0;

            new[] { 1, 2, 3 }.Each(_ => i++);

            Assert.Equal(3, i);
        }

        [Fact]
        public void Join_should_return_joined_string()
        {
            Assert.Equal("1, 2, 3", new[] { 1, 2, 3 }.Join());
            Assert.Equal("1-2-3", new[] { 1, 2, 3 }.Join(separator: "-"));
            Assert.Equal("s, s, s", new[] { 1, 2, 3 }.Join(i => "s"));
            Assert.Equal("s, s", new[] { "1", null, "3" }.Join(i => "s"));
        }

        [Fact]
        public void Prepend_adds_item_to_beginning_of_sequence()
        {
            var result = new[] { 2, 3 }.Prepend(1);

            Assert.Equal(3, result.Count());
            Assert.Equal(1, result.First());
            Assert.Equal(3, result.Last());
        }

        [Fact]
        public void Append_adds_item_to_end_of_sequence()
        {
            var result = new[] { 1, 2 }.Append(3);

            Assert.Equal(3, result.Count());
            Assert.Equal(1, result.First());
            Assert.Equal(3, result.Last());
        }
    }
}
