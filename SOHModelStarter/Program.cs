﻿using System;
using System.Diagnostics;
using System.IO;
using Mars.Common.Logging;
using Mars.Common.Logging.Enums;
using Mars.Components.Starter;
using Mars.Core.Model.Entities;
using Mars.Core.Simulation;
using SOHBicycleModel.Rental;
using SOHCarModel.Model;
using SOHCarModel.Parking;
using SOHMultimodalModel.Layers;
using SOHMultimodalModel.Layers.TrafficLight;
using SOHMultimodalModel.Model;
using SOHResources;

namespace SOHModelStarter
{
    internal static class Program
    {
        private static void Main()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("EN-US");
            LoggerFactory.SetLogLevel(LogLevel.Off);
            LoggerFactory.ActivateConsoleLogging();

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

            var startPoint = DateTime.Parse("2020-01-01T00:00:00");

            var suffix = DateTime.Now.ToString("yyyyMMddHHmm");
            var config = new SimulationConfig
            {
                // Execution =
                // {
                //     MaximalLocalProcess = 1,
                // },
                SimulationIdentifier = "h1-1",
                Globals =
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint + TimeSpan.FromHours(24),
                    DeltaTUnit = TimeSpanUnit.Seconds,
                    ShowConsoleProgress = true,
                    OutputTarget = OutputTargetType.SqLite,
                    CsvOptions =
                    {
                        FileSuffix = "_" + suffix,
                        Delimiter = ",",
                        IncludeHeader = true
                    },
                    // SgeOption =
                    // {
                    //     File = "bicycle.graph",
                    //     Files = new object[3]
                    //     {
                    //         {
                    //             GraphFile = "bicycle.graphml",
                    //             Modality = "Bicycle",
                    //             PreventCollision = false
                    //         },
                    //         {
                    //             GraphFile = "bicycle.graphml",
                    //             Modality = "",
                    //             PreventCollision = false
                    //         },
                    //         {
                    //             GraphFile = "car.graphml",
                    //             Modality = "Car",
                    //             PreventCollision = true
                    //         }
                    //     }
                    // },
                    SqLiteOptions =
                    {
                        DistinctTable = false
                    },
                    PostgresSqlOptions =
                    {
                        Host = "localhost",
                        HostUserName = "postgres",
                        HostPassword = "simulationProject",
                        DistinctTable = true
                    }
                },
                LayerMappings =
                {
                    new LayerMapping
                    {
                        Name = nameof(TrafficLightLayer),
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
                        File = Path.Combine(ResourcesConstants.VectorDataFolder, "Landuse_Altona_altstadt.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(VectorPoiLayer),
                        File = Path.Combine(ResourcesConstants.VectorDataFolder, "POIS_Altona_altstadt.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CarLayer),
                        File = Path.Combine(ResourcesConstants.GraphFolder, "altona_altstadt_drive_graph.graphml")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CarParkingLayer),
                        File = Path.Combine(ResourcesConstants.VectorDataFolder, "Parking_Altona_altstadt.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(BicycleParkingLayer),
                        File = Path.Combine(ResourcesConstants.VectorDataFolder,
                            "Bicycle_Rental_Altona_altstadt.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CitizenLayer),
                        File = Path.Combine(ResourcesConstants.GraphFolder, "altona_altstadt_walk_graph.graphml"),
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
                        InstanceCount = 10000,
                        File = Path.Combine("res", "agent_inits", "CitizenInit10k.csv"),
                        Options = {{"csvSeparator", ';'}},
                        OutputFilter =
                        {
                            new OutputFilter
                            {
                                Name = "StoreTickResult",
                                Values = new object[] {true},
                                Operator = ContainsOperator.In
                            }
                        },
                        IndividualMapping =
                        {
                            new IndividualMapping {Name = "CanDrive", Value = true},
                            new IndividualMapping {Name = "CanCycle", Value = true}
                            // new IndividualMapping {Name = "CanDriveWithProbability", Value = 0.326},
                        }
                    }
                }
            };

            SimulationConfig simulationConfig;
            var configValue = Environment.GetEnvironmentVariable("CONFIG");
            if (configValue != null)
            {
                Console.WriteLine("Use passed simulation config by environment variable");
                simulationConfig = SimulationConfig.Deserialize(configValue);
            }
            else
            {
                Console.WriteLine("Use pre-defined simulation config");
                simulationConfig = config;
            }

            Console.WriteLine("Used simulation config:");
            Console.WriteLine(simulationConfig.Serialize());

            var application = SimulationStarter.BuildApplication();
            var simulation = application.Resolve<ISimulation>();

            simulation.PrepareInfrastructure(description, simulationConfig);
            var watch = Stopwatch.StartNew();
            var state = simulation.StartSimulation();
            watch.Stop();
            Console.WriteLine($"Executed iterations {state.Iterations} lasted {watch.Elapsed}");
        }
    }
}
