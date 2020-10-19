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
using SOHBicycleModel.Model;
using SOHBicycleModel.Rental;
using SOHMultimodalModel.Model;
using SOHMultimodalModel.Output.Trips;
using BicycleLayer = SOHMultimodalModel.Model.BicycleLayer;

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
            Thread.CurrentThread.CurrentCulture = new CultureInfo("EN-US");
            LoggerFactory.SetLogLevel(LogLevel.Off);

            var description = new ModelDescription();

            description.AddLayer<BicycleParkingLayer>();
            description.AddLayer<CycleTravelerLayer>();
            description.AddLayer<BicycleLayer>();
            description.AddLayer<WalkLayer>();
            description.AddLayer<CycleTravelerSchedulerLayer>();

            description.AddAgent<CycleTraveler, CycleTravelerLayer>();
            description.AddEntity<Bicycle>();
            
            
            

            ISimulationContainer application;

            if (args != null && args.Any())
            {
                application = SimulationStarter.BuildApplication(description, args);
            }
            else
            {
                var config = CreateDefaultConfig();
                application = SimulationStarter.BuildApplication(description, config);
            }

            var simulation = application.Resolve<ISimulation>();

            var watch = Stopwatch.StartNew();
            var state = simulation.StartSimulation();

            var layers = state.Model.Layers;


            foreach (var layer in layers)
            {
                if (layer.Value is CycleTravelerLayer cycleTravelerLayer)
                {
                    TripsOutputAdapter.PrintTripResult(cycleTravelerLayer.Travelers.Values);
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
                SimulationIdentifier = "green4bikes",
                Globals =
                {
                    StartPoint = startPoint,
                    EndPoint = startPoint + TimeSpan.FromHours(24),
                    DeltaTUnit = TimeSpanUnit.Seconds,
                    ShowConsoleProgress = true,
                    OutputTarget = OutputTargetType.None
                },
                LayerMappings =
                {
                    new LayerMapping
                    {
                        Name = nameof(WalkLayer),
                        File = Path.Combine("resources", "harburg_walk_graph.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(BicycleLayer),
                        File = Path.Combine("resources", "harburg_bike_graph.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(BicycleParkingLayer),
                        File = Path.Combine("resources", "harburg_rental_stations.geojson")
                    },
                    new LayerMapping
                    {
                        Name = nameof(CycleTravelerSchedulerLayer),
                        File = Path.Combine("resources", "cycle_traveler.csv")
                    }
                },
                AgentMappings =
                {
                    new AgentMapping
                    {
                        Name = nameof(CycleTraveler),
                        OutputTarget = OutputTargetType.None,
                        IndividualMapping =
                        {
                            new IndividualMapping {Name = "ResultTrajectoryEnabled", Value = true},
                            new IndividualMapping {Name = "CapabilityCycling", Value = true},
                            new IndividualMapping {Name = "CapabilityDrivingWithProbability", Value = 0.326}
                        }
                    }
                },
                EntityMappings =
                {
                    new EntityMapping
                    {
                        Name = nameof(Bicycle),
                        File = Path.Combine("resources", "bicycle.csv")
                    }
                }
            };
            return config;
        }
    }
}