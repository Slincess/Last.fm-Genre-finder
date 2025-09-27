IMPORTANT NOTE:<br/>
This tool uses tags from Last.fm to determine the genre of your music. Only the following genres are supported all others will be ignored.

<details> <summary>Click to see the full list of supported genres</summary>
rock, indie-rock, pop, indie-pop, hip-hop, rap, trap, drill, r&b, soul, funk,
jazz, blues, metal, heavy-metal, death-metal, black-metal, hardcore, post-hardcore,
alternative-rock, grunge, progressive-rock, psychedelic-rock, garage-rock, classic-rock,
punk, punk-rock, pop-punk, new-wave, synth-pop, folk, folk-rock, indie-folk, country,
alt-country, bluegrass, reggae, dub, dancehall, ska, latin, reggaeton, bachata, salsa,
merengue, cumbia, k-pop, j-pop, city-pop, electronic, edm, house, deep-house, techno,
minimal-techno, progressive-house, drum-and-bass, dubstep, brostep, trance, psytrance,
hardstyle, hardcore-techno, ambient, downtempo, chillout, lofi, chillhop, electro,
industrial, noise, experimental, glitch, trip-hop, breakbeat, grime, uk-garage, 2-step,
disco, italo-disco, shoegaze, dream-pop, math-rock, post-rock, emo, screamo, gospel,
christian, opera, classical, baroque, romantic-period, modern-classical, soundtrack,
film-score, anime-score, video-game-music, acoustic, instrumental, spoken-word, world,
afrobeat, krautrock
</details>

This tool reads your music library directly from the path you give it.
No Docker or servers

# installation and Usage

after installing tagrmSetup.exe run it once.

restart the computer to apply changes.

open cmd and use it.

## usage:

<br/>
	
`tagrm easytag <"PathToMusic"> <LastFmApiKey>` 												- Tag your music with genres  
`tagrm settings`                                                      - View or change settings  
`tagrm settings scannscannedfiles <true/false>`                         - scan(true) or dont scan(false) already scanned files  
`tagrm settings savedata <true/false>`                                  - save API key and libPath to quick scan   
`tagrm settings addgenre <genre>`                                     - adds the genre to allowed list  
`tagrm settings removegenre <genre>`                                  - remove the genre from allowed list  
`tagrm settings getartistgenre true/false`                            - if its true and if the track doesnt have a genre it will get the genre of the first artist  
`tagrm settings show`                                                 - shows your settings  
`tagrm -help`                                                         - Show this help menu  


examples:  
tagrm easyTag "Z:\music" 73f12e9e514f9c65fac2g123few58538af798d5   
tagrm settings scannscannedfiles true  

# How to compile for yourself
## Prerequisites
- **.NET SDK 7.0** or later installed (needed to compile the project)
  - Download here: [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
Make sure the .NET SDK is installed and added to your system PATH so the dotnet command works in CMD. <br/>
- **TagLibSharp 2.3.0** <br/>
  - Install it via dotnet: `dotnet add package TagLibSharp --version 2.3.0` <br/>
- **Newtonsoft.json 13.0.3** <br/>
  - Install it via dotnet: `dotnet add package Newtonsoft.Json --version 13.0.3` <br/>

- clone the repo:
<pre> git clone https://github.com/Slincess/Last.fm-Genre-finder.git </pre>
move to the folder where you cloned the repo:
<pre> cd RepoClonePath/MusicTagFinder </pre>
example:
`C:\Users\<yourUsername>\source\repos\MusicTagFinder\MusicTagFinder` this is where .sln file is.<br/>

- compile it:
<pre>dotnet publish -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true </pre>
the exe will be in
`C:\Users\<yourUsername\source\repos\MusicTagFinder\MusicTagFinder\bin\Release\net8.0\win-x64\publish`

- move it into Program Files directory:<br/>
create a folder called "tagrm" not TAGRM or Tagrm or anything else.
and move the exe into the folder you just created<br/>
rename the exe to : tagrm

- press windows key + r on your keyboard to open the run box, then type `sysdm.cpl` press enter.
- In the resulting window in the "Advanced" tab, click the "Environment variables" button.
- In the next window under "System variables", select "Path", then press "Edit".
- Add the path to tagrm. example `C:\Program Files\tagrm\` .

you may have to restart your pc for it to work.

Questions or Problems?
Feel free to contact me on Discord:

discord username : slincess
