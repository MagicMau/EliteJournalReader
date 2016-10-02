using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class FileheaderEvent : JournalEvent<FileheaderEvent.FileheaderEventArgs>
    {
        public FileheaderEvent() : base("Fileheader") { }

        public class FileheaderEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                GameVersion = evt.Value<string>("gameversion");
                Build = evt.Value<string>("build");
                Language = evt.Value<string>("language");
                Part = evt.Value<int>("part");
            }

            public string GameVersion { get; set; }
            public string Build { get; set; }
            public string Language { get; set; }
            public int Part { get; set; }
        }
    }
}
