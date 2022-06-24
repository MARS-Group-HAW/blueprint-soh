using Mars.Interfaces.Environments;
using SOHDemonstrationModel.Layer;
using SOHMultimodalModel.Model;

namespace SOHDemonstrationModel.Agents;

public class Demonstrator : Traveler<DemonstrationLayer>
{
    #region Properties and Fields

    /**
     * Start position of the demonstration route
     */
    public Position? Source { get; set; }

    /**
     * Goal position of the demonstration route
     */
    public Position? Target { get; set; }

    private DemonstrationLayer? _demonstrationLayer;

    #endregion

    #region Initialization

    public override void Init(DemonstrationLayer layer)
    {
        base.Init(layer);
        _demonstrationLayer = layer;
        EnvironmentLayer = layer.SpatialGraphMediatorLayer;
        MultimodalRoute = GetDemonstrationRoute();

        //File.WriteAllLines("walk_graph.geojson",EnvironmentLayer.Environment.ToGeoJson().Split());
    }

    #endregion

    #region Tick

    public override void Tick()
    {
        base.Move();
        if (GoalReached) MultimodalLayer.UnregisterAgent(MultimodalLayer, this);
    }

    #endregion

    #region Methods

    /**
     * Constructs a MultimodalRoute that represents the demonstration route
     */
    protected MultimodalRoute GetDemonstrationRoute()
    {
        var stop1 = new Position(9.954585, 53.569523);
        var stop2 = new Position(9.960884, 53.562870);
        var stop3 = new Position(9.956431, 53.560984);
        var stop4 = new Position(9.969564, 53.550965);
        var stop5 = new Position(9.971033, 53.547829);
        var stop6 = new Position(9.969364, 53.546080);
        var stop7 = new Position(9.950247, 53.544827);

        var path = MultimodalLayer.Search(this, Source, stop1, ModalChoice.Walking);

        path.Add(MultimodalLayer.Search(this, stop1, stop2, ModalChoice.Walking).CurrentRoute, ModalChoice.Walking);
        path.Add(MultimodalLayer.Search(this, stop2, stop3, ModalChoice.Walking).CurrentRoute, ModalChoice.Walking);
        path.Add(MultimodalLayer.Search(this, stop3, stop4, ModalChoice.Walking).CurrentRoute, ModalChoice.Walking);
        path.Add(MultimodalLayer.Search(this, stop4, stop5, ModalChoice.Walking).CurrentRoute, ModalChoice.Walking);
        path.Add(MultimodalLayer.Search(this, stop5, stop6, ModalChoice.Walking).CurrentRoute, ModalChoice.Walking);
        path.Add(MultimodalLayer.Search(this, stop6, stop7, ModalChoice.Walking).CurrentRoute, ModalChoice.Walking);
        path.Add(MultimodalLayer.Search(this, stop7, Target, ModalChoice.Walking).CurrentRoute, ModalChoice.Walking);

        return path;
    }

    #endregion
}