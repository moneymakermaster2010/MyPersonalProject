// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    public interface IDbModelCacheKeyFactory
    {
        IDbModelCacheKey Create(DbContext context);
    }
}
