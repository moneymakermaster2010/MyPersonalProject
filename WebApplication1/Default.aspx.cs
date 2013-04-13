using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Web.Services;
using System.Threading;
using Services;

namespace WebApplication1
{
    public partial class _Default : System.Web.UI.Page
    {
        static int pageNumbnerBeingDownloaded;
        static int startPage;
        static int endPage;
        byte[] imageBytes = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        public void OnClickGetDLIBookButton(object sender, EventArgs e)
        {
            Thread downloadThread = new Thread(DownloadAndConvertToPDF);
            downloadThread.Start();
        }

        public void DownloadAndConvertToPDF()
        {
            String baseUrl = DLIBaseURLTextBox.Text;
            String finalUrl = String.Empty;
            startPage = Convert.ToInt32(StartPageDownloadTextBox.Text);
            endPage = Convert.ToInt32(EndPageDownloadTextBox.Text);
            ToatalPageNumberLabel.Text = endPage.ToString();
            using (BookService bookService = new BookService())
            {
                int bookID;
                long pageNumber;
                bookService.GetBook_DetailByURL(baseUrl, out bookID, out pageNumber);
                bookID = (bookID == 0) ? bookService.GetNextBookID() : bookID;
                pageNumber = (pageNumber == 0) ? bookService.GetNextPageNumber(bookID) : pageNumber;
                for (pageNumbnerBeingDownloaded = startPage; pageNumbnerBeingDownloaded < endPage; pageNumbnerBeingDownloaded++)
                {
                    finalUrl = baseUrl + pageNumbnerBeingDownloaded.ToString("00000000") + ".tif";
                    HttpWebRequest dLIRequest = WebRequest.Create(finalUrl) as HttpWebRequest;
                    HttpWebResponse dLIResponse = dLIRequest.GetResponse() as HttpWebResponse;
                    GetImageFromStream(dLIResponse, pageNumbnerBeingDownloaded);
                    //bookService.SaveImageToBookContent(imageBytes, bookID, pageNumber);
                }
            }
            //CreatePDFFile(pageCount);
        }

        [WebMethod()]
        public static String UpdateDownloadCount()
        {
            //DownloadingPageNumberLabel.Text = pageNumbnerBeingDownloaded.ToString();
            return pageNumbnerBeingDownloaded >= endPage ? "null" : pageNumbnerBeingDownloaded.ToString();
        }

        private void GetImageFromStream(HttpWebResponse dLIResponse, int imageNumber)
        {
            Stream responseStream = dLIResponse.GetResponseStream();
            imageBytes = null;
            string saveLocation = Server.MapPath("~/Images/Image" + imageNumber + ".bmp");// @"C:\someImage.jpg";
            using (BinaryReader br = new BinaryReader(responseStream))
            {
                imageBytes = br.ReadBytes(500000);
                br.Close();
            }
            responseStream.Close();
            dLIResponse.Close();

            FileStream fs = new FileStream(saveLocation, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            try
            {
                bw.Write(imageBytes);
            }
            finally
            {
                fs.Close();
                bw.Close();
            }
        }

        private void CreatePDFFile(int pageCount)
        {
            // creation of the document with a certain size and certain margins  
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 0, 0, 0, 0);

            // creation of the different writers  
            iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new System.IO.FileStream(Server.MapPath("~/App_Data/result.pdf"), System.IO.FileMode.Create));

            document.Open();
            for (int i = 1; i < pageCount; i++)
            {
                // load the tiff image and count the total pages  
                System.Drawing.Bitmap bm = new System.Drawing.Bitmap(Server.MapPath("~/Images/Image" + i + ".bmp"));
                int total = bm.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page);

                iTextSharp.text.pdf.PdfContentByte cb = writer.DirectContent;
                for (int k = 0; k < total; ++k)
                {
                    bm.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, k);
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(bm, System.Drawing.Imaging.ImageFormat.Bmp);
                    // scale the image to fit in the page  
                    img.ScalePercent(72f / img.DpiX * 100);
                    img.SetAbsolutePosition(0, 0);
                    cb.AddImage(img);
                    document.NewPage();
                }
            }
            document.Close(); 
        }
    }
}
