﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.SimpleMigrationsModel
{
    using System.Data.Entity.Migrations;

    public class MigrateInitializerConfiguration : DbMigrationsConfiguration<MigrateInitializerContext>
    {
        public MigrateInitializerConfiguration()
        {
            MigrationsNamespace = "FunctionalTests.SimpleMigrationsModel";
        }

        protected override void Seed(MigrateInitializerContext context)
        {
            context.Blogs.AddOrUpdate(
                b => b.Name,
                new Blog
                    {
                        Name = "romiller.com"
                    },
                new Blog
                    {
                        Name = "blogs.msdn.com\adonet"
                    });
        }
    }
}
