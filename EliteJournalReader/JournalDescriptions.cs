using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteJournalReader
{

    public enum ScanType
    {
        Unknown,
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


    public enum PlanetClass
    {
        Unknown,
        MetalRichBody,
        HighMetalContentBody,
        RockyBody,
        IcyBody,
        RockyIceBody,
        EarthlikeBody,
        WaterWorld,
        AmmoniaWorld,
        WaterGiant,
        WaterGiantWithLife,
        GasGiantWithWaterBasedLife,
        GasGiantWithAmmoniaBasedLife,
        SudarskyClassIGasGiant,
        SudarskyClassIIGasGiant,
        SudarskyClassIIIGasGiant,
        SudarskyClassIVGasGiant,
        SudarskyClassVGasGiant,
        HeliumRichGasGiant,
        HeliumGasGiant
    }

    public enum AtmosphereClass
    {
        Unknown,
        NoAtmosphere,
        SuitableForWaterBasedLife,
        AmmoniaAndOxygen,
        Ammonia,
        Water,
        CarbonDioxide,
        SulphurDioxide,
        Nitrogen,
        WaterRich,
        MethaneRich,
        AmmoniaRich,
        CarbonDioxideRich,
        Methane,
        Helium,
        SilicateVapour,
        MetallicVapour,
        NeonRich,
        ArgonRich,
        Neon,
        Argon,
        Oxygen
    }

    public enum AtmosphereType
    {
        Unknown,
        None,
        EarthLike,
        AmmoniaOxygen,
        Oxygen,
        Ammonia,
        Water,
        CarbonDioxide,
        SulphurDioxide,
        Nitrogen,
        WaterRich,
        MethaneRich,
        AmmoniaRich,
        CarbonDioxideRich,
        Methane,
        Helium,
        Neon,
        Argon,
        NeonRich,
        ArgonRich,
        SilicateVapour,
        MetallicVapour
    }

    public enum VolcanismClass
    {
        Unknown,
        None,
        WaterMagma,
        SulphurDioxideMagma,
        AmmoniaMagma,
        MethaneMagma,
        NitrogenMagma,
        SilicateMagma,
        MetallicMagma,
        WaterGeysers,
        CarbonDioxideGeysers,
        AmmoniaGeysers,
        MethaneGeysers,
        NitrogenGeysers,
        HeliumGeysers,
        SilicateVapourGeysers
    }

    public enum CrimeType
    {
        Unknown,
        Assault,
        Murder,
        Piracy,
        Interdiction,
        IllegalCargo,
        DisobeyPolice,
        FireInNoFireZone,
        FireInStation,
        DumpingDangerous,
        DumpingNearStation,
        DockingMinor_BlockingAirlock,
        DockingMajor_BlockingAirlock,
        DockingMinor_BlockingLandingPad,
        DockingMajor_BlockingLandingPad,
        DockingMinor_Trespass,
        DockingMajor_Trespass,
        CollidedAtSpeedInNoFireZone,
        CollidedAtSpeedInNoFireZone_HullDamage
    }

    public enum BodyType
    {
        Unknown,
        Null,
        Star,
        Planet,
        PlanetaryRing,
        StellarRing,
        Station,
        AsteroidCluster
    }

    public enum GameMode
    {
        Unknown,
        Open,
        Solo,
        Group
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
        Depleted,
        Low,
        Common,
        Major,
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
        Player,
        Wing,
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
        DSS_RateMult
    }

    public enum ReputationStatus
    {
        Hostile = -2,
        Unfriendly = 1,
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
}
