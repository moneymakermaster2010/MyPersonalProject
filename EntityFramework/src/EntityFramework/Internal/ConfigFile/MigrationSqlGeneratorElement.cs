// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;

    internal class MigrationSqlGeneratorElement : ConfigurationElement
    {
        private const string TypeKey = "type";

        [ConfigurationProperty(TypeKey, IsRequired = true)]
        public string SqlGeneratorTypeName
        {
            get { return (string)this[TypeKey]; }
            set { this[TypeKey] = value; }
        }
    }
}
