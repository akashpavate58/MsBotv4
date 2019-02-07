using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MsBotv4.Models.Translation
{
    public class TranslationOutputModel
    {
        public string DetectedLanguage { get; set; }
        public string Text { get; set; }
    }
}
