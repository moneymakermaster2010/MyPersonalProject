﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System.Data.Entity;

    public class CeLoginsContext : LoginsContext
    {
        public CeLoginsContext()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CeLoginsContext>());
        }
    }
}
