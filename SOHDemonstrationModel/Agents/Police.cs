using Mars.Interfaces.Environments;
using Mars.Numerics;
using SOHDemonstrationModel.Layer;
using SOHMultimodalModel.Model;

namespace SOHDemonstrationModel.Agents;

public class Police : MultiCapableAgent<DemonstrationLayer>
{
    #region Properties and Fields

    /**
     * The position of the roadblock that is guarded by the Police agent
     */
    public Position? Source { get; set; }

    /**
     * Currently not used explicitly
     */
    public Position? Target { get; set; }

    /**
     * Currently not used
     */
    public int SquadSize { get; set; }

    /**
     * Number of arrests the police agent made.
     */
    public int ArrestsCounter { get; set; }

    public int MaxAllowedDistanceToDemonstrators { get; set; } = 150;

    /**
     * CONFIG PARAMETER
     * The distance (in meters) that Police agent can search for RadicalDemonstrator agents that are breaking out
     */
    public double MaxSearchDistance { get; set; } = 400;

    /**
     * The current state of the Police agent (Stationary, Chasing, Returning)
     */
    public PoliceState State { get; set; } = PoliceState.Stationary;

    /**
     * The RadicalDemonstrator agent that is currently being chased by the Police agent
     */
    private RadicalDemonstrator? _myRadicalDemonstrator;

    /**
     * Police agent stops moving if MultimodalRoute is updated during each tick.
     * Therefore, we need to add this stupid counter...
     */
    private int _routeUpdateCounter = 60;

    private DemonstrationLayer? _demonstrationLayer;

    /**
     * Specify the max amount of ticks a police agent spends on chasing a deviant demonstrator.
     */
    public int MaxChasingCounter { get; set; } = 180;

    /**
     * Count the ticks spent on chasing a deviant demonstrator.
     */
    private int _currentChasingCounter;

    #endregion

    #region Initialization

    public override void Init(DemonstrationLayer layer)
    {
        base.Init(layer);

        _demonstrationLayer = layer;
        EnvironmentLayer = _demonstrationLayer.SpatialGraphMediatorLayer;
    }

    #endregion

    #region Tick

    public override void Tick()
    {
        // Police is at station and looking for a radical demonstrator to chase
        if (State is PoliceState.Stationary)
        {
            var nearestRadicalDemonstrator = FindNearestRadDemBreakingOut(MaxSearchDistance);
            if (nearestRadicalDemonstrator is not null) StartChasingRadDem(nearestRadicalDemonstrator);
        }

        // Police is chasing a radical demonstrator
        if (State is PoliceState.Chasing)
        {
            _routeUpdateCounter -= 1;
            if (_currentChasingCounter >= MaxChasingCounter)
            {
                State = PoliceState.Returning;
                _currentChasingCounter = 0;
            }

            if (_routeUpdateCounter == 0 || GoalReached)
            {
                MultimodalRoute = MultimodalLayer.Search(this, Position, _myRadicalDemonstrator?.Position,
                    ModalChoice.Walking);
                _routeUpdateCounter = 60;
            }

            _currentChasingCounter += 1;
            base.Move();
        }

        // Police is chasing a radical demonstrator who is no longer breaking out. So start returning to station
        if (State == PoliceState.Chasing && _myRadicalDemonstrator?.State != RadicalDemonstratorStates.BreakingOut)
        {
            if (DistanceToNearestDem() > MaxAllowedDistanceToDemonstrators)
            {
                if (_myRadicalDemonstrator?.Arrest() ?? false)
                {
                    ArrestsCounter += 1;
                }
            }

            _myRadicalDemonstrator = null;
            State = PoliceState.Returning;
            _currentChasingCounter = 0;
            MultimodalRoute = MultimodalLayer.Search(this, Position, Source, ModalChoice.Walking);
        }

        // Police is returning to station
        if (State is PoliceState.Returning)
        {
            base.Move();
            var nearestRadicalDemonstrator = FindNearestRadDemBreakingOut(MaxSearchDistance);
            if (nearestRadicalDemonstrator is not null) StartChasingRadDem(nearestRadicalDemonstrator);

            if (GoalReached)
            {
                State = PoliceState.Stationary;
                SetWalking();
            }
        }
    }

    #endregion

    #region Methods

    /**
     * Gets the closest radical demonstrators who is breaking out
     */
    private double DistanceToNearestDem()
    {
        if (_myRadicalDemonstrator != null)
        {
            var nearestDemonstrator = _demonstrationLayer?.DemonstratorMap.Values.MinBy(demonstrator =>
                demonstrator.Position != null
                    ? _myRadicalDemonstrator.Position.DistanceInMTo(demonstrator.Position)
                    : double.MaxValue);

            if (nearestDemonstrator != null)
            {
                return _myRadicalDemonstrator.Position.DistanceInMTo(nearestDemonstrator.Position);
            }
        }
        return 0;
    }

    /**
     * Gets the closest radical demonstrators who is breaking out
     */
    private RadicalDemonstrator? FindNearestRadDemBreakingOut(double maxSearchDistance)
    {
        var distanceToNearestRadDem = double.MaxValue;
        RadicalDemonstrator? nearestRadDem = null;
        var radicalDemonstrators = _demonstrationLayer?.RadicalDemonstratorMap.Values;
        
        if (radicalDemonstrators != null)
            foreach (var radicalDemonstrator in radicalDemonstrators)
            {
                if (radicalDemonstrator.State == RadicalDemonstratorStates.BreakingOut)
                {
                    var distanceToRadDem =
                        Distance.Euclidean(Position.PositionArray, radicalDemonstrator.Position.PositionArray);
                    if (Position.DistanceInMTo(radicalDemonstrator.Position) <= maxSearchDistance)
                    {
                        if (distanceToRadDem < distanceToNearestRadDem)
                        {
                            distanceToNearestRadDem = distanceToRadDem;
                            nearestRadDem = radicalDemonstrator;
                        }
                    }
                }
            }

        return nearestRadDem;
    }

    /**
     * Generates a route to a radical demonstrator that is breaking out and starts chasing him/her
     */
    private void StartChasingRadDem(RadicalDemonstrator radicalDemonstrator)
    {
        MultimodalRoute = MultimodalLayer.Search(this, Position, radicalDemonstrator.Position, ModalChoice.Walking);
        _myRadicalDemonstrator = radicalDemonstrator;
        State = PoliceState.Chasing;
        SetRunning();
    }

    #endregion
}