﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ConcurrencyModel
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Chassis
    {
        [Timestamp]
        public byte[] Version { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TeamId { get; set; }

        public string Name { get; set; }

        public virtual Team Team { get; set; }
    }
}
