IMPORTANT NOTE:<br/>
well it runs on hopes and dreams, Ill fix it soon.
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

installation and Usage

after installing the tagrmSetup.exe run it once.

restart the computer to apply changes.

open cmd and use it.

usage:  
tagrm easytag <LastFmApiKey> <"PathToMusic">                        - Tag your music with genres  
tagrm easytag WORKS ONLY IF APIkey&libPATH SAVE SETTING IS ON       - Tag your music with genres  
tagrm settings                                                      - View or change settings  
tagrm settings scannscannedfiles true/false                         - scan(true) or dont scan(false) already scanned files  
tagrm settings savedata true/false                                  - save API key and libPath to quick scan   
tagrm settings addgenre <genre>                                     - adds the genre to allowed list  
tagrm settings removegenre <genre>                                  - remove the genre to allowed list  
tagrm settings getartistgenre true/false                            - if its true and if the track doesnt have a genre it will get the genre of the first artist  
tagrm settings show                                                 - shows your settings  
tagrm -help                                                         - Show this help menu  


examples:  
tagrm easyTag 73f12e9e514f9c65fac2g123few58538af798d5 "Z:\music"   
tagrm settings scannscannedfiles true  

Questions or Problems?
Feel free to contact me on Discord:

discord username : slincess
