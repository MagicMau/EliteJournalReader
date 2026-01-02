using System.ComponentModel;

namespace EliteJournalReader
{

    public enum ScanType
    {
        Unknown,
        AutoScan,
        Basic,
        Detailed,
        NavBeacon,
        NavBeaconDetail
    }

    public enum StarType
    {
        Unknown,

        // main sequence
        O,
        B,
        A,
        F,
        G,
        K,
        M,
        L,
        T,
        Y,

        // proto stars
        TTS,
        AeBe,

        // wolf-rayet
        W,
        WN,
        WNC,
        WC,
        WO,

        // carbon stars
        CS,
        C,
        CN,
        CJ,
        CH,
        CHd,

        MS,
        S,

        // white drafs
        D,
        DA,
        DAB,
        DAO,
        DAZ,
        DAV,
        DB,
        DBZ,
        DBV,
        DO,
        DOV,
        DQ,
        DC,
        DCV,
        DX,

        // neutron
        N,

        // black hole
        H,

        // exotic
        X,

        // other
        SupermassiveBlackHole,
        A_BlueWhiteSuperGiant,
        F_WhiteSuperGiant,
        M_RedSuperGiant,
        M_RedGiant,
        K_OrangeGiant,
        RoguePlanet,
        Nebula,
        StellarRemnantNebula
    }

    public enum StarLuminosity
    {
        None,
        I,
        Ia0,
        Ia,
        Ib,
        Iab,
        II,
        IIa,
        IIab,
        IIb,
        III,
        IIIa,
        IIIab,
        IIIb,
        IV,
        IVa,
        IVab,
        IVb,
        V,
        Va,
        Vab,
        Vb,
        Vz,
        VI,
        VII
    }

    public enum AtmosphereClass
    {
        Unknown,

        None,

        [Description("No atmosphere")]
        NoAtmosphere,

        [Description("Suitable for water-based life")]
        SuitableForWaterBasedLife,

        [Description("Ammonia and oxygen")]
        AmmoniaAndOxygen,

        Ammonia,

        Water,

        [Description("Carbon dioxide")]
        CarbonDioxide,

        [Description("Sulphur dioxide")]
        SulphurDioxide,

        Nitrogen,

        [Description("Water-rich")]
        WaterRich,

        [Description("Methane-rich")]
        MethaneRich,

        [Description("Ammonia-rich")]
        AmmoniaRich,

        [Description("Carbon dioxide-rich")]
        CarbonDioxideRich,

        Methane,

        Helium,

        [Description("Silicate vapour")]
        SilicateVapour,

        [Description("Metallic vapour")]
        MetallicVapour,

        [Description("Neon-rich")]
        NeonRich,

        [Description("Argon-rich")]
        ArgonRich,

        Neon,

        Argon,

        Oxygen
    }

    public enum BodyType
    {
        Unknown,
        Null,
        Star,
        Planet,
        PlanetaryRing,
        StellarRing,
        AsteroidCluster
    }

    public enum GameMode
    {
        Unknown,
        Open,
        Solo,
        Group,
        Console
    }

    public enum PowerplayState
    {
        Unknown,
        InPrepareRadius,
        Prepared,
        Exploited,
        Contested,
        Controlled,
        Turmoil,
        HomeSystem
    }

    public class PowerplayConflictProgress
    {
        public string Power { get; set; }
        public double ConflictProgress { get; set; }
    }

    public enum TerraformState
    {
        Unknown,
        None,
        Terraformable,
        Terraforming,
        Terraformed
    }

    public enum ReserveLevel
    {
        Unknown,

        [Description("DepletedResources")]
        Depleted,

        [Description("LowResources")]
        Low,

        [Description("CommonResources")]
        Common,

        [Description("MajorResources")]
        Major,

        [Description("PristineResources")]
        Pristine
    }

    public enum InfluenceLevel
    {
        Unknown,
        None,
        Low,
        Med,
        High
    }

    public enum ReputationLevel
    {
        Unknown,
        None,
        Low,
        Med,
        High
    }

    public enum JumpType
    {
        Unknown,
        Hyperspace,
        Supercruise
    }

    public enum TextChannel
    {
        Unknown,
        NPC,
        Local,
        Starsystem,
        System,
        Player,
        Wing,
        Squadron,
        Friend,
        VoiceChat
    }

    public enum DockingDeniedReason
    {
        Unknown,
        NoSpace,
        TooLarge,
        Hostile,
        Offences,
        Distance,
        ActiveFighter,
        NoReason
    }

    public enum ModuleAttribute
    {
        Size,
        Class,
        Mass,
        Integrity,
        PowerDraw,
        BootTime,
        ShieldBankSpinUp,
        ShieldBankDuration,
        ShieldBankReinforcement,
        ShieldBankHeat,
        DamageFalloffRange,
        DamagePerSecond,
        Damage,
        DistributorDraw,
        ThermalLoad,
        ArmourPenetration,
        MaximumRange,
        ShotSpeed,
        RateOfFire,
        BurstRateOfFire,
        BurstSize,
        AmmoClipSize,
        AmmoMaximum,
        RoundsPerShot,
        ReloadTime,
        BreachDamage,
        MinBreachChance,
        MaxBreachChance,
        Jitter,
        WeaponMode,
        DamageType,
        ShieldGenMinimumMass,
        ShieldGenOptimalMass,
        ShieldGenMaximumMass,
        ShieldGenMinStrength,
        ShieldGenStrength,
        ShieldGenMaxStrength,
        RegenRate,
        BrokenRegenRate,
        EnergyPerRegen,
        FSDOptimalMass,
        FSDHeatRate,
        MaxFuelPerJump,
        EngineMinimumMass,
        EngineOptimalMass,
        MaximumMass,
        EngineMinPerformance,
        EngineOptPerformance,
        EngineMaxPerformance,
        EngineHeatRate,
        PowerCapacity,
        HeatEfficiency,
        WeaponsCapacity,
        WeaponsRecharge,
        EnginesCapacity,
        EnginesRecharge,
        SystemsCapacity,
        SystemsRecharge,
        DefenceModifierHealthMultiplier,
        DefenceModifierHealthAddition,
        DefenceModifierShieldMultiplier,
        DefenceModifierShieldAddition,
        KineticResistance,
        ThermicResistance,
        ExplosiveResistance,
        CausticResistance,
        FSDInterdictorRange,
        FSDInterdictorFacingLimit,
        ScannerRange,
        DiscoveryScannerRange,
        DiscoveryScannerPassiveRange,
        MaxAngle,
        ScannerTimeToScan,
        ChaffJamDuration,
        ECMRange,
        ECMTimeToCharge,
        ECMActivePowerConsumption,
        ECMHeat,
        ECMCooldown,
        HeatSinkDuration,
        ThermalDrain,
        NumBuggySlots,
        CargoCapacity,
        MaxActiveDrones,
        DroneTargetRange,
        DroneLifeTime,
        DroneSpeed,
        DroneMultiTargetSpeed,
        DroneFuelCapacity,
        DroneRepairCapacity,
        DroneHackingTime,
        DroneMinJettisonedCargo,
        DroneMaxJettisonedCargo,
        FuelScoopRate,
        FuelCapacity,
        OxygenTimeCapacity,
        RefineryBins,
        AFMRepairCapacity,
        AFMRepairConsumption,
        AFMRepairPerAmmo,
        MaxRange,
        SensorTargetScanAngle,
        Range,
        VehicleCargoCapacity,
        VehicleHullMass,
        VehicleFuelCapacity,
        VehicleArmourHealth,
        VehicleShieldHealth,
        FighterMaxSpeed,
        FighterBoostSpeed,
        FighterPitchRate,
        FighterDPS,
        FighterYawRate,
        FighterRollRate,
        CabinCapacity,
        CabinClass,
        DisruptionBarrierRange,
        DisruptionBarrierChargeDuration,
        DisruptionBarrierActivePower,
        DisruptionBarrierCooldown,
        WingDamageReduction,
        WingMinDuration,
        WingMaxDuration,
        ShieldSacrificeAmountRemoved,
        ShieldSacrificeAmountGiven,
        FSDJumpRangeBoost,
        FSDFuelUseIncrease,
        BoostSpeedMultiplier,
        BoostAugmenterPowerUse,
        ModuleDefenceAbsorption,
        FalloffRange,
        DSS_RangeMult,
        DSS_AngleMult,
        DSS_RateMult,
        DSS_PatchRadius
    }

    public enum ReputationStatus
    {
        Hostile = -2,
        Unfriendly = -1,
        Neutral = 0,
        Cordial = 1,
        Friendly = 2,
        Allied = 3
    }

    public enum FriendStatus
    {
        Unknown,
        Requested,
        Declined,
        Added,
        Lost,
        Offline,
        Online
    }

    public enum DroneType
    {
        Unknown,
        Hatchbreaker,
        FuelTransfer,
        Collection,
        Prospector,
        Repair,
        Research,
        Decontamination
    }

    public enum Economies
    {
        Unknown,

        [Description("$economy_Agri;")]
        Agriculture,

        [Description("$economy_Colony;")]
        Colony,

        [Description("$economy_Extraction;")]
        Extraction,

        [Description("$economy_HighTech;")]
        HighTech,

        [Description("$economy_Industrial;")]
        Industrial,

        [Description("$economy_Military;")]
        Military,

        [Description("$economy_None;")]
        None,

        [Description("$economy_Refinery;")]
        Refinery,

        [Description("$economy_Service;")]
        Service,

        [Description("$economy_Terraforming;")]
        Terraforming,

        [Description("$economy_Tourism;")]
        Tourism,

        [Description("$economy_Prison;")]
        Prison,

        [Description("$economy_Damaged;")]
        Damaged,

        [Description("$economy_Rescue;")]
        Rescue,

        [Description("$economy_Repair;")]
        Repair,

        [Description("$economy_Carrier;")]
        PrivateEnterprise,

        [Description("$economy_Engineer;")]
        Engineering,
    }

    public enum Allegiances
    {
        Unknown,
        Independent,
        PilotsFederation,
        Federation,
        Empire,
        Alliance,
        Guardian,
        Thargoid,
        PlayerPilots,
    }

    public enum Governments
    {
        Unknown,
        [Description("$government_Anarchy;")] Anarchy,
        [Description("$government_Communism;")] Communism,
        [Description("$government_Confederacy;")] Confederacy,
        [Description("$government_Cooperative;")] Cooperative,
        [Description("$government_Corporate;")] Corporate,
        [Description("$government_Democracy;")] Democracy,
        [Description("$government_Dictatorship;")] Dictatorship,
        [Description("$government_Feudal;")] Feudal,
        [Description("$government_Imperial;")] Imperial,
        [Description("$government_None;")] None,
        [Description("$government_Patronage;")] Patronage,
        [Description("$government_PrisonColony;")] PrisonColony,
        [Description("$government_Theocracy;")] Theocracy,
        [Description("$government_Engineer;")] Engineer,
        [Description("$government_Carrier;")] PrivateOwnership,
    }

    public enum SecurityLevels
    {
        Unknown,
        [Description("$GAlAXY_MAP_INFO_state_anarchy;")]
        Anarchy,

        [Description("$GALAXY_MAP_INFO_state_lawless;")]
        Lawless,

        [Description("$SYSTEM_SECURITY_high;")]
        High,

        [Description("$SYSTEM_SECURITY_low;")]
        Low,

        [Description("$SYSTEM_SECURITY_medium;")]
        Medium,
    }

    public enum LegalState
    {
        Unknown,
        Clean,
        IllegalCargo,
        Speeding,
        Wanted,
        Hostile,
        PassengerWanted,
        Warrant,
        Lawless
    }

}
