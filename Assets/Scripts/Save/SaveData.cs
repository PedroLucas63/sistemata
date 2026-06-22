using System;
using UnityEngine.Serialization;

namespace Sistemata.Save
{
    [Serializable]
    public class SaveData
    {
        [Serializable]
        public class GeneralStatisticsData
        {
            public float timeSurvived = 0;
            public int roundsPlayed = 0;
            public int roundsSurvived = 0;
            public int monstersKilled = 0;
            public float xpCollected = 0;
            public float goldCollected = 0;
        }
        
        [Serializable]
        public class RecordStatisticsData
        {
            public float timeSurvived = 0;
            public int monstersKilled = 0;
            public int level = 0;
            public float difficultyMultiplier = 0;
        }
        
        public GeneralStatisticsData generalStatistics = new GeneralStatisticsData();
        public RecordStatisticsData recordStatistics = new RecordStatisticsData();
        public float availableGold = 0;
    }
}