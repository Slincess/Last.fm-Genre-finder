using TagLib;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using System.IO;

/// <summary>
/// todo: 
/// 
/// </summary>
class Program
{
    static List<string> MusicFiles = new List<string>();
    static string APIkey;
    static string MusicLib;

    static int FoundTagsCount = 0;
    static int NotFoundTagsCount = 0;
    static List<string> TagsNotFoundMusics = new List<string>();

    static readonly HashSet<string> AllowedGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

    static async Task Main(string[] args)
    {
        if (args.Length < 3 || args[0].ToLower() != "easytag")
        {
            Console.WriteLine("Usage: tagrm easyTag <LastFmApiKey> <PathToMusic>");
            return;
        }

        APIkey = args[1];
        MusicLib = args[2];

        await GetTag();
        Console.WriteLine(" ");
        Console.WriteLine("Finished Tags are saved in Genre section");
        Console.WriteLine(" ");
        Console.WriteLine($"found tags for {FoundTagsCount} music and not for {NotFoundTagsCount}");
        if(NotFoundTagsCount > 0)
        {
            Console.WriteLine("do you wanna see which music/s we couldn't find Tag for? (y/n)");
            string Answer = Console.ReadLine();
            if (Answer.ToLower() == "y" || Answer.ToLower() == "yes")
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

    static async Task GetAllMusicFiles(string MusicLibPath)
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

    static async Task GetTag()
    {
        foreach (var item in MusicFiles)
        {

            try
            {
                var tfile = TagLib.File.Create(item);
                string title = tfile.Tag.Title;
                string artist = tfile.Tag.FirstArtist;


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
                    }
                    else
                    {
                        Console.WriteLine("No tags to be found.");
                        NotFoundTagsCount++;
                        TagsNotFoundMusics.Add(title);
                    }
                    tfile.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"a problem {ex}");
                Console.WriteLine("No tags to be found.");
                NotFoundTagsCount++;
                var tfile = TagLib.File.Create(item);
                string title = tfile.Tag.Title;
                TagsNotFoundMusics.Add(tfile.Tag.Title);
                throw;
            }
        }
    }
}
