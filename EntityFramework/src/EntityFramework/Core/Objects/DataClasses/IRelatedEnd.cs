// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Collections;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents one end of a relationship.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public interface IRelatedEnd
    {
        // ----------
        // Properties
        // ----------

        /// <summary>
        ///     Gets or sets a value that indicates whether all related objects have been loaded.
        /// </summary>
        /// <remarks>
        ///     Loading entities from the database either using lazy-loading, as part of a query, or explicitly with one of
        ///     the Load methods will set the IsLoaded flag to true.
        ///     IsLoaded can be set to true to prevent related entities from being lazy-loaded. This can be useful
        ///     if the application has caused a subset of related entities to be loaded and wants to prevent any other
        ///     entities from being loaded automatically.
        ///     Calling the Clear method on an <see cref="EntityCollection{TEntity}" /> sets IsLoaded to false.
        ///     An <see cref="EntityCollection{TEntity}" /> or <see cref="EntityReference{TEntity}" /> may also be loaded if
        ///     the related end is included in the query path. For more information about span,
        ///     see http://msdn.microsoft.com/en-us/library/acd0tfbe.aspx.
        ///     Note that the explict loading methods such as <see cref="Load()" /> will load related objects from the data
        ///     source whether or not IsLoaded is true.
        ///     When a related entity is detached the IsLoaded flag is reset to false indicating that all entities are no
        ///     longer loaded.
        ///     To guarantee that a related end is fully you should check whether IsLoaded is false. If IsLoaded is false,
        ///     then you should call one of the Load methods to ensure related entities have been loaded.
        /// </remarks>
        /// <param name="value">
        ///     <c>true</c> if the related end contains all the related objects from the database; otherwise, <c>false</c>.
        /// </param>
        bool IsLoaded { get; set; }

        /// <summary>
        ///     Name of the relationship in which this IRelatedEnd is participating
        /// </summary>
        string RelationshipName { get; }

        /// <summary>
        ///     Name of the relationship source role used to generate this IRelatedEnd
        /// </summary>
        string SourceRoleName { get; }

        /// <summary>
        ///     Name of the relationship target role used to generate this IRelatedEnd
        /// </summary>
        string TargetRoleName { get; }

        /// <summary>
        ///     The relationship metadata cooresponding to this IRelatedEnd
        /// </summary>
        RelationshipSet RelationshipSet { get; }

        // -------
        // Methods
        // -------

        /// <summary>
        ///     Loads the related entity or entities into the related end using the default merge option.
        /// </summary>
        void Load();

#if !NET40

        /// <summary>
        ///     Asynchronously loads the related entity or entities into the related end using the default merge option.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        Task LoadAsync(CancellationToken cancellationToken);

#endif

        /// <summary>
        ///     Loads the related entity or entities into the related end using the specified merge option.
        /// </summary>
        /// <param name="mergeOption"> Merge option to use for loaded entity or entities. </param>
        void Load(MergeOption mergeOption);

#if !NET40

        /// <summary>
        ///     Asynchronously loads the related entity or entities into the related end using the specified merge option.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="mergeOption"> Merge option to use for loaded entity or entities. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        Task LoadAsync(MergeOption mergeOption, CancellationToken cancellationToken);

#endif

        /// <summary>
        ///     Adds an entity to the related end.  If the owner is
        ///     attached to a cache then the all the connected ends are
        ///     added to the object cache and their corresponding relationships
        ///     are also added to the ObjectStateManager. The RelatedEnd of the
        ///     relationship is also fixed.
        /// </summary>
        /// <param name="entity"> Entity instance to add to the related end </param>
        void Add(IEntityWithRelationships entity);

        /// <summary>
        ///     Adds an entity to the related end.  If the owner is
        ///     attached to a cache then all the connected ends are
        ///     added to the object cache and their corresponding relationships
        ///     are also added to the ObjectStateManager. The RelatedEnd of the
        ///     relationship is also fixed.
        ///     This overload is meant to be used by classes that do not implement IEntityWithRelationships.
        /// </summary>
        /// <param name="entity"> Entity instance to add to the related end </param>
        void Add(object entity);

        /// <summary>
        ///     Removes an entity from the related end.  If owner is
        ///     attached to a cache, marks relationship for deletion and if
        ///     the relationship is composition also marks the entity for deletion.
        /// </summary>
        /// <param name="entity"> Entity instance to remove from the related end </param>
        /// <returns> Returns true if the entity was successfully removed, false if the entity was not part of the IRelatedEnd. </returns>
        bool Remove(IEntityWithRelationships entity);

        /// <summary>
        ///     Removes an entity from the related end.  If owner is
        ///     attached to a cache, marks relationship for deletion and if
        ///     the relationship is composition also marks the entity for deletion.
        ///     This overload is meant to be used by classes that do not implement IEntityWithRelationships.
        /// </summary>
        /// <param name="entity"> Entity instance to remove from the related end </param>
        /// <returns> Returns true if the entity was successfully removed, false if the entity was not part of the IRelatedEnd. </returns>
        bool Remove(object entity);

        /// <summary>
        ///     Attaches an entity to the related end. If the related end is already filled
        ///     or partially filled, this merges the existing entities with the given entity. The given
        ///     entity is not assumed to be the complete set of related entities.
        ///     Owner and all entities passed in must be in Unchanged or Modified state.
        ///     Deleted elements are allowed only when the state manager is already tracking the relationship
        ///     instance.
        /// </summary>
        /// <param name="entity"> The entity to attach to the related end </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when
        ///     <paramref name="entity" />
        ///     is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity cannot be related via the current relationship end.</exception>
        void Attach(IEntityWithRelationships entity);

        /// <summary>
        ///     Attaches an entity to the related end. If the related end is already filled
        ///     or partially filled, this merges the existing entities with the given entity. The given
        ///     entity is not assumed to be the complete set of related entities.
        ///     Owner and all entities passed in must be in Unchanged or Modified state.
        ///     Deleted elements are allowed only when the state manager is already tracking the relationship
        ///     instance.
        ///     This overload is meant to be used by classes that do not implement IEntityWithRelationships.
        /// </summary>
        /// <param name="entity"> The entity to attach to the related end </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when
        ///     <paramref name="entity" />
        ///     is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity cannot be related via the current relationship end.</exception>
        void Attach(object entity);

        /// <summary>
        ///     This is the query which represents the source of the
        ///     related end.  It is constructed on demand using the
        ///     _connection and _cache fields and a query string based on
        ///     the type of relationship end and the metadata passed into its
        ///     constructor.
        /// </summary>
        IEnumerable CreateSourceQuery();

        /// <summary>
        ///     Returns an enumerator of all of the values contained within this related end
        /// </summary>
        IEnumerator GetEnumerator();
    }
}
