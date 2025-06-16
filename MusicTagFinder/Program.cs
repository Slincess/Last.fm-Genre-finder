using TagLib;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using System.IO;

class Program
{
    static List<string> MusicFiles = new List<string>();
    static string APIkey;
    static string MusicLib;

    static int FoundTagsCount = 0;
    static int NotFoundTagsCount = 0;
    static List<string> TagsNotFoundMusics = new List<string>();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Last.fm API key:");
        APIkey = Console.ReadLine();
        Console.WriteLine("your music lib path:");
        MusicLib = Console.ReadLine();
        await GetAllMusicFiles(MusicLib);
        await GetTag();
        Console.WriteLine(" ");
        Console.WriteLine("Finished Tags are saved in Genre section");
        Console.WriteLine(" ");
        Console.WriteLine($"found tags for {FoundTagsCount} music and not for {NotFoundTagsCount}");
        if(NotFoundTagsCount > 0)
        {
            Console.WriteLine("do you wanna see which music/s we couldn't find Tag for? (y/n)");
            string Answer = Console.ReadLine();
            if (Answer == "y" && Answer == "yes")
            {
                Console.WriteLine($"Not found for:");
                foreach (var i in TagsNotFoundMusics)
                {
                    Console.WriteLine(i);
                }
                Console.WriteLine("press any key to escape");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("press any key to escape");
                Console.ReadKey();
            }
            Console.WriteLine("press any key to escape");
            Console.ReadKey();
        }
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
                        var genreNames = tags.Select(tag => (string)tag["name"]).ToArray();
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
                        TagsNotFoundMusics.Add(tfile.Tag.Title);
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
