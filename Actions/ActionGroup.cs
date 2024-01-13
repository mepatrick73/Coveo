using System.Numerics;
using System.Xml.Schema;

namespace Application.Actions;

public class ActionGroup
{
    protected ActionGroup()
    {
        Actions = new List<CustomAction>();
    }

    public void Add(Action action)
    {
        Actions.Add(new CustomAction(action));
    }

    public List<Action> GetActions()
    {
        return Actions.Select(a => a.Action).ToList();
    }

    public bool IsValid()
    {
        return Valid;
    }

    private List<CustomAction> Actions { get; set; }
    protected bool Valid = true;
}

public class TurretUtil
{
    public static int ShootingTurn(GameMessage gameState, List<TurretStation> turrets)
    {
        if (turrets.Count == 0)
            return 0;
        int chargingTurns = 0;
        double totalShieldDamage = 0;
        while(true)
        {
            totalShieldDamage = 0;
            foreach (var turret in turrets)
            {
                totalShieldDamage += turret.Charge +
                                    (chargingTurns % gameState.Constants.Ship.Stations.TurretInfos[turret.TurretType].RocketChargeCost 
                                     * (gameState.Constants.Ship.Stations.TurretInfos[turret.TurretType].RocketDamage 
                                        + gameState.Constants.Ship.Stations.TurretInfos[turret.TurretType].RocketBonusShieldDamage));
            }
            
            if(totalShieldDamage >= gameState.Constants.Ship.MaxShield)
            {
                break;
            }
            
            ++chargingTurns;
        }
        
        return chargingTurns;
    }
}

public class MathUtil
{
    public static double AngleBetween(Vector a, Vector b)
    {
        var dotProduct = Dot(a, b);
        var determinant = a.X * b.Y - a.Y * b.X;
        return Math.Atan2(determinant, dotProduct) * 180 / Math.PI;
    }

    public static double Dot(Vector a, Vector b)
    {
        return a.X * b.X + a.Y * b.Y;
    }
    
    public static double Distance(Vector a, Vector b)
    {
        return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }
    
    public static Vector Normalize(Vector a)
    {
        var length = Math.Sqrt(Math.Pow(a.X, 2) + Math.Pow(a.Y, 2));
        return new Vector(a.X / length, a.Y / length);
    }
    
    public static Vector Rotate(Vector a, double angle)
    {
        var radians = angle * Math.PI / 180;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return new Vector(a.X * cos - a.Y * sin, a.X * sin + a.Y * cos);
    }
    
    public static Vector Subtract(Vector a, Vector b)
    {
        return new Vector(a.X - b.X, a.Y - b.Y);
    }
    
    public static Vector Add(Vector a, Vector b)
    {
        return new Vector(a.X + b.X, a.Y + b.Y);
    }
    
    public static Vector Multiply(Vector a, double b)
    {
        return new Vector(a.X * b, a.Y * b);
    }
    
    public static double Length(Vector a)
    {
        return Math.Sqrt(Math.Pow(a.X, 2) + Math.Pow(a.Y, 2));
    }
    
    public static double LengthSquared(Vector a)
    {
        return Math.Pow(a.X, 2) + Math.Pow(a.Y, 2);
    }
}

public class MoveAction : ActionGroup
{
    public MoveAction(Dictionary<string, Vector> dict)
    {
        foreach (var pair in dict)
        {
            Add(new CrewMoveAction(pair.Key, pair.Value));
        }
    }
}

public class BreakShieldAction : ActionGroup
{
    public BreakShieldAction(GameMessage gameState, Vector shipPosition)
    {
        var turretActions = new Dictionary<TurretStation, List<CustomAction>>();
        var helmActions = new Dictionary<HelmStation, List<CustomAction>>();
        foreach (var station in gameState.Ships[gameState.CurrentTeamId].Stations.Turrets
                     .Where(station => station.Operator != null).ToList())
        {
            turretActions.Add(station, new List<CustomAction>());
        }

        foreach (var station in gameState.Ships[gameState.CurrentTeamId].Stations.Helms
                     .Where(station => station.Operator != null).ToList())
        {
            helmActions.Add(station, new List<CustomAction>());
        }
        
        // Determine at which turn we should shoot to burst all shots on the shield and one shot it
        int shootingTurn = TurretUtil.ShootingTurn(gameState, turretActions.Keys.ToList());
        

        var ownShipPosition = gameState.Ships[gameState.CurrentTeamId].WorldPosition;
        var remainingAngleForShipWeaponsToFaceTarget = MathUtil.AngleBetween(ownShipPosition, shipPosition) -
                                                       gameState.Ships[gameState.CurrentTeamId].OrientationDegrees;
        var turnCount = 0;
        
        // If someone is on the helm, rotate the ship
        if (helmActions.Count != 0)
        {
            while (remainingAngleForShipWeaponsToFaceTarget > 0)
            {
                foreach(var helm in helmActions)
                {
                    remainingAngleForShipWeaponsToFaceTarget -= gameState.Constants.Ship.MaxRotationDegrees;
                    turnCount++;
                    helm.Value.Add(new CustomAction(new ShipRotateAction(gameState.Constants.Ship.MaxRotationDegrees)));
                }
            }
        }
        
        // Turrets that can rotate
        foreach (var turret in turretActions.Where(tur =>
                     gameState.Constants.Ship.Stations.TurretInfos[tur.Key.TurretType].Rotatable))
        {
            var remainingAngle = MathUtil.AngleBetween(turret.Key.WorldPosition, shipPosition) -
                                 turret.Key.OrientationDegrees;
            while (remainingAngle > 0)
            {
                remainingAngle -= gameState.Constants.Ship.MaxRotationDegrees;
                turret.Value.Add(new CustomAction(new TurretRotateAction(turret.Key.Id, gameState.Constants.Ship.MaxRotationDegrees)));
            }
            
            int remainingChargeTurns = shootingTurn;
            while (remainingChargeTurns > 0)
            {
                turret.Value.Add(new CustomAction(new TurretChargeAction(turret.Key.Id)));
                --remainingChargeTurns;
            }
            
            turret.Value.Add(new CustomAction(new TurretShootAction(turret.Key.Id)));
        }

        // Turrets that can't rotate
        foreach (var turret in turretActions.Where(tur =>
                     !gameState.Constants.Ship.Stations.TurretInfos[tur.Key.TurretType].Rotatable))
        {
            int remainingChargeTurns = shootingTurn;
            while (remainingChargeTurns > 0)
            {
                turret.Value.Add(new CustomAction(new TurretChargeAction(turret.Key.Id)));
                --remainingChargeTurns;
            }
            
            if(helmActions.Count != 0)
            {
                turret.Value.Add(new CustomAction(new TurretShootAction(turret.Key.Id)));
            }
        }
        
        // Add all actions to the action group
        int index = 0;
        bool actionAdded = true;
        
        while (actionAdded)
        {
            actionAdded = false;
            foreach (var helm in helmActions)
            {
                if (index >= helm.Value.Count)
                {
                    continue;
                }
                
                Add(helm.Value[index].Action);
                actionAdded = true;
            }
            
            foreach (var turret in turretActions)
            {
                if (index >= turret.Value.Count)
                {
                    continue;
                }
                
                Add(turret.Value[index].Action);
                actionAdded = true;
            }
            
            ++index;
        }
    }
}

public class ActionsGenerator
{
    public static List<ActionGroup> GenerateActions(GameMessage gameState)
    {
        var actions = new List<ActionGroup>();

        foreach (var ship in gameState.ShipsPositions.Where(ship => ship.Key != gameState.CurrentTeamId))
        {
            actions.Add(new BreakShieldAction(gameState, ship.Value));
        }

        return actions.Where(action => action.IsValid()).ToList();
    }
}