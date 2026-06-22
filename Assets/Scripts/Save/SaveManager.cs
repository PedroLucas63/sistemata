using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Sistemata.Save
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance;
        public SaveData data;
        private string _saveFilePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _saveFilePath = Application.persistentDataPath + "/gamesave.sav";

            LoadGame();
        }
        
        public void SaveGame()
        {
            var jsonResult = JsonUtility.ToJson(data, true);
            var resultBytes = Encoding.UTF8.GetBytes(jsonResult);
            jsonResult = Convert.ToBase64String(resultBytes);

            File.WriteAllText(_saveFilePath, jsonResult);
        }

        public void LoadGame()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    var dataLoaded = File.ReadAllText(_saveFilePath);
                    var resultBytes = Convert.FromBase64String(dataLoaded);
                    dataLoaded = Encoding.UTF8.GetString(resultBytes);

                    data = JsonUtility.FromJson<SaveData>(dataLoaded);
                }
                catch (Exception _)
                {
                    NewSave();
                }
            }
            else
            {
                NewSave();
            }
        }

        public void UpdateSave(RoundData roundData)
        {
            data.generalStatistics.timeSurvived += roundData.TimeSurvived;
            data.generalStatistics.roundsPlayed++;
            data.generalStatistics.roundsSurvived += roundData.Completed ? 1 : 0;
            data.generalStatistics.monstersKilled += roundData.MonstersKilled;
            data.generalStatistics.xpCollected += roundData.XpCollected;
            data.generalStatistics.goldCollected += roundData.GoldCollected;

            if (roundData.TimeSurvived > data.recordStatistics.timeSurvived)
                data.recordStatistics.timeSurvived = roundData.TimeSurvived;
            
            if (roundData.MonstersKilled > data.recordStatistics.monstersKilled)
                data.recordStatistics.monstersKilled = roundData.MonstersKilled;
            
            if (roundData.Level > data.recordStatistics.level)
                data.recordStatistics.level = roundData.Level;
            
            if (roundData.DifficultyMultiplier > data.recordStatistics.difficultyMultiplier)
                data.recordStatistics.difficultyMultiplier = roundData.DifficultyMultiplier;

            data.availableGold += roundData.GoldCollected;
            
            SaveGame();
        }
        
        private void NewSave()
        {
            data = new SaveData();
            SaveGame();
        }
    }
}