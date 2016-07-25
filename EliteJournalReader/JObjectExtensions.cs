using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalReader
{
    public static class JObjectExtensions
    {
        public static string StringValue(this JObject jObject, string key)
        {
            var token = jObject[key];
            return token?.Value<string>()?.Trim();
        }
    }
}
