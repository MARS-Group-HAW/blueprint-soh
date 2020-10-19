using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Mars.Common;
using Mars.Common.Collections;
using Mars.Common.IO.Mapped.Collections;
using Mars.Common.Logging;
using Mars.Common.Logging.Enums;
using Mars.Components.Starter;
using Mars.Core.Model.Entities;
using Mars.Core.Simulation;
using Mars.Interfaces;
using SOHBicycleModel.Rental;
using SOHCarModel.Model;
using SOHCarModel.Parking;
using SOHFerryModel.Model;
using SOHMultimodalModel.Layers;
using SOHMultimodalModel.Layers.TrafficLight;
using SOHMultimodalModel.Model;
using SOHMultimodalModel.Output.Trips;
using SOHResources;

namespace SOHModelStarter
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("EN-US");
            LoggerFactory.SetLogLevel(LogLevel.Off);

            var description = new ModelDescription();
            description.AddLayer<CarParkingLayer>();
            description.AddLayer<CarLayer>();
            description.AddLayer<BicycleParkingLayer>();
            description.AddLayer<VectorBuildingsLayer>();
            description.AddLayer<VectorLanduseLayer>();
            description.AddLayer<VectorPoiLayer>();
            description.AddLayer<MediatorLayer>();
            description.AddLayer<CitizenLayer>();
            description.AddLayer<TrafficLightLayer>();
            description.AddAgent<Citizen, CitizenLayer>();


            ISimulationContainer application;
            if (args != null && args.Any())
            {
                application = SimulationStarter.BuildApplication(description, args);
            }
            else
            {
                var config = GetConfig();
                application = SimulationStarter.BuildApplication(description, config);
            }

            var simulation = application.Resolve<ISimulation>();

            var watch = Stopwatch.StartNew();
            var state = simulation.StartSimulation();
            watch.Stop();

            
            var modelAllActiveLayers = state.Model.Layers;
            List<ITripSavingAgent> agents = new List<ITripSavingAgent>();
            foreach (var pair in modelAllActiveLayers)
            {
                var layer = pair.Value;

                if (layer is CitizenLayer citizenLayer)
                {
                    var tm =
                        description.SimulationConfig.TypeMappings.FirstOrDefault(m => m.Name == "Citizen");
                    if (tm != null)
                    {
                        if (tm.ParameterMapping.TryGetValue("ResultTrajectoryEnabled", out var p) &&
                            p.Value != null && p.Value.Value<bool>())
                        {
                            agents.AddRange(citizenLayer.PedestrianMap.Values);
                        }
                    }
                }
            }

            TripsOutputAdapter.PrintTripResult(agents);
            
            
            Console.WriteLine($"Executed iterations {state.Iterations} lasted {watch.Elapsed}");
            
            
            
        }

        private static SimulationConfig GetConfig()
        {
            SimulationConfig simulationConfig;
            var configValue = Environment.GetEnvironmentVariable("CONFIG");

            if (configValue != null)
            {
                Console.WriteLine("Use passed simulation config by environment variable");
                simulationConfig = SimulationConfig.Deserialize(configValue);
                Console.WriteLine(simulationConfig.Serialize());
            }


            var startPoint = DateTime.Parse("2020-01-01T00:00:00");
            var config = new SimulationConfig
            {
                Globals =
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint + TimeSpan.FromHours(12),
                    DeltaTUnit = TimeSpanUnit.Seconds,
                    ShowConsoleProgress = true,
                    OutputTarget = OutputTargetType.SqLite,
                    SqLiteOptions =
                    {
                        DistinctTable = false
                    }
                },
                LayerMappings =
                {
                    new LayerMapping
                    {
                        Name = nameof(TrafficLightLayer),
                        OutputTarget = OutputTargetType.None,
                        File = ResourcesConstants.TrafficLightsAltona
                    },
                    new LayerMapping
                    {
                        Name = nameof(VectorBuildingsLayer),
                        File = Path.Combine(ResourcesConstants.VectorDataFolder,
                            "Buildings_Altona_altstadt.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(VectorLanduseLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine(ResourcesConstants.VectorDataFolder, "Landuse_Altona_altstadt.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(VectorPoiLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine(ResourcesConstants.VectorDataFolder, "POIS_Altona_altstadt.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CarLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine(ResourcesConstants.NetworkFolder, "altona_altstadt_drive_graph.graphml")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CarParkingLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine(ResourcesConstants.VectorDataFolder, "Parking_Altona_altstadt.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(BicycleParkingLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine(ResourcesConstants.VectorDataFolder,
                            "Bicycle_Rental_Altona_altstadt.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CitizenLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine(ResourcesConstants.NetworkFolder, "altona_altstadt_walk_graph.graphml"),
                        IndividualMapping =
                        {
                            new IndividualMapping {Name = "ParkingOccupancy", Value = 0.779}
                        }
                    }
                },
                AgentMappings =
                {
                    new AgentMapping
                    {
                        Name = nameof(Citizen),
                        InstanceCount = 100,
                        OutputTarget = OutputTargetType.SqLite,
                        File = Path.Combine("res", "agent_inits", "CitizenInit10k.csv"),

                        OutputFilter =
                        {
                            new OutputFilter
                            {
                                Name = "StoreTickResult",
                                Values = new object[] {true},
                                Operator = ContainsOperator.In
                            }
                        },
                        Options = {{"csvSeparator", ';'}},
                        IndividualMapping =
                        {
                            new IndividualMapping {Name = "ResultTrajectoryEnabled", Value = true},
                            new IndividualMapping {Name = "CapabilityDriving", Value = true},
                            new IndividualMapping {Name = "CapabilityCycling", Value = true}
                        }
                    }
                }
            };

            Console.WriteLine("Use pre-defined simulation config");
            simulationConfig = config;

            Console.WriteLine("Used simulation config:");
            Console.WriteLine(simulationConfig.Serialize());

            return config;
        }
    }
}