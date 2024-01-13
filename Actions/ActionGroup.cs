

using System.Numerics;

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
    private List<CustomAction> Actions { get; set; }
}

public class BreakShieldAction : ActionGroup
{
    public BreakShieldAction(GameMessage gameState, Vector shipPosition)
    {
        var weaponStations = gameState.Ships[gameState.CurrentTeamId].Stations.Turrets;
        var mannedStations = weaponStations.Where(station => station.Operator != null).ToList();
        
        if (mannedStations.Count == 0)
        {
        }
        
        
    }
}

public class ActionsGenerator
{
    public static List<ActionGroup> GenerateActions(GameMessage gameState)
    {
        var actions = new List<ActionGroup>();

        foreach (var ship in gameState.ShipsPositions)
        {
            actions.Add(new BreakShieldAction(gameState, ship.Value));
        }

        return actions;
    }
}