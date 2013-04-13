// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Config;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using Xunit;

    public class ProviderConfigTests
    {
        public class TryGetMigrationSqlGeneratorFactory : AppConfigTestBase
        {
            [Fact]
            public void TryGetMigrationSqlGeneratorFactory_returns_factory_for_null_if_invariant_name_is_not_in_config()
            {
                Assert.Null(CreateAppConfig().Providers.TryGetMigrationSqlGeneratorFactory("System.Data.SqlClient")());
            }

            [Fact]
            public void TryGetMigrationSqlGeneratorFactory_returns_factory_for_null_if_no_migrations_SQL_generator_is_registered()
            {
                Assert.Null(
                    CreateAppConfig(
                        "Learning.To.Fly", typeof(ProviderServicesFactoryTests.FakeProviderWithPublicProperty).AssemblyQualifiedName)
                        .Providers
                        .TryGetMigrationSqlGeneratorFactory("Learning.To.Fly")());
            }

            public class MySqlGenerator : MigrationSqlGenerator
            {
                public override IEnumerable<MigrationStatement> Generate(
                    IEnumerable<MigrationOperation> migrationOperations,
                    string providerManifestToken)
                {
                    throw new NotImplementedException();
                }
            }

            [Fact]
            public void TryGetMigrationSqlGeneratorFactory_returns_generator_if_registered_in_config()
            {
                Assert.IsType<MySqlGenerator>(
                    CreateAppConfig(
                        "The.Hamster",
                        typeof(ProviderServicesFactoryTests.FakeProviderWithPublicProperty).AssemblyQualifiedName,
                        typeof(MySqlGenerator).AssemblyQualifiedName)
                        .Providers
                        .TryGetMigrationSqlGeneratorFactory("The.Hamster")());
            }

            [Fact]
            public void TryGetMigrationSqlGeneratorFactory_throws_if_type_cannot_be_loaded()
            {
                Assert.Equal(
                    Strings.SqlGeneratorTypeMissing("Ben.Collins", "The.Stig"),
                    Assert.Throws<InvalidOperationException>(
                        () => CreateAppConfig(
                            "The.Stig",
                            typeof(ProviderServicesFactoryTests.FakeProviderWithPublicProperty).AssemblyQualifiedName,
                            "Ben.Collins")
                                  .Providers
                                  .TryGetMigrationSqlGeneratorFactory("The.Stig")).Message);
            }

            [Fact]
            public void TryGetMigrationSqlGeneratorFactory_throws_if_type_cannot_be_used()
            {
                Assert.Equal(
                    Strings.CreateInstance_BadSqlGeneratorType(typeof(object).ToString(), typeof(MigrationSqlGenerator).ToString()),
                    Assert.Throws<InvalidOperationException>(
                        () => CreateAppConfig(
                            "Jezza",
                            typeof(ProviderServicesFactoryTests.FakeProviderWithPublicProperty).AssemblyQualifiedName,
                            typeof(object).AssemblyQualifiedName)
                                  .Providers
                                  .TryGetMigrationSqlGeneratorFactory("Jezza")()).Message);
            }
        }

        public class TryGetDbProviderServices : AppConfigTestBase
        {
            [Fact]
            public void TryGetDbProviderServices_returns_null_if_invariant_name_is_not_in_config()
            {
                Assert.Null(CreateAppConfig().Providers.TryGetDbProviderServices("System.Data.SqlClient"));
            }

            [Fact]
            public void TryGetDbProviderServices_returns_provider_if_invariant_name_is_in_config()
            {
                Assert.Same(
                    ProviderServicesFactoryTests.FakeProviderWithPublicProperty.Instance,
                    CreateAppConfig(
                        "Learning.To.Fly", typeof(ProviderServicesFactoryTests.FakeProviderWithPublicProperty).AssemblyQualifiedName)
                        .Providers
                        .TryGetDbProviderServices("Learning.To.Fly"));
            }
        }

        public class TryGetSpatialProvider : AppConfigTestBase
        {
            [Fact]
            public void TryGetSpatialProvider_returns_null_if_spatialProviderType_is_not_in_config()
            {
                Assert.Null(CreateAppConfig().Providers.TryGetSpatialProvider());
            }

            [Fact]
            public void TryGetSpatialProvider_returns_provider_instance()
            {
                Assert.Same(
                    SqlSpatialServices.Instance,
                    CreateAppConfigWithSpatial(typeof(SqlSpatialServices).AssemblyQualifiedName)
                        .Providers
                        .TryGetSpatialProvider());
            }

            [Fact]
            public void TryLoadFromConfig_throws_if_type_cannot_be_loaded()
            {
                var providerConfig = CreateAppConfigWithSpatial("I.Is.Not.A.Type").Providers;

                Assert.Equal(
                    Strings.DbSpatialServicesTypeNotFound("I.Is.Not.A.Type"),
                    Assert.Throws<InvalidOperationException>(() => providerConfig.TryGetSpatialProvider()).Message);
            }

            [Fact]
            public void TryLoadFromConfig_throws_if_type_does_not_have_Instance_member()
            {
                var providerConfig = CreateAppConfigWithSpatial(typeof(Random).AssemblyQualifiedName).Providers;

                Assert.Equal(
                    Strings.DbSpatialServices_InstanceMissing(typeof(Random).AssemblyQualifiedName),
                    Assert.Throws<InvalidOperationException>(() => providerConfig.TryGetSpatialProvider()).Message);
            }

            [Fact]
            public void TryLoadFromConfig_throws_if_Instance_member_returns_wrong_thing()
            {
                var providerConfig = CreateAppConfigWithSpatial(typeof(SqlProviderServices).AssemblyQualifiedName).Providers;

                Assert.Equal(
                    Strings.DbSpatialServices_NotDbSpatialServices(typeof(SqlProviderServices).AssemblyQualifiedName),
                    Assert.Throws<InvalidOperationException>(() => providerConfig.TryGetSpatialProvider()).Message);
            }
        }
    }
}
