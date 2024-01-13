﻿using System.Numerics;

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
        this.gameMessage = gameMessage;


        var actions = new List<Action>();
        MOVECREW(gameMessage, actions);


        Debris[] targetableMeteors = gameMessage.Debris.ToArray();


        //enlever les cibles qui ont deja ete tirees

        Debris[] untargetedTargetableMeteors =
            targetableMeteors.Except(targetedMeteors.Keys, new DebrisEqualityComparer()).ToArray();

        //selectionner la meilleure cible selon lheuristique
        var ourShip = this.gameMessage.Ships[gameMessage.CurrentTeamId];
        foreach (var turret in ourShip.Stations.Turrets.Where(turret => turret.Operator != null))
        {
            FindBestTarget(turret,gameMessage.Constants.Ship.Stations.TurretInfos[turret.TurretType],untargetedTargetableMeteors,out (double x,double y ) shotPosition);
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

    private void FindBestTarget(TurretStation turretStation, TurretInfo turret,Debris[] targetableMeteors, out (double x, double y ) shotPosition)
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

                if (distancesInCannonTicks < currentTick && isInBounds(nextPosition) && willCollide())
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