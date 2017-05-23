using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: When you join another player ship's crew
    //Parameters:
    //•	Captain: Helm player's commander name
    public class JoinACrewEvent : JournalEvent<JoinACrewEvent.JoinACrewEventArgs>
    {
        public JoinACrewEvent() : base("JoinACrew") { }

        public class JoinACrewEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Captain = evt.Value<string>("Captain");
            }

            public string Captain { get; set; }
        }
    }
}
