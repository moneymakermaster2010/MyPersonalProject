﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    internal class MigrationAssembly
    {
        public static string CreateMigrationId(string migrationName)
        {
            DebugCheck.NotEmpty(migrationName);

            return UtcNowGenerator.UtcNowAsMigrationIdTimestamp() + "_" + migrationName;
        }

        public static string CreateBootstrapMigrationId()
        {
            return new String('0', 15) + "_" + Strings.BootstrapMigration;
        }

        private readonly IList<IMigrationMetadata> _migrations;

        protected MigrationAssembly()
        {
        }

        public MigrationAssembly(Assembly migrationsAssembly, string migrationsNamespace)
        {
            DebugCheck.NotNull(migrationsAssembly);

            _migrations
                = (from t in migrationsAssembly.GetAccessibleTypes()
                   where t.IsSubclassOf(typeof(DbMigration))
                         && typeof(IMigrationMetadata).IsAssignableFrom(t)
                         && t.GetConstructor(Type.EmptyTypes) != null
                         && !t.IsAbstract
                         && !t.IsGenericType
                         && t.Namespace == migrationsNamespace
                   select (IMigrationMetadata)Activator.CreateInstance(t))
                    .Where(mm => !string.IsNullOrWhiteSpace(mm.Id) && mm.Id.IsValidMigrationId())
                    .OrderBy(mm => mm.Id)
                    .ToList();
        }

        public virtual IEnumerable<string> MigrationIds
        {
            get { return _migrations.Select(t => t.Id).ToList(); }
        }

        public virtual string UniquifyName(string migrationName)
        {
            var uniqueName = migrationName;
            var counter = 1;

            while (_migrations.Any(m => string.Equals(m.GetType().Name, uniqueName, StringComparison.Ordinal)))
            {
                uniqueName = migrationName + counter++;
            }

            return uniqueName;
        }

        public virtual DbMigration GetMigration(string migrationId)
        {
            DebugCheck.NotEmpty(migrationId);

            var migration
                = (DbMigration)_migrations
                                   .SingleOrDefault(m => string.Equals(m.Id, migrationId, StringComparison.Ordinal));

            if (migration != null)
            {
                migration.Reset();
            }

            return migration;
        }
    }
}
