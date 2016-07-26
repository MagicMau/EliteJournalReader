using EliteJournalReader;
using EliteJournalReader.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalFeedTester
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Don't forget to supply a path where the journal feed is located.");
                return;
            }

            string path = args[0];

            var watcher = new JournalWatcher(path);

            watcher.GetEvent<JournalHeadingEvent>()?.AddHandler((s, e) => Console.WriteLine("Heading received: gameversion = " + e.GameVersion));
            watcher.GetEvent<FSDJumpEvent>()?.AddHandler((s, e) => Console.WriteLine("Woohoo, jumped to Y = " + e.StarPos.Y));
            watcher.GetEvent<ScanEvent>()?.AddHandler((s, e) => Console.WriteLine("Scanned a body {0}", e.BodyName));
            watcher.GetEvent<DockedEvent>()?.AddHandler((s, e) => Console.WriteLine("Docked at {0}", e.StationName));
            
            watcher.StartWatching();


            Console.ReadLine();

            watcher.StopWatching();
        }
    }
}
