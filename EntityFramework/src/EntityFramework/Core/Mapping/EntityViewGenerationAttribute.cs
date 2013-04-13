// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Attribute to mark the assemblies that contain the generated views type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class EntityViewGenerationAttribute : Attribute
    {
        /// <summary>
        ///     Constructor for EntityViewGenerationAttribute
        /// </summary>
        public EntityViewGenerationAttribute(Type viewGenerationType)
        {
            Check.NotNull(viewGenerationType, "viewGenerationType");
            m_viewGenType = viewGenerationType;
        }

        private readonly Type m_viewGenType;

        public Type ViewGenerationType
        {
            get { return m_viewGenType; }
        }
    }
}
