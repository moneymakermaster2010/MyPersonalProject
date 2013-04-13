// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    ///     Class for representing a relationship set
    /// </summary>
    public abstract class RelationshipSet : EntitySetBase
    {
        /// <summary>
        ///     The constructor for constructing the RelationshipSet with a given name and an relationship type
        /// </summary>
        /// <param name="name"> The name of the RelationshipSet </param>
        /// <param name="schema"> The db schema </param>
        /// <param name="table"> The db table </param>
        /// <param name="definingQuery"> The provider specific query that should be used to retrieve the EntitySet </param>
        /// <param name="relationshipType"> The entity type of the entities that this entity set type contains </param>
        /// <exception cref="System.ArgumentNullException">Thrown if the argument name or entityType is null</exception>
        internal RelationshipSet(string name, string schema, string table, string definingQuery, RelationshipType relationshipType)
            : base(name, schema, table, definingQuery, relationshipType)
        {
        }

        /// <summary>
        ///     Returns the relationship type associated with this relationship set
        /// </summary>
        public new RelationshipType ElementType
        {
            get { return (RelationshipType)base.ElementType; }
        }

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.RelationshipSet; }
        }
    }
}
