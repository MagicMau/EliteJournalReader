using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: another player has joined the wing
    //Parameters:
    //•	Name
    public class WingInviteEvent : JournalEvent<WingInviteEvent.WingInviteEventArgs>
    {
        public WingInviteEvent() : base("WingInvite") { }

        public class WingInviteEventArgs : JournalEventArgs
        {
            public string Name { get; set; }
        }
    }
}
