using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Application;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ActionTypes
{
    [JsonPropertyName("TURRET_ROTATE")]
    TurretRotate,

    [JsonPropertyName("TURRET_LOOK_AT")]
    TurretLookAt,

    [JsonPropertyName("TURRET_CHARGE")]
    TurretCharge,

    [JsonPropertyName("TURRET_SHOOT")]
    TurretShoot,

    [JsonPropertyName("RADAR_SCAN")]
    RadarScan,

    [JsonPropertyName("CREW_MOVE")]
    CrewMove,

    [JsonPropertyName("SHIP_ROTATE")]
    ShipRotate,

    [JsonPropertyName("SHIP_LOOK_AT")]
    ShipLookAt,
}

public enum DebrisType
{
    [JsonPropertyName("LARGE")]
    Large,

    [JsonPropertyName("MEDIUM")]
    Medium,

    [JsonPropertyName("SMALL")]
    Small
}

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TurretType
{
    [JsonPropertyName("NORMAL")]
    Normal,

    [JsonPropertyName("FAST")]
    Fast,

    [JsonPropertyName("CANNON")]
    Cannon,

    [JsonPropertyName("SNIPER")]
    Sniper,

    [JsonPropertyName("EMP")]
    EMP
}

public record Vector(double X, double Y);

public record Projectile(
    string Id, 
    Vector Position, 
    Vector Velocity, 
    double Radius, 
    double Damage, 
    double BonusShieldDamage, 
    double BonusHullDamage, 
    string? TeamId
);

public record Debris(
    string Id,
    Vector Position,
    Vector Velocity,
    double Radius,
    double Damage, 
    double BonusShieldDamage, 
    double BonusHullDamage, 
    DebrisType DebrisType,
    string? TeamId
) : Projectile(Id, Position, Velocity, Radius, Damage, BonusShieldDamage, BonusHullDamage, TeamId);

[JsonDerivedType(typeof(TurretShootAction))]
[JsonDerivedType(typeof(TurretChargeAction))]
[JsonDerivedType(typeof(TurretLookAtAction))]
[JsonDerivedType(typeof(TurretRotateAction))]
[JsonDerivedType(typeof(RadarScanAction))]
[JsonDerivedType(typeof(CrewMoveAction))]
[JsonDerivedType(typeof(ShipLookAtAction))]
[JsonDerivedType(typeof(ShipRotateAction))]
public abstract record Action(ActionTypes Type);

public record TurretShootAction(string StationId) : Action(ActionTypes.TurretShoot);

public record TurretChargeAction(string StationId) : Action(ActionTypes.TurretCharge);

public record TurretLookAtAction(string StationId, Vector Target)
    : Action(ActionTypes.TurretLookAt);

public record TurretRotateAction(string StationId, double Angle) : Action(ActionTypes.TurretRotate);

public record RadarScanAction(string StationId, string TargetShip) : Action(ActionTypes.RadarScan);

public record CrewMoveAction(string CrewMemberId, Vector Destination)
    : Action(ActionTypes.CrewMove);

public record ShipRotateAction(double Angle) : Action(ActionTypes.ShipRotate);

public record ShipLookAtAction(Vector Target) : Action(ActionTypes.ShipLookAt);

public record DebrisInfos(
    double Damage,
    double Radius,
    double ApproximateSpeed,
    ExplodesInto[] ExplodesInto
);

public record ExplodesInto(DebrisType DebrisType, double ApproximateAngle);

public record World(double Width, double Height);

public record Constants(
    World World,
    Dictionary<DebrisType, DebrisInfos> DebrisInfos,
    ShipInfo Ship
);

public record ShipInfo(Grid Grid, int MaxHealth, int MaxShield, double MaxRotationDegrees, StationsInfo Stations);

public record Grid(int Width, int Height);

public record StationsInfo(
    Dictionary<TurretType, TurretInfo> TurretInfos,
    ShieldInfo Shield,
    RadarInfo Radar
);

public record TurretInfo(
    bool Rotatable,
    int RocketChargeCost,
    int MaxCharge,
    double RocketSpeed,
    double RocketRadius,
    double RocketDamage,
    double RocketBonusShieldDamage,
    double RocketBonusHullDamage
);

public record ShieldInfo(double ShieldRadius, double ShieldRegenerationPercent, int ShieldBreakHandicap);

public record RadarInfo(double RadarRadius);

public record GameMessage(
    int CurrentTickNumber,
    string[] LastTickErrors,
    string CurrentTeamId,
    Constants Constants,
    Debris[] Debris,
    Projectile[] Rockets,
    Dictionary<string, Vector> ShipsPositions,
    Dictionary<string, Ship> Ships
);

public record Ship(
    string TeamId,
    Vector WorldPosition,
    double CurrentHealth,
    double CurrentShield,
    double OrientationDegrees,
    Vector[] WalkableTiles,
    Crewmate[] Crew,
    Stations Stations
);

//                  ඞ
public record Crewmate(
    string Id,
    Vector GridPosition,
    string Name,
    int Age,
    string SocialInsurance,
    Vector? Destination,
    string? CurrentStation,
    Distances DistanceFromStations
);

public record Distances(
    DistanceFromStation[] Turrets,
    DistanceFromStation[] Shields,
    DistanceFromStation[] Radars,
    DistanceFromStation[] Helms
);

public record DistanceFromStation(string StationId, int Distance, Vector StationPosition);

public abstract record Station(string Id, Vector GridPosition, string? Operator);

public record TurretStation(
    string Id,
    Vector GridPosition,
    TurretType TurretType,
    string? Operator,
    Vector WorldPosition,
    double OrientationDegrees,
    double Charge,
    double Cooldown
) : Station(Id, GridPosition, Operator);

public record ShieldStation(string Id, Vector GridPosition, string? Operator)
    : Station(Id, GridPosition, Operator);

public record RadarStation(
    string Id,
    Vector GridPosition,
    string? Operator,
    string? CurrentTarget
) : Station(Id, GridPosition, Operator);

public record HelmStation(string Id, Vector GridPosition, string? Operator)
    : Station(Id, GridPosition, Operator);

public record Stations(
    TurretStation[] Turrets,
    ShieldStation[] Shields,
    RadarStation[] Radars,
    HelmStation[] Helms
);
