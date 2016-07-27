using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EliteJournalReader.Events
{
    //When Written: detailed discovery scan of a star, planet or moon
    //Parameters(star)
    //•	Bodyname: name of body
    //•	DistanceFromArrivalLS
    //•	StarType: Stellar classification (for a star)
    //•	StellarMass: mass as multiple of Sol’s mass
    //•	Radius
    //•	AbsoluteMagnitude
    //•	OrbitalPeriod (seconds)
    //•	RotationPeriod (seconds)
    //•	Rings
    //
    //Parameters(Planet/Moon) 
    //•	Bodyname: name of body
    //•	DistanceFromArrivalLS
    //•	TidalLock: 1 if tidally locked
    //•	TerraformState: Terraformable, Terraforming, Terraformed, or null
    //•	PlanetClass
    //•	Atmosphere
    //•	Volcanism
    //•	SurfaceTemperature
    //•	SurfacePressure
    //•	Landable: true (if landable)
    //•	Materials: JSON object with material names and percentage occurrence
    //•	OrbitalPeriod (seconds)
    //•	RotationPeriod (seconds)
    //•	Rings
    //
    public class ScanEvent : JournalEvent<ScanEvent.ScanEventArgs>
    {
        public ScanEvent() : base("Scan") { }

        public class ScanEventArgs : JournalEventArgs
        {
            public override void Initialize(JObject evt)
            {
                base.Initialize(evt);
                BodyName = evt.Value<string>("BodyName");
                StarType = evt.Value<string>("StarType");
                StellarMass = evt.Value<decimal?>("StellarMass");
                Radius = evt.Value<decimal?>("StellarMass");
                AbsoluteMagnitude = evt.Value<decimal?>("StellarMass");
                OrbitalPeriod = evt.Value<decimal>("StellarMass");
                RotationPeriod = evt.Value<decimal>("StellarMass");
                Rings = evt.Value<int>("StellarMass");

                TidalLock = evt.Value<bool?>("TidalLock") ?? false;
                TerraformState = evt.Value<string>("TerraformState");
                PlanetClass = evt.Value<string>("PlanetClass");
                Atmosphere = evt.Value<string>("Atmosphere");
                Volcanism = evt.Value<string>("Volcanism");
                MassEM = evt.Value<decimal?>("MassEM");
                SurfaceGravity = evt.Value<decimal?>("SurfaceGravity");
                SurfaceTemperature = evt.Value<decimal?>("SurfaceTemperature");
                SurfacePressure = evt.Value<decimal?>("SurfacePressure");
                Landable = evt.Value<bool?>("Landable") ?? false;
                Materials = evt["Materials"]?.ToObject<Dictionary<string, decimal>>();
            }

            public string BodyName { get; set; }
            public decimal DistanceFromArrivalLS { get; set; }
            public string StarType { get; set; }
            public decimal? StellarMass { get; set; }
            public decimal? Radius { get; set; }
            public decimal? AbsoluteMagnitude { get; set; }
            public decimal OrbitalPeriod { get; set; }
            public decimal RotationPeriod { get; set; }
            public int Rings { get; set; }

            public bool? TidalLock { get; set; }
            public string TerraformState { get; set; }
            public string PlanetClass { get; set; }
            public string Atmosphere { get; set; }
            public string Volcanism { get; set; }
            public decimal? MassEM { get; set; } // not in description of event
            public decimal? SurfaceGravity { get; set; } // not in description of event
            public decimal? SurfaceTemperature { get; set; }
            public decimal? SurfacePressure { get; set; }
            public bool? Landable { get; set; }
            public Dictionary<string, decimal> Materials { get; set; }
        }
    }
}
