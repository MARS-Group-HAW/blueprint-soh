using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mars.Common.Logging;
using Mars.Common.Logging.Enums;
using Mars.Components.Starter;
using Mars.Core.Model.Entities;
using Mars.Core.Simulation;
using Mars.Interfaces;
using SOHBicycleModel.Rental;
using SOHCarModel.Model;
using SOHCarModel.Parking;
using SOHMultimodalModel.Layers;
using SOHMultimodalModel.Layers.TrafficLight;
using SOHMultimodalModel.Model;
using SOHMultimodalModel.Output.Trips;
using SOHResources;

namespace SOHGreen4Bikes
{
    /// <summary>
    ///     This pre-defined starter program runs the the Green4Bike scenario with outside passed arguments or
    ///     a default simulation configuration with CSV output and trips.
    /// </summary>
    internal static class Program
    {
        public static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("EN-US");
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

            var config = CreateDefaultConfig();

            ISimulationContainer application;

            if (args != null && args.Any())
            {
                var file = File.ReadAllText(Path.Combine(ResourcesConstants.SimConfigFolder, args[0]));
                var simConfig = SimulationConfig.Deserialize(file);
                Console.WriteLine($"Use simulation config: {args[0]}");
                Console.WriteLine(simConfig.Serialize());
                application = SimulationStarter.BuildApplication(description, simConfig);
            }
            else
            {
                Console.WriteLine("Use default simulation config:");
                Console.WriteLine(config.Serialize());
                application = SimulationStarter.BuildApplication(description, config);
            }

            var simulation = application.Resolve<ISimulation>();

            var watch = Stopwatch.StartNew();
            var state = simulation.StartSimulation();

            var layers = state.Model.AllActiveLayers;


            foreach (var layer in layers)
            {
                if ((layer is CitizenLayer citizenLayer))
                {
                    var citizens = citizenLayer.PedestrianMap.Values;
                    TripsOutputAdapter.PrintTripResult(citizens);
                }
            }

            watch.Stop();

            Console.WriteLine($"Executed iterations {state.Iterations} lasted {watch.Elapsed}");
            application.Dispose();
        }

        private static SimulationConfig CreateDefaultConfig()
        {
            var startPoint = DateTime.Parse("2020-01-01T00:00:00");
            var suffix = DateTime.Now.ToString("yyyyMMddHHmm");
            var config = new SimulationConfig
            {
                SimulationIdentifier = "h1-1",
                Globals =
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint + TimeSpan.FromHours(24),
                    DeltaTUnit = TimeSpanUnit.Seconds,
                    ShowConsoleProgress = true,
                    OutputTarget = OutputTargetType.Csv,
                    CsvOptions =
                    {
                        FileSuffix = "_" + suffix,
                        Delimiter = ";",
                        IncludeHeader = true
                    },
                    SqLiteOptions =
                    {
                        DistinctTable = false
                    },
                    PostgresSqlOptions =
                    {
                        Host = "127.0.0.1",
                        HostUserName = "root",
                        HostPassword = "password",
                        DistinctTable = true,
                        DatabaseName = "green4bikes"
                    }
                },
                LayerMappings =
                {
                    new LayerMapping
                    {
                        Name = nameof(TrafficLightLayer),
                        File = ResourcesConstants.TrafficLightsHarburgZentrum
                    },
                    new LayerMapping
                    {
                        Name = nameof(VectorBuildingsLayer),
                        File = Path.Combine(ResourcesConstants.VectorDataFolder, "Buildings_Harburg_zentrum.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(VectorLanduseLayer),
                        File = Path.Combine(ResourcesConstants.VectorDataFolder, "Landuse_Harburg_zentrum.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(VectorPoiLayer),
                        File = Path.Combine(ResourcesConstants.VectorDataFolder, "POIS_Harburg_zentrum.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CarLayer),
                        File = Path.Combine(ResourcesConstants.GraphFolder, "harburg_zentrum_drive_graph.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CarParkingLayer),
                        File = Path.Combine(ResourcesConstants.VectorDataFolder, "Parking_Harburg_zentrum.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(BicycleParkingLayer),
                        File = Path.Combine(ResourcesConstants.VectorDataFolder,
                            "Bicycle_Rental_Harburg_zentrum.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CitizenLayer),
                        File = Path.Combine(ResourcesConstants.GraphFolder, "harburg_zentrum_walk_graph.geojson"),
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
                        InstanceCount = 10,
                        File = Path.Combine("res", "agent_inits", "CitizenInit10k.csv"),
                        Options = {{"csvSeparator", ';'}},
                        IndividualMapping =
                        {
                            new IndividualMapping {Name = "ResultTrajectoryEnabled", Value = true},
                            new IndividualMapping {Name = "CapabilityCycling", Value = true},
                            new IndividualMapping {Name = "CapabilityDrivingWithProbability", Value = 0.326},
                        }
                    }
                }
            };
            return config;
        }
    }
}