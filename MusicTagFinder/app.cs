using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MusicTagFinder
{
    internal class App
    {
        private List<string> MusicFiles = new List<string>();
        private string APIkey;
        private string MusicLib;

        private int FoundTagsCount = 0;
        private int NotFoundTagsCount = 0;
        private List<string> TagsNotFoundMusics = new List<string>();
        private List<string> ScannedMusicFiles = new List<string>();

        private Settings settings = new Settings();

        private HashSet<string> AllowedGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
        "rock","indie-rock","pop","indie-pop","hip-hop","rap","trap","drill","r&b","soul","funk","jazz",
        "blues","metal","heavy-metal","death-metal","black-metal","hardcore","post-hardcore","alternative-rock",
        "grunge","progressive-rock","psychedelic-rock","garage-rock","classic-rock","punk","punk-rock","pop-punk",
        "new-wave","synth-pop","folk","folk-rock","indie-folk","country","alt-country","bluegrass","reggae","dub",
        "dancehall","ska","latin","reggaeton","bachata","salsa","merengue","cumbia","k-pop","j-pop","city-pop",
        "electronic","edm","house","deep-house","techno","minimal-techno","progressive-house","drum-and-bass",
        "dubstep","brostep","trance","psytrance","hardstyle","hardcore-techno","ambient","downtempo","chillout",
        "lofi","chillhop","electro","industrial","noise","experimental","glitch","trip-hop","breakbeat","grime",
        "uk-garage","2-step","disco","italo-disco","shoegaze","dream-pop","math-rock","post-rock","emo","screamo",
        "gospel","christian","opera","classical","baroque","romantic-period","modern-classical","soundtrack",
        "film-score","anime-score","video-game-music","acoustic","instrumental","spoken-word","world","afrobeat",
        "krautrock"
        };

        public async Task Run(string[] args)
        {
            LoadSettings();
            string Command = args[0].ToLower();
            switch (Command)
            {
                case ("easytag"):
                    if (args.Length >= 3 && args[1] != null && args[2] != null)
                    {
                        APIkey = args[1];
                        MusicLib = args[2];

                        if (settings.sSaveAPIkeyandPath == true)
                        {
                            settings.sAPIKey = APIkey;
                            settings.sMusicLibPath = MusicLib;
                        }
                         await Start();
                    }
                    else if(Directory.Exists(MusicLib) && settings.sAPIKey != null) { await Start(); }
                    else { Console.WriteLine($"APIkey or Path is missing"); return; }
                    break;
                case ("settings"):
                    if (args.Length >= 3)
                        HandleSettings(args);
                    else { Console.WriteLine("missing arguments (-help for commands)"); }
                    break;
                case ("-help"):
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine("unknown command");
                    break;
            }

        }

        private async Task GetAllMusicFiles(string MusicLibPath)
        {
            string[] allMp3Files = Directory.GetFiles(MusicLibPath, "*.mp3", SearchOption.AllDirectories);
            MusicFiles.AddRange(allMp3Files);

            foreach (var item in MusicFiles)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine(" ");
            Console.WriteLine($"we found {MusicFiles.Count} mp3 files");
            Console.WriteLine(" ");
            Console.WriteLine("Directories searched: ");
            foreach (var i in Directory.GetDirectories(MusicLibPath))
            {
                Console.WriteLine($"{Directory.GetFiles(i).Length} mp3 in {i}");
            }
            Console.WriteLine("Writing the tags in 3 seconds");
            await Task.Delay(3000);

        }

        private async Task Start()
        {

            await GetAllMusicFiles(MusicLib);
            await GetTag();
            Console.WriteLine(" ");
            Console.WriteLine("Finished Tags are saved in Genre section");
            Console.WriteLine(" ");
            Console.WriteLine($"found tags for {FoundTagsCount} music and not for {NotFoundTagsCount}");
            if (NotFoundTagsCount > 0)
            {
                Console.WriteLine("do you wanna see which music/s we couldn't find Tag for? (y/n)");
                string Answer = Console.ReadLine();
                if (string.Equals(Answer, "y") || string.Equals(Answer, "yes"))
                {
                    Console.WriteLine($"Not found for:");

                    foreach (var i in TagsNotFoundMusics)
                    {
                        Console.WriteLine(i);
                    }
                    Console.WriteLine("press any key to escape");
                    Console.ReadKey();
                }
            }
            Console.WriteLine("press any key to escape");
            Console.ReadKey();
        }
        private async Task GetTag()
        {
            foreach (string item in MusicFiles)
            {
                if (!settings.sScann_ScannedMusicFiles) // if scanned music shoulnd be scanned.
                {
                    if (!ScannedMusicFiles.Contains(item))
                    {
                        await GetTagFile(item);
                    }
                }
                else
                {
                    await GetTagFile(item);
                }

            }
        }

        private async Task GetTagFile(string item)
        {
            try
            {
                var tfile = TagLib.File.Create(item);
                string title = tfile.Tag.Title;
                string artist = tfile.Tag.FirstArtist;

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist))
                {
                    NotFoundTagsCount++;
                    TagsNotFoundMusics.Add(Path.GetFileName(item));
                    return;
                }   


                string url = $"http://ws.audioscrobbler.com/2.0/?method=track.getTopTags&artist={HttpUtility.UrlEncode(artist)}&track={HttpUtility.UrlEncode(title)}&api_key={APIkey}&format=json";

                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync(url);
                    JObject json = JObject.Parse(response);


                    var tags = json["toptags"]?["tag"];
                    if (tags != null)
                    {
                        var genreNames = tags
                        .Select(tag => (string)tag["name"])
                        .Where(name => AllowedGenres.Contains(name))
                        .ToArray();
                        foreach (var tag in tags)
                        {
                            Console.WriteLine($"Tag/s Found for {tfile.Tag.Title} :");
                            Console.WriteLine($"Tag: {tag["name"]} - Count: {tag["count"]}");
                        }
                        tfile.Tag.Genres = genreNames;
                        FoundTagsCount++;
                        if (!settings.sScann_ScannedMusicFiles && !settings.sScannedMusicFiles.Contains(item))
                        {
                            settings.sScannedMusicFiles.Add(item);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No tags to be found.");
                        NotFoundTagsCount++;
                        if(!TagsNotFoundMusics.Contains(title))
                            TagsNotFoundMusics.Add(title);
                    }
                    tfile.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"a problem {ex}");
                Console.WriteLine("No connaction or connaction failed.");
                return;
            }
        }

        private void HandleSettings(string[] args)
        {
            //tagrm settings addGenre kpop
            switch (args[1].ToLower())
            {
                case ("addgenre"):
                    if (!AllowedGenres.Contains(args[2]))
                    {
                        AllowedGenres.Add(args[2]);
                        Console.WriteLine($"{args[2]} is added to allowed genres");
                        SaveSettings();
                    }
                    else { Console.WriteLine($" {args[2]} already exist in allowed genres"); }
                    break;
                case ("removegenre"):
                    AllowedGenres.Remove(args[2]);
                    SaveSettings();
                    Console.WriteLine($"{args[2]} is removed from allowed genres");
                    break;
                case ("scannscannedfiles"):
                    if (args[2].ToLower() == "true")
                        settings.sScann_ScannedMusicFiles = true;
                    else if (args[2].ToLower() == "false")
                        settings.sScann_ScannedMusicFiles = false;
                    else
                    {
                        Console.WriteLine("only true or false. true = it will scan scanned music files. false = it wont scan scanned files.");
                        return;
                    }
                    SaveSettings();
                    break;

                default:
                    Console.WriteLine($"unvalid argument {args[1]}");
                    break;
            }
        }

        private void LoadSettings()
        {
            if (File.Exists("settings.json")){
                string json = File.ReadAllText("settings.json");
                settings = JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
            }

            APIkey = settings.sAPIKey;
            MusicLib = settings.sMusicLibPath;
            AllowedGenres = settings.sAllowedGenres.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
            ScannedMusicFiles = settings.sScannedMusicFiles ?? new();
        }

        private void SaveSettings()
        {

            settings.sAPIKey = APIkey;
            settings.sMusicLibPath = MusicLib;
            settings.sAllowedGenres = AllowedGenres.ToList();
            settings.sScannedMusicFiles = ScannedMusicFiles;

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText("settings.json",json);
        }

        private void ShowHelp()
        {
            Console.WriteLine("tagrm - Last.fm Genre Tagger");
            Console.WriteLine("Usage:");
            Console.WriteLine("  tagrm easytag <LastFmApiKey> <PathToMusic>                          - Tag your music with genres");
            Console.WriteLine("  tagrm easytag WORKS ONLY IF APIkey&libPATH SAVE SETTING IS ON       - Tag your music with genres");
            Console.WriteLine("  tagrm settings                                                      - View or change settings");
            Console.WriteLine("  tagrm settings scannscannedfiles true/false                         - scan(true) or dont scan(false) already scanned files");
            Console.WriteLine("  tagrm settings Save APIkey and libPath true/false                   - save API key and libPath to quick scan");
            Console.WriteLine("  tagrm help                                                          - Show this help menu");
        }
    }
}
