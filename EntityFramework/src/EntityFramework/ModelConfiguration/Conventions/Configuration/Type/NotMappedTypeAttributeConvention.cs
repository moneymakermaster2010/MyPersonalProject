// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Convention to process instances of <see cref="NotMappedAttribute" /> found on types in the model.
    /// </summary>
    public class NotMappedTypeAttributeConvention :
        AttributeConfigurationConvention<Type, ModelConfiguration, NotMappedAttribute>
    {
        public override void Apply(Type memberInfo, ModelConfiguration configuration, NotMappedAttribute attribute)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            configuration.Ignore(memberInfo);
        }
    }
}
