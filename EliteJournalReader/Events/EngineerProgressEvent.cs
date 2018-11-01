using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when a player increases their access to an engineer
    //Parameters
    //•	Engineer: name of engineer
    //•	Rank: rank reached (when unlocked)
    //•	Progress: progress stage (Invited/Acquainted/Unlocked/Barred)
    public class EngineerProgressEvent : JournalEvent<EngineerProgressEvent.EngineerProgressEventArgs>
    {
        public EngineerProgressEvent() : base("EngineerProgress") { }

        public class EngineerProgressEventArgs : JournalEventArgs
        {
            public string Engineer { get; set; }
            public long EngineerID { get; set; }
            public int? Rank { get; set; }
            public string Progress { get; set; }

            public List<EngineerProgressEventArgs> Engineers { get; set; }
        }
    }
}
