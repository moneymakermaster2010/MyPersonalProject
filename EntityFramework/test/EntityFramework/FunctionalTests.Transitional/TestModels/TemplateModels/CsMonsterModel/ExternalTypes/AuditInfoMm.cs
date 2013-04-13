﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Another.Place
{
    using System;
    using FunctionalTests.ProductivityApi.TemplateModels.CsMonsterModel;

    public class AuditInfoMm
    {
        public AuditInfoMm()
        {
            Concurrency = new ConcurrencyInfoMm();
        }

        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        public ConcurrencyInfoMm Concurrency { get; set; }
    }
}
