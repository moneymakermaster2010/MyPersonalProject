// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.TestHelpers;
    using Moq;
    using Xunit;

    public class DbConfigurationTests
    {
        public class SetConfiguration
        {
            [Fact]
            public void DbConfiguration_cannot_be_set_to_null()
            {
                Assert.Equal(
                    "configuration",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.SetConfiguration(null)).ParamName);
            }
        }

        public class AddDependencyResolver
        {
            [Fact]
            public void AddDependencyResolver_throws_if_given_a_null_resolver()
            {
                Assert.Equal(
                    "resolver",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDependencyResolver(null)).ParamName);
            }

            [Fact]
            public void AddDependencyResolver_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDependencyResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDependencyResolver(new Mock<IDbDependencyResolver>().Object)).Message);
            }

            [Fact]
            public void AddDependencyResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDependencyResolver(resolver);

                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(resolver, false));
            }
        }

        public class OnLockingConfiguration
        {
            [Fact]
            public void OnLockingConfiguration_throws_when_attempting_to_add_or_remove_a_null_handler()
            {
                Assert.Equal(
                    "value",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.OnLockingConfiguration += null).ParamName);

                Assert.Equal(
                    "value",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.OnLockingConfiguration -= null).ParamName);
            }
            
        }

        public class AddDbProviderServices
        {
            [Fact]
            public void AddDbProviderServices_throws_if_given_a_null_provider_or_bad_invariant_name()
            {
                Assert.Equal(
                    "provider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDbProviderServices("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderServices(null, new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderServices("", new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderServices(" ", new Mock<DbProviderServices>().Object)).Message);
            }

            [Fact]
            public void AddDbProviderServices_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var providerServices = new Mock<DbProviderServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDbProviderServices("900.FTW", providerServices);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(providerServices, "900.FTW"));
            }

            [Fact]
            public void AddDbProviderServices_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDbProviderServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDbProviderServices("Karl", new Mock<DbProviderServices>().Object)).Message);
            }
            
            [Fact]
            public void AddDbProviderServices_throws_if_no_ProviderInvariantNameAttribute()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var providerServices = new Mock<DbProviderServices>().Object;

                var configuration = new DbConfiguration(mockInternalConfiguration.Object);

                Assert.Equal(
                    Strings.DbProviderNameAttributeNotFound(providerServices.GetType()),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDbProviderServices(providerServices)).Message);
            }
        }

        public class AddDbProviderFactory
        {
            [Fact]
            public void AddDbProviderFactory_throws_if_given_a_null_provider_or_bad_invariant_name()
            {
                Assert.Equal(
                    "providerFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDbProviderFactory("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderFactory(null, new Mock<DbProviderFactory>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderFactory("", new Mock<DbProviderFactory>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddDbProviderFactory(" ", new Mock<DbProviderFactory>().Object)).Message);
            }

            [Fact]
            public void AddDbProviderFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var providerFactory = new Mock<DbProviderFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDbProviderFactory("920.FTW", providerFactory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(providerFactory, "920.FTW"));
                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(new InvariantNameResolver(providerFactory, "920.FTW"), false));
            }

            [Fact]
            public void AddDbProviderFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDbProviderFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDbProviderFactory("Karl", new Mock<DbProviderFactory>().Object)).Message);
            }
        }

        public class AddExecutionStrategy
        {
            [Fact]
            public void Throws_if_given_a_null_server_name_or_bad_invariant_name()
            {
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddExecutionStrategy<IExecutionStrategy>(null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(() => new Mock<IExecutionStrategy>().Object, null)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(() => new Mock<IExecutionStrategy>().Object, "")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(() => new Mock<IExecutionStrategy>().Object, " ")).Message);
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddExecutionStrategy<IExecutionStrategy>(null, "a")).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(null,() => new Mock<IExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy("", () => new Mock<IExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(" ", () => new Mock<IExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddExecutionStrategy("a", null)).ParamName);

                
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(null,() => new Mock<IExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy("", () => new Mock<IExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy(" ", () => new Mock<IExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy("a",() => new Mock<IExecutionStrategy>().Object, null)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy("a",() => new Mock<IExecutionStrategy>().Object, "")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddExecutionStrategy("a", () => new Mock<IExecutionStrategy>().Object, " ")).Message);
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddExecutionStrategy("a", null, "b")).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddExecutionStrategy"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddExecutionStrategy("a", () => new Mock<IExecutionStrategy>().Object, "b")).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var executionStrategy = new Func<IExecutionStrategy>(() => new Mock<IExecutionStrategy>().Object);

                new DbConfiguration(mockInternalConfiguration.Object).AddExecutionStrategy("a", executionStrategy, "b");

                mockInternalConfiguration.Verify(
                    m => m.AddDependencyResolver(It.IsAny<ExecutionStrategyResolver<IExecutionStrategy>>(), false));
            }

            [Fact]
            public void Throws_if_no_ProviderInvariantNameAttribute()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var executionStrategy = new Func<IExecutionStrategy>(() => new Mock<IExecutionStrategy>().Object);

                var configuration = new DbConfiguration(mockInternalConfiguration.Object);

                Assert.Equal(
                    Strings.DbProviderNameAttributeNotFound(typeof(IExecutionStrategy).FullName),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddExecutionStrategy(executionStrategy, "a")).Message);
            }
        }

        public class SetDefaultConnectionFactory : TestBase
        {
            [Fact]
            public void Setting_DefaultConnectionFactory_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "connectionFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetDefaultConnectionFactory(null)).ParamName);
            }

            [Fact]
            public void Setting_DefaultConnectionFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetDefaultConnectionFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDefaultConnectionFactory(new Mock<IDbConnectionFactory>().Object)).Message);
            }

            [Fact]
            public void SetDefaultConnectionFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var connectionFactory = new Mock<IDbConnectionFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDefaultConnectionFactory(connectionFactory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(connectionFactory, null));
            }

            [Fact]
            public void DefaultConnectionFactory_set_in_code_can_be_overriden_before_config_is_locked()
            {
                Assert.IsType<SqlConnectionFactory>(DbConfiguration.GetService<IDbConnectionFactory>());
                Assert.IsType<DefaultUnitTestsConnectionFactory>(FunctionalTestsConfiguration.OriginalConnectionFactories[0]);
            }
        }

        public class SetPluralizationService
        {
            [Fact]
            public void Setting_PluralizationService_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "pluralizationService",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetPluralizationService(null)).ParamName);
            }

            [Fact]
            public void Setting_PluralizationService_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetPluralizationService"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetPluralizationService(new Mock<IPluralizationService>().Object)).Message);
            }

            [Fact]
            public void SetPluralizationService_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var pluralizationService = new Mock<IPluralizationService>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetPluralizationService(pluralizationService);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(pluralizationService, null));
            }
        }

        public class DependencyResolver
        {
            [Fact]
            public void Default_IDbModelCacheKeyFactory_is_returned_by_default()
            {
                Assert.IsType<DefaultModelCacheKeyFactory>(DbConfiguration.GetService<IDbModelCacheKeyFactory>());
            }
        }

        public class SetDatabaseInitializer
        {
            [Fact]
            public void SetDatabaseInitializer_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetDatabaseInitializer"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDatabaseInitializer(new Mock<IDatabaseInitializer<DbContext>>().Object)).Message);
            }

            [Fact]
            public void SetDatabaseInitializer_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var initializer = new Mock<IDatabaseInitializer<DbContext>>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDatabaseInitializer(initializer);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(initializer, null));
            }

            [Fact]
            public void SetDatabaseInitializer_creates_null_initializer_when_given_null_argument()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();

                new DbConfiguration(mockInternalConfiguration.Object).SetDatabaseInitializer<DbContext>(null);

                mockInternalConfiguration.Verify(
                    m => m.RegisterSingleton<IDatabaseInitializer<DbContext>>(It.IsAny<NullDatabaseInitializer<DbContext>>(), null));
            }
        }

        public class AddMigrationSqlGenerator
        {
            [Fact]
            public void AddMigrationSqlGenerator_throws_if_given_a_null_generator_or_bad_invariant_name()
            {
                Assert.Equal(
                    "sqlGenerator",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddMigrationSqlGenerator("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddMigrationSqlGenerator(null, () => new Mock<MigrationSqlGenerator>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddMigrationSqlGenerator("", () => new Mock<MigrationSqlGenerator>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().AddMigrationSqlGenerator(" ", () => new Mock<MigrationSqlGenerator>().Object)).Message);
            }

            [Fact]
            public void AddMigrationSqlGenerator_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddMigrationSqlGenerator"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddMigrationSqlGenerator("Karl", () => new Mock<MigrationSqlGenerator>().Object)).Message);
            }

            [Fact]
            public void AddMigrationSqlGenerator_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var generator = new Func<MigrationSqlGenerator>(() => new Mock<MigrationSqlGenerator>().Object);

                new DbConfiguration(mockInternalConfiguration.Object).AddMigrationSqlGenerator("Karl", generator);

                mockInternalConfiguration.Verify(
                    m => m.AddDependencyResolver(It.IsAny<TransientDependencyResolver<MigrationSqlGenerator>>(), false));
            }

            [Fact]
            public void AddMigrationSqlGenerator_throws_if_no_ProviderInvariantNameAttribute()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var generator = new Func<MigrationSqlGenerator>(() => new Mock<MigrationSqlGenerator>().Object);

                var configuration = new DbConfiguration(mockInternalConfiguration.Object);

                Assert.Equal(
                    Strings.DbProviderNameAttributeNotFound(typeof(MigrationSqlGenerator)),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddMigrationSqlGenerator(generator)).Message);
            }
        }

        public class SetManifestTokenService
        {
            [Fact]
            public void SetManifestTokenService_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "service",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetManifestTokenService(null)).ParamName);
            }

            [Fact]
            public void SetManifestTokenService_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetManifestTokenService"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetManifestTokenService(new Mock<IManifestTokenService>().Object)).Message);
            }

            [Fact]
            public void SetManifestTokenService_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var service = new Mock<IManifestTokenService>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetManifestTokenService(service);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(service, null));
            }
        }

        public class SetProviderFactoryService
        {
            [Fact]
            public void SetProviderFactoryService_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "providerFactoryService",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetProviderFactoryService(null)).ParamName);
            }

            [Fact]
            public void SetProviderFactoryService_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetProviderFactoryService"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetProviderFactoryService(new Mock<IDbProviderFactoryService>().Object)).Message);
            }

            [Fact]
            public void SetProviderFactoryService_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var service = new Mock<IDbProviderFactoryService>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetProviderFactoryService(service);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(service, null));
            }
        }

        public class SetModelCacheKeyFactory
        {
            [Fact]
            public void SetModelCacheKeyFactory_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "keyFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetModelCacheKeyFactory(null)).ParamName);
            }

            [Fact]
            public void SetModelCacheKeyFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetModelCacheKeyFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetModelCacheKeyFactory(new Mock<IDbModelCacheKeyFactory>().Object)).Message);
            }

            [Fact]
            public void SetModelCacheKeyFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var factory = new Mock<IDbModelCacheKeyFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetModelCacheKeyFactory(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory, null));
            }
        }

        public class SetHistoryContextFactory
        {
            [Fact]
            public void Throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "historyContextFactory",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().SetHistoryContextFactory<DbMigrationsConfiguration>(null)).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetHistoryContextFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetHistoryContextFactory<DbMigrationsConfiguration>(
                            new Mock<IHistoryContextFactory>().Object)).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var factory = new Mock<IHistoryContextFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object)
                    .SetHistoryContextFactory<DbMigrationsConfiguration>(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory, typeof(DbMigrationsConfiguration)));
            }
        }

        public class SetSpatialProvider
        {
            [Fact]
            public void SetSpatialProvider_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "spatialProvider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetSpatialProvider(null)).ParamName);
            }

            [Fact]
            public void SetSpatialProvider_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetSpatialProvider"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetSpatialProvider(new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetSpatialProvider_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var provider = new Mock<DbSpatialServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetSpatialProvider(provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(provider, null));
            }
        }

        public class SetViewAssemblyCache
        {
            [Fact]
            public void SetViewAssemblyCache_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "cache",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetViewAssemblyCache(null)).ParamName);
            }

            [Fact]
            public void SetViewAssemblyCache_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetViewAssemblyCache"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetViewAssemblyCache(new Mock<IViewAssemblyCache>().Object)).Message);
            }

            [Fact]
            public void SetViewAssemblyCache_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>();
                var factory = new Mock<IViewAssemblyCache>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetViewAssemblyCache(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory, null));
            }
        }

        private static DbConfiguration CreatedLockedConfiguration()
        {
            var configuration = new DbConfiguration();
            configuration.InternalConfiguration.Lock();
            return configuration;
        }
    }
}
