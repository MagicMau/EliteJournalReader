using Newtonsoft.Json;
using System;
using System.Text;

namespace EliteJournalReader.Events
{
    public class StatusEvent : JournalEvent<StatusFileEvent>
    {
        public StatusEvent() : base("Status") { }
    }

    public class StatusFileEvent : JournalEventArgs
    {
        public StatusFlags Flags { get; set; }

        public MoreStatusFlags Flags2 { get; set; }

        [JsonConverter(typeof(JsonPipsConverter))]
        public (int System, int Engine, int Weapons) Pips { get; set; }

        public int Firegroup { get; set; }

        public StatusGuiFocus GuiFocus { get; set; }

        public StatusFuel Fuel { get; set; }

        public double Cargo { get; set; }

        [JsonConverter(typeof(ExtendedStringEnumConverter<LegalState>))]
        public LegalState LegalState { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Altitude { get; set; }

        public double Heading { get; set; }

        public string BodyName { get; set; }

        public double PlanetRadius { get; set; }

        public long Balance { get; set; }

        public Destination Destination { get; set; }

        public double Oxygen { get; set; }

        public double Health { get; set; }

        public double Temperature { get; set; }

        public string SelectedWeapon { get; set; }

        public double Gravity { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Status: " + OriginalEvent?.ToString(Formatting.None) ?? "<no json>");
            sb.AppendLine($"Status Flags : {(long)Flags:X8} - {string.Join(", ", Flags.GetIndividualFlags())}");
            sb.AppendLine($"Status Flags2: {(long)Flags2:X8} - {string.Join(", ", Flags2.GetIndividualFlags())}");

            return sb.ToString();
        }

        class JsonPipsConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => true;
            public override bool CanWrite => false;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                (int System, int Engine, int Weapons) result;

                if (reader.TokenType == JsonToken.StartArray)
                {
                    result.System = ReadInt(reader);
                    result.Engine = ReadInt(reader);
                    result.Weapons = ReadInt(reader);
                }
                else
                {
                    result = (0, 0, 0);
                }
                reader.Read(); // read EndArray
                return result;
            }

            private static int ReadInt(JsonReader reader) => reader.Read() && reader.TokenType == JsonToken.Integer ? Convert.ToInt32(reader.Value) : 0;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        }
    }

    public class Destination
    {
        public long System { get; set; }
        public int Body { get; set; }
        public string Name { get; set; }
    }

    [Flags]
    public enum StatusFlags : long
    {
        None = 0,
        Docked = 0x00000001,
        Landed = 0x00000002,
        LandingGearDown = 0x00000004,
        ShieldsUp = 0x00000008,
        Supercruise = 0x00000010,
        FlightAssistOff = 0x00000020,
        HardpointsDeployed = 0x00000040,
        InWing = 0x00000080,
        LightsOn = 0x00000100,
        CargoScoopDeployed = 0x00000200,
        SilentRunning = 0x00000400,
        ScoopingFuel = 0x00000800,
        SrvHandbrake = 0x00001000,
        SrvTurret = 0x00002000,
        SrvUnderShip = 0x00004000,
        SrvDriveAssist = 0x00008000,
        FsdMassLocked = 0x00010000,
        FsdCharging = 0x00020000,
        FsdCooldown = 0x00040000,
        LowFuel = 0x00080000,
        Overheating = 0x00100000,
        HasLatLong = 0x00200000,
        IsInDanger = 0x00400000,
        BeingInterdicted = 0x00800000,
        InMainShip = 0x01000000,
        InFighter = 0x02000000,
        InSRV = 0x04000000,
        HudInAnalysisMode = 0x08000000,
        NightVision = 0x10000000,
        AltitudeFromAverageRadius = 0x20000000,
        FsdJump = 0x40000000,
        SrvHighBeam = 0x80000000,
    }

    [Flags]
    public enum MoreStatusFlags : long
    {
        None = 0,
        OnFoot = 0x00000001,
        InTaxi = 0x00000002,
        InMulticrew = 0x00000004,
        OnFootInStation = 0x00000008,
        OnFootOnPlanet = 0x00000010,
        AimDownSight = 0x00000020,
        LowOxygen = 0x00000040,
        LowHealth = 0x00000080,
        Cold = 0x00000100,
        Hot = 0x00000200,
        VeryCold = 0x00000400,
        VeryHot = 0x00000800,
        GlideMode = 0x00001000,
        OnFootInHangar = 0x00002000,
        OnFootInSocialSpace = 0x00004000,
        OnFootExterior = 0x00008000,
        BreathableAtmosphere = 0x000010000,
        TelepresenceMulticrew = 0x000020000,
        PhysicalMulticrew = 0x000040000,
        FsdHyperdriveCharging = 0x000080000,
        SuperCruiseOvercharge = 0x000100000,
        SuperCruiseAssist = 0x000200000,
        NpcCrewIsActive = 0x000400000,
    }

    public enum StatusGuiFocus
    {
        NoFocus = 0,
        InternalPanel = 1,
        ExternalPanel = 2,
        CommsPanel = 3,
        RolePanel = 4,
        StationServices = 5,
        GalaxyMap = 6,
        SystemMap = 7,
        Orrery = 8,
        FSSMode = 9,
        SAAMode = 10,
        Codex = 11
    }

    public class StatusFuel
    {
        public double FuelMain { get; set; }
        public double FuelReservoir { get; set; }
    }


}
