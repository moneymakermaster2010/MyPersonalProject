<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="WebApplication1._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <script src="Scripts/jquery-1.4.1.js" type="text/javascript"></script>
    <style type="text/css">
        .pageStatus
        {
            padding-top: 5px;
            width:20%;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
<asp:ScriptManager ID="SManager" EnablePageMethods="true" runat="server"></asp:ScriptManager>
    <script type="text/javascript">
        function UpdateDownloadCount() {
            $.ajax({
                type: "POST",
                url: "Default.aspx/UpdateDownloadCount",
                data: "{}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    if (msg.d) {
                        for (var i = 0; i < msg.d.length; i++ ) {
                            var elementWithStatus = msg.d[i].split(",");
                            var elementsStatus = document.getElementById(elementWithStatus[0]);
                            elementsStatus.innerHTML = elementWithStatus[1];
                        }
                    }
                    else {
                        document.getElementById('MainContent_TotalStatusLabel').innerHTML = "Processing Completed";
                        clearInterval(downloadCountIntervalID);
                    }
                }
            });
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
        <asp:Button ID="GetDLIBookButton" Text="Download" OnClick="OnClickGetDLIBookButton" runat="server" />
        <br />
        <br />
        <asp:PlaceHolder ID="DownoadingPagesPlaceHolder" runat="server">
        
        </asp:PlaceHolder>
        <h2>
        <asp:Label ID="TotalStatusLabel" Text="" runat="server" />
        </h2>
    </p>
    <p>
        You can also find <a href="http://go.microsoft.com/fwlink/?LinkID=152368&amp;clcid=0x409"
            title="MSDN ASP.NET Docs">documentation on ASP.NET at MSDN</a>.
    </p>
    
    <br />
    <br />
    <asp:Label Text="" ID="ErrorLabel" runat="server" />
     <%--OnClick="OnClickTestPageMethodButton"--%>
</asp:Content>
