using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mars.Common;
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
using SOHMultimodalModel.Output.Trips;

namespace SOHFerryTransferBox
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Thread.CurrentThread.CurrentCulture = new CultureInfo("EN-US");
            LoggerFactory.SetLogLevel(LogLevel.Info);

            var description = new ModelDescription();
            description.AddLayer<FerryLayer>();
            description.AddLayer<FerrySchedulerLayer>();
            description.AddLayer<FerryStationLayer>();
            description.AddLayer<FerryRouteLayer>();
            description.AddLayer<DockWorkerLayer>();
            description.AddLayer<DockWorkerSchedulerLayer>();

            description.AddAgent<FerryDriver, FerryLayer>();
            description.AddAgent<DockWorker, DockWorkerLayer>();

            description.AddEntity<Ferry>();

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

            var modelAllActiveLayers = state.Model.Layers;
            List<ITripSavingAgent> agents = new List<ITripSavingAgent>();
            foreach (var pair in modelAllActiveLayers)
            {
                var layer = pair.Value;
                    
                if (layer is FerryLayer ferryLayer)
                {
                    var tm =
                        description.SimulationConfig.TypeMappings.FirstOrDefault(m => m.Name == "FerryDriver");
                    if (tm != null)
                    {
                        if (tm.ParameterMapping.TryGetValue("ResultTrajectoryEnabled", out var p) &&
                            p.Value != null && p.Value.Value<bool>())
                        {
                            // agents.AddRange(ferryLayer.Driver.Select(valuePair => valuePair.Value));
                        }
                    }
                }
                
                
                if (layer is DockWorkerLayer dockWorkerLayer)
                {
                    var tm =
                        description.SimulationConfig.TypeMappings.FirstOrDefault(m => m.Name == "DockWorker");
                    if (tm != null)
                    {
                        if (tm.ParameterMapping.TryGetValue("ResultTrajectoryEnabled", out var p) &&
                            p.Value != null && p.Value.Value<bool>())
                        {
                            agents.AddRange(dockWorkerLayer.Agents.Select(valuePair => valuePair.Value));
                        }
                    }
                }

                
            }

            TripsOutputAdapter.PrintTripResult(agents);
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

            var startPoint = DateTime.Parse("2020-01-01T06:00:00");
            var config = new SimulationConfig
            {
                Globals =
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint + TimeSpan.FromHours(2),
                    DeltaTUnit = TimeSpanUnit.Seconds,
                    ShowConsoleProgress = true,
                    OutputTarget = OutputTargetType.None
                },
                LayerMappings =
                {
                    new LayerMapping
                    {
                        Name = nameof(DockWorkerLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine("resources", "hamburg_south_graph_filtered.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(DockWorkerSchedulerLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine("resources", "dock_worker.csv")
                    },
                    new LayerMapping
                    {
                        Name = nameof(FerryStationLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine("resources", "hamburg_ferry_stations.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(FerryRouteLayer),
                        File = Path.Combine("resources", "ferry_line.csv")
                    },
                    new LayerMapping
                    {
                        Name = nameof(FerryLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine("resources", "hamburg_ferry_graph.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(FerrySchedulerLayer),
                        OutputTarget = OutputTargetType.None,
                        File = Path.Combine("resources", "ferry_driver.csv")
                    }
                },
                AgentMappings =
                {
                    new AgentMapping
                    {
                        Name = nameof(DockWorker),
                        OutputTarget = OutputTargetType.None,
                        IndividualMapping =
                        {
                            new IndividualMapping {Name = "ResultTrajectoryEnabled", Value = true}
                        }
                    },
                    new AgentMapping
                    {
                        Name = nameof(FerryDriver),
                        OutputTarget = OutputTargetType.None
                    }
                },
                EntityMappings =
                {
                    new EntityMapping
                    {
                        Name = nameof(Ferry),
                        File = Path.Combine("resources", "ferry.csv")
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