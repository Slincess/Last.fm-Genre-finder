using TagLib;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using System.IO;
using MusicTagFinder;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

/// <summary>
/// todo: 
/// settings{
/// add or remove genre types [needs to be tested]
/// save your music lib path and API key to quick tagscan [needs to be tested]
/// save scanned files so you dont have to scan the samefiles you already scanned [needs to be tested]
/// auto detect new files and auto tagScan
/// }
/// </summary>
class Program
{
  

    static async Task Main(string[] args)
    {
        App app = new App();

        await app.Run(args);
    }

    
}
