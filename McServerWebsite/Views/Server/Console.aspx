<%@ Page Title="" Language="C#" MasterPageFile="~/Views/ViewMaster.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Server Console
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        var LastMessage = '';
        var ConsoleIntervalHandle;

        $(document).ready(function () {
            commandField.onkeyup = commandKeyUp;

            $.ajaxSetup({ cache: false });

            refresh();
            ConsoleIntervalHandle = setInterval('refresh()', 2000);
        });

        function startServer() {
            $.get('<%= Url.Action("Start", "Server") %>');
        }

        function refresh() {
            $.getJSON('<%= Url.Action("GetOutput", "Server") %>?since=' + LastMessage, function (obj) {
                LastMessage = obj.LastMessage;
                if (obj.Output != "") {
                    ServerOutput.innerHTML += obj.Output;
                    scrollToBottom();
                }
            });
        }

        function sendCommand(command) {
            if (command == undefined) command = commandField.value;
            $.get('<%= Url.Action("SendCommand", "Server") %>?command=' + command, setTimeout('refresh()', 100));
            commandField.value = '';
        }

        function scrollToBottom() {
            //$("#ServerOutput").attr({ scrollTop: $("#ServerOutput").attr("scrollHeight") });
            $("#ServerOutput").animate({ scrollTop: $("#ServerOutput")[0].scrollHeight }, 1000);
        }

        function commandKeyUp(e) {
            if (e.key == "Enter" && commandField.value != "")
                sendCommand(commandField.value);
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Server Console
    </h2>
    <table style="width: 100%; height: calc(100% - 204px);">
        <tr>
            <td>
                <input type="button" value="Start Server" onclick="startServer()" />
                <input type="button" value="Stop Server" onclick="sendCommand('stop')" />
            </td>
        </tr>
        <tr>
            <td>
                <input id="commandField" type="text" style="width: 86%" />
                <input type="button" value="Send" onclick="sendCommand()" style="width: 10%; float: right" />
            </td>
        </tr>
        <tr>
            <td style="height: 100%;">
                <div id="ServerOutput" style="overflow: auto; background-color: #DDDDDD; height: 100%" />
            </td>
        </tr>
    </table>
</asp:Content>
