// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Convention to process instances of <see cref="ComplexTypeAttribute" /> found on types in the model.
    /// </summary>
    public class ComplexTypeAttributeConvention :
        AttributeConfigurationConvention<Type, ModelConfiguration, ComplexTypeAttribute>
    {
        public override void Apply(Type memberInfo, ModelConfiguration configuration, ComplexTypeAttribute attribute)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            configuration.ComplexType(memberInfo);
        }
    }
}
