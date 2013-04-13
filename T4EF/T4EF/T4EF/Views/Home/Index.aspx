<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<T4EF.Models.Default.Movie>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Movies!
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Home</h2>

      <table>
        <tr>
            <th></th>
            <th>
                Title
            </th>
            <th>
                ReleaseDate
            </th>
        </tr>

    <% foreach (var item in Model) { %>
    
        <tr>
            <td>
                <%: Html.ActionLink("Edit", "Edit", new { id=item.ID }) %> |
            </td>
            <td>
                <%: item.Title %>
            </td>
            <td>
                <%: item.ReleaseDate.Year %>
            </td>
        </tr>
    
    <% } %>

    </table>

</asp:Content>
