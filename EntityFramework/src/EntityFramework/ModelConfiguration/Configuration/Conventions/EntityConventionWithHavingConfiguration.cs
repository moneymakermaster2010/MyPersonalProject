﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Allows configuration to be performed for a lightweight convention based on
    ///     the entity types in a model and a captured value.
    /// </summary>
    /// <typeparam name="T"> Type of the captured value. </typeparam>
    public class EntityConventionWithHavingConfiguration<T>
        where T : class
    {
        private readonly ConventionsConfiguration _conventionsConfiguration;
        private readonly IEnumerable<Func<Type, bool>> _predicates;
        private readonly Func<Type, T> _capturingPredicate;

        internal EntityConventionWithHavingConfiguration(
            ConventionsConfiguration conventionsConfiguration,
            IEnumerable<Func<Type, bool>> predicates,
            Func<Type, T> capturingPredicate)
        {
            DebugCheck.NotNull(conventionsConfiguration);
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(capturingPredicate);

            _conventionsConfiguration = conventionsConfiguration;
            _predicates = predicates;
            _capturingPredicate = capturingPredicate;
        }

        internal ConventionsConfiguration ConventionsConfiguration
        {
            get { return _conventionsConfiguration; }
        }

        internal IEnumerable<Func<Type, bool>> Predicates
        {
            get { return _predicates; }
        }

        internal Func<Type, T> CapturingPredicate
        {
            get { return _capturingPredicate; }
        }

        /// <summary>
        ///     Allows configuration of the entity types that this convention applies to.
        /// </summary>
        /// <param name="entityConfigurationAction">
        ///     An action that performs configuration against a <see cref="LightweightEntityConfiguration" />
        ///     using a captured value.
        /// </param>
        public void Configure(Action<LightweightEntityConfiguration, T> entityConfigurationAction)
        {
            Check.NotNull(entityConfigurationAction, "entityConfigurationAction");

            _conventionsConfiguration.Add(
                new EntityConventionWithHaving<T>(
                    _predicates,
                    _capturingPredicate,
                    entityConfigurationAction));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
