using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class JoinedSquadronEvent : JournalEvent<JoinedSquadronEvent.JoinedSquadronEventArgs>
    {
        public JoinedSquadronEvent() : base("JoinedSquadron") { }

        public class JoinedSquadronEventArgs : JournalEventArgs
        {
            public long SquadronID { get; set; }
            public string SquadronName { get; set; }
        }
    }
}
