<%@ Page Title="" Language="C#" MasterPageFile="~/Views/ViewMaster.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Server Map
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        var ProgressIntervalHandle;

        $(document).ready(function () {
            $.ajaxSetup({ cache: false });
            ChangeProgress();
            ProgressIntervalHandle = setInterval("ChangeProgress()", 1000);
            <% if (Roles.IsUserInRole("Moderator"))
               { %>
            var iframe = $(document.getElementsByTagName("iframe")[0]);
            iframe.height(iframe.height() - 48);
            <% } %>
        });

        function StartMapper() {
            $.get('<%= Url.Action("StartMapper") %>');
            ChangeProgress();
            ProgressIntervalHandle = setInterval("ChangeProgress()", 1000);
        }

        function ChangeProgress() {
            $.get('<%= Url.Action("MapperProgress") %>', function (result) {

                if (result == "")
                    $("#MapperProgress").text("");
                else
                    $("#MapperProgress").text("(" + result + ")");

                if (result == "Completed" || result == "")
                    clearInterval(ProgressIntervalHandle);
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Server Map
    </h2>
    <% if (Roles.IsUserInRole("Moderator"))
       { %>
    <a href="#" onclick="StartMapper(); return false;">Update Map</a>
    <span id="MapperProgress"></span>
    <br />
    <br />
    <% } %>
    <iframe src="/mcmap" style="width: 100%; height: calc(100% - 140px);" />
</asp:Content>
