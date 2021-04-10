using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when docking an SRV with the ship
    //Parameters: none
    public class ModuleInfoEvent : JournalEvent<ModuleInfoEvent.ModuleInfoEventArgs>
    {
        public ModuleInfoEvent() : base("ModuleInfo") { }

        public class ModuleInfoEventArgs : JournalEventArgs
        {
            public ModuleInfo[] Modules { get; set; }
        }
    }

    public class ModuleInfo
    {
        public string Slot { get; set; }
        public string Item { get; set; }
        public decimal Power { get; set; }
        public int Priority { get; set; }

    }
}
