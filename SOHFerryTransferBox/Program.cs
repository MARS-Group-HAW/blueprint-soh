using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Mars.Common.Logging;
using Mars.Common.Logging.Enums;
using Mars.Components.Starter;
using Mars.Core.Model.Entities;
using Mars.Core.Simulation;
using Mars.Interfaces;
using SOHFerryModel.Model;
using SOHFerryModel.Route;
using SOHFerryModel.Station;
using SOHMultimodalModel.Model;
using SOHResources;

namespace SOHFerryTransferBox
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("EN-US");
            LoggerFactory.SetLogLevel(LogLevel.Off);

            var description = new ModelDescription();
            // description.AddLayer<CarParkingLayer>();
            // description.AddLayer<CarLayer>();
            // description.AddLayer<BicycleParkingLayer>();
            // description.AddLayer<VectorBuildingsLayer>();
            // description.AddLayer<VectorLanduseLayer>();
            // description.AddLayer<VectorPoiLayer>();
            // description.AddLayer<MediatorLayer>();
            description.AddLayer<CitizenLayer>();
            description.AddLayer<FerryLayer>();
            description.AddLayer<FerryStationLayer>();
            description.AddLayer<FerryRouteLayer>();
            // description.AddLayer<TrafficLightLayer>();
            description.AddAgent<Citizen, CitizenLayer>();
            description.AddAgent<FerryDriver, FerryLayer>();


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
                Execution =
                {
                    MaximalLocalProcess = 1
                },
                Globals =
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint + TimeSpan.FromHours(24),
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
                        Name = nameof(FerryStationLayer),
                        OutputTarget = OutputTargetType.None,
                        File = ResourcesConstants.FerryStations
                    },
                    new LayerMapping
                    {
                        Name = nameof(FerryRouteLayer),
                        File = ResourcesConstants.FerryLineCsv
                    },
                    new LayerMapping
                    {
                        Name = nameof(FerryLayer),
                        OutputTarget = OutputTargetType.None,
                        File = ResourcesConstants.FerryGraph
                    },
                    new LayerMapping
                    {
                        Name = nameof(FerrySchedulerLayer),
                        OutputTarget = OutputTargetType.None,
                        File = ResourcesConstants.FerryDriverCsv
                    },
                    new LayerMapping
                    {
                        Name = nameof(CitizenLayer),
                        OutputTarget = OutputTargetType.None,
                        File = ResourcesConstants.FerryContainerWalkingGraph,
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
                            new IndividualMapping {Name = "CapabilityDriving", Value = false},
                            new IndividualMapping {Name = "CapabilityCycling", Value = false}
                        }
                    },
                    new AgentMapping
                    {
                        Name = nameof(FerryDriver),
                        InstanceCount = 5,
                        OutputTarget = OutputTargetType.SqLite
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