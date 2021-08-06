using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    public class ShipLockerEvent : JournalEvent<ShipLockerEvent.ShipLockerEventArgs>
    {
        public ShipLockerEvent() : base("ShipLocker") { }

        public class ShipLockerEventArgs : JournalEventArgs
        {

        }
    }
}