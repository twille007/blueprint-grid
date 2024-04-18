using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;

namespace GridBlueprint.Model
{
    public class RLAgent : ComplexAgent, IAgent<GridLayer>, IPositionable
    {
        /// <summary>
        ///     The initialization method of the ComplexAgent which is executed once at the beginning of a simulation.
        ///     It sets an initial Position and an initial State and generates a list of movement directions.
        /// </summary>
        /// <param name="layer">The GridLayer that manages the agents</param>
        public new void Init(GridLayer layer)
        {
            _layer = layer;
            Position = new Position(StartX, StartY);
            _directions = CreateMovementDirectionsList();
            _layer.RLAgentEnvironment.Insert(this);
            _goal = new Position(59, 1);//_random.Next(2) == 1 ? new Position(59, 1) : new Position(59, 49);
            //_episode = episode; //must be given by Train loop to decay epsilon
            if (File.Exists(_saveFile))
            {
                LoadQTable(_saveFile);
            }
            else
            {
                InitQTable();
            }
        }

        public new void Tick()
        {
            int action;
            double reward;
            //epsilon-greedy
            if (_random.NextDouble() < _epsilon)
            {
                action = _random.Next(9);
            }
            else
            {
                //which action has max Q value in state (Position, a) 
                action = GetBestActionForState();
            }

            reward = TakeAction(action);
            //Console.WriteLine($"{Position};{action};{reward}");
            UpdateQTable(action, reward);
            
            if (_layer.GetCurrentTick() == 595)
            {
                DecayEpsilon();
                ExportQTable(_saveFile);
                RemoveFromSimulation();
            }
        }

        #region Methods

        /// <summary>
        ///     Generates a list of eight movement directions that the agent uses for random movement.
        /// </summary>
        /// <returns>The list of movement directions</returns>
        private static List<Position> CreateMovementDirectionsList()
        {
            return new List<Position>
            {
                MovementDirections.North,
                MovementDirections.Northeast,
                MovementDirections.East,
                MovementDirections.Southeast,
                MovementDirections.South,
                MovementDirections.Southwest,
                MovementDirections.West,
                MovementDirections.Northwest
            };
        }

        // Initialize Q Table with zeroes
        private void InitQTable()
        {
            for (int x = 0; x < _layer.Width; x++)
            {
                for (int y = 0; y < _layer.Height; y++)
                {
                    for (int a = 0; a < 9; a++)
                    {
                        _Q.Add((new Position(x, y), a), 0);
                    }
                }
            }
        }

        public void LoadQTable(string file)
        {
            using StreamReader reader = new StreamReader(file);
            // Skip header
            reader.ReadLine();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(';');

                // Parse state coordinates, action, Q-value, and epsilon
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                int action = int.Parse(parts[2]);
                double qValue = double.Parse(parts[3]);
                _epsilon = double.Parse(parts[4]);

                Position state = new Position(x, y);

                _Q.Add((state, action), qValue);
            }
        }

        public void ExportQTable(string file)
        {
            using StreamWriter writer = new StreamWriter(file);
            Console.WriteLine($"Exporting Q Table to file {file}");
            // header
            writer.WriteLine("X;Y;Action;QValue;Epsilon");

            foreach (var entry in _Q)
            {
                Position state = entry.Key.state;
                int action = entry.Key.action;
                double qValue = entry.Value;

                // Write state coordinates, action, Q-value, and epsilon to CSV
                writer.WriteLine($"{state.X};{state.Y};{action};{qValue};{_epsilon}");
            }
        }

        public Dictionary<(Position state, int action), double> GetQ()
        {
            return _Q;
        }

        // linear epsilon decay
        private void DecayEpsilon()
        {
            _epsilon = Math.Max(_epsilon * (1 - _decay), _epsilon_min);
        }

        // Take chosen action and get reward
        private double TakeAction(int action)
        {
            if (action == 0)
            {
                return -1;
            }

            var nextDirection = _directions[action - 1];
            var newX = Position.X + nextDirection.X;
            var newY = Position.Y + nextDirection.Y;

            // Check if chosen move is within the bounds of the grid
            if (0 <= newX && newX < _layer.Width && 0 <= newY && newY < _layer.Height)
            {
                // Check if chosen move goes to a cell that is routable
                if (_layer.IsRoutable(newX, newY))
                {
                    Position = new Position(newX, newY);
                    _layer.RLAgentEnvironment.MoveTo(this, new Position(newX, newY));
                    //TODO maybe positive reward for crossing x=50?
                    //check for goal and give reward 100
                    if (Position.Equals(_goal))
                    {
                        return 100;
                    }
                    //else reward is -0.1
                    //Console.WriteLine($"{GetType().Name} moved to a new cell: {Position}");
                    return -0.1;
                }

                Console.WriteLine($"{GetType().Name} tried to move to a blocked cell: ({newX}, {newY})");
                return -1;
            }

            Console.WriteLine($"{GetType().Name} tried to leave the world: ({newX}, {newY})");
            return -1;
        }

        // Bellman Update for QTable at current Position + action and received reward
        private void UpdateQTable(int action, double reward)
        {
            double currentValue = _Q.TryGetValue((Position, action), out double val) ? val : 0.0f;

            // Calculate the new Q-value using the Bellman equation
            double newQValue = currentValue +
                            _alpha * (reward + _gamma * _Q[(Position, GetBestActionForState())] - currentValue);
            
            //Console.WriteLine($"Before: {currentValue}, After: {newQValue}");
            // Update the Q-value in the dictionary
            _Q[(Position, action)] = newQValue;
        }

        private int GetBestActionForState()
        {
            return _Q.Where(entry => entry.Key.state.Equals(Position)).OrderByDescending(entry => entry.Value)
                .FirstOrDefault().Key.action;
        }
        
        /// <summary>
        ///     Removes this agent from the simulation and, by extension, from the visualization.
        /// </summary>
        private void RemoveFromSimulation()
        {
            Console.WriteLine($"ComplexAgent {ID} is removing itself from the simulation.");
            _layer.ComplexAgentEnvironment.Remove(this);
            UnregisterAgentHandle.Invoke(_layer, this);
        }
        
        #endregion

        #region Fields and Properties

        public Guid ID { get; set; }

        public Position Position { get; set; }

        [PropertyDescription(Name = "StartX")] public int StartX { get; set; }

        [PropertyDescription(Name = "StartY")] public int StartY { get; set; }
        
        private GridLayer _layer;
        private List<Position> _directions;
        private readonly Random _random = new();
        private AgentState _state;
        private Position _goal;

        private Dictionary<(Position state, int action), double> _Q = new();

        private string _saveFile = @"../../../Resources/q.csv";

        private double _epsilon = 1.0;
        private double _epsilon_min = 0.1;
        private double _decay = 0.01;
        private double _gamma = 0.99;
        private double _alpha = 0.01;

        #endregion
    }
}