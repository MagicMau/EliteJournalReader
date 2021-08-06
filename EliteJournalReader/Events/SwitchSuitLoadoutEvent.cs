using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class SwitchSuitLoadoutEvent : JournalEvent<SwitchSuitLoadoutEvent.SwitchSuitLoadoutEventArgs>
    {
        public SwitchSuitLoadoutEvent() : base("SwitchSuitLoadout") { }

        public class SwitchSuitLoadoutEventArgs : JournalEventArgs
        {
            public long SuitID { get; set; }
            public string SuitName { get; set; }
            public string[] SuitMods { get; set; }
            public long LoadoutID { get; set; }
            public string LoadoutName { get; set; }
            public List<SuitModule> Modules { get; set; }
        }
    }
}