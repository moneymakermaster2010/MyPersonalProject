// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Configures a relationship that can support cascade on delete functionality.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cascadable")]
    public abstract class CascadableNavigationPropertyConfiguration
    {
        private readonly NavigationPropertyConfiguration _navigationPropertyConfiguration;

        protected CascadableNavigationPropertyConfiguration(
            NavigationPropertyConfiguration navigationPropertyConfiguration)
        {
            Check.NotNull(navigationPropertyConfiguration, "navigationPropertyConfiguration");

            _navigationPropertyConfiguration = navigationPropertyConfiguration;
        }

        /// <summary>
        ///     Configures cascade delete to be on for the relationship.
        /// </summary>
        public void WillCascadeOnDelete()
        {
            WillCascadeOnDelete(true);
        }

        /// <summary>
        ///     Configures whether or not cascade delete is on for the relationship.
        /// </summary>
        /// <param name="value"> Value indicating if cascade delete is on or not. </param>
        public void WillCascadeOnDelete(bool value)
        {
            _navigationPropertyConfiguration.DeleteAction
                = value
                      ? OperationAction.Cascade
                      : OperationAction.None;
        }

        internal NavigationPropertyConfiguration NavigationPropertyConfiguration
        {
            get { return _navigationPropertyConfiguration; }
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
