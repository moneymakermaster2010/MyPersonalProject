// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    ///     Class representing the Documentation associated with an item
    /// </summary>
    public sealed class Documentation : MetadataItem
    {
        private string _summary = "";
        private string _longDescription = "";

        /// <summary>
        ///     Default constructor - primarily created for supporting usage of this Documentation class by SOM.
        /// </summary>
        internal Documentation()
        {
        }

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.Documentation; }
        }

        /// <summary>
        ///     Gets the Summary for this Documentation instance.
        /// </summary>
        public string Summary
        {
            get { return _summary; }
            internal set
            {
                if (value != null)
                {
                    _summary = value;
                }
                else
                {
                    _summary = "";
                }
            }
        }

        /// <summary>
        ///     Gets the LongDescription for this Documentation instance.
        /// </summary>
        public string LongDescription
        {
            get { return _longDescription; }
            internal set
            {
                if (value != null)
                {
                    _longDescription = value;
                }
                else
                {
                    _longDescription = "";
                }
            }
        }

        /// <summary>
        ///     This property is required to be implemented for inheriting from MetadataItem. As there can be atmost one
        ///     instance of a nested-Documentation, return the constant "Documentation" as it's identity.
        /// </summary>
        internal override string Identity
        {
            get { return "Documentation"; }
        }

        /// <summary>
        ///     Returns true if this Documentation instance contains only null/empty summary and longDescription
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (string.IsNullOrEmpty(_summary)
                    && string.IsNullOrEmpty(_longDescription))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// </summary>
        public override string ToString()
        {
            return _summary;
        }
    }
}
