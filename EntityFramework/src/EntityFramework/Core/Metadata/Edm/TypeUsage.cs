// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    ///     Class representing a type information for an item
    /// </summary>
    [DebuggerDisplay("EdmType={EdmType}, Facets.Count={Facets.Count}")]
    public class TypeUsage : MetadataItem
    {
        internal TypeUsage()
        {
        }

        /// <summary>
        ///     The constructor for TypeUsage taking in a type
        /// </summary>
        /// <param name="edmType"> The type which the TypeUsage object describes </param>
        /// <exception cref="System.ArgumentNullException">Thrown if edmType argument is null</exception>
        private TypeUsage(EdmType edmType)
            : base(MetadataFlags.Readonly)
        {
            Check.NotNull(edmType, "edmType");

            _edmType = edmType;

            // I would like to be able to assert that the edmType is ReadOnly, but
            // because some types are still in loading while the TypeUsage is being created
            // that won't work. We should consider a way to change this
        }

        /// <summary>
        ///     The constructor for TypeUsage taking in a type and a collection of facets
        /// </summary>
        /// <param name="edmType"> The type which the TypeUsage object describes </param>
        /// <param name="facets"> The replacement collection of facets </param>
        /// <exception cref="System.ArgumentNullException">Thrown if edmType argument is null</exception>
        private TypeUsage(EdmType edmType, IEnumerable<Facet> facets)
            : this(edmType)
        {
            var facetCollection = new MetadataCollection<Facet>(facets);
            facetCollection.SetReadOnly();
            _facets = facetCollection.AsReadOnlyMetadataCollection();
        }

        /// <summary>
        ///     Factory method for creating a TypeUsage with specified EdmType
        /// </summary>
        /// <param name="edmType"> EdmType for which to create a type usage </param>
        /// <returns> new TypeUsage instance with default facet values </returns>
        internal static TypeUsage Create(EdmType edmType)
        {
            return new TypeUsage(edmType);
        }

        /// <summary>
        ///     Factory method for creating a TypeUsage with specified EdmType
        /// </summary>
        /// <param name="edmType"> EdmType for which to create a type usage </param>
        /// <returns> new TypeUsage instance with default facet values </returns>
        internal static TypeUsage Create(EdmType edmType, FacetValues values)
        {
            return new TypeUsage(
                edmType,
                GetDefaultFacetDescriptionsAndOverrideFacetValues(edmType, values));
        }

        /// <summary>
        ///     Factory method for creating a TypeUsage with specified EdmType and facets
        /// </summary>
        /// <param name="edmType"> EdmType for which to create a type usage </param>
        /// <param name="facets"> facets to be copied into the new TypeUsage </param>
        /// <returns> new TypeUsage instance </returns>
        public static TypeUsage Create(EdmType edmType, IEnumerable<Facet> facets)
        {
            return new TypeUsage(edmType, facets);
        }

        internal TypeUsage ShallowCopy(FacetValues facetValues)
        {
            return Create(_edmType, OverrideFacetValues(Facets, facetValues));
        }

        internal TypeUsage ShallowCopy(params Facet[] facetValues)
        {
            return Create(_edmType, OverrideFacetValues(Facets, facetValues));
        }

        private static IEnumerable<Facet> OverrideFacetValues(IEnumerable<Facet> facets, IEnumerable<Facet> facetValues)
        {
            return facets.Except(facetValues, (f1, f2) => f1.EdmEquals(f2)).Union(facetValues);
        }

        /// <summary>
        ///     Factory method for creating a "readonly" TypeUsage with specified EdmType
        /// </summary>
        /// <param name="edmType"> An EdmType for which to create a TypeUsage </param>
        /// <returns> A TypeUsage instance with default facet values for the specified EdmType </returns>
        public static TypeUsage CreateDefaultTypeUsage(EdmType edmType)
        {
            Check.NotNull(edmType, "edmType");

            var type = Create(edmType);
            return type;
        }

        /// <summary>
        ///     Factory method for creating a string TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType"> A PrimitiveType for which to construct the TypeUsage </param>
        /// <param name="isUnicode"> Whether the string type is unicode or not </param>
        /// <param name="isFixedLength"> Whether the string type is fixed length or not </param>
        /// <param name="maxLength"> The max length of the string type </param>
        /// <returns> A TypeUsage object describing a string type with the given facet values </returns>
        public static TypeUsage CreateStringTypeUsage(
            PrimitiveType primitiveType,
            bool isUnicode,
            bool isFixedLength,
            int maxLength)
        {
            Check.NotNull(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.String)
            {
                throw new ArgumentException(Strings.NotStringTypeForTypeUsage);
            }

            ValidateMaxLength(maxLength);

            var typeUsage = Create(
                primitiveType,
                new FacetValues
                    {
                        MaxLength = maxLength,
                        Unicode = isUnicode,
                        FixedLength = isFixedLength
                    });

            return typeUsage;
        }

        /// <summary>
        ///     Factory method for creating a string TypeUsage object with the specified facets and
        ///     unbounded MaxLength
        /// </summary>
        /// <param name="primitiveType"> A PrimitiveType for which to construct the TypeUsage </param>
        /// <param name="isUnicode"> Whether the string type is unicode or not </param>
        /// <param name="isFixedLength"> Whether the string type is fixed length or not </param>
        /// <returns> A TypeUsage object describing a string type with the given facet values and unbounded MaxLength </returns>
        public static TypeUsage CreateStringTypeUsage(
            PrimitiveType primitiveType,
            bool isUnicode,
            bool isFixedLength)
        {
            Check.NotNull(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.String)
            {
                throw new ArgumentException(Strings.NotStringTypeForTypeUsage);
            }
            var typeUsage = Create(
                primitiveType,
                new FacetValues
                    {
                        MaxLength = DefaultMaxLengthFacetValue,
                        Unicode = isUnicode,
                        FixedLength = isFixedLength
                    });

            return typeUsage;
        }

        /// <summary>
        ///     Factory method for creating a Binary TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType"> A PrimitiveType for which to construct TypeUsage </param>
        /// <param name="isFixedLength"> Whether the binary type is fixed length or not </param>
        /// <param name="maxLength"> The max length of the binary type </param>
        /// <returns> A TypeUsage object describing a binary type with the given facet values </returns>
        public static TypeUsage CreateBinaryTypeUsage(
            PrimitiveType primitiveType,
            bool isFixedLength,
            int maxLength)
        {
            Check.NotNull(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Binary)
            {
                throw new ArgumentException(Strings.NotBinaryTypeForTypeUsage);
            }

            ValidateMaxLength(maxLength);

            var typeUsage = Create(
                primitiveType,
                new FacetValues
                    {
                        MaxLength = maxLength,
                        FixedLength = isFixedLength
                    });

            return typeUsage;
        }

        /// <summary>
        ///     Factory method for creating a Binary TypeUsage object with the specified facets and
        ///     unbounded MaxLength
        /// </summary>
        /// <param name="primitiveType"> A PrimitiveType for which to construct the TypeUsage </param>
        /// <param name="isFixedLength"> Whether the binary type is fixed length or not </param>
        /// <returns> A TypeUsage object describing a binary type with the given facet values </returns>
        public static TypeUsage CreateBinaryTypeUsage(PrimitiveType primitiveType, bool isFixedLength)
        {
            Check.NotNull(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Binary)
            {
                throw new ArgumentException(Strings.NotBinaryTypeForTypeUsage);
            }
            var typeUsage = Create(
                primitiveType,
                new FacetValues
                    {
                        MaxLength = DefaultMaxLengthFacetValue,
                        FixedLength = isFixedLength
                    });

            return typeUsage;
        }

        /// <summary>
        ///     Factory method for creating a DateTime TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType"> A PrimitiveType for which to construct the TypeUsage </param>
        /// <param name="precision"> Precision for seconds </param>
        /// <returns> A TypeUsage object describing a DateTime type with the given facet values </returns>
        public static TypeUsage CreateDateTimeTypeUsage(
            PrimitiveType primitiveType,
            byte? precision)
        {
            Check.NotNull(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.DateTime)
            {
                throw new ArgumentException(Strings.NotDateTimeTypeForTypeUsage);
            }
            var typeUsage = Create(
                primitiveType,
                new FacetValues
                    {
                        Precision = precision
                    });

            return typeUsage;
        }

        /// <summary>
        ///     Factory method for creating a DateTimeOffset TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType"> A PrimitiveType for which to construct the TypeUsage </param>
        /// <param name="precision"> Precision for seconds </param>
        /// <returns> A TypeUsage object describing a DateTime type with the given facet values </returns>
        public static TypeUsage CreateDateTimeOffsetTypeUsage(
            PrimitiveType primitiveType,
            byte? precision)
        {
            Check.NotNull(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.DateTimeOffset)
            {
                throw new ArgumentException(Strings.NotDateTimeOffsetTypeForTypeUsage);
            }

            var typeUsage = Create(
                primitiveType,
                new FacetValues
                    {
                        Precision = precision
                    });

            return typeUsage;
        }

        /// <summary>
        ///     Factory method for creating a Time TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType"> A PrimitiveType for which to construct the TypeUsage </param>
        /// <param name="precision"> Precision for seconds </param>
        /// <returns> A TypeUsage object describing a Time type with the given facet values </returns>
        public static TypeUsage CreateTimeTypeUsage(
            PrimitiveType primitiveType,
            byte? precision)
        {
            Check.NotNull(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Time)
            {
                throw new ArgumentException(Strings.NotTimeTypeForTypeUsage);
            }
            var typeUsage = Create(
                primitiveType,
                new FacetValues
                    {
                        Precision = precision
                    });

            return typeUsage;
        }

        /// <summary>
        ///     Factory method for creating a Decimal TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType"> A PrimitiveType for which to construct type usage </param>
        /// <param name="precision"> The precision of the decimal type </param>
        /// <param name="scale"> The scale of the decimal type </param>
        /// <returns> A TypeUsage object describing a decimal type with the given facet values </returns>
        public static TypeUsage CreateDecimalTypeUsage(
            PrimitiveType primitiveType,
            byte precision,
            byte scale)
        {
            Check.NotNull(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Decimal)
            {
                throw new ArgumentException(Strings.NotDecimalTypeForTypeUsage);
            }

            var typeUsage = Create(
                primitiveType,
                new FacetValues
                    {
                        Precision = precision,
                        Scale = scale
                    });

            return typeUsage;
        }

        /// <summary>
        ///     Factory method for creating a Decimal TypeUsage object with unbounded precision and scale
        /// </summary>
        /// <param name="primitiveType"> The PrimitiveType for which to construct type usage </param>
        /// <returns> A TypeUsage object describing a decimal type with unbounded precision and scale </returns>
        public static TypeUsage CreateDecimalTypeUsage(PrimitiveType primitiveType)
        {
            Check.NotNull(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Decimal)
            {
                throw new ArgumentException(Strings.NotDecimalTypeForTypeUsage);
            }
            var typeUsage = Create(
                primitiveType,
                new FacetValues
                    {
                        Precision = DefaultPrecisionFacetValue,
                        Scale = DefaultScaleFacetValue
                    });

            return typeUsage;
        }

        private TypeUsage _modelTypeUsage;
        private readonly EdmType _edmType;
        private ReadOnlyMetadataCollection<Facet> _facets;
        private string _identity;

        /// <summary>
        ///     Set of facets that should be included in identity for TypeUsage
        /// </summary>
        /// <remarks>
        ///     keep this sorted for binary searching
        /// </remarks>
        private static readonly string[] _identityFacets = new[]
            {
                DbProviderManifest.DefaultValueFacetName,
                DbProviderManifest.FixedLengthFacetName,
                DbProviderManifest.MaxLengthFacetName,
                DbProviderManifest.NullableFacetName,
                DbProviderManifest.PrecisionFacetName,
                DbProviderManifest.ScaleFacetName,
                DbProviderManifest.UnicodeFacetName,
                DbProviderManifest.SridFacetName
            };

        internal static readonly EdmConstants.Unbounded DefaultMaxLengthFacetValue = EdmConstants.UnboundedValue;
        internal static readonly EdmConstants.Unbounded DefaultPrecisionFacetValue = EdmConstants.UnboundedValue;
        internal static readonly EdmConstants.Unbounded DefaultScaleFacetValue = EdmConstants.UnboundedValue;
        internal const bool DefaultUnicodeFacetValue = true;
        internal const bool DefaultFixedLengthFacetValue = false;
        internal static readonly byte? DefaultDateTimePrecisionFacetValue = null;

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.TypeUsage; }
        }

        /// <summary>
        ///     Gets the type that this TypeUsage describes
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EdmType, false)]
        public virtual EdmType EdmType
        {
            get { return _edmType; }
        }

        /// <summary>
        ///     Gets the list of facets for the type in this TypeUsage
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.Facet, true)]
        public virtual ReadOnlyMetadataCollection<Facet> Facets
        {
            get
            {
                if (null == _facets)
                {
                    var facets = new MetadataCollection<Facet>(GetFacets());
                    // we never modify the collection so we can set it readonly from the start
                    facets.SetReadOnly();
                    Interlocked.CompareExchange(ref _facets, facets.AsReadOnlyMetadataCollection(), null);
                }
                return _facets;
            }
        }

        /// <summary>
        ///     Returns a Model type usage for a provider type
        /// </summary>
        /// <returns> model (CSpace) type usage </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public TypeUsage GetModelTypeUsage()
        {
            if (_modelTypeUsage == null)
            {
                var edmType = EdmType;

                // If the edm type is already a cspace type, return the same type
                if (edmType.DataSpace == DataSpace.CSpace
                    || edmType.DataSpace == DataSpace.OSpace)
                {
                    return this;
                }

                TypeUsage result;
                if (Helper.IsRowType(edmType))
                {
                    var sspaceRowType = (RowType)edmType;
                    var properties = new EdmProperty[sspaceRowType.Properties.Count];
                    for (var i = 0; i < properties.Length; i++)
                    {
                        var sspaceProperty = sspaceRowType.Properties[i];
                        var newTypeUsage = sspaceProperty.TypeUsage.GetModelTypeUsage();
                        properties[i] = new EdmProperty(sspaceProperty.Name, newTypeUsage);
                    }
                    var edmRowType = new RowType(properties, sspaceRowType.InitializerMetadata);
                    result = Create(edmRowType, Facets);
                }
                else if (Helper.IsCollectionType(edmType))
                {
                    var sspaceCollectionType = ((CollectionType)edmType);
                    var newTypeUsage = sspaceCollectionType.TypeUsage.GetModelTypeUsage();
                    result = Create(new CollectionType(newTypeUsage), Facets);
                }
                else if (Helper.IsPrimitiveType(edmType))
                {
                    result = ((PrimitiveType)edmType).ProviderManifest.GetEdmType(this);

                    if (result == null)
                    {
                        throw new ProviderIncompatibleException(Strings.Mapping_ProviderReturnsNullType(ToString()));
                    }

                    if (!TypeSemantics.IsNullable(this))
                    {
                        result = Create(
                            result.EdmType,
                            OverrideFacetValues(
                                result.Facets,
                                new FacetValues
                                    {
                                        Nullable = false
                                    }));
                    }
                }
                else if (Helper.IsEntityTypeBase(edmType)
                         || Helper.IsComplexType(edmType))
                {
                    result = this;
                }
                else
                {
                    Debug.Assert(false, "Unexpected type found in entity data reader");
                    return null;
                }
                Interlocked.CompareExchange(ref _modelTypeUsage, result, null);
            }
            return _modelTypeUsage;
        }

        /// <summary>
        ///     check if "this" is a subtype of the specified TypeUsage
        /// </summary>
        /// <param name="typeUsage"> The typeUsage to be checked </param>
        /// <returns> true if this typeUsage is a subtype of the specified typeUsage </returns>
        public bool IsSubtypeOf(TypeUsage typeUsage)
        {
            if (EdmType == null
                || typeUsage == null)
            {
                return false;
            }

            return EdmType.IsSubtypeOf(typeUsage.EdmType);
        }

        private IEnumerable<Facet> GetFacets()
        {
            return _edmType.GetAssociatedFacetDescriptions().Select(facetDescription => facetDescription.DefaultValueFacet);
        }

        internal override void SetReadOnly()
        {
            Debug.Fail("TypeUsage.SetReadOnly should not need to ever be called");
            base.SetReadOnly();
        }

        /// <summary>
        ///     returns the identity of the type usage
        /// </summary>
        internal override String Identity
        {
            get
            {
                if (Facets.Count == 0)
                {
                    return EdmType.Identity;
                }

                if (_identity == null)
                {
                    var builder = new StringBuilder(128);
                    BuildIdentity(builder);
                    var identity = builder.ToString();
                    Interlocked.CompareExchange(ref _identity, identity, null);
                }
                return _identity;
            }
        }

        private static IEnumerable<Facet> GetDefaultFacetDescriptionsAndOverrideFacetValues(EdmType type, FacetValues values)
        {
            return OverrideFacetValues(
                type.GetAssociatedFacetDescriptions(),
                fd => fd,
                fd => fd.DefaultValueFacet,
                values);
        }

        private static IEnumerable<Facet> OverrideFacetValues(IEnumerable<Facet> facets, FacetValues values)
        {
            return OverrideFacetValues(
                facets,
                f => f.Description,
                f => f,
                values);
        }

        private static IEnumerable<Facet> OverrideFacetValues<T>(
            IEnumerable<T> facetThings,
            Func<T, FacetDescription> getDescription,
            Func<T, Facet> getFacet,
            FacetValues values)
        {
            // yield all the non custom values
            foreach (var thing in facetThings)
            {
                var description = getDescription(thing);
                Facet facet;
                if (!description.IsConstant
                    && values.TryGetFacet(description, out facet))
                {
                    yield return facet;
                }
                else
                {
                    yield return getFacet(thing);
                }
            }
        }

        internal override void BuildIdentity(StringBuilder builder)
        {
            // if we've already cached the identity, simply append it
            if (null != _identity)
            {
                builder.Append(_identity);
                return;
            }

            builder.Append(EdmType.Identity);

            builder.Append("(");
            var first = true;
            for (var j = 0; j < Facets.Count; j++)
            {
                var facet = Facets[j];

                if (0 <= Array.BinarySearch(_identityFacets, facet.Name, StringComparer.Ordinal))
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append(",");
                    }

                    builder.Append(facet.Name);
                    builder.Append("=");
                    // If the facet is present, add its value to the identity
                    // We only include built-in system facets for the identity
                    builder.Append(facet.Value ?? String.Empty);
                }
            }
            builder.Append(")");
        }

        /// <summary>
        /// </summary>
        public override string ToString()
        {
            // Note that ToString is actually used to get the full name of the type, so changing the value returned here
            // will break code.
            return EdmType.ToString();
        }

        /// <summary>
        ///     EdmEquals override verifying the equivalence of all facets. Two facets are considered
        ///     equal if they have the same name and the same value (Object.Equals)
        /// </summary>
        /// <param name="item"> </param>
        /// <returns> </returns>
        internal override bool EdmEquals(MetadataItem item)
        {
            // short-circuit if this and other are reference equivalent
            if (ReferenceEquals(this, item))
            {
                return true;
            }

            // check type of item
            if (null == item
                || BuiltInTypeKind.TypeUsage != item.BuiltInTypeKind)
            {
                return false;
            }
            var other = (TypeUsage)item;

            // verify edm types are equivalent
            if (!EdmType.EdmEquals(other.EdmType))
            {
                return false;
            }

            // if both usages have default facets, no need to compare
            if (null == _facets
                && null == other._facets)
            {
                return true;
            }

            // initialize facets and compare
            if (Facets.Count
                != other.Facets.Count)
            {
                return false;
            }

            foreach (var thisFacet in Facets)
            {
                Facet otherFacet;
                if (!other.Facets.TryGetValue(thisFacet.Name, false, out otherFacet))
                {
                    // other type usage doesn't have the same facets as this type usage
                    return false;
                }

                // check that the facet values are the same
                if (!Equals(thisFacet.Value, otherFacet.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private static void ValidateMaxLength(int maxLength)
        {
            if (maxLength <= 0)
            {
                throw new ArgumentOutOfRangeException("maxLength", Strings.InvalidMaxLengthSize);
            }
        }
    }
}
