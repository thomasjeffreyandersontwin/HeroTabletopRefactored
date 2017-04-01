using System.Collections.Generic;
using System.Linq;

namespace HeroVirtualTableTop.AnimatedAbility
{
    public class PauseBasedOnDistanceManagerImpl : PauseBasedOnDistanceManager
    {
        private PauseElement _element;
        private Dictionary<double, double> distanceDelayMappingDictionary;

        public PauseBasedOnDistanceManagerImpl(PauseElement pauseElement)
        {
            PauseElement = pauseElement;
        }

        public PauseElement PauseElement
        {
            get { return _element; }
            set
            {
                _element = value;
                if (_element.DistanceDelayManager != this)
                    _element.DistanceDelayManager = this;
                constructDelayDictionary();
            }
        }

        public double Distance { get; set; }

        public double Duration
        {
            get
            {
                double targetDelay;
                if (distanceDelayMappingDictionary.ContainsKey(Distance))
                {
                    targetDelay = distanceDelayMappingDictionary[Distance];
                }
                else if (Distance <= 10)
                {
                    targetDelay = distanceDelayMappingDictionary[10];
                }
                else if (Distance < 100)
                {
                    var nearestLowerDistance =
                        distanceDelayMappingDictionary.Keys.OrderBy(d => d).Last(d => d < Distance);
                    var nearestHigherDistance =
                        distanceDelayMappingDictionary.Keys.OrderBy(d => d).First(d => d > Distance);
                    targetDelay = getLinearDelayBetweenTwoDelays(nearestLowerDistance,
                        distanceDelayMappingDictionary[nearestLowerDistance], nearestHigherDistance,
                        distanceDelayMappingDictionary[nearestHigherDistance], Distance);
                }
                else
                {
                    var baseDelayDiff = distanceDelayMappingDictionary[50] - distanceDelayMappingDictionary[100];
                    var baseDelay = distanceDelayMappingDictionary[100];
                    var nearestLowerHundredMultiplier = (int) (Distance / 100);
                    var nearestHigherHundredMultiplier = nearestLowerHundredMultiplier + 1;
                    double nearestLowerHundredDistance = nearestLowerHundredMultiplier * 100;
                    double nearestHigherHundredDistance = nearestHigherHundredMultiplier * 100;
                    var currentLowerDelay = baseDelay;
                    var currentHigherDelay = baseDelay - baseDelayDiff * 0.5;
                    for (var i = 1; i < nearestLowerHundredMultiplier; i++)
                    {
                        baseDelayDiff = currentLowerDelay - currentHigherDelay;
                        currentLowerDelay = currentHigherDelay;
                        currentHigherDelay = currentLowerDelay - baseDelayDiff * 0.5;
                    }
                    targetDelay = getLinearDelayBetweenTwoDelays(nearestLowerHundredDistance, currentLowerDelay,
                        nearestHigherHundredDistance, currentHigherDelay, Distance);
                }
                var targetDistance = Distance < 10 ? 10 : Distance;
                return targetDelay * targetDistance;
            }
        }

        private void constructDelayDictionary()
        {
            distanceDelayMappingDictionary = new Dictionary<double, double>
            {
                {10, PauseElement.CloseDistanceDelay},
                {20, PauseElement.ShortDistanceDelay},
                {50, PauseElement.MediumDistanceDelay},
                {100, PauseElement.LongDistanceDelay}
            };
            distanceDelayMappingDictionary.Add(15,
                getPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[10],
                    distanceDelayMappingDictionary[20], 0.70));
            distanceDelayMappingDictionary.Add(30,
                getPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[20],
                    distanceDelayMappingDictionary[50], 0.6));
            distanceDelayMappingDictionary.Add(40,
                getPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[20],
                    distanceDelayMappingDictionary[50], 0.875));
            distanceDelayMappingDictionary.Add(60,
                getPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[50],
                    distanceDelayMappingDictionary[100], 0.4));
            distanceDelayMappingDictionary.Add(70,
                getPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[50],
                    distanceDelayMappingDictionary[100], 0.5));
            distanceDelayMappingDictionary.Add(80,
                getPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[50],
                    distanceDelayMappingDictionary[100], 0.7));
            distanceDelayMappingDictionary.Add(90,
                getPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[50],
                    distanceDelayMappingDictionary[100], 0.87));
        }

        private double getPercentageDelayBetweenTwoDelays(double firstDelay, double secondDelay, double percentage)
        {
            return firstDelay - (firstDelay - secondDelay) * percentage;
        }

        private double getLinearDelayBetweenTwoDelays(double firstDistance, double firstDelay, double secondDistance,
            double secondDelay, double targetDistance)
        {
            // y - y1 = m(x - x1); m = (y2 - y1)/(x2 - x1)
            var m = (secondDelay - firstDelay) / (secondDistance - firstDistance);
            var targetDelay = firstDelay + m * (targetDistance - firstDistance);
            return targetDelay;
        }
    }
}