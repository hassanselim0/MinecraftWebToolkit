﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/ViewMaster.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Welcome
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Welcome to my Minecraft Server</h2>
    <p>
        Currently this server is mainly for me and my friends.<br />
        This server doesn&#39;t verify your login against minecraft.net (ie: offline-mode),
        which means that you can still connect to my server even if minecraft.net is down,
        it also means that you don&#39;t have to login from the minecraft launcher to be
        able to play, I did this so I can give my friends a chance to try playing minecraft
        on multiplayer before they decide to purchase the game.</p>
    <p>
        Not verifying against minecraft.net causes a security threat where hackers can send
        a false username and thus impersonating a user (connect as that user), so to avoid
        this I made a white-list system that works as follows:</p>
    <ol>
        <li>Create an account on this website, and wait for me to approve it (sorry, I have
            to do this because this server can only handle very few players)</li>
        <li>Download a Minecraft Name Changer, I recommend 
            <a href="/MinecraftNameChanger.exe">this one</a></li>
        <li>Open the Name Changer and enter your username for this website when asked for the
            name you want</li>
        <li>If you used the Name Changer I recommeded, you&#39;ll have to close it then run
            the Minecraft Launcher, others automatically open the launcher for you.</li>
        <li>When the launcher opens, don&#39;t type a password then press the Login button</li>
        <li>Ofcourse the login will fail, and a &quot;Play Offline&quot; button will appear,
            click on it to launch the game in offline mode</li>
        <li>Keep this website open while you are connecting to the server</li>
        <li>Click on the &quot;Mulitplayer&quot; button and type &quot;vm.hassanselim.me&quot;
            in the text box then press &quot;Connect&quot; and have fun!</li>
    </ol>
    <p>
        In case you bought the game and have a payed account on minecraft.net, then you
        can ignore the part about the Name Changer, but remember to enter your Minecraft
        username correctly when creating an acocunt on this website, and do a proper login
        in the Minecraft Launcher.</p>
    <p>
        I know it&#39;s a bit complicated and maybe annoying, but that&#39;s the ONLY way
        I could think of that would allow people who haven&#39;t bought the game yet to
        play on my server and at the same time avoid impersonation by hackers.</p>
    <p>
        Have Fun,<br />
        Hassan Selim<br />
        <a href="http://www.hassanselim.me/">hassanselim.me</a><br />
        <a href="http://twitter.com/hassanselim0">@hassanselim0</a></p>
</asp:Content>
