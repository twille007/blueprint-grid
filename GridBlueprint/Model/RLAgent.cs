using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Appender;
using Mars.Common;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using Mars.Numerics;

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
            _layer.ComplexAgentEnvironment.Insert(this);
            //_episode = episode; //must be given by Train loop to decay epsilon
            InitQTable();
        }

        public new void Tick()
        {
            int action;
            double reward;
            if (_random.Next(1) > _epsilon)
            {
                action = _random.Next(9);
            }
            else
            {
                // TODO: welches a hat max Q value bei Position 
                //action = _Q[(Position, 0)]
                action = 1;
            }

            reward = TakeAction(action);
            UpdateQTable(action, reward);
            DecayEpsilon();
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

        // linear epsilon decay
        // TODO: smoother exponential
        private void DecayEpsilon()
        {
            _epsilon = Math.Max(_epsilon - _decay, _epsilon_min);
        }
        
        // Take choosen action and get reward
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
                    _layer.ComplexAgentEnvironment.MoveTo(this, new Position(newX, newY));
                    // TODO: check for goal and give reward 100
                    Console.WriteLine($"{GetType().Name} moved to a new cell: {Position}");
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
            //TODO: Bellman update
        }
        
        #endregion
        
        #region Fields and Properties

        public Guid ID { get; set; }
    
        public Position Position { get; set; }

        [PropertyDescription(Name = "StartX")]
        public int StartX { get; set; }
    
        [PropertyDescription(Name = "StartY")]
        public int StartY { get; set; }
    
        [PropertyDescription(Name = "MaxTripDistance")]
        public double MaxTripDistance { get; set; }
    
        [PropertyDescription(Name = "AgentExploreRadius")]
        public double AgentExploreRadius { get; set; }
        
        private GridLayer _layer;
        private List<Position> _directions;
        private readonly Random _random = new();
        private AgentState _state;
        private List<Position>.Enumerator _path;
        private Dictionary<(Position state, int action), float> _Q = new Dictionary<(Position state, int action), float>();
        private double _epsilon = 1.0;
        private double _epsilon_min = 0.1;
        private double _decay = 0.01;
        private double _gamma = 0.99;
        private int _episode;

        #endregion
    }
    
}