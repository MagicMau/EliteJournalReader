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
        public void Test1_WithItems()
        {
            NavRouteEvent.NavRouteEventArgs args = null;
            // create journal watcher - checking local directory
            FakeJournalWatcher jw = new FakeJournalWatcher(".");

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed
            jw.GetEvent<NavRouteEvent>().AddHandler((s, e) => { if( s==jw && e.Route!=null) args = e; });

            // insert a journal cargoevent with inventory
            jw.ParseText(
                @"{ ""timestamp"":""2020 - 07 - 01T14: 16:08Z"", ""event"":""NavRoute"" }",lineByLine:false);

            // check that the cargo from inventory was seen
            Assert.IsNotNull(args);
            Assert.IsNotNull(args.Route);
            Assert.IsNotNull(args.Route.FirstOrDefault(x =>
                string.Compare(x.StarSystem, "Miromi", StringComparison.CurrentCultureIgnoreCase) == 0));

        }

        [TestMethod]
        public void Test1_WithOutItemsPreviousJournal()
        {
            NavRouteEvent.NavRouteEventArgs args = null;

            // create journal watcher - checking local directory
            FakeJournalWatcher jw = new FakeJournalWatcher(".",isLive:false);

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed
            jw.GetEvent<NavRouteEvent>().AddHandler((s, e) => { if( s==jw && e.Route!=null) args = e; });

            // insert a journal cargoevent with NO inventory, forcing the watcher to load the cargo.json file
            jw.ParseText(
                @"{ ""timestamp"":""2020 - 07 - 01T14: 16:08Z"", ""event"":""NavRoute"" }",lineByLine:false);

            // check args contain cargo from cargo.json file.

            Assert.IsNull(args);
            
        }

        [TestMethod]
        public void Test1_WithOutItemsLive()
        {
            NavRouteEvent.NavRouteEventArgs args = null;

            // create journal watcher - checking local directory
            FakeJournalWatcher jw = new FakeJournalWatcher(".",isLive:true);

            // create inline eventHandler for CargoEvent, which remembers last eventArgs passed
            jw.GetEvent<NavRouteEvent>().AddHandler((s, e) => { if( s==jw && e.Route!=null) args = e; });

            // insert a journal cargoevent with NO inventory, forcing the watcher to load the cargo.json file
            jw.ParseText(
                @"{ ""timestamp"":""2020 - 07 - 01T14: 16:08Z"", ""event"":""NavRoute"" }",lineByLine:false);

            // check args contain cargo from cargo.json file.

            Assert.IsNotNull(args);
            Assert.IsNotNull(args.Route);
            Assert.IsNotNull(args.Route.FirstOrDefault(x =>
                string.Compare(x.StarSystem, "Miromi", StringComparison.CurrentCultureIgnoreCase) == 0));

        }




    }
}