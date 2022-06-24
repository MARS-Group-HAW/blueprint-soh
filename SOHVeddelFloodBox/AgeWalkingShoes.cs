using SOHDomain.Graph;
using SOHDomain.Model;
using SOHMultimodalModel.Commons;
using SOHMultimodalModel.Model;

namespace SOHFloodBox
{
    public class AgeWalkingShoes : WalkingShoes
    {
        public AgeWalkingShoes(SpatialGraphMediatorLayer layer, GenderType gender, int age)
            : base(layer, CalcSpeed(gender, age), CalcSpeed(gender, age))
        {
        }
        private static double CalcSpeed(GenderType gender, int age)
        {
            var speed = PedestrianAverageSpeedGenerator.CalculateWalkingSpeed(gender);
            if (age < 50)
            {
                speed = speed * 1;
            }
            else if (age >= 50)
            {
                speed = speed * 0.8;
            }

            return speed;
        }
        
    }
}