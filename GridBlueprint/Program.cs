using System;
using System.IO;
using GridBlueprint.Model;
using Mars.Components.Starter;
using Mars.Interfaces.Model;

namespace GridBlueprint;

internal static class Program
{
    public static bool train_mode = true;
    private static void Main()
    {   
        var iterations = train_mode ? 500 : 1;
        var configFileName = train_mode ? "train_config.json" : "viz_config.json";
        for (int i = 1; i <= iterations; i++)
        {
            Console.WriteLine("Start iteration " + i + "...");

            // Create a new model description and add model components to it
            var description = new ModelDescription();
            description.AddLayer<GridLayer>();
            description.AddAgent<SimpleAgent, GridLayer>();
            description.AddAgent<ComplexAgent, GridLayer>();
            description.AddAgent<RLAgent, GridLayer>();
            description.AddAgent<HelperAgent, GridLayer>();

            // Load the simulation configuration from a JSON configuration file
            var file = File.ReadAllText(configFileName);
            var config = SimulationConfig.Deserialize(file);
            
            // Couple model description and simulation configuration
            var starter = SimulationStarter.Start(description, config);

            // Run the simulation
            var handle = starter.Run();

            // Close the program
            Console.WriteLine("Successfully executed iterations: " + handle.Iterations);
            starter.Dispose();
        }
    }
}