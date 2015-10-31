<%@ Page Title="" Language="C#" MasterPageFile="~/Views/ViewMaster.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Create Account
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="../../Scripts/jquery.validate.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $("#CreateForm").validate({
                rules: {
                    password2: { equalTo: "#pass1" }
                }
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Create Account</h2>
    <div style="color: Red; font-weight: bold">
        <%= ViewData["error"]?? "" %>
    </div>
    <% using (Html.BeginForm("Create", "Account", FormMethod.Post, new { id = "CreateForm" }))
       { %>
    <label>
        Username:</label><br />
    <input name="username" type="text" class="required" /><br />
    <label>
        Password:</label><br />
    <input id="pass1" name="password" type="password" class="required" /><br />
    <label>
        Confirm Password:</label><br />
    <input id="pass2" name="password2" type="password" class="required" /><br />
    <label>
        E-Mail:</label><br />
    <input name="email" type="text" class="required email" /><br />
    <br />
    <input type="submit" value="Create" />
    (already have an account?
    <%= Html.ActionLink("login here", "Login")%>)
    <% } %>
</asp:Content>
