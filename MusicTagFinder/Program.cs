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

class Program
{
  

    static async Task Main(string[] args)
    {
        App app = new App();

        await app.Run(args);
    }

    
}
