// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;

    internal abstract class EdmModelVisitor
    {
        protected static void VisitCollection<T>(IEnumerable<T> collection, Action<T> visitMethod)
        {
            if (collection != null)
            {
                foreach (var element in collection)
                {
                    visitMethod(element);
                }
            }
        }

        protected internal virtual void VisitEdmModel(EdmModel item)
        {
            if (item != null)
            {
                VisitComplexTypes(item.ComplexTypes);
                VisitEntityTypes(item.EntityTypes);
                VisitEnumTypes(item.EnumTypes);
                VisitAssociationTypes(item.AssociationTypes);
                VisitFunctions(item.Functions);
                VisitEntityContainers(item.Containers);
            }
        }

        protected virtual void VisitAnnotations(MetadataItem item, IEnumerable<DataModelAnnotation> annotations)
        {
            VisitCollection(annotations, VisitAnnotation);
        }

        protected virtual void VisitAnnotation(DataModelAnnotation item)
        {
        }

        protected internal virtual void VisitMetadataItem(MetadataItem item)
        {
            if (item != null)
            {
                if (item.Annotations.Any())
                {
                    VisitAnnotations(item, item.Annotations);
                }
            }
        }

        protected virtual void VisitEntityContainers(IEnumerable<EntityContainer> entityContainers)
        {
            VisitCollection(entityContainers, VisitEdmEntityContainer);
        }

        protected virtual void VisitEdmEntityContainer(EntityContainer item)
        {
            VisitMetadataItem(item);
            if (item != null)
            {
                if (item.EntitySets.Any())
                {
                    VisitEntitySets(item, item.EntitySets);
                }

                if (item.AssociationSets.Any())
                {
                    VisitAssociationSets(item, item.AssociationSets);
                }

                if (item.FunctionImports.Any())
                {
                    VisitFunctionImports(item, item.FunctionImports);
                }
            }
        }

        protected internal virtual void VisitEdmFunction(EdmFunction function)
        {
            VisitMetadataItem(function);

            if ((function != null)
                && (function.Parameters != null))
            {
                VisitFunctionParameters(function.Parameters);
            }
        }

        protected virtual void VisitEntitySets(EntityContainer container, IEnumerable<EntitySet> entitySets)
        {
            VisitCollection(entitySets, VisitEdmEntitySet);
        }

        protected internal virtual void VisitEdmEntitySet(EntitySet item)
        {
            VisitMetadataItem(item);
        }

        protected virtual void VisitAssociationSets(
            EntityContainer container, IEnumerable<AssociationSet> associationSets)
        {
            VisitCollection(associationSets, VisitEdmAssociationSet);
        }

        protected virtual void VisitEdmAssociationSet(AssociationSet item)
        {
            VisitMetadataItem(item);
            if (item.SourceSet != null)
            {
                VisitEdmAssociationSetEnd(item.SourceSet);
            }
            if (item.TargetSet != null)
            {
                VisitEdmAssociationSetEnd(item.TargetSet);
            }
        }

        protected virtual void VisitEdmAssociationSetEnd(EntitySet item)
        {
            VisitMetadataItem(item);
        }

        protected internal virtual void VisitFunctionImports(EntityContainer container, IEnumerable<EdmFunction> functionImports)
        {
            VisitCollection(functionImports, VisitFunctionImport);
        }

        protected internal virtual void VisitFunctionImport(EdmFunction functionImport)
        {
            VisitMetadataItem(functionImport);

            if (functionImport.Parameters != null)
            {
                VisitFunctionImportParameters(functionImport.Parameters);
            }

            if (functionImport.ReturnParameters != null)
            {
                VisitFunctionImportReturnParameters(functionImport.ReturnParameters);
            }
        }

        protected internal virtual void VisitFunctionImportParameters(IEnumerable<FunctionParameter> parameters)
        {
            VisitCollection(parameters, VisitFunctionImportParameter);
        }

        protected internal virtual void VisitFunctionImportParameter(FunctionParameter parameter)
        {
            VisitMetadataItem(parameter);
        }

        protected internal virtual void VisitFunctionImportReturnParameters(IEnumerable<FunctionParameter> parameters)
        {
            VisitCollection(parameters, VisitFunctionImportReturnParameter);
        }

        protected internal virtual void VisitFunctionImportReturnParameter(FunctionParameter parameter)
        {
            VisitMetadataItem(parameter);
        }

        protected virtual void VisitComplexTypes(IEnumerable<ComplexType> complexTypes)
        {
            VisitCollection(complexTypes, VisitComplexType);
        }

        protected virtual void VisitComplexType(ComplexType item)
        {
            VisitMetadataItem(item);
            if (item.Properties.Any())
            {
                VisitCollection(item.Properties, VisitEdmProperty);
            }
        }

        protected virtual void VisitDeclaredProperties(ComplexType complexType, IEnumerable<EdmProperty> properties)
        {
            VisitCollection(properties, VisitEdmProperty);
        }

        protected virtual void VisitEntityTypes(IEnumerable<EntityType> entityTypes)
        {
            VisitCollection(entityTypes, VisitEdmEntityType);
        }

        protected virtual void VisitEnumTypes(IEnumerable<EnumType> enumTypes)
        {
            VisitCollection(enumTypes, VisitEdmEnumType);
        }

        protected internal virtual void VisitFunctions(IEnumerable<EdmFunction> functions)
        {
            VisitCollection(functions, VisitEdmFunction);
        }

        protected virtual void VisitFunctionParameters(IEnumerable<FunctionParameter> parameters)
        {
            VisitCollection(parameters, VisitFunctionParameter);
        }

        protected internal virtual void VisitFunctionParameter(FunctionParameter functionParameter)
        {
            VisitMetadataItem(functionParameter);
        }

        protected virtual void VisitEdmEnumType(EnumType item)
        {
            VisitMetadataItem(item);
            if (item != null)
            {
                if (item.Members.Any())
                {
                    VisitEnumMembers(item, item.Members);
                }
            }
        }

        protected virtual void VisitEnumMembers(EnumType enumType, IEnumerable<EnumMember> members)
        {
            VisitCollection(members, VisitEdmEnumTypeMember);
        }

        protected virtual void VisitEdmEntityType(EntityType item)
        {
            VisitMetadataItem(item);
            if (item != null)
            {
                if (item.BaseType == null
                    && item.KeyProperties.Any())
                {
                    VisitKeyProperties(item, item.KeyProperties);
                }

                if (item.DeclaredProperties.Any())
                {
                    VisitDeclaredProperties(item, item.DeclaredProperties);
                }

                if (item.DeclaredNavigationProperties.Any())
                {
                    VisitDeclaredNavigationProperties(item, item.DeclaredNavigationProperties);
                }
            }
        }

        protected virtual void VisitKeyProperties(EntityType entityType, IList<EdmProperty> properties)
        {
            VisitCollection(properties, VisitEdmProperty);
        }

        protected virtual void VisitDeclaredProperties(EntityType entityType, IList<EdmProperty> properties)
        {
            VisitCollection(properties, VisitEdmProperty);
        }

        protected virtual void VisitDeclaredNavigationProperties(
            EntityType entityType, IEnumerable<NavigationProperty> navigationProperties)
        {
            VisitCollection(navigationProperties, VisitEdmNavigationProperty);
        }

        protected virtual void VisitAssociationTypes(IEnumerable<AssociationType> associationTypes)
        {
            VisitCollection(associationTypes, VisitEdmAssociationType);
        }

        protected virtual void VisitEdmAssociationType(AssociationType item)
        {
            VisitMetadataItem(item);

            if (item != null)
            {
                if (item.SourceEnd != null)
                {
                    VisitEdmAssociationEnd(item.SourceEnd);
                }
                if (item.TargetEnd != null)
                {
                    VisitEdmAssociationEnd(item.TargetEnd);
                }
            }
            if (item.Constraint != null)
            {
                VisitEdmAssociationConstraint(item.Constraint);
            }
        }

        protected virtual void VisitEdmProperty(EdmProperty item)
        {
            VisitMetadataItem(item);
        }

        protected virtual void VisitEdmEnumTypeMember(EnumMember item)
        {
            VisitMetadataItem(item);
        }

        protected virtual void VisitEdmAssociationEnd(RelationshipEndMember item)
        {
            VisitMetadataItem(item);
        }

        protected virtual void VisitEdmAssociationConstraint(ReferentialConstraint item)
        {
            if (item != null)
            {
                VisitMetadataItem(item);
                if (item.ToRole != null)
                {
                    VisitEdmAssociationEnd(item.ToRole);
                }
                VisitCollection(item.ToProperties, VisitEdmProperty);
            }
        }

        protected virtual void VisitEdmNavigationProperty(NavigationProperty item)
        {
            VisitMetadataItem(item);
        }
    }
}
