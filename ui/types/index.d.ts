type Vector = {
    x: number;
    y: number;
};
declare enum DebrisSize {
    LARGE = 0,
    MEDIUM = 1,
    SMALL = 2
}
export declare enum DebrisSkin {
    Debris = "DEBRIS",
    Meteor = "METEOR"
}
export interface Size {
    width: number;
    height: number;
}
export interface Coordinates {
    x: number;
    y: number;
}
export interface ProjectileType {
    position: Vector;
    velocity: Vector;
    size: number;
    orientation: number;
}
export interface ShipConstants {
    grid: Size;
    maxHealth: number;
    maxShield: number;
    stations: {
        turretInfos: Record<TurretType, TurretConstants>;
        radar: RadarConstants;
        shield: ShieldConstants;
    };
}
export interface ShieldConstants {
    shieldBreakHandicap: number;
    shieldRadius: number;
    shieldRegenerationPercent: number;
}
export interface RadarConstants {
    minimumScanCharge: number;
}
export interface TurretConstants {
    cooldownTicks: number;
    maxCharge: number;
    maxConeAngleDegrees: number;
    maxRotationDegrees: number;
    rocketChargeCost: number;
    rocketDamage: number;
    rocketRadius: number;
    rocketSpeed: number;
}
export interface Explosion {
    debrisType: DebrisSize;
    approximateAngle: number;
}
export interface DebrisConstants {
    size: number;
    damage: number;
    aproximateSpeed: number;
    explodesInto: Explosion[];
}
export interface GameConstants {
    world: Size;
    ship: ShipConstants;
    meteorInfos: Record<DebrisSize, DebrisConstants>;
}
export interface Crew {
    id: string;
    name: string;
    age: number;
    socialInsurance: string;
    worldPosition: Coordinates;
    relativePosition: Coordinates;
    currentStation: string;
    destination: Coordinates;
}
export interface RadarStation {
    charge: number;
    currentTarget: unknown;
    gridPosition: Coordinates;
    id: string;
    worldPosition: Coordinates;
    relativePosition: Coordinates;
}
export interface ShieldStation {
    id: string;
    gridPosition: Coordinates;
    worldPosition: Coordinates;
    relativePosition: Coordinates;
}
export interface HelmStation {
    id: string;
    gridPosition: Coordinates;
    worldPosition: Coordinates;
    relativePosition: Coordinates;
}
export type TurretType = "NORMAL" | "EMP" | "FAST" | "CANNON" | "SNIPER";
export interface TurretStation {
    charge: number;
    cooldown: number;
    gridPosition: Coordinates;
    id: string;
    orientationDegrees: number;
    worldPosition: Coordinates;
    relativePosition: Coordinates;
    turretType: TurretType;
}
export interface HelmStation {
    charge: number;
    currentTarget: null;
    gridPosition: Coordinates;
    worldPosition: Coordinates;
    relativePosition: Coordinates;
    id: string;
}
export interface Stations {
    radars: RadarStation[];
    shields: ShieldStation[];
    turrets: TurretStation[];
    helms: HelmStation[];
}
export interface Ship {
    crew: Crew[];
    currentHealth: number;
    currentShield: number;
    stations: Stations;
    teamId: string;
    teamName?: string;
    tileSize: number;
    walkableTiles: Coordinates[];
    nonWalkableTiles: Coordinates[];
    worldPosition: Coordinates;
    relativePosition: Coordinates;
    orientationDegrees: number;
}
declare enum CollisionType {
    RocketDebris = "ROCKET_DEBRIS",
    RocketShield = "ROCKET_SHIELD",
    RocketShip = "ROCKET_SHIP",
    DebrisShield = "DEBRIS_SHIELD",
    DebrisShip = "DEBRIS_SHIP"
}
export interface Collision {
    intersection: Coordinates;
    radius: number;
    type: CollisionType;
}
export interface Debris {
    debrisType: DebrisSize;
    debrisSkin: DebrisSkin;
    id: string;
    position: Coordinates;
    radius: number;
    velocity: Coordinates;
    orientationDegrees: number;
}
export interface Tick {
    currentTickNumber: number;
    collisions: Collision[];
    constants: GameConstants;
    debris: Debris[];
    rockets: Rocket[];
    ships: Ship[];
    shipName: string;
}
export interface Rocket {
    id: string;
    position: Coordinates;
    radius: number;
    teamId: string;
    velocity: Coordinates;
    bonusShieldDamage: number;
    bonusHullDamage: number;
    projectileSkin: TurretType | null;
}
export interface AdditionalProperties {
}
export {};
