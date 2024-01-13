using Application.Actions;

namespace Application;


public class CrewmateManager
{
    private List<Crewmate> crewmates;
    private bool isThereTwoPeopleWithTheSameGoal;

    public CrewmateManager(List<Crewmate> pCrewmates)
    {
        this.crewmates = pCrewmates;
        this.isThereTwoPeopleWithTheSameGoal = checkIfItMakeSense();
        if (isThereTwoPeopleWithTheSameGoal)
        {
            Console.WriteLine("There is two crewmates with the same goal");
        }
    }

    public bool checkIfItMakeSense()
    {
        foreach (var crewmate in this.crewmates)
        {
            foreach (var other_crewmate in this.crewmates)
            {
                if (!crewmate.Id.Equals(other_crewmate.Id))
                {
                    if (CrewHaveSameGoal(crewmate,other_crewmate))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    //return the Destination if the crewmate has one, otherwise returns the current position
    private Vector GetGoal(Crewmate crewmate)
    {
        if (crewmate.Destination != null)
        {
            return crewmate.Destination;
        }
        return crewmate.GridPosition;
    }
    private bool CrewHaveSameGoal(Crewmate crewmate, Crewmate other_crewmate)
    {
        return GetGoal(crewmate).Equals(GetGoal(other_crewmate));
    }

    public Tuple<ActionGroup, int> moveCrewmates(Dictionary<TurretType, int> plannedState)
    {
        return null;
    }

    public Crewmate? GetCrewmateFromId(string Id)
    {
        foreach (var crewmate in this.crewmates)
        {
            if (crewmate.Id.Equals(Id))
            {
                return crewmate;
            }
        }
        return null;
    }
    
    
}