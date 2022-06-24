using Mars.Common;
using Mars.Components.Layers;
using SOHDemonstrationModel.Agents;

namespace SOHDemonstrationModel.Layer;

public class DemonstratorSchedulerLayer : SchedulerLayer
{
    public DemonstrationLayer DemonstrationLayer { get; set; }

    public DemonstratorSchedulerLayer(DemonstrationLayer demonstrationLayer)
    {
        DemonstrationLayer = demonstrationLayer;
    }

    protected override void Schedule(SchedulerEntry dataRow)
    {
        var source = dataRow.SourceGeometry.RandomPositionFromGeometry();
        var target = dataRow.TargetGeometry.RandomPositionFromGeometry();
        var isRadical = Convert.ToBoolean(dataRow.Data["isRadical"]);

        if (isRadical)
        {
            var demonstrator = new RadicalDemonstrator { Source = source, Target = target };
            demonstrator.Init(DemonstrationLayer);
            DemonstrationLayer.RadicalDemonstratorMap[demonstrator.ID] = demonstrator;
            RegisterAgent(DemonstrationLayer, demonstrator);
        }
        else
        {
            var demonstrator = new Demonstrator { Source = source, Target = target };
            demonstrator.Init(DemonstrationLayer);
            DemonstrationLayer.DemonstratorMap[demonstrator.ID] = demonstrator;
            RegisterAgent(DemonstrationLayer, demonstrator);
        }
    }
}