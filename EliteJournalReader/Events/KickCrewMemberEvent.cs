using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: When you force another player to leave your ship's crew
    //Parameters:
    //•	Crew: player's commander name
    public class KickCrewMemberEvent : JournalEvent<KickCrewMemberEvent.KickCrewMemberEventArgs>
    {
        public KickCrewMemberEvent() : base("KickCrewMember") { }

        public class KickCrewMemberEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Crew = evt.Value<string>("Crew");
            }

            public string Crew { get; set; }
        }
    }
}
