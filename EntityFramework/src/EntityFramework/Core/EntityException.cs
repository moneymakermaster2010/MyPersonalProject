// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Data.Entity.Resources;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Provider exception - Used by the entity client.
    /// </summary>
    [Serializable]
    public class EntityException : DataException
    {
        /// <summary>
        ///     Constructor with default message
        /// </summary>
        public EntityException() // required ctor
            : base(Strings.EntityClient_ProviderGeneralError)
        {
        }

        /// <summary>
        ///     Constructor that accepts a pre-formatted message
        /// </summary>
        /// <param name="message"> localized error message </param>
        public EntityException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Constructor that accepts a pre-formatted message and an inner exception
        /// </summary>
        /// <param name="message"> localized error message </param>
        /// <param name="innerException"> inner exception </param>
        public EntityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Constructor for deserialization
        /// </summary>
        /// <param name="info"> </param>
        /// <param name="context"> </param>
        protected EntityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
