using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class ScanEvent : JournalEvent<ScanEvent.ScanEventArgs>
    {
        public ScanEvent() : base("Scan") { }

        public class ScanEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                BodyName = evt.Value<string>("BodyName");
                Description = evt.Value<string>("Description");
                StarType = evt.Value<string>("StarType");
                Landable = evt.Value<bool?>("Landable") ?? false;
                Materials = evt["Materials"]?.ToObject<Dictionary<string, decimal>>();
            }

            public string BodyName { get; set; }
            public string Description { get; set; }
            public string StarType { get; set; }
            public bool Landable { get; set; }
            public Dictionary<string, decimal> Materials { get; set; }
        }
    }
}
