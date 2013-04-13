// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal static class INamedDataModelItemExtensions
    {
        public static string UniquifyName(this IEnumerable<INamedDataModelItem> namedDataModelItems, string name)
        {
            DebugCheck.NotNull(namedDataModelItems);
            DebugCheck.NotEmpty(name);

            var uniqueName = name;
            var i = 0;

            while (namedDataModelItems.Any(n => string.Equals(n.Name, uniqueName, StringComparison.Ordinal)))
            {
                uniqueName = name + ++i;
            }

            return uniqueName;
        }
    }
}
