using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: If you should ever reset your game
    //Parameters:
    //•	Name: commander name
    public class ChangeCrewRoleEvent : JournalEvent<ChangeCrewRoleEvent.ChangeCrewRoleEventArgs>
    {
        public ChangeCrewRoleEvent() : base("ChangeCrewRole") { }

        public class ChangeCrewRoleEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Role = evt.Value<string>("Role").ToEnum(RoleType.Unknown);
            }

            public RoleType Role { get; set; }
        }

        public enum RoleType
        {
            Unknown,
            Idle,
            FireCon,
            FighterCon
        }
    }
}
