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

        Debris[] targetableMeteors = gameMessage.Debris.ToArray();
        Debris[] untargetedTargetableMeteors =
            targetableMeteors.Except(targetedMeteors.Keys, new DebrisEqualityComparer()).ToArray();
        
        var ourShip = this.gameMessage.Ships[gameMessage.CurrentTeamId];
        var theirShips = this.gameMessage.ShipsPositions.Where(ship => ship.Key != gameMessage.CurrentTeamId).ToList();
        try
        {
            if (gameMessage.CurrentTickNumber == 1)
            {
                CrewmateManager crewmateManager = new CrewmateManager(ourShip.Crew.ToList());
    
                actions.AddRange(crewmateManager.moveCrewmates(ourShip.Stations.Turrets.ToList<Station>().GetRange(0,4)));
                //CrewmateManager crewmateManager2 = new CrewmateManager(ourShip.Crew.ToList().GetRange(3,1));
               // List<Station> helm = new List<Station>(ourShip.Stations.Helms.Take(1).ToList());
               // actions.AddRange(crewmateManager2.moveCrewmates(helm).Item1.GetActions());
            }
            if (gameMessage.CurrentTickNumber % 1000 == 0)
            {
                foreach (var crewmate in ourShip.Crew.ToList().GetRange(0,4))
                {
  

                }
            }

        }
        catch (Exception e)
        {
            MOVECREW(gameMessage, actions);
        }
        
        
        
        
      
        //enlever les cibles qui ont deja ete tirees

        foreach (var turret in ourShip.Stations.Turrets.Where(turret => turret.Operator != null))
        {
            Action orient;

            if (turret.TurretType == TurretType.EMP)
            {
                orient = new TurretLookAtAction(turret.Id,new Vector(theirShips.First().Value.X, theirShips.First().Value.Y)); // changer avant la compet pour le premier ennemi
            }
            else
            {
                FindBestTarget(turret,gameMessage.Constants.Ship.Stations.TurretInfos[turret.TurretType],untargetedTargetableMeteors,out (double x,double y ) shotPosition, ourShip.WorldPosition, gameMessage.Constants.Ship.Stations.Shield.ShieldRadius);
                if (shotPosition is { x: 0, y: 0 })
                {
                    orient = new TurretLookAtAction(turret.Id,new Vector(theirShips.First().Value.X,theirShips.First().Value.Y));
                }
                else
                {
                    orient = new TurretLookAtAction(turret.Id,new Vector(shotPosition.x,shotPosition.y));
                }

            }
            
            if (!gameMessage.Constants.Ship.Stations.TurretInfos[turret.TurretType].Rotatable)
            {
                actions.Add(new ShipLookAtAction(new Vector(theirShips.First().Value.X,
                    theirShips.First().Value.Y))); 
                //actions = actions.Concat(PositionWeaponTowardsFirstEnemy(new Vector(theirShips.First().Value.X, theirShips.First().Value.Y))).ToList(); 
            }
 
            Action shoot = new TurretShootAction(turret.Id);
            
            actions.Add(orient);
            actions.Add(shoot);
        }



        //selectionner la meilleure cible selon lheuristique


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

    private List<Action> PositionWeaponTowardsFirstEnemy(Vector enemyPosition)
    {
        try
        {
            var enemyShip = enemyPosition;

            var weaponToShootFrom = gameMessage.Ships[gameMessage.CurrentTeamId].Stations.Turrets.Where(turret =>
                !gameMessage.Constants.Ship.Stations.TurretInfos[turret.TurretType].Rotatable).ToList().First();
            var weaponAngle = weaponToShootFrom.OrientationDegrees;
            var ownShipPosition = gameMessage.Ships[gameMessage.CurrentTeamId].WorldPosition;

            var actions = new List<Action>();
            if (Math.Abs(MathUtil.AngleBetween(MathUtil.Subtract(enemyShip, ownShipPosition),
                    MathUtil.FromAngleDegrees(weaponAngle))) > 1e-3f)
            {
                actions.Add(new ShipRotateAction(Math.Abs(MathUtil.AngleBetween(
                    MathUtil.Subtract(enemyShip, ownShipPosition),
                    MathUtil.FromAngleDegrees(weaponAngle)))));
            }

            return actions;
        }
        catch (Exception e)
        {
            return new List<Action>();
        }
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