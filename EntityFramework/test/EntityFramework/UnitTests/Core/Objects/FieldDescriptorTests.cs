﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using Xunit;

    public class FieldDescriptorTests
    {
        [Fact]
        public void GetValue_throws_for_null_argument()
        {
            Assert.Equal(
                "item",
                Assert.Throws<ArgumentNullException>(
                    () => new FieldDescriptor("foo").GetValue(null)).ParamName);
        }

        [Fact]
        public void SetValue_throws_for_null_argument()
        {
            Assert.Equal(
                "item",
                Assert.Throws<ArgumentNullException>(
                    () => new FieldDescriptor("foo").SetValue(null, new object())).ParamName);
        }
    }
}
