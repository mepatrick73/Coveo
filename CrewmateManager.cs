﻿using System.Diagnostics;
using Application.Actions;
using Microsoft.VisualBasic.CompilerServices;

namespace Application;


public class CrewmateManager
{
    private List<Crewmate> crewmates;
    private List<TurretStation> pTurretStation;
    private bool isThereTwoPeopleWithTheSameGoal;

    public CrewmateManager(List<Crewmate> pCrewmates, List<TurretStation> pTurretStation)
    {
        this.crewmates = pCrewmates;
        this.pTurretStation = pTurretStation;
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

    public Tuple<ActionGroup, int> moveCrewmates(List<Station> plannedStation)
    {
        int numberOfCrewMatesNeeded = plannedStation.Count;
        List<Crewmate[]> crewmatePermutation =
            heapPermutation(crewmates.ToArray(), numberOfCrewMatesNeeded, this.crewmates.Count);
        int shortest_time = Int32.MaxValue;
        List<Crewmate> shortest_permutation = new List<Crewmate>();
        foreach (var permutation in crewmatePermutation)
        {
            int longest_crewmate = 0;
            for (int i = 0; i < numberOfCrewMatesNeeded; i++)
            {
                Crewmate currCrewmate = permutation[i];
                int time_taken = getDistanceToStation(currCrewmate, plannedStation[i].Id);
                if (time_taken > longest_crewmate)
                {
                    longest_crewmate = time_taken;
                }
            }

            if (shortest_time > longest_crewmate)
            {
                shortest_permutation = permutation.ToList();
                shortest_time = longest_crewmate;
            }
        }

        Dictionary<string, Vector> dict = new Dictionary<String,Vector>();
        for (int i = 0; i < numberOfCrewMatesNeeded ; i++)
        {
            dict[shortest_permutation[i].Id] = plannedStation[i].GridPosition;
        }

        
        return new Tuple<ActionGroup,int>(new MoveAction(dict),shortest_time);
    }

    private int getDistanceToStation(Crewmate crewmate, string stationId)
    {
        int distance = FindDistance(crewmate.DistanceFromStations.Turrets, stationId) ??
                                  FindDistance(crewmate.DistanceFromStations.Shields, stationId) ??
                                  FindDistance(crewmate.DistanceFromStations.Radars, stationId) ??
                                  FindDistance(crewmate.DistanceFromStations.Helms, stationId) ??
                                  int.MaxValue; // Indicate that the station was not found
        
                return distance;
    }

     private static int? FindDistance(DistanceFromStation[] distances, string stationId)
        {
            var distance = distances.FirstOrDefault(d => d.StationId == stationId);
            return distance?.Distance;
        }
     public static List<Crewmate[]> heapPermutation(Crewmate[] a, int size, int n)
     {
         var permutations = new List<Crewmate[]>();
            // if size becomes 1 then prints the obtained
            // permutation
            if (size == 1)
            {
                Crewmate[] currentPermutation = new Crewmate[n];
                Array.Copy(a, currentPermutation, n);
                permutations.Add(currentPermutation);
            }
                
            for (int i = 0; i < size; i++)
                    {
                        permutations.AddRange(heapPermutation(a, size - 1, n));
            
                        // if size is odd, swap 0th i.e (first) and
                        // (size-1)th i.e (last) element
                        if (size % 2 == 1)
                        {
                            var temp = a[0];
                            a[0] = a[size - 1];
                            a[size - 1] = temp;
                        }
            
                        // If size is even, swap ith and
                        // (size-1)th i.e (last) element
                        else
                        {
                            var temp = a[i];
                            a[i] = a[size - 1];
                            a[size - 1] = temp;
                        }
                    }
            
                    return permutations;
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