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

            watcher.GetEvent<FileheaderEvent>()?.AddHandler((s, e) => Console.WriteLine("Heading received: gameversion {0}, build {1}.", e.GameVersion, e.Build));
            watcher.GetEvent<FSDJumpEvent>()?.AddHandler((s, e) => Console.WriteLine("Woohoo, jumped to [{0}, {1}, {2}]", e.StarPos.X, e.StarPos.Y, e.StarPos.Z));
            watcher.GetEvent<ScanEvent>()?.AddHandler((s, e) => Console.WriteLine("Scanned a body {0}, it is {1}landable.", e.BodyName, (e.Landable ?? false) ? "" : "not "));
            watcher.GetEvent<DockedEvent>()?.AddHandler((s, e) => Console.WriteLine("Docked at {0}", e.StationName));

            watcher.GetEvent<DiedEvent>()?.AddHandler((s, e) =>
            {
            Console.WriteLine("Killed by {0}",
                e.Killers
                    .Select(k => string.Format("{0} ({2}, flying a {1})", k.Name, k.Ship, k.Rank))
                    .Aggregate((x, y) => string.Format("{0}, {1}", x, y)));
            });

            watcher.GetEvent<RankEvent>()?.AddHandler((s, e) => Console.WriteLine("Combat rank is {0}, Exploration rank is {1}", e.Combat.ToString(), e.Explore.ToString()));

            watcher.StartWatching();


            Console.ReadLine();

            watcher.StopWatching();
        }
    }
}
