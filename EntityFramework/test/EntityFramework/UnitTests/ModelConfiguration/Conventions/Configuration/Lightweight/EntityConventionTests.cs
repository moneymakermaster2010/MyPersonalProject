﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Linq;
    using Xunit;

    public class EntityConventionTests
    {
        [Fact]
        public void Apply_invokes_action_when_no_predicates()
        {
            var actionInvoked = false;
            var convention = new EntityConvention(
                Enumerable.Empty<Func<Type, bool>>(),
                c => actionInvoked = true);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_invokes_action_when_single_predicate_true()
        {
            var actionInvoked = false;
            var convention = new EntityConvention(
                new Func<Type, bool>[] { t => true },
                c => actionInvoked = true);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_invokes_action_when_all_predicates_true()
        {
            var actionInvoked = false;
            var convention = new EntityConvention(
                new Func<Type, bool>[]
                    {
                        t => true,
                        t => true
                    },
                c => actionInvoked = true);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_does_not_invoke_action_when_single_predicate_false()
        {
            var actionInvoked = false;
            var convention = new EntityConvention(
                new Func<Type, bool>[] { t => false },
                c => actionInvoked = true);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.False(actionInvoked);
        }

        [Fact]
        public void Apply_does_not_invoke_action_and_short_circuts_when_first_predicate_false()
        {
            var lastPredicateInvoked = false;
            var actionInvoked = false;
            var convention = new EntityConvention(
                new Func<Type, bool>[]
                    {
                        t => false,
                        t => lastPredicateInvoked = true
                    },
                c => actionInvoked = true);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.False(lastPredicateInvoked);
            Assert.False(actionInvoked);
        }

        [Fact]
        public void Apply_does_not_invoke_action_when_last_predicate_false()
        {
            var actionInvoked = false;
            var convention = new EntityConvention(
                new Func<Type, bool>[]
                    {
                        t => true,
                        t => false
                    },
                c => actionInvoked = true);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.False(actionInvoked);
        }
    }
}
