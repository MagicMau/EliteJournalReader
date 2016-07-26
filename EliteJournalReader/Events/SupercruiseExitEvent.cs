using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: leaving supercruise for normal space
    //Parameters:
    //•	Starsystem
    //•	Body
    public class SupercruiseExitEvent : JournalEvent<SupercruiseExitEvent.SupercruiseExitEventArgs>
    {
        public SupercruiseExitEvent() : base("SupercruiseExit") { }

        public class SupercruiseExitEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StarSystem = evt.Value<string>("StarSystem");
                Body = evt.Value<string>("Body");
            }

            public string StarSystem { get; set; }
            public string Body { get; set; }
        }
    }
}
