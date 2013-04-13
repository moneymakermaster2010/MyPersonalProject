// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Diagnostics.CodeAnalysis;

    public abstract class LengthColumnConfiguration : PrimitiveColumnConfiguration
    {
        internal LengthColumnConfiguration(LengthPropertyConfiguration configuration)
            : base(configuration)
        {
        }

        internal new LengthPropertyConfiguration Configuration
        {
            get { return (LengthPropertyConfiguration)base.Configuration; }
        }

        public LengthColumnConfiguration IsMaxLength()
        {
            Configuration.IsMaxLength = true;
            Configuration.MaxLength = null;

            return this;
        }

        public LengthColumnConfiguration HasMaxLength(int? value)
        {
            Configuration.MaxLength = value;
            Configuration.IsMaxLength = null;

            return this;
        }

        public LengthColumnConfiguration IsFixedLength()
        {
            Configuration.IsFixedLength = true;

            return this;
        }

        public LengthColumnConfiguration IsVariableLength()
        {
            Configuration.IsFixedLength = false;

            return this;
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
