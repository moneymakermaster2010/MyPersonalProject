using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DLIDBDataModels;
using Repositories.Base;

namespace Repositories
{
    public class Book_DetailRepository  : RepositoryBase<Book_Detail>
    {
        public Book_Page_Content GetByUrl(string baseUrl)
        {
            Book_Detail bookDetail = GetAll().Where(bookDeatil => bookDeatil.BookSourceURL == baseUrl).FirstOrDefault();
            if (bookDetail != null)
            {
                return bookDetail.Book_Page_Content.FirstOrDefault();
            }
            return null;
        }
    }
}
