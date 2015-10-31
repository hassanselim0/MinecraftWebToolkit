<%@ Page Title="" Language="C#" MasterPageFile="~/Views/ViewMaster.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Login
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Login</h2>
    <div style="color: Red; font-weight: bold">
        <%= ViewData["error"] ?? "" %>
    </div>
    <% using (Html.BeginForm("Login", "Account", FormMethod.Post, new { id = "LoginForm" }))
       { %>
    <input type="hidden" name="returnUrl" value="<%= ViewData["returnUrl"] ?? Request.QueryString["ReturnUrl"] %>" />
    <label>
        Username:</label><br />
    <input name="username" type="text" /><br />
    <label>
        Password:</label><br />
    <input name="password" type="password" /><br />
    <br />
    <input type="submit" value="Login" />
    (don't have an account?
    <%= Html.ActionLink("create a new one here", "Create")%>)
    <% } %>
</asp:Content>
