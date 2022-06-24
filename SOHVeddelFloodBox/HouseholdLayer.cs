using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Common;
using Mars.Common.Data;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Data;
using Mars.Interfaces.Layers;
using Mars.Interfaces.Model;
using NetTopologySuite.Geometries;

namespace SOHFloodBox
{
    public class HouseholdLayer : AbstractActiveLayer
    {
        // total number of agends
        private int population = 1415;
        
        // total number of housholds
        private int numHousehold = 770;
        
        // umber of households only have one agent
        private int numSingleHousehold = 458;

        // distance in m a familymember is considered separated form the rest of the family.
        // A meetingpoint the decided then where the family meets again
        private int familyDistanceThreshold = 20;
        
        // Diatance the agent is considere at it's goal
        private int minTargetDistance = 2;

        // Number of houses used for the simulation. These are selected randomly for all houses
        private int numSpawnPoints = 233;
        
        // Number of households that are not evacuating
        private double waitingHouseholds = 0.1;
        
        // NUmber of households that are waiting and then evacuating
        private double delayHouseholds = 0.3;
        
        // Time in seconds the delayed households will at least wait
        private int minDelayTime = 5 * 60;
        
        // Time in seconds the delayed households will wait at max
        private int maxDelayTime = 20 * 60;
        
        // Target Point all agents move towards
        private Geometry evacuationPoint = new Point(10.0213807, 53.5248662);

        private OutputBuilder _outputBuilder = OutputBuilder.Builder();
        
        private void addConfigToOutput()
        {
            _outputBuilder.AddConfig("population", population.ToString());
            _outputBuilder.AddConfig("household", numHousehold.ToString());
            _outputBuilder.AddConfig("singleHousehold", numSingleHousehold.ToString());
            _outputBuilder.AddConfig("familyDistanceThreshold", familyDistanceThreshold.ToString());
            _outputBuilder.AddConfig("minTargetDistance", minTargetDistance.ToString());
            _outputBuilder.AddConfig("numSpawnPoints", numSpawnPoints.ToString());
            _outputBuilder.AddConfig("evacuationPoint", evacuationPoint.ToString());
            _outputBuilder.AddConfig("minDelayTime", minDelayTime.ToString());
            _outputBuilder.AddConfig("maxDelayTime", maxDelayTime.ToString());
            _outputBuilder.AddConfig("WaitingHouseholds", waitingHouseholds.ToString());
            _outputBuilder.AddConfig("delayHouseholds", delayHouseholds.ToString());
        }
        
        public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle = null,
            UnregisterAgent unregisterAgent = null)
        {
            if (!base.InitLayer(layerInitData, registerAgentHandle, unregisterAgent))
                return false;

            addConfigToOutput();
            List<Household> households = new List<Household>();
            List<Household> familyHouseholds = new List<Household>();
            int remainingPeople = population;
            List<Geometry> spawnPoints = parseHouses(layerInitData);
            
            int familyHouseholdsCount = numHousehold - numSingleHousehold;
            var countHouseholdsPerSpawnpoints = numHousehold / spawnPoints.Count;
            
            // every Household gets a member
            foreach (var spawnPoint in spawnPoints)
            {
                for (int i = 0; i <= countHouseholdsPerSpawnpoints; i++)
                {
                    households.Add(new Household(1, spawnPoint));
                    remainingPeople -= 1;
                }
            }
            Console.WriteLine("Households cnt: "+ households.Count);
            Console.WriteLine("remainng ppl: "+ remainingPeople);

            setHouseholdTypes(households);
            
            // every familyHousehold gets a second Person
            for (int i = 0; i <= familyHouseholdsCount; i++)
            {
                households[i].IncrementNumFamilyMembers(1);
                familyHouseholds.Add(households[i]);
                remainingPeople -= 1;
            }
            Console.WriteLine("remainng ppl: "+ remainingPeople);

            //remaining people get shuffled to familys
            while (remainingPeople>=1)
            {
                familyHouseholds[new Random().Next(0,familyHouseholds.Count)].IncrementNumFamilyMembers(1);
                remainingPeople = remainingPeople - 1;
            }
            Console.WriteLine("remainng ppl: "+ remainingPeople);
            
            var agentManager = layerInitData.Container.Resolve<IAgentManager>();
            var data = (IEnumerable<IDomainData>)Enumerable.Range(0, population)
                .Select<int, StructuredData>((Func<int, StructuredData>)(_ => new StructuredData()));

            var travelers = agentManager.Create<VeddelTraveler, WaterLevelLayer>(data, null).ToList();
        
            /*if (spawnPoints.Count != travelers.Count)
            {
                throw new Exception("Invalid number of travelers.");
            }*/

            int j = 0;
            foreach (Household currHousehold in households)
            {
                for (int i = 0; i < currHousehold.numFamilyMembers; i++)
                {
                    travelers[j].StartPosition = currHousehold.adress.RandomPositionFromGeometry();
                    travelers[j].EvacuationPoint = evacuationPoint.RandomPositionFromGeometry();
                    travelers[j].FamilyDistanceThreshold = familyDistanceThreshold;
                    travelers[j].MinTargetDistance = minTargetDistance;
                    travelers[j].OutputBuilder = _outputBuilder;
                    
                    _outputBuilder.AddGenderDistribution(travelers[j].Gender, 1);
                    this.RegisterAgent(null, travelers[j]);
                    currHousehold.AddFamilyMember(travelers[j]);
                    j += 1;
                }
                _outputBuilder.AddFamilyDistribution(currHousehold.numFamilyMembers, 1);
            }
            /*for (int i = 0; i < travelers.Count; i++)
            {
                travelers[i].StartPosition = spawnPoints[i].RandomPositionFromGeometry();
                this.RegisterAgent(null, travelers[i]);
            }*/
            
            return true;
        }
        
        private List<Geometry> parseHouses(LayerInitData layerInitData)
        {
            List<Geometry> geometry = new List<Geometry>();
            IEnumerable<object> households = DomainDataImporter.Import((IEnumerable<object>) layerInitData.LayerInitConfig.Inputs, (InputConfiguration) null);

            foreach (IDomainData domainData in households)
            {
                if (domainData is IGeometryData dataRow)
                {
                    if (dataRow.Geometry == null)
                    {
                        throw new Exception("Invalid geometry in household input file");
                    }
                    geometry.Add(dataRow.Geometry);
                }
            }
            
            Random rnd = new Random();
            List<Geometry> selected = new List<Geometry>();

            for (int i = 0; i < numSpawnPoints; i++)
            {
                int index = rnd.Next(geometry.Count);
                selected.Add(geometry[index]);
                _outputBuilder.AddHouses(geometry[index]);
                geometry.Remove(geometry[index]);
            }

            return selected;
        }

        private void setHouseholdTypes(List<Household> households)
        {
            int numOfDelayHouseholds = (int)(households.Count * delayHouseholds);
            int numOfWaitingHouseholds = (int)(households.Count * waitingHouseholds);
            
            _outputBuilder.AddHouseholdType(HouseholdType.DELAY, numOfDelayHouseholds);
            _outputBuilder.AddHouseholdType(HouseholdType.WAITING, numOfWaitingHouseholds);
            _outputBuilder.AddHouseholdType(HouseholdType.WALKING, households.Count - numOfDelayHouseholds - numOfWaitingHouseholds);
            Random rnd = new Random();

            List<Household> copy = new List<Household>(households);
            
            for (int i = 0; i < Math.Max(numOfDelayHouseholds, numOfWaitingHouseholds); i++)
            {
                if (i < numOfDelayHouseholds)
                {
                    int index = rnd.Next(copy.Count);
                    copy[index].Type = HouseholdType.DELAY;
                    copy[index].DelayTime = rnd.Next(minDelayTime, maxDelayTime);
                    copy.Remove(copy[index]);
                }

                if (i < numOfWaitingHouseholds)
                {
                    int index = rnd.Next(copy.Count);
                    copy[index].Type = HouseholdType.WAITING;
                    copy.Remove(copy[index]);
                }
            }

            foreach (var household in copy)
            {
                household.Type = HouseholdType.WALKING;
            }
        }

        private bool hasSimulationEnded = false;
        public override void PostTick()
        {
            if ((GetCurrentTick() < 0 || GetCurrentTick() == Context.MaxTicks)&& !hasSimulationEnded)
            {
                _outputBuilder.Build();
                hasSimulationEnded = true;
            }
        }
    }
}