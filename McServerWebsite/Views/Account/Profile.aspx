<%@ Page Title="" Language="C#" MasterPageFile="~/Views/ViewMaster.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Profile
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Your Profile
    </h2>
    <table>
        <tr>
            <th style="text-align: right">
                Minecraft Username:
            </th>
            <td>
                <%= Model.UserName %>
            </td>
        </tr>
        <tr>
            <th style="text-align: right">
                E-mail:
            </th>
            <td>
                <%= Model.Email %>
            </td>
        </tr>
        <tr>
            <th style="text-align: right">
                Roles:
            </th>
            <td>
                <% if (Request.QueryString["username"] != null)
                       foreach (var role in Roles.GetRolesForUser(Request.QueryString["username"]))
                           Response.Write("[" + role + "] ");
                   else
                       foreach (var role in Roles.GetRolesForUser())
                           Response.Write("[" + role + "] "); %>
            </td>
        </tr>
        <tr>
            <th style="text-align: right">
                IP Adress:
            </th>
            <td>
                <%
                    var server = McServerWebsite.MvcApplication.McServer;
                    string ip;
                    if (server.UserIPs.TryGetValue(Model.UserName, out ip))
                        if (DateTime.UtcNow.Subtract(server.UserLastPing[Model.UserName]).TotalMinutes > 2)
                            Response.Write("Expired");
                        else
                            Response.Write(ip);
                    else
                        Response.Write("Not Logged In");
                %>
            </td>
        </tr>
        <% if (Request.QueryString["username"] == null || Roles.IsUserInRole("Admin"))
           { %>
        <tr>
            <td colspan="2">
                Change Password:
                <% using (Html.BeginForm("ChangePassword", "Account"))
                   { %>
                <% if (TempData["error"] != null)
                   { %>
                <div style="color: red; font-weight: bold"><%= TempData["error"] %></div>
                <% } %>
                <% if (Request.QueryString["username"] != null)
                       Response.Write(Html.Hidden("username", Request.QueryString["username"])); %>
                old:
                <input type="password" name="oldPass" />
                new:
                <input type="password" name="newPass" />
                <input type="submit" value="Change" />
                <% } %>
            </td>
        </tr>
        <% } %>
        <%--<tr>
            <td colspan="2">
                <% if (!Model.IsApproved)
                   { %>
                <span style="color: Red; font-weight: bold">Your Account is not Approved yet !</span><br />
                If you're in a hurry, then e-mail me or send me a tweet and I'll approve you account.
                <% }
                   else if (Session["WhitelistUntil"] == null)
                   {
                       if (Request.QueryString["username"] == null)
                           Response.Write(Html.ActionLink("White-list Me", "Whitelist"));
                       else
                           Response.Write(Html.ActionLink("White-list " + Request.QueryString["username"],
                               "Whitelist", new { username = Request.QueryString["username"] }));
                   }
                   else
                   {
                       Response.Write("Whitelisted! Seconds left for you to login: "
                           + ((DateTime)Session["WhitelistUntil"]).Subtract(DateTime.Now).TotalSeconds);
                   } %>
            </td>
        </tr>--%>
    </table>
</asp:Content>
