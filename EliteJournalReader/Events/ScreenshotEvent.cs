using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: when a screen snapshot is saved
    //Parameters: 
    //•	Filename: filename of screenshot
    //•	Width: size in pixels
    //•	Height: size in pixels
    //•	System: current star system
    //•	Body: name of nearest body
    public class ScreenshotEvent : JournalEvent<ScreenshotEvent.ScreenshotEventArgs>
    {
        public ScreenshotEvent() : base("Screenshot") { }

        public class ScreenshotEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                Filename = evt.Value<string>("Filename");
                Width = evt.Value<int>("Width");
                Height = evt.Value<int>("Height");
                System = evt.Value<string>("System");
                Body = evt.Value<string>("Body");
            }

            public string Filename { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string System { get; set; }
            public string Body { get; set; }
        }
    }
}
