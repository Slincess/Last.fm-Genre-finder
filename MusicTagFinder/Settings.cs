using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicTagFinder
{
    internal class Settings
    {
        public string? sAPIKey { get; set; }
        public string? sMusicLibPath { get; set; }

        public bool sScann_ScannedMusicFiles { get; set; } = false;
        public bool sSaveAPIkeyandPath { get; set; } = true;

        public List<string> sScannedMusicFiles { get; set; } = new();
        public List<string> sAllowedGenres { get; set; } = new();
    }
}
