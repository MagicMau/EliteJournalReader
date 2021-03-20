﻿using EliteJournalReader.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EliteJournalReader.Tests
{
    [TestClass]
    [TestCategory("Status events")]
    public class TestStatusEvents
    {
        private static string tempFolder;
        private FakeStatusWatcher watcher;

        [ClassInitialize]
        public static void Startup(TestContext context)
        {
            tempFolder = Path.Combine(Path.GetTempPath(), "TestStatusEvents");
            Directory.CreateDirectory(tempFolder);
        }

        [ClassCleanup]
        public static void AllTestsDone()
        {
            Directory.Delete(tempFolder, true);
        }

        [TestInitialize]
        public void Initialize()
        {
            watcher = new FakeStatusWatcher();
        }

        [TestCleanup]
        public void CleanUp()
        {
            watcher.StopWatching();
            watcher = null;
        }

        [TestMethod]
        public void Test_Simple_StatusEvent()
        {
            StatusFileEvent evt = null;
            watcher.StatusUpdated += (s, e) => evt = e;
            watcher.SendFakeStatusUpdate(new StatusFileEvent {Longitude = 14, Latitude = 7});

            Assert.IsNotNull(evt);
            Assert.AreEqual(14, evt.Longitude);
            Assert.AreEqual(7, evt.Latitude);
        }


        [TestMethod]
        public void Test_Parse_StatusJson()
        {
            using var hodor = new AutoResetEvent(false);

            StatusFileEvent evt = null;
            int counter = 0;
            watcher.StatusUpdated += (s, e) => {
                counter++;
                evt = e;
                hodor.Set();
            };
            watcher.StartWatching(tempFolder);

            while (!hodor.WaitOne(100))
            {
                WriteStatusFile(new StatusFileEvent
                    {Timestamp = DateTime.UtcNow, Longitude = 14, Latitude = 7, Pips = (2, 4, 2)});
                Thread.Sleep(1000); // wait a bit
            }

            Assert.IsNotNull(evt);
            Assert.AreEqual(14, evt.Longitude, "Longitude");
            Assert.AreEqual(7, evt.Latitude, "Latitude");

            Thread.Sleep(1000); // wait a bit more for all the notifications to be handled
            Assert.AreEqual(1, counter); // update only triggered once

        }

        [TestMethod]

        public void Test_Parse_Multiple_Updates_Of_StatusJson()
        {

            // only open the door when the status updated event has been processed
            using var hodor = new AutoResetEvent(false);

            StatusFileEvent evt = null;

            watcher.StatusUpdated += (s, e) => {
                if (e.Longitude >= 15 && evt == null)
                {
                    evt = e;
                    hodor.Set();
                }
            };
            watcher.StartWatching(tempFolder);

            double lon = 14;
            while (!hodor.WaitOne(100))
            {
                Thread.Sleep(1000); // wait a bit
                WriteStatusFile(new StatusFileEvent {Longitude = lon, Latitude = 7, Pips = (2, 4, 2)});
                lon = Math.Round(lon + 0.2, 6);
            }

            Assert.IsNotNull(evt);
            Assert.AreEqual(15, evt.Longitude);
            Assert.AreEqual(7, evt.Latitude);
        }

        private void WriteStatusFile(StatusFileEvent evt)
        {
            string file = Path.Combine(tempFolder, "Status.json");

            var jo = new JObject {
                ["timestamp"] = DateTime.Now,
                ["event"] = "Status",
                ["Flags"] = (int)evt.Flags,
                ["Pips"] = JArray.FromObject(new[] {evt.Pips.System, evt.Pips.Engine, evt.Pips.Weapons}),
                ["FireGroup"] = evt.Firegroup,
                ["GuiFocus"] = (int)evt.GuiFocus,
                ["Latitude"] = evt.Latitude,
                ["Longitude"] = evt.Longitude,
                ["Altitude"] = evt.Altitude,
                ["Heading"] = evt.Heading
            };

            using (var streamWriter =
                new StreamWriter(new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read)))
                using (var writer = new JsonTextWriter(streamWriter))
                {
                    jo.WriteTo(writer);
                }
        }
    }
}
