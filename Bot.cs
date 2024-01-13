using System.Numerics;
using Application.Actions;

namespace Application;

public class Bot
{
    public const string NAME = "My cool C# bot";
    private Dictionary<Debris, Projectile> targetedMeteors = new();
    private Debris targetMeteor;
    private Debris targetedMeteor;

    private GameMessage gameMessage;

    /// <summary>
    /// This method should be use to initialize some variables you will need throughout the game.
    /// </summary>
    public Bot()
    {
        Console.WriteLine("Initializing your super mega bot!");
        
    }

    /// <summary>
    /// Here is where the magic happens, for now the moves are random. I bet you can do better ;)
    /// </summary>
    public IEnumerable<Action> GetNextMoves(GameMessage gameMessage)
    {
        var actions = new List<Action>();
        this.gameMessage = gameMessage;
    //     string our_team = gameMessage.CurrentTeamId;
    //     Ship ship = gameMessage.Ships[our_team];
    //     List<Crewmate> crew = ship.Crew.ToList();
    //     Console.WriteLine(ship.Stations.Turrets);
    //     
    // var turretStations = ship.Stations.Turrets
    //     .Where(turret =>
    //         turret.TurretType == TurretType.EMP ||
    //         turret.TurretType == TurretType.Sniper ||
    //         turret.TurretType == TurretType.Fast ||
    //         turret.TurretType == TurretType.Normal||
    //         turret.TurretType == TurretType.Cannon)
    //     .GroupBy(turret => turret.TurretType)
    //     .SelectMany(group => group.Take(1))
    //     .ToList();
    // Console.WriteLine(turretStations.ToString());
    // Console.WriteLine("yoooooo");
    //     var crewmateManager = new CrewmateManager(crew,turretStations);
    //     List<Station> turret_dict = new List<Station>();
    //     turret_dict.AddRange(turretStations);
    //     Tuple<ActionGroup, int> temp = crewmateManager.moveCrewmates(turret_dict);
    //         foreach (var t_action in temp.Item1.GetActions())
    //         {
    //             actions.Add(t_action);
    //         }
        

        Debris[] targetableMeteors = gameMessage.Debris.Where(DebrisInfos => DebrisInfos.TeamId == null).ToArray();

        Debris[] targetableMeteors = gameMessage.Debris.ToArray();


        //enlever les cibles qui ont deja ete tirees

        Debris[] untargetedTargetableMeteors =
            targetableMeteors.Except(targetedMeteors.Keys, new DebrisEqualityComparer()).ToArray();

        //selectionner la meilleure cible selon lheuristique
        var ourShip = this.gameMessage.Ships[gameMessage.CurrentTeamId];
        foreach (var turret in ourShip.Stations.Turrets.Where(turret => turret.Operator != null))
        {
            FindBestTarget(turret,gameMessage.Constants.Ship.Stations.TurretInfos[turret.TurretType],untargetedTargetableMeteors,out (double x,double y ) shotPosition, ourShip.WorldPosition, gameMessage.Constants.Ship.Stations.Shield.ShieldRadius);
            if (shotPosition is { x: 0, y: 0 })
            {
                return Array.Empty<Action>();
            }

            Action orient;

            if(!gameMessage.Constants.Ship.Stations.TurretInfos[turret.TurretType].Rotatable)
                orient = new ShipLookAtAction(new Vector(shotPosition.x,shotPosition.y));
            else
            {
                orient = new TurretLookAtAction(turret.Id,new Vector(shotPosition.x,shotPosition.y));
            }

            Action shoot = new TurretShootAction(turret.Id);
            
            actions.Add(orient);
            actions.Add(shoot);
        }
        
        





        return actions;
    }

    private static void MOVECREW(GameMessage gameMessage, List<Action> actions)
    {
        var myShip = gameMessage.Ships[gameMessage.CurrentTeamId];
        // var otherShipsIds = gameMessage.ShipsPositions.Keys.Where(shipId => shipId != gameMessage.CurrentTeamId)
        //     .ToList();
        //
        // // You could find who's not doing anything and try to give them a job?
        var idleCrewmates = myShip.Crew
            .Where(crewmate => crewmate.CurrentStation == null && crewmate.Destination == null)
            .ToList();
        foreach (var crewmate in idleCrewmates)
        {
            var visitableStations = crewmate.DistanceFromStations.Shields
                .Concat(crewmate.DistanceFromStations.Turrets)
                .Concat(crewmate.DistanceFromStations.Helms)
                .Concat(crewmate.DistanceFromStations.Radars)
                .ToList();
         
            var stationToMoveTo = visitableStations[Random.Shared.Next(visitableStations.Count)];
         
            actions.Add(new CrewMoveAction(crewmate.Id, stationToMoveTo.StationPosition));
        }
    }

    private void FindBestTarget(TurretStation turretStation, TurretInfo turret,Debris[] targetableMeteors, out (double x, double y ) shotPosition, Vector shipPosition, double shipRadius)
    {
        var validShots = new List<Shot>();

        var bestShots = new List<Shot>();

        foreach (var meteor in targetableMeteors)
        {
            var rocketPosition = turretStation;

            for (var currentTick = 0; currentTick < 1000; currentTick++)
            {
                var nextPosition = PositionAt(meteor, currentTick);
                var distancesInCannonTicks = Distance(nextPosition, (rocketPosition.WorldPosition.X, rocketPosition.WorldPosition.Y)) /
                                             turret.RocketSpeed;

                if (distancesInCannonTicks < currentTick && isInBounds(nextPosition) && WillCollide(shipPosition, shipRadius, meteor))
                {
                    validShots.Add(new Shot()
                    {
                        lostTicks = currentTick - distancesInCannonTicks,
                        totalTicks = currentTick,
                        X = nextPosition.x,
                        Y = nextPosition.y,
                        target = meteor
                    });

                }

            }

            if (validShots.Count > 0)
            {
                bestShots.Add(validShots.MinBy(shot => shot.lostTicks));
            }

            validShots.Clear();
        }

        if (bestShots.Count > 0)
        {
            Shot bestShot = bestShots.OrderBy(shot => Distance( (shot.X, shot.Y ), (gameMessage.Ships[gameMessage.CurrentTeamId].WorldPosition.X,gameMessage.Ships[gameMessage.CurrentTeamId].WorldPosition.Y)) ).ThenByDescending(shot => shot.target.DebrisType)
                .First();

            targetedMeteor = bestShot.target;
            shotPosition = (bestShot.X, bestShot.Y);
        }
        else
        {
            targetedMeteor = null;
            shotPosition = (0, 0);
        }
    }

    private bool WillCollide(Vector shipPosition, double shipRadius, Debris meteor)
    {
        double meteorRadius = gameMessage.Constants.DebrisInfos[meteor.DebrisType].Radius;
        Vector meteorPosition = meteor.Position;
        Vector meteorVelocity = meteor.Velocity;

        // Calculate the time of collision using relative velocity
        double timeToCollision = MathUtil.Dot(MathUtil.Subtract(shipPosition, meteorPosition), meteorVelocity) / MathUtil.LengthSquared(meteorVelocity);

        // Calculate the predicted position of the meteor at the time of collision
        Vector predictedMeteorPosition = MathUtil.Add(meteorPosition, MathUtil.Multiply(meteorVelocity, timeToCollision));

        // Check if the distance between the ship and predicted meteor position is less than the sum of their radii
        double distance = MathUtil.Length(MathUtil.Subtract(predictedMeteorPosition, shipPosition));
        double combinedRadii = shipRadius + meteorRadius;

        return distance < combinedRadii;
    }


    private (double x, double y) Velocity((double x, double y) position1, (double x, double y) position2, double t)
    {
        double vx = (position2.x - position1.x) / t;
        double vy = (position2.y - position1.y) / t;

        return (vx, vy);
    }

    private bool isInBounds((double x, double y) position)
    {
        if (position.x > gameMessage.Constants.World.Width || position.x < 140 || position.y > gameMessage.Constants.World.Height || position.y < 0)
        {
            return false;
        }

        return true;
    }

    private double Distance((double x, double y) position1, (double x, double y) position2)
    {
        double deltaX = position2.x - position1.x;
        double deltaY = position2.y - position1.y;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    private (double x, double y) PositionAt(Projectile projectile, int t)
    {
        return (projectile.Position.X + projectile.Velocity.X * t, projectile.Position.Y + projectile.Velocity.Y * t);
    }

    private List<Action> PositionWeaponTowardsFirstEnemy()
    {
        var enemyShip = gameMessage.Ships.Where(ship => ship.Key != gameMessage.CurrentTeamId).ToList().First(ship =>ship.Value.CurrentHealth > 0).Value;
        var weaponToShootFrom = gameMessage.Ships[gameMessage.CurrentTeamId].Stations.Turrets.Where(turret => !gameMessage.Constants.Ship.Stations.TurretInfos[turret.TurretType].Rotatable && turret.Operator != null).ToList().First();
        var weaponAngle = weaponToShootFrom.OrientationDegrees;
        var ownShipPosition = gameMessage.Ships[gameMessage.CurrentTeamId].WorldPosition;

        var actions = new List<Action>();
        while (Math.Abs(MathUtil.AngleBetween(ownShipPosition, enemyShip.WorldPosition) - MathUtil.AngleBetween(weaponToShootFrom.WorldPosition, enemyShip.WorldPosition)) > 1e-6f)
        {
            actions.Add(new ShipRotateAction(Math.Abs(MathUtil.AngleBetween(ownShipPosition, enemyShip.WorldPosition)
                                                      - MathUtil.AngleBetween(weaponToShootFrom.WorldPosition,
                                                          enemyShip.WorldPosition))));
        }
        
        return actions;
    }

    class DebrisEqualityComparer : IEqualityComparer<Debris>
    {
        public bool Equals(Debris? x, Debris? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(Debris obj)
        {
            return obj.Id.GetHashCode();
        }
    }



    public struct Shot
    {
        public Shot()
        {
            lostTicks = Double.MaxValue;
        }

        public double totalTicks { get; set; }
        public double lostTicks { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Debris target { get; set; }

        public override string ToString() => $"({X}, {Y})";
    }
}