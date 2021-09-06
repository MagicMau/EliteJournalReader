using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class UpgradeWeaponEvent : JournalEvent<UpgradeWeaponEvent.UpgradeWeaponEventArgs>
    {
        public UpgradeWeaponEvent() : base("UpgradeWeapon") { }

        public class UpgradeWeaponEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
            public string Name_Localised { get; set; }
            public long SuitModuleID { get; set; }
            public int Class { get; set; }
            public int Cost { get; set; }
        }
    }
}