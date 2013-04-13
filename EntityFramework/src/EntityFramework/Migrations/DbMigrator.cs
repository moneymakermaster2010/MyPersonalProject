// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    /// <summary>
    ///     DbMigrator is used to apply existing migrations to a database.
    ///     DbMigrator can be used to upgrade and downgrade to any given migration.
    ///     To scaffold migrations based on changes to your model use <see cref="Design.MigrationScaffolder" />
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class DbMigrator : MigratorBase
    {
        /// <summary>
        ///     Migration Id representing the state of the database before any migrations are applied.
        /// </summary>
        public const string InitialDatabase = "0";

        private static readonly MethodInfo _setInitializerMethod
            = typeof(Database).GetMethod("SetInitializer");

        private readonly Lazy<XDocument> _emptyModel;
        private readonly DbMigrationsConfiguration _configuration;
        private readonly XDocument _currentModel;
        private readonly XDocument _currentHistoryModel;
        private readonly XDocument _initialHistoryModel;
        private readonly DbProviderFactory _providerFactory;
        private readonly HistoryRepository _historyRepository;
        private readonly MigrationAssembly _migrationAssembly;
        private readonly DbContextInfo _usersContextInfo;
        private readonly EdmModelDiffer _modelDiffer;
        private readonly bool _calledByCreateDatabase;
        private readonly string _providerManifestToken;
        private readonly string _targetDatabase;
        private readonly string _legacyContextKey;

        private MigrationSqlGenerator _sqlGenerator;

        private bool _emptyMigrationNeeded;

        /// <summary>
        ///     Initializes a new instance of the DbMigrator class.
        /// </summary>
        /// <param name="configuration"> Configuration to be used for the migration process. </param>
        public DbMigrator(DbMigrationsConfiguration configuration)
            : this(configuration, null)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(configuration.ContextType, "configuration.ContextType");
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal DbMigrator(DbMigrationsConfiguration configuration, DbContext usersContext)
            : base(null)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(configuration.ContextType, "configuration.ContextType");

            _configuration = configuration;
            _calledByCreateDatabase = usersContext != null;

            // If DbContext CreateDatabase is using Migrations then the user has not opted out of initializers
            // and if we disable the initializer here then future calls to Initialize the database (for this or
            // a different connection) will fail. So only disable the initializer if Migrations are being used
            // explicitly.
            if (usersContext == null)
            {
                DisableInitializer(_configuration.ContextType);
            }

            if (_calledByCreateDatabase)
            {
                _usersContextInfo = new DbContextInfo(usersContext);
            }
            else
            {
                _usersContextInfo
                    = configuration.TargetDatabase == null
                          ? new DbContextInfo(configuration.ContextType)
                          : new DbContextInfo(configuration.ContextType, configuration.TargetDatabase);

                if (!_usersContextInfo.IsConstructible)
                {
                    throw Error.ContextNotConstructible(configuration.ContextType);
                }
            }

            _modelDiffer = _configuration.ModelDiffer;

            var context = usersContext ?? _usersContextInfo.CreateInstance();
            try
            {
                _migrationAssembly
                    = new MigrationAssembly(_configuration.MigrationsAssembly, _configuration.MigrationsNamespace);

                _currentModel = context.GetModel();

                var connection = context.Database.Connection;

                _providerFactory = DbProviderServices.GetProviderFactory(connection);

                var defaultSchema = context.InternalContext.DefaultSchema;
                var historySchemas = GetHistorySchemas();

                if (!string.IsNullOrWhiteSpace(defaultSchema))
                {
                    historySchemas
                        = historySchemas.Concat(new[] { defaultSchema });
                }

                _historyRepository
                    = new HistoryRepository(
                        _usersContextInfo.ConnectionString,
                        _providerFactory,
                        _configuration.ContextKey,
                        _configuration.CommandTimeout,
                        historySchemas,
                        _migrationAssembly.MigrationIds.Any()
                            ? _configuration.HistoryContextFactory
                            : null);

                _providerManifestToken
                    = context.InternalContext.ModelProviderInfo != null
                          ? context.InternalContext.ModelProviderInfo.ProviderManifestToken
                          : DbConfiguration
                                .GetService<IManifestTokenService>()
                                .GetProviderManifestToken(connection);

                _targetDatabase
                    = Strings.LoggingTargetDatabaseFormat(
                        connection.DataSource,
                        connection.Database,
                        _usersContextInfo.ConnectionProviderName,
                        _usersContextInfo.ConnectionStringOrigin == DbConnectionStringOrigin.DbContextInfo
                            ? Strings.LoggingExplicit
                            : _usersContextInfo.ConnectionStringOrigin.ToString());

                _legacyContextKey = context.InternalContext.ContextKey;

                _currentHistoryModel = GetCurrentHistoryModel(defaultSchema);
                _initialHistoryModel = GetInitialHistoryModel();
                _emptyModel = GetEmptyModel();

                AttachHistoryModel(_currentModel);
            }
            finally
            {
                if (usersContext == null)
                {
                    context.Dispose();
                }
            }
        }

        private Lazy<XDocument> GetEmptyModel()
        {
            return new Lazy<XDocument>(
                () => new DbModelBuilder()
                          .Build(new DbProviderInfo(_usersContextInfo.ConnectionProviderName, _providerManifestToken))
                          .GetModel());
        }

        private XDocument GetCurrentHistoryModel(string defaultSchema)
        {
            using (var historyContext = _configuration.HistoryContextFactory.Create(CreateConnection(), true, defaultSchema))
            {
                var currentHistoryModel = historyContext.GetModel();

                currentHistoryModel
                    .Descendants()
                    .Each(a => a.SetAttributeValue(EdmXNames.IsSystemName, true));

                return currentHistoryModel;
            }
        }

        private XDocument GetInitialHistoryModel()
        {
            var initialHistoryModel
                = (from migrationId in _migrationAssembly.MigrationIds
                   let migrationMetadata = (IMigrationMetadata)_migrationAssembly.GetMigration(migrationId)
                   select new ModelCompressor().Decompress(Convert.FromBase64String(migrationMetadata.Target)))
                    .FirstOrDefault();

            if (initialHistoryModel == null)
            {
                using (var historyContext = new HistoryContext(CreateConnection(), true, null))
                {
                    initialHistoryModel = historyContext.GetModel();

                    initialHistoryModel
                        .Descendants()
                        .Each(a => a.SetAttributeValue(EdmXNames.IsSystemName, true));
                }
            }

            return initialHistoryModel;
        }

        private IEnumerable<string> GetHistorySchemas()
        {
            var modelCompressor = new ModelCompressor();

            return
                from migrationId in _migrationAssembly.MigrationIds
                let migrationMetadata = (IMigrationMetadata)_migrationAssembly.GetMigration(migrationId)
                from entitySet in
                    modelCompressor
                    .Decompress(Convert.FromBase64String(migrationMetadata.Target))
                    .Descendants(EdmXNames.Ssdl.EntitySetNames)
                where entitySet.IsSystem()
                select entitySet.SchemaAttribute();
        }

        /// <summary>
        ///     Gets the configuration that is being used for the migration process.
        /// </summary>
        public override DbMigrationsConfiguration Configuration
        {
            get { return _configuration; }
        }

        internal virtual void DisableInitializer(Type contextType)
        {
            Check.NotNull(contextType, "contextType");

            _setInitializerMethod
                .MakeGenericMethod(contextType)
                .Invoke(null, new object[] { null });
        }

        internal override string TargetDatabase
        {
            get { return _targetDatabase; }
        }

        private MigrationSqlGenerator SqlGenerator
        {
            get
            {
                return _sqlGenerator
                       ?? (_sqlGenerator = _configuration.GetSqlGenerator(_usersContextInfo.ConnectionProviderName));
            }
        }

        /// <summary>
        ///     Gets all migrations that are defined in the configured migrations assembly.
        /// </summary>
        public override IEnumerable<string> GetLocalMigrations()
        {
            return _migrationAssembly.MigrationIds;
        }

        /// <summary>
        ///     Gets all migrations that have been applied to the target database.
        /// </summary>
        public override IEnumerable<string> GetDatabaseMigrations()
        {
            if (_migrationAssembly.MigrationIds.Any())
            {
                DetectInvalidHistoryChange();
            }

            return _historyRepository.GetMigrationsSince(InitialDatabase);
        }

        /// <summary>
        ///     Gets all migrations that are defined in the assembly but haven't been applied to the target database.
        /// </summary>
        public override IEnumerable<string> GetPendingMigrations()
        {
            if (_migrationAssembly.MigrationIds.Any())
            {
                DetectInvalidHistoryChange();
            }

            return _historyRepository.GetPendingMigrations(_migrationAssembly.MigrationIds);
        }

        internal ScaffoldedMigration ScaffoldInitialCreate(string @namespace)
        {
            string migrationId;
            var databaseModel = _historyRepository.GetLastModel(out migrationId, contextKey: _legacyContextKey);

            if ((databaseModel == null)
                || !migrationId.MigrationName().Equals(Strings.InitialCreate))
            {
                return null;
            }

            var migrationOperations
                = _modelDiffer.Diff(_emptyModel.Value, databaseModel, false)
                              .ToList();

            var generatedMigration
                = _configuration.CodeGenerator.Generate(
                    migrationId,
                    migrationOperations,
                    null,
                    Convert.ToBase64String(new ModelCompressor().Compress(_currentModel)),
                    @namespace,
                    Strings.InitialCreate);

            generatedMigration.MigrationId = migrationId;
            generatedMigration.Directory = _configuration.MigrationsDirectory;

            return generatedMigration;
        }

        internal ScaffoldedMigration Scaffold(string migrationName, string @namespace, bool ignoreChanges)
        {
            string migrationId = null;
            var rescaffolding = false;

            var pendingMigrations = GetPendingMigrations().ToList();

            if (pendingMigrations.Any())
            {
                var lastMigration = pendingMigrations.Last();

                if (!lastMigration.EqualsIgnoreCase(migrationName)
                    && !lastMigration.MigrationName().EqualsIgnoreCase(migrationName))
                {
                    throw Error.MigrationsPendingException(pendingMigrations.Join());
                }

                rescaffolding = true;
                migrationId = lastMigration;
                migrationName = lastMigration.MigrationName();
            }

            XDocument sourceModel = null;
            CheckLegacyCompatibility(() => sourceModel = _currentModel);

            string sourceMigrationId = null;
            sourceModel = sourceModel ?? (_historyRepository.GetLastModel(out sourceMigrationId) ?? _emptyModel.Value);
            var modelCompressor = new ModelCompressor();

            var migrationOperations
                = ignoreChanges
                      ? Enumerable.Empty<MigrationOperation>()
                      : _modelDiffer.Diff(sourceModel, _currentModel, false)
                                    .ToList();

            if (!rescaffolding)
            {
                migrationName = _migrationAssembly.UniquifyName(migrationName);
                migrationId = MigrationAssembly.CreateMigrationId(migrationName);
            }

            var scaffoldedMigration
                = _configuration.CodeGenerator.Generate(
                    migrationId,
                    migrationOperations,
                    (sourceModel == _emptyModel.Value)
                    || (sourceModel == _currentModel)
                    || !sourceMigrationId.IsAutomaticMigration()
                        ? null
                        : Convert.ToBase64String(modelCompressor.Compress(sourceModel)),
                    Convert.ToBase64String(modelCompressor.Compress(_currentModel)),
                    @namespace,
                    migrationName);

            scaffoldedMigration.MigrationId = migrationId;
            scaffoldedMigration.Directory = _configuration.MigrationsDirectory;
            scaffoldedMigration.IsRescaffold = rescaffolding;

            return scaffoldedMigration;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CheckLegacyCompatibility(Action onCompatible)
        {
            DebugCheck.NotNull(onCompatible);

            if (!_calledByCreateDatabase
                && !_historyRepository.Exists())
            {
                using (var context = _usersContextInfo.CreateInstance())
                {
                    bool compatibleWithModel;

                    try
                    {
                        compatibleWithModel
                            = context.Database.CompatibleWithModel(true);
                    }
                    catch
                    {
                        // no EdmMetadata table
                        return;
                    }

                    if (!compatibleWithModel)
                    {
                        throw Error.MetadataOutOfDate();
                    }

                    onCompatible();
                }
            }
        }

        /// <summary>
        ///     Updates the target database to a given migration.
        /// </summary>
        /// <param name="targetMigration"> The migration to upgrade/downgrade to. </param>
        public override void Update(string targetMigration)
        {
            DetectInvalidHistoryChange();

            base.EnsureDatabaseExists(() => UpdateInternal(targetMigration));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private void UpdateInternal(string targetMigration)
        {
            var upgradeOperations = _historyRepository.GetUpgradeOperations();

            if (upgradeOperations.Any())
            {
                base.UpgradeHistory(upgradeOperations);
            }

            var pendingMigrations = GetPendingMigrations();

            if (!pendingMigrations.Any())
            {
                CheckLegacyCompatibility(
                    () => ExecuteOperations(
                        MigrationAssembly.CreateBootstrapMigrationId(),
                        _currentModel,
                        _modelDiffer.Diff(_emptyModel.Value, _currentModel, true).Where(o => o.IsSystem),
                        false));
            }

            var targetMigrationId = targetMigration;

            if (!string.IsNullOrWhiteSpace(targetMigrationId))
            {
                if (!targetMigrationId.IsValidMigrationId())
                {
                    if (targetMigrationId == Strings.AutomaticMigration)
                    {
                        throw Error.AutoNotValidTarget(Strings.AutomaticMigration);
                    }

                    targetMigrationId = GetMigrationId(targetMigration);
                }

                if (pendingMigrations.Any(m => m.EqualsIgnoreCase(targetMigrationId)))
                {
                    pendingMigrations
                        = pendingMigrations
                            .Where(
                                m =>
                                string.CompareOrdinal(m.ToLowerInvariant(), targetMigrationId.ToLowerInvariant()) <= 0);
                }
                else
                {
                    pendingMigrations
                        = _historyRepository.GetMigrationsSince(targetMigrationId);

                    if (pendingMigrations.Any())
                    {
                        base.Downgrade(pendingMigrations.Concat(new[] { targetMigrationId }));

                        return;
                    }
                }
            }

            base.Upgrade(pendingMigrations, targetMigrationId, null);
        }

        private void DetectInvalidHistoryChange()
        {
            if (_modelDiffer.Diff(_initialHistoryModel, _currentHistoryModel)
                            .Any(o => o.IsSystem & !(o is MoveTableOperation)))
            {
                throw Error.HistoryMigrationNotSupported();
            }
        }

        internal override void UpgradeHistory(IEnumerable<MigrationOperation> upgradeOperations)
        {
            var sqlStatements = SqlGenerator.Generate(upgradeOperations, _providerManifestToken);

            base.ExecuteStatements(sqlStatements);
        }

        internal override string GetMigrationId(string migration)
        {
            if (migration.IsValidMigrationId())
            {
                return migration;
            }

            var migrationId
                = GetPendingMigrations()
                      .SingleOrDefault(m => m.MigrationName().EqualsIgnoreCase(migration))
                  ?? _historyRepository.GetMigrationId(migration);

            if (migrationId == null)
            {
                throw Error.MigrationNotFound(migration);
            }

            return migrationId;
        }

        internal override void Upgrade(
            IEnumerable<string> pendingMigrations, string targetMigrationId, string lastMigrationId)
        {
            DbMigration lastMigration = null;

            if (lastMigrationId != null)
            {
                lastMigration = _migrationAssembly.GetMigration(lastMigrationId);
            }

            foreach (var pendingMigration in pendingMigrations)
            {
                var migration = _migrationAssembly.GetMigration(pendingMigration);

                base.ApplyMigration(migration, lastMigration);

                lastMigration = migration;

                _emptyMigrationNeeded = false;

                if (pendingMigration.EqualsIgnoreCase(targetMigrationId))
                {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(targetMigrationId)
                && ((_emptyMigrationNeeded && _configuration.AutomaticMigrationsEnabled)
                    || IsModelOutOfDate(_currentModel, lastMigration)))
            {
                if (!_configuration.AutomaticMigrationsEnabled)
                {
                    throw Error.AutomaticDisabledException();
                }

                base.AutoMigrate(
                    MigrationAssembly.CreateMigrationId(
                        _calledByCreateDatabase
                            ? Strings.InitialCreate
                            : Strings.AutomaticMigration),
                    _calledByCreateDatabase
                        ? _emptyModel.Value
                        : GetLastModel(lastMigration),
                    _currentModel,
                    false);
            }

            if (!IsModelOutOfDate(_currentModel, lastMigration))
            {
                base.SeedDatabase();
            }
        }

        internal override void SeedDatabase()
        {
            // Context may not be constructable when Migrations is being called by DbContext CreateDatabase
            // and the config cannot have Seed data anyway, so avoid creating the context to seed.
            if (!_calledByCreateDatabase)
            {
                using (var context = _usersContextInfo.CreateInstance())
                {
                    _configuration.OnSeed(context);

                    context.SaveChanges();
                }
            }
        }

        private bool IsModelOutOfDate(XDocument model, DbMigration lastMigration)
        {
            DebugCheck.NotNull(model);

            var sourceModel = GetLastModel(lastMigration);

            return _modelDiffer.Diff(sourceModel, model).Any();
        }

        private XDocument GetLastModel(DbMigration lastMigration, string currentMigrationId = null)
        {
            if (lastMigration != null)
            {
                var migrationMetadata = (IMigrationMetadata)lastMigration;

                return new ModelCompressor().Decompress(Convert.FromBase64String(migrationMetadata.Target));
            }

            string migrationId;
            var lastModel = _historyRepository.GetLastModel(out migrationId);

            if (lastModel != null
                && (currentMigrationId == null || string.CompareOrdinal(migrationId, currentMigrationId) < 0))
            {
                return lastModel;
            }

            return _emptyModel.Value;
        }

        internal override void Downgrade(IEnumerable<string> pendingMigrations)
        {
            for (var i = 0; i < pendingMigrations.Count() - 1; i++)
            {
                var migrationId = pendingMigrations.ElementAt(i);
                var migration = _migrationAssembly.GetMigration(migrationId);
                var nextMigrationId = pendingMigrations.ElementAt(i + 1);
                var targetModel = (nextMigrationId != InitialDatabase)
                                      ? _historyRepository.GetModel(nextMigrationId)
                                      : _emptyModel.Value;

                Debug.Assert(targetModel != null);

                var sourceModel = _historyRepository.GetModel(migrationId);

                if (migration == null)
                {
                    base.AutoMigrate(migrationId, sourceModel, targetModel, downgrading: true);
                }
                else
                {
                    base.RevertMigration(migrationId, migration, sourceModel, targetModel);
                }
            }
        }

        internal override void RevertMigration(string migrationId, DbMigration migration, XDocument sourceModel, XDocument targetModel)
        {
            bool? includeSystemOps = null;

            if (ReferenceEquals(targetModel, _emptyModel.Value)
                && !_historyRepository.IsShared())
            {
                includeSystemOps = true;

                if (!sourceModel.HasSystemOperations())
                {
                    // upgrade scenario, inject the history model
                    AttachHistoryModel(sourceModel);
                }
            }

            var systemOperations
                = _modelDiffer.Diff(sourceModel, targetModel, includeSystemOps)
                              .Where(o => o.IsSystem);

            migration.Down();

            ExecuteOperations(migrationId, targetModel, migration.Operations.Concat(systemOperations), downgrading: true);
        }

        internal override void ApplyMigration(DbMigration migration, DbMigration lastMigration)
        {
            var migrationMetadata = (IMigrationMetadata)migration;
            var compressor = new ModelCompressor();

            var lastModel = GetLastModel(lastMigration, migrationMetadata.Id);
            var targetModel = compressor.Decompress(Convert.FromBase64String(migrationMetadata.Target));

            if (migrationMetadata.Source != null)
            {
                var sourceModel
                    = compressor.Decompress(Convert.FromBase64String(migrationMetadata.Source));

                if (IsModelOutOfDate(sourceModel, lastMigration))
                {
                    base.AutoMigrate(
                        migrationMetadata.Id.ToAutomaticMigrationId(),
                        lastModel,
                        sourceModel,
                        downgrading: false);

                    lastModel = sourceModel;
                }
            }

            bool? includeSystemOps = null;
            var isFirstMigration = ReferenceEquals(lastModel, _emptyModel.Value);

            if (isFirstMigration && !base.HistoryExists())
            {
                includeSystemOps = true;

                if (!targetModel.HasSystemOperations())
                {
                    // upgrade scenario, inject the history model
                    AttachHistoryModel(targetModel);
                }
            }

            var systemOperations
                = _modelDiffer.Diff(lastModel, targetModel, includeSystemOps)
                              .Where(o => o.IsSystem);

            migration.Up();

            ExecuteOperations(migrationMetadata.Id, targetModel, migration.Operations.Concat(systemOperations), false);
        }

        internal override bool HistoryExists()
        {
            return _historyRepository.Exists();
        }

        internal override void AutoMigrate(
            string migrationId, XDocument sourceModel, XDocument targetModel, bool downgrading)
        {
            bool? includeSystemOps = null;

            if (ReferenceEquals(downgrading ? targetModel : sourceModel, _emptyModel.Value)
                && !_historyRepository.IsShared())
            {
                includeSystemOps = true;

                var appendableModel = downgrading ? sourceModel : targetModel;

                if (!appendableModel.HasSystemOperations())
                {
                    // upgrade scenario, inject the history model
                    AttachHistoryModel(appendableModel);
                }
            }

            var operations
                = _modelDiffer.Diff(sourceModel, targetModel, includeSystemOps)
                              .ToList();

            if (!_calledByCreateDatabase
                && !downgrading)
            {
                PreventAutoSystemChanges(operations);
            }

            if (!_configuration.AutomaticMigrationDataLossAllowed
                && operations.Any(o => o.IsDestructiveChange))
            {
                throw Error.AutomaticDataLoss();
            }

            ExecuteOperations(migrationId, targetModel, operations, downgrading, auto: true);
        }

        private static void PreventAutoSystemChanges(IEnumerable<MigrationOperation> operations)
        {
            operations.Where(o => o.IsSystem).Each(
                o =>
                    {
                        if (o is MoveTableOperation)
                        {
                            throw Error.UnableToMoveHistoryTableWithAuto();
                        }

                        var createTableOperation = o as CreateTableOperation;

                        if (createTableOperation != null)
                        {
                            var schema = createTableOperation.Name.ToDatabaseName().Schema;

                            if (schema != EdmModelExtensions.DefaultSchema)
                            {
                                throw Error.UnableToMoveHistoryTableWithAuto();
                            }
                        }
                    });
        }

        private void ExecuteOperations(
            string migrationId, XDocument targetModel, IEnumerable<MigrationOperation> operations, bool downgrading, bool auto = false)
        {
            DebugCheck.NotEmpty(migrationId);
            DebugCheck.NotNull(targetModel);
            DebugCheck.NotNull(operations);

            FillInForeignKeyOperations(operations, targetModel);

            var newTableForeignKeys
                = (from ct in operations.OfType<CreateTableOperation>()
                   from afk in operations.OfType<AddForeignKeyOperation>()
                   where ct.Name.EqualsIgnoreCase(afk.DependentTable)
                   select afk)
                    .ToList();

            var orderedOperations
                = operations
                    .Except(newTableForeignKeys)
                    .Concat(newTableForeignKeys)
                    .ToList();

            var createHistoryOperation
                = operations
                    .OfType<CreateTableOperation>()
                    .SingleOrDefault(o => o.IsSystem);

            if (createHistoryOperation != null)
            {
                _historyRepository.CurrentSchema
                    = createHistoryOperation.Name.ToDatabaseName().Schema;
            }

            var moveHistoryOperation
                = operations
                    .OfType<MoveTableOperation>()
                    .SingleOrDefault(o => o.IsSystem);

            if (moveHistoryOperation != null)
            {
                _historyRepository.CurrentSchema = moveHistoryOperation.NewSchema;

                moveHistoryOperation.ContextKey = _configuration.ContextKey;
            }

            if (!downgrading)
            {
                orderedOperations.Add(_historyRepository.CreateInsertOperation(migrationId, targetModel));
            }
            else if (!operations.Any(o => o.IsSystem && o is DropTableOperation))
            {
                orderedOperations.Add(_historyRepository.CreateDeleteOperation(migrationId));
            }

            var migrationStatements = SqlGenerator.Generate(orderedOperations, _providerManifestToken);

            if (auto)
            {
                // Filter duplicates when auto-migrating. Duplicates can be caused by
                // duplicates in the model such as shared FKs.

                migrationStatements
                    = migrationStatements.Distinct((m1, m2) => string.Equals(m1.Sql, m2.Sql, StringComparison.Ordinal));
            }

            base.ExecuteStatements(migrationStatements);

            if (operations.Any(o => o.IsSystem))
            {
                _historyRepository.ResetExists();
            }
        }

        internal override void ExecuteStatements(IEnumerable<MigrationStatement> migrationStatements)
        {
            using (var connection = CreateConnection())
            {
                DbProviderServices.GetExecutionStrategy(connection).Execute(
                    () => ExecuteStatementsInternal(migrationStatements, connection));
            }
        }

        private void ExecuteStatementsInternal(IEnumerable<MigrationStatement> migrationStatements, DbConnection connection)
        {
            connection.Open();

            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                foreach (var migrationStatement in migrationStatements)
                {
                    base.ExecuteSql(transaction, migrationStatement);
                }

                transaction.Commit();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        internal override void ExecuteSql(DbTransaction transaction, MigrationStatement migrationStatement)
        {
            if (string.IsNullOrWhiteSpace(migrationStatement.Sql))
            {
                return;
            }

            if (!migrationStatement.SuppressTransaction)
            {
                using (var command = transaction.Connection.CreateCommand())
                {
                    command.CommandText = migrationStatement.Sql;
                    command.Transaction = transaction;
                    ConfigureCommand(command);

                    command.ExecuteNonQuery();
                }
            }
            else
            {
                using (var connection = CreateConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = migrationStatement.Sql;
                        ConfigureCommand(command);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void ConfigureCommand(DbCommand command)
        {
            if (_configuration.CommandTimeout.HasValue)
            {
                command.CommandTimeout = _configuration.CommandTimeout.Value;
            }
        }

        private void FillInForeignKeyOperations(IEnumerable<MigrationOperation> operations, XDocument targetModel)
        {
            DebugCheck.NotNull(operations);
            DebugCheck.NotNull(targetModel);

            foreach (var foreignKeyOperation
                in operations.OfType<AddForeignKeyOperation>()
                             .Where(fk => fk.PrincipalTable != null && !fk.PrincipalColumns.Any()))
            {
                var principalTable = GetStandardizedTableName(foreignKeyOperation.PrincipalTable);
                var entitySetName
                    = (from es in targetModel.Descendants(EdmXNames.Ssdl.EntitySetNames)
                       where _modelDiffer.GetQualifiedTableName(es.TableAttribute(), es.SchemaAttribute())
                                         .EqualsIgnoreCase(principalTable)
                       select es.NameAttribute()).SingleOrDefault();

                if (entitySetName != null)
                {
                    var entityTypeElement
                        = targetModel.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                                     .Single(et => et.NameAttribute().EqualsIgnoreCase(entitySetName));

                    entityTypeElement
                        .Descendants(EdmXNames.Ssdl.PropertyRefNames).Each(
                            pr => foreignKeyOperation.PrincipalColumns.Add(pr.NameAttribute()));
                }
                else
                {
                    // try and find the table in the current list of ops
                    var table
                        = operations
                            .OfType<CreateTableOperation>()
                            .SingleOrDefault(ct => GetStandardizedTableName(ct.Name).EqualsIgnoreCase(principalTable));

                    if ((table != null)
                        && (table.PrimaryKey != null))
                    {
                        table.PrimaryKey.Columns.Each(c => foreignKeyOperation.PrincipalColumns.Add(c));
                    }
                    else
                    {
                        throw Error.PartialFkOperation(
                            foreignKeyOperation.DependentTable, foreignKeyOperation.DependentColumns.Join());
                    }
                }
            }
        }

        private static string GetStandardizedTableName(string tableName)
        {
            if (tableName.Contains('.'))
            {
                return tableName;
            }

            return EdmModelExtensions.DefaultSchema + "." + tableName;
        }

        /// <summary>
        ///     Ensures that the database exists by creating an empty database if one does not
        ///     already exist. If a new empty database is created but then the code in mustSucceedToKeepDatabase
        ///     throws an exception, then an attempt is made to clean up (delete) the new empty database.
        ///     This avoids leaving an empty database with no or incomplete metadata (e.g. MigrationHistory)
        ///     which can then cause problems for database initializers that check whether or not a database
        ///     exists.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal override void EnsureDatabaseExists(Action mustSucceedToKeepDatabase)
        {
            var databaseCreated = false;
            var databaseCreator = new DatabaseCreator(_configuration.CommandTimeout);
            using (var connection = CreateConnection())
            {
                if (!databaseCreator.Exists(connection))
                {
                    databaseCreator.Create(connection);

                    databaseCreated = true;
                }
            }

            _emptyMigrationNeeded = databaseCreated;

            try
            {
                mustSucceedToKeepDatabase();
            }
            catch
            {
                if (databaseCreated)
                {
                    try
                    {
                        using (var connection = CreateConnection())
                        {
                            databaseCreator.Delete(connection);
                        }
                    }
                    catch
                    {
                        // Intentionally swallowing this exception since it is better to throw the
                        // original exception again for the user to see what the real problem is. An
                        // exception here is unlikely and would not be a root cause, but rather a
                        // cleanup issue.
                    }
                }
                throw;
            }
        }

        private DbConnection CreateConnection()
        {
            var connection = _providerFactory.CreateConnection();
            connection.ConnectionString = _usersContextInfo.ConnectionString;

            return connection;
        }

        private void AttachHistoryModel(XDocument targetModel)
        {
            DebugCheck.NotNull(targetModel);

            var historyModel = new XDocument(_currentHistoryModel); // clone

            var csdlNamespace = targetModel.Descendants(EdmXNames.Csdl.SchemaNames).Single().Name.Namespace;
            var mslNamespace = targetModel.Descendants(EdmXNames.Msl.MappingNames).Single().Name.Namespace;
            var ssdlNamespace = targetModel.Descendants(EdmXNames.Ssdl.SchemaNames).Single().Name.Namespace;

            var entityTypes = historyModel.Descendants(EdmXNames.Csdl.EntityTypeNames);
            var entitySets = historyModel.Descendants(EdmXNames.Csdl.EntitySetNames);
            var entitySetMappings = historyModel.Descendants(EdmXNames.Msl.EntitySetMappingNames);
            var storeEntityTypes = historyModel.Descendants(EdmXNames.Ssdl.EntityTypeNames);
            var storeEntitySets = historyModel.Descendants(EdmXNames.Ssdl.EntitySetNames);

            // normalize namespaces
            entityTypes.DescendantsAndSelf().Each(e => e.Name = csdlNamespace + e.Name.LocalName);
            entitySets.DescendantsAndSelf().Each(e => e.Name = csdlNamespace + e.Name.LocalName);
            entitySetMappings.DescendantsAndSelf().Each(e => e.Name = mslNamespace + e.Name.LocalName);
            storeEntityTypes.DescendantsAndSelf().Each(e => e.Name = ssdlNamespace + e.Name.LocalName);
            storeEntitySets.DescendantsAndSelf().Each(e => e.Name = ssdlNamespace + e.Name.LocalName);

            targetModel.Descendants(EdmXNames.Csdl.SchemaNames).Single().Add(entityTypes);
            targetModel.Descendants(EdmXNames.Csdl.EntityContainerNames).Single().Add(entitySets);
            targetModel.Descendants(EdmXNames.Msl.EntityContainerMappingNames).Single().Add(entitySetMappings);
            targetModel.Descendants(EdmXNames.Ssdl.SchemaNames).Single().Add(storeEntityTypes);
            targetModel.Descendants(EdmXNames.Ssdl.EntityContainerNames).Single().Add(storeEntitySets);
        }
    }
}
