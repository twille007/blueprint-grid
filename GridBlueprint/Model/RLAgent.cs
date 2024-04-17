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
        
        #endregion
    }
    
}