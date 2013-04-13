using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DLIDBDataModels;
using Repositories;

namespace Services
{
    public class BookService  :IDisposable
    {
        # region Global Vaiables for Repositories
            Book_DetailRepository book_DetailRepository = null;
            Book_Page_ContentRepository book_Page_ContentRepository = null;
        # endregion

        public Book_Detail GetBook_DetailByID(int book_DetailByID)
        {
            book_DetailRepository = new Book_DetailRepository();
            return book_DetailRepository.GetByID(book_DetailByID);
        }

        public int GetNextBookID()
        {
            book_DetailRepository = new Book_DetailRepository();
            return book_DetailRepository.GetAll().Count + 1;
        }

        public long GetNextPageNumber(int bookID)
        {
            book_Page_ContentRepository = new Book_Page_ContentRepository();
            return book_Page_ContentRepository.GetAll().Where(pageCntent => pageCntent.BookID == bookID).ToList().Count + 1;
        }

        public void GetBook_DetailByURL(string baseUrl, out int bookID, out long pageNumber)
        {
            book_DetailRepository = new Book_DetailRepository();
            Book_Page_Content bookPageContent = book_DetailRepository.GetByUrl(baseUrl);
            bookID = bookPageContent != null ? bookPageContent.BookID : 0;
            pageNumber = bookPageContent != null ? bookPageContent.PageNumber : 0;
        }

        public bool SaveImageToBookContent(byte[] pageContent, int bookID, long pageNumber)
        {
            book_Page_ContentRepository = new Book_Page_ContentRepository();
            Book_Page_Content book_Page_Content = book_Page_ContentRepository.GetAll().Where(bookContent => bookContent.BookID == bookID && bookContent.PageNumber == pageNumber).FirstOrDefault();
            int recordsEffected;
            //If this record does not exist already, then add this record or else update the same record.
            if (book_Page_Content == null)
            {
                book_Page_Content = new Book_Page_Content();
                book_Page_Content.BookID = bookID;
                book_Page_Content.PageNumber = pageNumber;
                book_Page_Content.Content = pageContent;

                recordsEffected = book_Page_ContentRepository.Create(book_Page_Content);
            }
            else
            {
                book_Page_Content.Content = pageContent;
                recordsEffected = book_Page_ContentRepository.Update(book_Page_Content);
            }
            //If no of records effected is not zero, then creation or updation is successful.
            return recordsEffected > 0;
        }

        public void Dispose()
        {
            DisposeRepository(book_DetailRepository);
            DisposeRepository(book_Page_ContentRepository);
        }

        public void DisposeRepository(object repository)
        {
            if (repository != null)
            {
                (repository as IDisposable).Dispose();
            }
        }
    }
}
