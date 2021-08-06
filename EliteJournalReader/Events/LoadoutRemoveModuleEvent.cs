using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class LoadoutRemoveModuleEvent : JournalEvent<LoadoutRemoveModuleEvent.LoadoutRemoveModuleEventArgs>
    {
        public LoadoutRemoveModuleEvent() : base("LoadoutRemoveModule") { }

        public class LoadoutRemoveModuleEventArgs : JournalEventArgs
        {
            public long SuitID { get; set; }
            public string SuitName { get; set; }
            public long LoadoutID { get; set; }
            public string LoadoutName { get; set; }
            public string ModuleName { get; set; }
            public long SuitModuleID { get; set; }
            public string Class { get; set; }
            public string[] WeaponMods { get; set; }
        }
    }
}