//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DLIDBDataModels
{
    public partial class User
    {
        public User()
        {
            this.CreatedByBook_Detail = new HashSet<Book_Detail>();
            this.UpdatedByBook_Detail = new HashSet<Book_Detail>();
            this.CreatedByBook_Page_Content = new HashSet<Book_Page_Content>();
            this.UpdatedByBook_Page_Content = new HashSet<Book_Page_Content>();
        }
    
        public int UserID { get; set; }
        public string UserName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<bool> Active { get; set; }
    
        public virtual ICollection<Book_Detail> CreatedByBook_Detail { get; set; }
        public virtual ICollection<Book_Detail> UpdatedByBook_Detail { get; set; }
        public virtual ICollection<Book_Page_Content> CreatedByBook_Page_Content { get; set; }
        public virtual ICollection<Book_Page_Content> UpdatedByBook_Page_Content { get; set; }
    }
    
}
