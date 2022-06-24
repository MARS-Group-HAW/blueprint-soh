using System;
using System.Collections.Generic;
using Mars.Interfaces.Environments;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;


namespace SOHFloodBox;

public enum HouseholdType
{
    WAITING, DELAY, WALKING
}

public class Household
{
    public int numFamilyMembers { set; get; }
    public Geometry adress { set; get; }
    public List<VeddelTraveler> FamilyMemebers { set; get; }
    public int InMembersHouse { get; set; }
    
    private Position _meetingPoint = null;
    
    public HouseholdType Type { get; set; }

    public int DelayTime { get; set; }
    
    private HashSet<VeddelTraveler> _familyAtMeetingPoint  = new();

    public Position MeetingPoint
    {
        get => _meetingPoint;
        set {
            lock (Lock)
            {
                if (value == null)
                {
                    _meetingPoint = null;
                    return;
                }
                // same point already set, do noting
                if (value.Equals(_meetingPoint))
                    return;
                
                _meetingPoint = value;
                foreach(VeddelTraveler familyMember in FamilyMemebers)
                {
                    familyMember.OnMeetingPointDecided(value);
                }

                _familyAtMeetingPoint.Clear();
            }
        }  
    }

    private object Lock { get; } = new();
    public Household(int numFamilyMembers, Geometry adress)
    {
        this.numFamilyMembers = numFamilyMembers;
        this.adress = adress;
        this.InMembersHouse = 0;
        FamilyMemebers = new List<VeddelTraveler>();
    }
    public void AddFamilyMember(VeddelTraveler familyVeddelTraveler)
    {
        FamilyMemebers.Add(familyVeddelTraveler);
        if (familyVeddelTraveler.Household != this)
            familyVeddelTraveler.Household = this;
        InMembersHouse+=1;
    }

    public void IncrementNumFamilyMembers(int numberToAdd)
    {
        numFamilyMembers = numFamilyMembers + 1;
    }
    
    public void VoteFamilyMeetingPoint()
    {
        lock (Lock)
        {
            foreach(VeddelTraveler traveler in FamilyMemebers)
                if (traveler.Finished)
                    return;
            if (MeetingPoint != null)
                return;
            Position meetingPoint = null;
            double distance = Double.MaxValue;
            foreach (VeddelTraveler traveler in FamilyMemebers)
            {
                if (traveler.GoalPosition.Equals(traveler.EvacuationPoint))
                {
                    if (traveler.MultimodalRoute != null)
                    {
                        double length = traveler.MultimodalRoute.CurrentRoute.RemainingRouteDistanceToGoal;
                        if (length != 0 && length < distance)
                        {
                            distance = length;
                            meetingPoint = traveler.Position;
                        }
                    }
                }
                else
                {
                    // This else branch should never be executed due to the lock
                    
                    var tmp = traveler.MultimodalLayer.Search(traveler, traveler.Position, traveler.GoalPosition.Copy(), ModalChoice.Walking);

                    // Sometimes a route can't be found. This is ignored for now and should be examined in the future
                    if (tmp.Goal == null) continue;

                    if (tmp.RouteLength != 0 && tmp.RouteLength < distance)
                    {
                        distance = tmp.RouteLength;
                        meetingPoint = traveler.Position;
                    }
                }
            }

            // set the meeting point and notify all travelers
            MeetingPoint = meetingPoint;
        }
    }
    
    public void AddtravelerToMeetingPoint(VeddelTraveler traveler)
    {
        lock (Lock)
        {
            _familyAtMeetingPoint.Add(traveler);
            if (_familyAtMeetingPoint.Count == FamilyMemebers.Count)
            {
                foreach (VeddelTraveler familyMemeber in FamilyMemebers)
                {   
                    familyMemeber.OnFamilyGathered();
                }
                _meetingPoint = null;
            }
        }
    }

    public void RemoveFamilyMember(VeddelTraveler traveler)
    {
        lock (FamilyMemebers)
        {
            FamilyMemebers.Remove(traveler);
        }
    }

}