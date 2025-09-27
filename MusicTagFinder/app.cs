#pragma warning disable
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
        private string? APIkey;
        private string? MusicLib;

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
                // args[2] == API key args[1] = musicLib
                case ("easytag"):
                    if (args.Length >= 3) // user provided path + key
                    {
                        MusicLib = args[1];
                        APIkey = args[2];
                        await Start();
                    }
                    else if (args.Length == 2) // only path provided
                    {
                        if (!Directory.Exists(args[1]))
                        {
                            APIkey = args[1];
                            if (!string.IsNullOrEmpty(settings.sMusicLibPath))
                            {
                                MusicLib = settings.sMusicLibPath;
                                Console.WriteLine("Path is missing. using saved Path");
                                await Start();
                            }
                            else
                            {
                                Console.WriteLine("Path is missing also there is not saved path");
                                return;
                            }
                        }
                        else
                        {
                            MusicLib = args[1];
                            if (!string.IsNullOrEmpty(settings.sAPIKey))
                            {
                                APIkey = settings.sAPIKey;
                                Console.WriteLine("API key missing. Using saved API key.");
                                await Start();
                            }
                            else
                            {
                                Console.WriteLine("API key is missing also there is no saved API key");
                                return;
                            }
                        }
                    }
                    else // no args at all
                    {
                        if (!string.IsNullOrEmpty(settings.sMusicLibPath) && !string.IsNullOrEmpty(settings.sAPIKey))
                        {
                            MusicLib = settings.sMusicLibPath;
                            APIkey = settings.sAPIKey;
                            Console.WriteLine("No arguments. Using saved path and API key.");
                            await Start();
                        }
                        else
                        {
                            Console.WriteLine("Please provide a path and API key (no saved values found).");
                        }
                    }

                    break;
                case ("settings"):
                    if (args.Length >= 3)
                        HandleSettings(args);
                    else { Console.WriteLine("missing arguments (-help for commands)"); }
                    break;
                case ("-help"):
                    ShowHelp();
                    break;
                case ("help"):
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine("unknown command");
                    break;
            }

        }
        /// <summary>
        /// get the music files in given args directory.
        /// </summary>
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
            int found = ScannedMusicFiles.Count - TagsNotFoundMusics.Count;
            Console.WriteLine($"found tags for { found.ToString()} music and not for {TagsNotFoundMusics.Count.ToString()}");
            SaveSettings();
            if (TagsNotFoundMusics.Count > 0)
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
                }
            }
            Console.WriteLine("press any key to escape");
            Console.ReadKey();
        }
        /// <summary>
        /// scan the folder given in GetAllMusicFiles().
        /// use GetAllMusicFiles() before using this
        /// </summary>
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

        /// <summary>
        /// gets tag/s for one file. item = file path
        /// </summary>
        private async Task GetTagFile(string item)
        {
            try
            {
                string? url;
                FileInfo fileInfo = new FileInfo(item);
                if ((fileInfo.Attributes & FileAttributes.ReadOnly) != 0)
                {
                    Console.WriteLine($"{item} is read-only. Trying to remove read-only attribute...");
                    fileInfo.Attributes &= ~FileAttributes.ReadOnly;
                }

                using (var tfile = TagLib.File.Create(item))
                {
                    string title = tfile.Tag.Title;
                    string artist = tfile.Tag.FirstArtist;


                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist))
                    {
                        TagsNotFoundMusics.Add(Path.GetFileName(item));
                        return;
                    }

                    

                    if(APIkey == null)
                    {
                        url = $"http://ws.audioscrobbler.com/2.0/?method=track.getTopTags&artist={HttpUtility.UrlEncode(artist)}&track={HttpUtility.UrlEncode(title)}&api_key={settings.sAPIKey}&format=json";
                    }
                    else
                    {
                        url = $"http://ws.audioscrobbler.com/2.0/?method=track.getTopTags&artist={HttpUtility.UrlEncode(artist)}&track={HttpUtility.UrlEncode(title)}&api_key={APIkey}&format=json";
                    }
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            string response = await client.GetStringAsync(url);
                            JObject json = JObject.Parse(response);

                            var tags = json["toptags"]?["tag"];
                            if (tags != null)
                            {
                                string[] genreNames = tags?
                                    .Select(tag => (string)tag["name"])
                                    .Where(name => AllowedGenres.Contains(name))
                                    .ToArray() ?? Array.Empty<string>();

                                if (genreNames.Length == 0 && settings.sGetArtistGenre)
                                {
                                    string urlArtist = $"http://ws.audioscrobbler.com/2.0/?method=artist.getTopTags&artist={HttpUtility.UrlEncode(artist)}&api_key={APIkey}&format=json";
                                    try
                                    {

                                    }
                                    catch { Console.WriteLine("couldnt connect to last.fm you might be offline"); }
                                    string responseArtist = await client.GetStringAsync(urlArtist);
                                    JObject jsonArtist = JObject.Parse(responseArtist);
                                    var tagsArtist = jsonArtist["toptags"]?["tag"];
                                    if (tagsArtist != null)
                                    {
                                        genreNames = tagsArtist
                                        .Select(tag => (string)tag["name"])
                                        .Where(name => AllowedGenres.Contains(name))
                                        .ToArray() ?? Array.Empty<string>();
                                    }

                                }

                                genreNames = genreNames.Where(name => AllowedGenres.Contains(name)).ToArray();
                                var id3v2 = tfile.GetTag(TagLib.TagTypes.Id3v2, true);
                                if (settings.sFixArtistNames)
                                {
                                    artist = artist.Replace(",", "; ");
                                    try
                                    {
                                        tfile.Tag.AlbumArtists = new string[] { artist };
                                    }
                                    catch
                                    {
                                        Console.WriteLine($"{title} couldnt replace , with ; in album artist");
                                    }
                                    try
                                    {
                                        tfile.Tag.Artists = new string[] { artist };
                                    }
                                    catch
                                    {
                                        Console.WriteLine($"{title} couldnt replace , with ; in contributing artist");
                                    }
                                }
                                id3v2.Genres = genreNames;

                                Console.WriteLine("Genres written:");
                                foreach (var g in id3v2.Genres) Console.WriteLine($"- {g}");

                                if (!settings.sScann_ScannedMusicFiles && !settings.sScannedMusicFiles.Contains(item))
                                {
                                    settings.sScannedMusicFiles.Add(item);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No tags to be found.");
                                if (!TagsNotFoundMusics.Contains(title))
                                    TagsNotFoundMusics.Add(title);
                            }
                            tfile.Save();
                        }
                    }
                    catch
                    {
                        Console.WriteLine("couldnt connect to last.fm you might be offline");
                        throw;
                    }
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"a problem {ex}");
                Console.WriteLine("No connaction or connaction failed.");
                return;
            }
        }

        /// <summary>
        /// handles settings
        /// </summary>
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

                case ("savedata"):
                    if (args[2].ToLower() == "true")
                        settings.sSaveAPIkeyandPath = true;
                    else if (args[2].ToLower() == "false")
                        settings.sSaveAPIkeyandPath = false;
                    SaveSettings();
                        break;
                case ("getartistgenre"):
                    if (args[2].ToLower() == "true")
                        settings.sGetArtistGenre = true;
                    else if (args[2].ToLower() == "false")
                        settings.sGetArtistGenre = false;
                    SaveSettings();
                    break;
                case ("show"):
                    ShowSettings(); 
                    break;
                case ("fixartistname"):
                   if (args[2].ToLower() == "true")
                        settings.sFixArtistNames = true;
                    else if (args[2].ToLower() == "false")
                        settings.sFixArtistNames = false;
                 break;

                default:
                    Console.WriteLine($"unvalid argument {args[1]}");
                    break;
            }
        }

        static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "tagrm",
            "settings.json"
            );

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    settings = JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                    Console.WriteLine("Loaded existing settings.");
                }
                else
                {
                    settings = new Settings();
                    SaveSettings();
                    Console.WriteLine("No settings found. Created default settings.json.");
                }

                APIkey = settings.sAPIKey;
                MusicLib = settings.sMusicLibPath;
                AllowedGenres = settings.sAllowedGenres?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
                ScannedMusicFiles = settings.sScannedMusicFiles ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading settings: " + ex.Message);
                settings = new Settings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(SettingsPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);

                settings.sAPIKey = APIkey;
                settings.sMusicLibPath = MusicLib;
                settings.sAllowedGenres = AllowedGenres.ToList();
                settings.sScannedMusicFiles = ScannedMusicFiles;

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to save settings: " + ex.Message);
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("tagrm - Last.fm Genre Tagger");
            Console.WriteLine("Usage:");
            Console.WriteLine("  tagrm easytag  <PathToMusic> <LastFmApiKey>                         - Tag your music with genres");
            Console.WriteLine("  tagrm settings                                                      - View or change settings");
            Console.WriteLine("  tagrm settings scannscannedfiles true/false                         - scan(true) or dont scan(false) already scanned files");
            Console.WriteLine("  tagrm settings savedata true/false                                  - save API key and libPath to quick scan");
            Console.WriteLine("  tagrm settings addgenre <genre>                                     - adds the genre to allowed list");
            Console.WriteLine("  tagrm settings removegenre <genre>                                  - remove the genre from allowed list");
            Console.WriteLine("  tagrm settings getartistgenre true/false                            - if its true and if the track doesnt have a genre it will get the genre of the first artist");
            Console.WriteLine("  tagrm settings FixArtistName true/false                             - if its true it will replace {,} with {;}");
            Console.WriteLine("  tagrm settings show                                                 - shows your settings"); 
            Console.WriteLine("  tagrm -help                                                         - Show this help menu");
        }

        private void ShowSettings()
        {
            Console.WriteLine($" getartistgenre    : {settings.sGetArtistGenre}");
            Console.WriteLine($" saved API key     : {settings.sAPIKey}");
            Console.WriteLine($" saved Path        : {settings.sMusicLibPath}");
            Console.WriteLine($" fix artist names  : {settings.sFixArtistNames}");
            Console.WriteLine($" scannscannedfiles : {settings.sScannedMusicFiles}");
            Console.WriteLine($" save data         : {settings.sSaveAPIkeyandPath}");
        }
    }
}
