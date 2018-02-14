using System;
using EliteJournalReader.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EliteJournalReader.Tests
{
    [TestClass]
    [TestCategory("Journal Events")]
    public class TestJournalEvents
    {
        private FakeJournalWatcher watcher;
        
        [TestInitialize]
        public void Initialize()
        {
            watcher = new FakeJournalWatcher();
            watcher.StartWatching();
        }

        [TestCleanup]
        public void CleanUp()
        {
            watcher.StopWatching();
            watcher = null;
        }

        [TestMethod]
        public void Test_FileheaderEvent()
        {
            FileheaderEvent.FileheaderEventArgs args = null;
            //{ "timestamp":"2017-12-30T13:36:28Z", "event":"Fileheader", "part":1, "language":"English\\UK", "gameversion":"2.4", "build":"r160439/r0 " }
            watcher.GetEvent<FileheaderEvent>().Fired += (s, e) => args = e;
            watcher.FireFakeEvent(@"{ ""timestamp"":""2017-12-30T13:36:28Z"", ""event"":""Fileheader"", ""part"":1, ""language"":""English\\UK"", ""gameversion"":""2.4"", ""build"":""r160439/r0 "" }");

            Assert.IsNotNull(args);
            Assert.AreEqual("English\\UK", args.Language);
        }

        [TestMethod]
        public void Test_DiedEvent_Single()
        {
            //{ "timestamp":"2016-12-05T19:14:42Z", "event":"Died", "KillerName":"Wightman", "KillerShip":"diamondback", "KillerRank":"Expert" }
            DiedEvent.DiedEventArgs args = null;
            watcher.GetEvent<DiedEvent>().Fired += (s, e) => args = e;
            watcher.FireFakeEvent(@"{ ""timestamp"":""2016-12-05T19:14:42Z"", ""event"":""Died"", ""KillerName"":""Wightman"", ""KillerShip"":""diamondback"", ""KillerRank"":""Expert"" }");

            Assert.IsNotNull(args);
            Assert.AreEqual(1, args.Killers.Length);
            Assert.AreEqual("Wightman", args.Killers[0].Name);
        }

        [TestMethod]
        public void Test_DiedEvent_Wing()
        {
            //{ "timestamp":"2016-12-14T21:55:24Z", "event":"Died", "Killers":[ { "Name":"Cmdr Ekol Tieja", "Ship":"federation_gunship", "Rank":"Elite" }, { "Name":"Cmdr Yellowboze", "Ship":"federation_gunship", "Rank":"Novice" } ] }
            DiedEvent.DiedEventArgs args = null;
            watcher.GetEvent<DiedEvent>().Fired += (s, e) => args = e;
            watcher.FireFakeEvent(@"{ ""timestamp"":""2016-12-14T21:55:24Z"", ""event"":""Died"", ""Killers"":[ { ""Name"":""Cmdr Ekol Tieja"", ""Ship"":""federation_gunship"", ""Rank"":""Elite"" }, { ""Name"":""Cmdr Yellowboze"", ""Ship"":""federation_gunship"", ""Rank"":""Novice"" } ] }");

            Assert.IsNotNull(args);
            Assert.AreEqual(2, args.Killers.Length);
            Assert.AreEqual("Cmdr Ekol Tieja", args.Killers[0].Name);
            Assert.AreEqual("Cmdr Yellowboze", args.Killers[1].Name);
        }
    }
}
