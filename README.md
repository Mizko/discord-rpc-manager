# Discord RPC Manager

This is a tool made in C# to quickly and easily make use of Discord's Rich Presence feature.  
It saves your settings to a JSON file so you can just run it, select the app/game/whatever you want to display as your playing status, and boom! your status has been changed.  
If you don't know what Rich Presence is, you can see examples [here](https://discordapp.com/rich-presence) and [here](https://discordapp.com/developers/docs/rich-presence/how-to).  
Discord's [Applications page](https://discordapp.com/developers/applications/) now has a Rich Presence Visualizer (once you created an app)!  

## Requirements

* Windows 7/8/10
* .NET Framework 4.5+, 4.6.1 (Recommended).

### Dependencies (for devs)

* .NET Framework 4.5+ (4.6.1 recommended)
* Newtonsoft.Json (11.0.2+ recommended)
* (already built as DiscordRPC.dll) [discord-rpc-csharp by Lachee](https://github.com/Lachee/discord-rpc-csharp)
* ILMerge

## Usage

### 1. Creating an application

Go to the [Applications page](https://discordapp.com/developers/applications/) and click "Create an application" (note: the '10 apps per account' limit has been changed / removed).  
You can upload an icon for it (optional - it won't show in your playing status) and give it a name (this ***will*** show as "Playing My Application"). Note your app's Client ID, although you will always be able to see it when coming to this page. The description is optional too and not shown.  
Save the changes. 

### 2. Adding images (optional, but the fun part)
On the same page, go to the "Rich Presence" tab (on the left).  
What you will see is the Rich Presence ***Visualizer***, it does **not** allow you to set your playing status by itself!  
You can play with it to see what "details", "state", ..., are.  
Go to the "Art Assets" tab, and under "Rich Presence Assets" and click "Add Images".  
Give each image a name (the only way to rename them later is to reupload them) and click "Upload Assets".  
The name doesn't require a file extension (.png, .jpg).  

**Note:** there *used to be* a distinction between "large" and "small" images.  
Now, any image can be used as a large or small image (or both at the same time).

### 3. Using the RPC Manager

After you have made at least one application, you will be able to use it to change your playing status.  
![User Popout](https://raw.githubusercontent.com/Mizko/discord-rpc-manager/master/Discord%20RPC%20Manager/Assets/UserPopout.png)  
![Discord RPC Manager](https://raw.githubusercontent.com/Mizko/discord-rpc-manager/master/Discord%20RPC%20Manager/Assets/DiscordRPCManager.png)  

**Update Presence:** Once you filled at least the client ID, and details or state, this will shows the informations you've entered as your playing status, or update it. Congrats, you've done it!  
**Remove Presence:** Removes your rich presence/playing status without having to close the software.  

**Name:** A name you will recognize (not shown on Discord) that will allows you to quickly select the playing status you want later on.  
**Add:** Add a new RPC setting to the list.  
**Remove:** Deletes the selected RPC setting.  
**Up/Down Arrow:** Move your setting up or down from the list for easier management.  
**Save:** Save all of your settings. They don't autosave so you can use an existing setting as a template without having to revert it back everytime.  

**Client ID:** It can be found on your application's page, for which there's a link below the text box. How nice of me.  
**Details:** The first line of your playing status (min. 2 characters).  
**State:** The second line of your playing status (min. 2 characters).  
**Large Image Key / Name:** The name of the image to be displayed as large, which is the one you entered when uploading it.  
**Large Image Text:** The text people will see when hovering the large image.  
**Small Image Key / Name:** Same but for the small image (only available when also using a large image).  
**Small Image Text:** Same but for the small image (only available when also using a large image).  

**Start Timestamp:** The time at which you started the activity as a unix timestamp (00:00 elapsed).  
If not set, it will use the current time. If you have no clue what this is, you probably won't need it.  
**End Timestamp:** The time until the activity will end as a unix timestamp (00:00 left).  
This is lesser known, but it actually can do that, and it's pretty cool.  
If not set, this feature isn't being used at all so don't worry.

**Add/Sub x hours:** Adds (substracts if negative) the chosen number, in hours, to the current time and converts it to a unix timestamp (and puts it as the End Timestamp). Useful for the "00:00 left" thing, but negative numbers can also be used for the Start Timestamp.  
Note: Discord seems to only get the hours (and not the day) from a timestamp, which limits us to a -24/+24h time elapsed / left.
