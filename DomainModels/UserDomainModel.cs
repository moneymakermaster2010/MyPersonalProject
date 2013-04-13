using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainModels.Base;

namespace DomainModels
{
    public class UserDomainModel : DomainModelBase
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
    }
}
