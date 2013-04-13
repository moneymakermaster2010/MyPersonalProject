// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    /// <summary>
    ///     Represents the EDM Association Type
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public class AssociationType : RelationshipType
    {
        /// <summary>
        ///     Initializes a new instance of Association Type with the given name, namespace, version and ends
        /// </summary>
        /// <param name="name"> name of the association type </param>
        /// <param name="namespaceName"> namespace of the association type </param>
        /// <param name="foreignKey"> is this a foreign key (FK) relationship? </param>
        /// <param name="dataSpace"> dataSpace in which this AssociationType belongs to </param>
        /// <exception cref="System.ArgumentNullException">Thrown if either the name, namespace or version attributes are null</exception>
        internal AssociationType(
            string name,
            string namespaceName,
            bool foreignKey,
            DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
            _referentialConstraints
                = new ReadOnlyMetadataCollection<ReferentialConstraint>(
                    new MetadataCollection<ReferentialConstraint>());

            _isForeignKey = foreignKey;
        }

        private readonly ReadOnlyMetadataCollection<ReferentialConstraint> _referentialConstraints;
        private FilteredReadOnlyMetadataCollection<AssociationEndMember, EdmMember> _associationEndMembers;
        private bool _isForeignKey;

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.AssociationType; }
        }

        /// <summary>
        ///     Returns the list of ends for this association type
        /// </summary>
        public ReadOnlyMetadataCollection<AssociationEndMember> AssociationEndMembers
        {
            get
            {
                Debug.Assert(
                    IsReadOnly,
                    "this is a wrapper around this.Members, don't call it during metadata loading, only call it after the metadata is set to read-only");

                if (null == _associationEndMembers)
                {
                    Interlocked.CompareExchange(
                        ref _associationEndMembers,
                        new FilteredReadOnlyMetadataCollection<AssociationEndMember, EdmMember>(
                            KeyMembers, Helper.IsAssociationEndMember), null);
                }
                return _associationEndMembers;
            }
        }

        public ReferentialConstraint Constraint
        {
            get { return ReferentialConstraints.SingleOrDefault(); }
            set
            {
                Check.NotNull(value, "value");
                Util.ThrowIfReadOnly(this);

                var constraint = Constraint;

                if (constraint != null)
                {
                    ReferentialConstraints.Source.Remove(constraint);
                }

                AddReferentialConstraint(value);

                _isForeignKey = true;
            }
        }

        internal AssociationEndMember SourceEnd
        {
            get { return KeyMembers.FirstOrDefault() as AssociationEndMember; }
            set
            {
                DebugCheck.NotNull(value);
                Util.ThrowIfReadOnly(this);

                if (KeyMembers.Count == 0)
                {
                    AddKeyMember(value);
                }
                else
                {
                    KeyMembers.Source[0] = value;
                }
            }
        }

        internal AssociationEndMember TargetEnd
        {
            get { return KeyMembers.ElementAtOrDefault(1) as AssociationEndMember; }
            set
            {
                DebugCheck.NotNull(value);
                Util.ThrowIfReadOnly(this);
                Debug.Assert(KeyMembers.Any());

                if (KeyMembers.Count == 1)
                {
                    AddKeyMember(value);
                }
                else
                {
                    KeyMembers.Source[1] = value;
                }
            }
        }

        /// <summary>
        ///     Returns the list of constraints for this association type
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.ReferentialConstraint, true)]
        public ReadOnlyMetadataCollection<ReferentialConstraint> ReferentialConstraints
        {
            get { return _referentialConstraints; }
        }

        /// <summary>
        ///     Indicates whether this is a foreign key relationship.
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.Boolean, false)]
        public bool IsForeignKey
        {
            get { return _isForeignKey; }
        }

        /// <summary>
        ///     Validates a EdmMember object to determine if it can be added to this type's
        ///     Members collection. If this method returns without throwing, it is assumed
        ///     the member is valid.
        /// </summary>
        /// <param name="member"> The member to validate </param>
        /// <exception cref="System.ArgumentException">Thrown if the member is not an AssociationEndMember</exception>
        internal override void ValidateMemberForAdd(EdmMember member)
        {
            Debug.Assert(
                (member is AssociationEndMember),
                "Only members of type AssociationEndMember may be added to Association definitions.");
        }

        /// <summary>
        ///     Sets this item to be read-only, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();
                ReferentialConstraints.Source.SetReadOnly();
            }
        }

        /// <summary>
        ///     Add the given referential constraint to the collection of referential constraints
        /// </summary>
        /// <param name="referentialConstraint"> </param>
        internal void AddReferentialConstraint(ReferentialConstraint referentialConstraint)
        {
            ReferentialConstraints.Source.Add(referentialConstraint);
        }

        /// <summary>
        ///     Creates a read-only AssociationType instance from the specified parameters.
        /// </summary>
        /// <param name="name">The name of the association type.</param>
        /// <param name="namespaceName">The namespace of the association type.</param>
        /// <param name="foreignKey">Flag that indicates a foreign key (FK) relationship.</param>
        /// <param name="dataSpace">The data space for the association type.</param>
        /// <param name="sourceEnd">The source association end member.</param>
        /// <param name="targetEnd">The target association end member.</param>
        /// <param name="constraint">A referential constraint.</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the instance.</param>
        /// <returns>The newly created AssociationType instance.</returns>
        /// <exception cref="System.ArgumentException">The specified name is null or empty.</exception>
        /// <exception cref="System.ArgumentException">The specified namespace is null or empty.</exception>
        public static AssociationType Create(
            string name,
            string namespaceName,
            bool foreignKey,
            DataSpace dataSpace,
            AssociationEndMember sourceEnd,
            AssociationEndMember targetEnd,
            ReferentialConstraint constraint,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(namespaceName, "namespaceName");

            var instance = new AssociationType(name, namespaceName, foreignKey, dataSpace);

            if (sourceEnd != null)
            {
                instance.SourceEnd = sourceEnd;
            }

            if (targetEnd != null)
            {
                instance.TargetEnd = targetEnd;
            }

            if (constraint != null)
            {
                instance.AddReferentialConstraint(constraint);
            }

            if (metadataProperties != null)
            {
                instance.AddMetadataProperties(metadataProperties.ToList());
            }

            instance.SetReadOnly();

            return instance;
        }
    }
}
