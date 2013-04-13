// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     This class represents a query parameter at the object layer, which consists
    ///     of a Name, a Type and a Value.
    /// </summary>
    public sealed class ObjectParameter
    {
        #region Static Methods

        // --------------
        // Static Methods
        // --------------

        #region ValidateParameterName

        /// <summary>
        ///     This internal method uses regular expression matching to ensure that the
        ///     specified parameter name is valid. Parameter names must start with a letter,
        ///     and may only contain letters (A-Z, a-z), numbers (0-9) and underscores (_).
        /// </summary>
        internal static bool ValidateParameterName(string name)
        {
            // Note: Parameter names must begin with a letter, and may contain only
            // letters, numbers and underscores.
            return DbCommandTree.IsValidParameterName(name);
        }

        #endregion

        #endregion

        #region Public Constructors

        // -------------------
        // Public Constructors
        // -------------------

        #region ObjectParameter (string, Type)

        /// <summary>
        ///     This constructor creates an unbound (i.e., value-less) parameter from the
        ///     specified name and type. The value can be set at any time through the
        ///     public 'Value' property.
        /// </summary>
        /// <param name="name"> The parameter name. </param>
        /// <param name="type"> The CLR type of the parameter. </param>
        /// <returns> A new unbound ObjectParameter instance. </returns>
        /// <exception cref="ArgumentNullException">If the value of either argument is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     If the value of the name argument is invalid. Parameter names must start
        ///     with a letter and may only contain letters (A-Z, a-z), numbers (0-9) and
        ///     underscores (_).
        /// </exception>
        public ObjectParameter(string name, Type type)
        {
            Check.NotNull(name, "name");
            Check.NotNull(type, "type");

            if (!ValidateParameterName(name))
            {
                throw new ArgumentException(Strings.ObjectParameter_InvalidParameterName(name), "name");
            }

            _name = name;
            _type = type;

            // If the parameter type is Nullable<>, we need to extract out the underlying
            // Nullable<> type argument.
            _mappableType = TypeSystem.GetNonNullableType(_type);
        }

        #endregion

        #region ObjectParameter (string, object)

        /// <summary>
        ///     This constructor creates a fully-bound (i.e., valued) parameter from the
        ///     specified name and value. The type is inferred from the initial value, but
        ///     the value can be changed at any time through the public 'Value' property.
        /// </summary>
        /// <param name="name"> The parameter name. </param>
        /// <param name="value"> The initial value (and inherently, type) of the parameter. </param>
        /// <returns> A new fully-bound ObjectParameter instance. </returns>
        /// <exception cref="ArgumentNullException">If the value of either argument is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     If the value of the name argument is invalid. Parameter names must start
        ///     with a letter and may only contain letters (A-Z, a-z), numbers (0-9) and
        ///     underscores (_).
        /// </exception>
        public ObjectParameter(string name, object value)
        {
            Check.NotNull(name, "name");
            Check.NotNull(value, "value");

            if (!ValidateParameterName(name))
            {
                throw new ArgumentException(Strings.ObjectParameter_InvalidParameterName(name), "name");
            }

            _name = name;
            _type = value.GetType();
            _value = value;

            // If the parameter type is Nullable<>, we need to extract out the underlying
            // Nullable<> type argument.
            _mappableType = TypeSystem.GetNonNullableType(_type);
        }

        #endregion

        #endregion

        #region Private Constructors

        // -------------------
        // Copy Constructor
        // -------------------

        /// <summary>
        ///     This constructor is used by <see cref="ShallowCopy" /> to create a new ObjectParameter
        ///     with field values taken from the field values of an existing ObjectParameter.
        /// </summary>
        /// <param name="template"> The existing ObjectParameter instance from which field values should be taken. </param>
        /// <returns> A new ObjectParameter instance with the same field values as the specified ObjectParameter </returns>
        private ObjectParameter(ObjectParameter template)
        {
            DebugCheck.NotNull(template);

            _name = template._name;
            _type = template._type;
            _mappableType = template._mappableType;
            _effectiveType = template._effectiveType;
            _value = template._value;
        }

        #endregion

        #region Private Fields

        // --------------
        // Private Fields
        // --------------

        /// <summary>
        ///     The name of the parameter. Cannot be null and is immutable.
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     The CLR type of the parameter. Cannot be null and is immutable.
        /// </summary>
        private readonly Type _type;

        /// <summary>
        ///     The mappable CLR type of the parameter. Unless the parameter type is
        ///     Nullable, this type is equal to the parameter type. In the case of
        ///     Nullable parameters, this type is the underlying Nullable argument
        ///     type. Cannot be null and is immutable.
        /// </summary>
        private readonly Type _mappableType;

        /// <summary>
        ///     Used to specify the exact metadata type of this parameter.
        ///     Typically null, can only be set using the internal <see cref="TypeUsage" /> property.
        /// </summary>
        private TypeUsage _effectiveType;

        /// <summary>
        ///     The value of the parameter. Does not need to be bound until execution
        ///     time and can be modified at any time.
        /// </summary>
        private object _value;

        #endregion

        #region Public Properties

        // -----------------
        // Public Properties
        // -----------------

        /// <summary>
        ///     The parameter name, which can only be set through a constructor.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     The parameter type, which can only be set through a constructor.
        /// </summary>
        public Type ParameterType
        {
            get { return _type; }
        }

        /// <summary>
        ///     The parameter value, which can be set at any time (and subsequently
        ///     changed) before query execution. Note that type-checking is not
        ///     enforced between the declared parameter type and the type of the
        ///     specified value; such validation is left up to the underlying
        ///     provider(s) at execution time.
        /// </summary>
        public object Value
        {
            get { return _value; }

            set { _value = value; }
        }

        #endregion

        #region Internal Properties

        // -------------------
        // Internal Properties
        // -------------------

        /// <summary>
        ///     Gets or sets a <see cref="TypeUsage" /> that specifies the exact
        ///     type of which the parameter value is considered an instance.
        /// </summary>
        internal TypeUsage TypeUsage
        {
            get { return _effectiveType; }

            set
            {
                Debug.Assert(null == _effectiveType, "Effective type should only be set once");
                _effectiveType = value;
            }
        }

        /// <summary>
        ///     The mappable parameter type; this is primarily used to handle the case of
        ///     Nullable parameter types. For example, metadata knows nothing about 'int?',
        ///     only 'Int32'. For internal use only.
        /// </summary>
        internal Type MappableType
        {
            get { return _mappableType; }
        }

        #endregion

        #region Internal Methods

        // ----------------
        // Internal Methods
        // ----------------

        /// <summary>
        ///     Creates a new ObjectParameter instance with identical field values to this instance.
        /// </summary>
        /// <returns> The new ObjectParameter instance </returns>
        internal ObjectParameter ShallowCopy()
        {
            return new ObjectParameter(this);
        }

        /// <summary>
        ///     This internal method ensures that the specified type is a scalar
        ///     type supported by the underlying provider by ensuring that scalar
        ///     metadata for this type is retrievable.
        /// </summary>
        internal bool ValidateParameterType(ClrPerspective perspective)
        {
            TypeUsage type;

            // The parameter type metadata is only valid if it's scalar or enumeration type metadata.
            if ((perspective.TryGetType(_mappableType, out type))
                && (TypeSemantics.IsScalarType(type)))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
