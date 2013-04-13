// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.Sets
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    internal static class V2ConventionSet
    {
        private static readonly IConvention[] _conventions;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static V2ConventionSet()
        {
            var conventions = new List<IConvention>(V1ConventionSet.Conventions);

            var columnOrderingConventionIndex
                = conventions.FindIndex(c => c.GetType() == typeof(ColumnOrderingConvention));

            Debug.Assert(columnOrderingConventionIndex != -1);

            conventions[columnOrderingConventionIndex] = new ColumnOrderingConventionStrict();

            _conventions = conventions.ToArray();
        }

        public static IConvention[] Conventions
        {
            get { return _conventions; }
        }
    }
}
