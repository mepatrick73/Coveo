namespace Application;


public class CrewmateManager
{
    private List<Crewmate> crewmates;

    public CrewmateManager(List<Crewmate> pCrewmates)
    {
        this.crewmates = pCrewmates;
        if (checkIfItMakeSense())
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

    private bool CrewHaveSameGoal(Crewmate crewmate, Crewmate other_crewmate)
    {
        if (crewmate.Destination != null)
        {
            if (other_crewmate.Destination != null)
            {
                {
                    if (crewmate.Destination.Equals(other_crewmate.Destination))
                    {
                        return true;
                    }
                }
            }
            else
            {
                {
                    if (crewmate.Destination.Equals(other_crewmate.GridPosition))
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            if (other_crewmate.Destination != null)
            {
                if (other_crewmate.Destination.Equals(crewmate.GridPosition))
                {
                    return true;
                }
            }
            else
            {
                if (crewmate.GridPosition.Equals(other_crewmate.GridPosition))
                {
                    return true;
                }
            }
        }
        return false;
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