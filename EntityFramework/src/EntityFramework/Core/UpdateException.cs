// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Objects;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Exception during save changes to store
    /// </summary>
    [Serializable]
    public class UpdateException : DataException
    {
        [NonSerialized]
        private readonly ReadOnlyCollection<ObjectStateEntry> _stateEntries;

        #region constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public UpdateException()
        {
        }

        /// <summary>
        ///     Constructor that takes a message
        /// </summary>
        /// <param name="message"> </param>
        public UpdateException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Constructor that takes a message and an inner exception
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="innerException"> </param>
        public UpdateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Constructor that takes a message and an inner exception
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="innerException"> </param>
        /// <param name="stateEntries"> </param>
        public UpdateException(string message, Exception innerException, IEnumerable<ObjectStateEntry> stateEntries)
            : base(message, innerException)
        {
            var list = new List<ObjectStateEntry>(stateEntries);
            _stateEntries = list.AsReadOnly();
        }

        /// <summary>
        ///     Gets state entries implicated in the error.
        /// </summary>
        public ReadOnlyCollection<ObjectStateEntry> StateEntries
        {
            get { return _stateEntries; }
        }

        /// <summary>
        ///     The protected constructor for serialization
        /// </summary>
        /// <param name="info"> </param>
        /// <param name="context"> </param>
        protected UpdateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
