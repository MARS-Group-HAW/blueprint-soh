using Mars.Common;
using Mars.Common.Core;
using Mars.Components.Layers;
using SOHDemonstrationModel.Agents;

namespace SOHDemonstrationModel.Layer;

public class PoliceSchedulerLayer: SchedulerLayer
{
    public DemonstrationLayer DemonstrationLayer { get; set; }
    
    public PoliceSchedulerLayer(DemonstrationLayer demonstrationLayer)
    {
        DemonstrationLayer = demonstrationLayer;
    }
    
    protected override void Schedule(SchedulerEntry dataRow)
    {
        var source = dataRow.SourceGeometry.RandomPositionFromGeometry();
        var target = dataRow.TargetGeometry.RandomPositionFromGeometry();
        var squadSize = Convert.ToInt32(dataRow.Data["squadSize"]);

        var police = new Police
        {
            Source = source,
            Target = target,
            SquadSize = squadSize
        };
        police.Init(DemonstrationLayer);
        DemonstrationLayer.PoliceMap[police.ID] = police;

        RegisterAgent(DemonstrationLayer, police);
    }
}