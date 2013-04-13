<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="WebApplication1._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
<asp:ScriptManager ID="SManager" EnablePageMethods="true" runat="server"></asp:ScriptManager>
    <script type="text/javascript">
        var downloadCountIntervalID;// = setInterval(function () { UpdateDownloadCount(); }, 5000);
        function DownloadButtonClick() {
            downloadCountIntervalID = setInterval(function () { UpdateDownloadCount(); }, 1000);
        }

        function UpdateDownloadCount() {
            PageMethods.UpdateDownloadCount(onSuccess, onFailure);
        }

        function onSuccess(result) {
            if (result == "null") {
                clearInterval(downloadCountIntervalID);
            }
            else {
                document.getElementById('MainContent_DownloadingPageNumberLabel').innerHtml = result;
            }
        }

        function onFailure(error) {

        }
    </script>
    <h2>
        Welcome to ASP.NET!
    </h2>
    <p>
        Enter DLI Base URL: <asp:TextBox ID="DLIBaseURLTextBox" runat="server"></asp:TextBox>
        <br />
        <div>
            <div style="display:inline" width="50%">
                Enter Start page to download: 
                <asp:TextBox ID="StartPageDownloadTextBox" runat="server" />
             </div>
             <div style="display:inline" width="50%">
                Enter End page to download: 
                <asp:TextBox ID="EndPageDownloadTextBox" runat="server" />
            </div>
        </div>
        <br />
        <asp:Button ID="GetDLIBookButton" Text="Download" OnClientClick = "DownloadButtonClick()" OnClick="OnClickGetDLIBookButton" runat="server" />
        Downloading Page <asp:Label ID="DownloadingPageNumberLabel" runat="server"></asp:Label> of 
        <asp:Label ID="ToatalPageNumberLabel" runat="server"></asp:Label>
        To learn more about ASP.NET visit <a href="http://www.asp.net" title="ASP.NET Website">www.asp.net</a>.
    </p>
    <p>
        You can also find <a href="http://go.microsoft.com/fwlink/?LinkID=152368&amp;clcid=0x409"
            title="MSDN ASP.NET Docs">documentation on ASP.NET at MSDN</a>.
    </p>
</asp:Content>
