<%@ Page Title="" Language="C#" MasterPageFile="~/Views/ViewMaster.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Admin Page
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        td, th {
            text-align: center;
            border-width: 1px;
            border-style: solid;
        }
    </style>
    <script type="text/javascript">
        var ProgressIntervalHandle;

        $(document).ready(function () {
            $.ajaxSetup({ cache: false });
            ChangeProgress();
            ProgressIntervalHandle = setInterval("ChangeProgress()", 1000);
        });

        function StartUpdate() {
            $.get('<%= Url.Action("UpdateServer") %>');
            ChangeProgress();
            ProgressIntervalHandle = setInterval("ChangeProgress()", 1000);
        }

        function ChangeProgress() {
            $.get('<%= Url.Action("UpdateProgress") %>', function (result) {

                if (result == "")
                    $("#UpdateProgress").text("");
                else
                    $("#UpdateProgress").text("(" + result + ")");

                if (result == "Completed" || result == "")
                    clearInterval(ProgressIntervalHandle);
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Admin Page
    </h2>
    Server Status:
    <%= ViewData["ServerStatus"] %><br />
    <br />
    Server Admin Commands:<br />
    <%= Html.ActionLink("Kill Server", "KillServer") %><br />
    <a href="#" onclick="StartUpdate(); return false;">Update Server</a>
    <span id="UpdateProgress"></span>
    <br />
    <br />
     <% using (Html.BeginForm("SetPingIP", "Admin"))
       { %>
    Set Ping IP to Ignore:
    <input type="text" name="ip" value="<%= ConfigurationManager.AppSettings["AzurePingIP"] %>" />
    <input type="submit" value="Set" />
    <% } %>
    <br />
    <br />
    <% using (Html.BeginForm("ApproveAccount", "Admin"))
       { %>
    Approve User:
    <input type="text" name="username" />
    <input type="submit" value="Approve" />
    <% } %>
    <br />
    <br />
    <% using (Html.BeginForm("AssignRole", "Admin"))
       { %>
    Assign User:
    <input type="text" name="username" />
    to role:
    <input type="text" name="role" />
    <input type="submit" value="Assign" />
    <% } %>
    <br />
    <br />
    <% using (Html.BeginForm("UnlockAccount", "Admin"))
       { %>
    Unlock User:
    <input type="text" name="username" />
    <input type="submit" value="Unlock" />
    <% } %>
    <br />
    <br /><% using (Html.BeginForm("DeleteAccount", "Admin"))
       { %>
    Delete User:
    <input type="text" name="username" />
    <input type="submit" value="Delete!" />
    <% } %>
    <br />
    <br />
    List of all User Accounts:
    <table width="100%" cellspacing="0" cellpadding="4" style="border-collapse: collapse">
        <tr>
            <th>
                Username
            </th>
            <th>
                E-Mail
            </th>
            <th>
                Approved?
            </th>
            <th>
                Locked Out?
            </th>
            <th>
                Last Activity
            </th>
            <th>
                Roles
            </th>
        </tr>
        <% foreach (MembershipUser user in Membership.GetAllUsers())
           { %>
        <tr>
            <td>
                <%= Html.ActionLink(user.UserName, "Profile", "Account", new { username = user.UserName }, null) %>
            </td>
            <td>
                <%= user.Email %>
            </td>
            <td>
                <%= user.IsApproved %>
            </td>
            <td>
                <%= user.IsLockedOut %>
            </td>
            <td>
                <%= user.LastActivityDate %>
            </td>
            <td>
                <% foreach (var role in Roles.GetRolesForUser(user.UserName))
                       Response.Write("[" + role + "] "); %>
            </td>
        </tr>
        <% } %>
    </table>
    <br />
</asp:Content>
