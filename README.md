# Discord RPC Manager

This is a tool made in C# to quickly and easily make use of Discord's Rich Presence feature.  
It saves your settings to a JSON file so you can just run it, select the app/game/whatever you want to display as your playing status, and boom! your status has been changed.  
If you don't know what Rich Presence is, you can find examples [here](https://discord.com/rich-presence).  
Discord's [Applications page](https://discord.com/developers/applications/) now has a Rich Presence Visualizer for your apps!  

## Requirements

* Windows 7/8/10
* .NET Framework 4.7.1 (Recommended).

### Dependencies (for devs)

* .NET Framework 4.7.1
* Newtonsoft.Json 11.0.2+
* [discord-rpc-csharp by Lachee](https://github.com/Lachee/discord-rpc-csharp)
* WebView2

## Usage

### 1. Creating an application

Go to Discord's [Applications page](https://discord.com/developers/applications/) and click "New Application".  
You can upload an icon for it (optional - it won't show in your playing status) and give it a name (this ***will*** show as "Playing My Application").  

### 2. Adding images (optional, yet fun part)
On your application's page, go to the "Rich Presence" tab.  
Go to the "Art Assets" tab, and under "Rich Presence Assets" click "Add Images".  
Give each image a name (the only way to rename them later is to reupload them) and click "Upload Assets".  
The name doesn't require a file extension (.png, .jpg).  

**Note:** any image can be used both as a "large" or "small" image.

### 3. Using the RPC Manager

After you have made at least one application, you will be able to use it to change your playing status.  
![User Popout](https://raw.githubusercontent.com/Mizko/discord-rpc-manager/master/Discord%20RPC%20Manager/Assets/UserPopout.png)  
![Discord RPC Manager](https://raw.githubusercontent.com/Mizko/discord-rpc-manager/master/Discord%20RPC%20Manager/Assets/DiscordRPCManager.png)  
