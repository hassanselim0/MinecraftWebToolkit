# MCWTK: Minecraft Web Toolkit #

## What is this? ##
This is an old project I started back in 2011, I wanted to play Minecraft with my friends but I wanted them to be able to start the server and play on it even if I'm not available, so I decided to make a web interface for the Minecraft Server :)

Since I'm a .Net guy, I decided to make this using ASP.NET MVC, I think it was version 2 at that time, that's why you'll notice that the project structure looks like the old templates even though I'm referencing MVC 5 now :D

Some of the code is hard-coded to my use-case, but I hope I could make it more flexible and modular over time.

## Features ##
- Control Minecraft Server (start, stop, send commands, and see output)
- Update and Show server map (using Overviewer)
- Download any Minecraft Server Jar version (stable or preview) and switch between them
- Edit server.properties file
- Manage Multiple Worlds (including Backup and Restore)
- Assign "Admin" and "Moderator" roles for users, this gives them access to the various parts of the web app
- An alternate authentication system for the Minecraft Server that works with online-mode=false
- ... and HttpProcessWrapper, a general-purpose Web Service for managing and communicating with console apps via a REST API

## How can I deploy this? ##
First you'll need an SQL Server Database (LocalDB is fine), change the connection string "DefaultConnection" in Web.config accordingly. I didn't try SQL Compact but it *should* work, I think.

Next you'll need IIS, just create an application and deploy to it (I use IIS Web Deploy), you'll need to configure Access Control (the security tab in a file/folder property window), this can get a bit confusing and I admit that it still takes me some time to figure out, but googling/binging helps. Just make sure to set the access control for the web app's folder as well as the minecraft server's folder.

Of course make sure Java 6 or 7 or 8 (JRE or JDK) is installed, you can change the "JrePath" value in Web.config to the path of the JRE version you want to use or just leave it as "java" to use the on in your PATH environment variable.

Finally, in Web.config change the value of "McServerPath" to the folder where the server jar exists, and change the value of "McJarFile" to the exact file name of the jar file (eg: minecraft_server.1.8.8.jar).

If you plan on putting this on Azure, then you need an "A2 Basic" VM or better, it could work on an "A1 Basic" but it can get laggy, and nobody wants to lag next to a creeper :D

## How does the Server Map Work? ##
I use a nice program called "Minecraft Overviewer", you can get it from here: http://overviewer.org/  
The code assumes that overviewer is placed in folder called "overviewer" right beside the minecraft server jar file.  
It also assumes that you've set an IIS virtual directory called "mcmap" that points to a folder called "Map" right beside the server jar (this is where the code tells overviewer to put the generated map), or your could create a symbolic link instead.

The code for that functionality is in ServerController (Mapper, StartMapper, MapperProgress) and /Views/Server/Map.cshtml

## What's up with the weird Authentication System? ##
Ok I'll be honest here, I had friends who wanted to try playing the game with me before they could buy it.  
This means I need to set online-mode=false in server.properties, this means the server wont authenticate the players against Mojang's servers, so players who don't have an Mojang account (or do but haven't bought the game) could enter the server. This has a disadvantage though, which is that players could impersonate other players easily, so to counter this I made an elaborate authentication system that's quite amusing.

I wanted a way to make the Minecraft server use the authentication that happens on the website without having to make a server mod. I noticed that when a user attempts to join the server I get in the console his username and IP address. So I decided to make the website record the IP address of logged in users, then I wrote code to detect the player login console message and parse it, then I check if that user is currently logged in and has the same IP Address then he's allowed, otherwise a kick command is send with a message asking the user to login to the website.

This might seem like a pointless feature to most people, and not very secure (since two players in the same local network could still impersonate each other), so you can easily disable this feature by commenting the relevant code in HttpProcessWrapper/McAuthModule.cs, it's the McWrapper.proc_OutputDataReceived method override.

BTW this feature could also be useful in cases where Mojang's servers are down, it's very rare, but it happens :D

## Contribution ##
If you made any improvements to the code feel free to submit a pull request and I'll check it out, this is honestly the first time for me to do something like this, but I'd be very happy if anybody is interested in this project :)

## License ##
Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0)