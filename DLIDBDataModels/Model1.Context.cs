﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace DLIDBDataModels
{
    public partial class DownloadsFromDLIDatabaseEntities : DbContext
    {
        public DownloadsFromDLIDatabaseEntities()
            : base("name=DownloadsFromDLIDatabaseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Book_Detail> Book_Detail { get; set; }
        public DbSet<Book_Page_Content> Book_Page_Content { get; set; }
        public DbSet<User> Users { get; set; }
    }
}