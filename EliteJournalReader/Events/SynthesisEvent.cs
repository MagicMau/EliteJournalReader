using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when synthesis is used to repair or rearm
    //Parameters:
    //•	Name: synthesis blueprint
    //•	Materials: JSON object listing materials used and quantities
    public class SynthesisEvent : JournalEvent<SynthesisEvent.SynthesisEventArgs>
    {
        public SynthesisEvent() : base("Synthesis") { }

        public class SynthesisEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Name = evt.Value<string>("Name");

                var mats = evt["Materials"];
                if (mats != null)
                {
                    if (mats.Type == JTokenType.Object)
                        Materials = mats.ToObject<Dictionary<string, int>>();
                    else if (mats.Type == JTokenType.Array)
                    {
                        Materials = new Dictionary<string, int>();
                        foreach (var jo in (JArray)mats)
                        {
                            Materials[jo.Value<string>("Name")] = jo.Value<int>("Count");
                        }
                    }
                }
            }

            public string Name { get; set; }
            public Dictionary<string, int> Materials { get; set; }
        }
    }
}
