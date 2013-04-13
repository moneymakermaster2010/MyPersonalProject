﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using Xunit;

    public class CodeGenEmitterTests
    {
        [Fact]
        public void Static_fields_are_initialized()
        {
            Assert.NotNull(CodeGenEmitter.CodeGenEmitter_BinaryEquals);
            Assert.NotNull(CodeGenEmitter.CodeGenEmitter_CheckedConvert);
            Assert.NotNull(CodeGenEmitter.CodeGenEmitter_Compile);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetValue);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetString);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetInt16);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetInt32);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetInt64);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetBoolean);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetDecimal);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetFloat);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetDouble);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetDateTime);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetGuid);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetByte);
            Assert.NotNull(CodeGenEmitter.DbDataReader_IsDBNull);
            Assert.NotNull(CodeGenEmitter.DBNull_Value);
            Assert.NotNull(CodeGenEmitter.EntityKey_ctor_SingleKey);
            Assert.NotNull(CodeGenEmitter.EntityKey_ctor_CompositeKey);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetEntityWithChangeTrackerStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetEntityWithKeyStrategyStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityProxyTypeInfo_SetEntityWrapper);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetNullPropertyAccessorStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetPocoEntityKeyStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetPocoPropertyAccessorStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetSnapshotChangeTrackingStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_NullWrapper);
            Assert.NotNull(CodeGenEmitter.IEntityWrapper_Entity);
            Assert.NotNull(CodeGenEmitter.IEqualityComparerOfString_Equals);
            Assert.NotNull(CodeGenEmitter.MaterializedDataRecord_ctor);
            Assert.NotNull(CodeGenEmitter.RecordState_GatherData);
            Assert.NotNull(CodeGenEmitter.RecordState_SetNullRecord);
            Assert.NotNull(CodeGenEmitter.Shaper_Discriminate);
            Assert.NotNull(CodeGenEmitter.Shaper_GetPropertyValueWithErrorHandling);
            Assert.NotNull(CodeGenEmitter.Shaper_GetColumnValueWithErrorHandling);
            Assert.NotNull(CodeGenEmitter.Shaper_GetGeographyColumnValue);
            Assert.NotNull(CodeGenEmitter.Shaper_GetGeometryColumnValue);
            Assert.NotNull(CodeGenEmitter.Shaper_GetSpatialColumnValueWithErrorHandling);
            Assert.NotNull(CodeGenEmitter.Shaper_GetSpatialPropertyValueWithErrorHandling);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleEntity);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleEntityAppendOnly);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleEntityNoTracking);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleFullSpanCollection);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleFullSpanElement);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleIEntityWithKey);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleRelationshipSpan);
            Assert.NotNull(CodeGenEmitter.Shaper_SetColumnValue);
            Assert.NotNull(CodeGenEmitter.Shaper_SetEntityRecordInfo);
            Assert.NotNull(CodeGenEmitter.Shaper_SetState);
            Assert.NotNull(CodeGenEmitter.Shaper_SetStatePassthrough);
            Assert.NotNull(CodeGenEmitter.Shaper_Parameter);
            Assert.NotNull(CodeGenEmitter.Shaper_Reader);
            Assert.NotNull(CodeGenEmitter.Shaper_Workspace);
            Assert.NotNull(CodeGenEmitter.Shaper_State);
            Assert.NotNull(CodeGenEmitter.Shaper_Context);
            Assert.NotNull(CodeGenEmitter.Shaper_Context_Options);
            Assert.NotNull(CodeGenEmitter.Shaper_ProxyCreationEnabled);
        }
    }
}
