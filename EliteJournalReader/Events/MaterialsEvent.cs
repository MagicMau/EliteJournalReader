using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: If you should ever reset your game
    //Parameters:
    //•	Name: commander name
    public class MaterialsEvent : JournalEvent<MaterialsEvent.MaterialsEventArgs>
    {
        public MaterialsEvent() : base("Materials") { }

        public class MaterialsEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);

                var raw = evt["Raw"];
                if (raw != null)
                {
                    if (raw.Type == JTokenType.Object)
                        Raw = raw.ToObject<Dictionary<string, int>>();
                    else if (raw.Type == JTokenType.Array)
                    {
                        Raw = new Dictionary<string, int>();
                        foreach (var jo in (JArray)raw)
                        {
                            Raw[jo.Value<string>("Name")] = jo.Value<int>("Count");
                        }
                    }
                }

                var manufactured = evt["Manufactured"];
                if (manufactured != null)
                {
                    if (manufactured.Type == JTokenType.Object)
                        Manufactured = manufactured.ToObject<Dictionary<string, int>>();
                    else if (manufactured.Type == JTokenType.Array)
                    {
                        Manufactured = new Dictionary<string, int>();
                        foreach (var jo in (JArray)manufactured)
                        {
                            Manufactured[jo.Value<string>("Name")] = jo.Value<int>("Count");
                        }
                    }
                }

                var encoded = evt["Encoded"];
                if (encoded != null)
                {
                    if (encoded.Type == JTokenType.Object)
                        Encoded = encoded.ToObject<Dictionary<string, int>>();
                    else if (encoded.Type == JTokenType.Array)
                    {
                        Encoded = new Dictionary<string, int>();
                        foreach (var jo in (JArray)encoded)
                        {
                            Encoded[jo.Value<string>("Name")] = jo.Value<int>("Count");
                        }
                    }
                }
            }

            public Dictionary<string, int> Raw { get; set; }
            public Dictionary<string, int> Manufactured { get; set; }
            public Dictionary<string, int> Encoded { get; set; }
        }
    }
}
