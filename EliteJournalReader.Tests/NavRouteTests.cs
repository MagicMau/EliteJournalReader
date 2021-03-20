using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EliteJournalReader.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EliteJournalReader.Tests
{
    [TestClass]

    public class NavRouteTests
    {

        
        [TestMethod]
        public void Test1()
        {
            NavRouteEvent.NavRouteEventArgs args = null;

            // create journal watcher - checking local directory
            JournalWatcher jw = new JournalWatcher(".");

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed
            jw.GetEvent<NavRouteEvent>().AddHandler((s, e) => { args = e; });

            // insert a journal cargoevent with NO inventory, forcing the watcher to load the cargo.json file
            jw.ParseText(
                @"{ ""timestamp"":""2020 - 07 - 01T14: 16:08Z"", ""event"":""NavRoute"" }",lineByLine:false);

            // check args contain cargo from cargo.json file.

            Assert.IsNotNull(args);
            Assert.IsTrue(
                string.Compare(args.Route.Last().StarSystem, "Miromi", StringComparison.CurrentCultureIgnoreCase) == 0);
        }

        


    }
}