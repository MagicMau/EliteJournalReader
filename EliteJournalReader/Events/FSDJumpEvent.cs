using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When written: when jumping from one star system to another
    //Parameters:
    //•	StarSystem: name of destination starsystem
    //•	StarPos: star position, as a Json array [x, y, z], in light years
    //•	Body: star’s body name
    //•	JumpDist: distance jumped
    //•	BoostUsed: whether FSD boost was used
    public class FSDJumpEvent : JournalEvent<FSDJumpEvent.FSDJumpEventArgs>
    {
        public FSDJumpEvent() : base("FSDJump") { }

        public class FSDJumpEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                StarSystem = evt.Value<string>("StarSystem");
                StarPos = new Position(evt.Value<JArray>("StarPos"));
                Body = evt.Value<string>("Body");
                JumpDist = evt.Value<decimal>("JumpDist");
                BoostUsed = evt.Value<bool>("BoostUsed");
            }

            public string StarSystem { get; set; }
            public Position StarPos { get; set; }
            public string Body { get; set; }
            public decimal JumpDist { get; set; }
            public bool BoostUsed { get; set; }
        }
    }
}
