// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Data.Entity.Core.Metadata.Edm;

    [Serializable]
    public class DataModelErrorEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets an optional value indicating which property of the source item caused the event to be raised.
        /// </summary>
        public string PropertyName { get; internal set; }

        /// <summary>
        ///     Gets an optional descriptive message the describes the error that is being raised.
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        ///     Gets a value indicating the <see cref="IMetadataItem" /> that caused the event to be raised.
        /// </summary>
        public IMetadataItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        [NonSerialized]
        private IMetadataItem _item;
    }
}
