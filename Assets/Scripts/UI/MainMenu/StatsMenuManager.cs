using System;
using Sistemata.Save;
using TMPro;
using UnityEngine;

namespace Sistemata.UI.MainMenu
{
    public class StatsMenuManager : MonoBehaviour
    {
        [Header("Textos de Recordes")] 
        [SerializeField] private TextMeshProUGUI txtRecTime;
        [SerializeField] private TextMeshProUGUI txtRecMonsters;
        [SerializeField] private TextMeshProUGUI txtRecLevel;
        [SerializeField] private TextMeshProUGUI txtRecDifficulty;

        [Header("Textos Gerais")] [SerializeField] private TextMeshProUGUI txtGenTime;
        [SerializeField] private TextMeshProUGUI txtGenRounds;
        [SerializeField] private TextMeshProUGUI txtGenMonsters;
        [SerializeField] private TextMeshProUGUI txtGenXp;
        [SerializeField] private TextMeshProUGUI txtGenCoins;

        private void OnEnable()
        {
            LoadStats();
        }
        
        private void LoadStats()
        {
            var data = SaveManager.Instance.data;
            
            txtRecTime.text = $"Tempo Vivido: <color=#FFB300>{data.recordStatistics.timeSurvived}</color>";
            txtRecMonsters.text = $"Monstros Mortos: <color=#FFB300>{data.recordStatistics.monstersKilled}</color>";
            txtRecLevel.text = $"Nível: <color=#FFB300>{data.recordStatistics.level}</color>";
            txtRecDifficulty.text = $"Dificuldade Máxima Concluída: <color=#FFB300>{data.recordStatistics.difficultyMultiplier}</color>";

            txtGenTime.text = $"Tempo Vivido Geral: <color=#FFB300>{data.generalStatistics.timeSurvived}</color>";
            txtGenRounds.text = $"Rodadas (Concluídas): <color=#FFB300>{data.generalStatistics.roundsPlayed}</color> (<color=#FFB300>{data.generalStatistics.roundsSurvived}</color>)";
            txtGenMonsters.text = $"Número de Monstros Mortos: <color=#FFB300>{data.generalStatistics.monstersKilled}</color>";
            txtGenXp.text = $"XP Total Coletado: <color=#FFB300>{data.generalStatistics.xpCollected}</color>";
            txtGenCoins.text = $"Moedas Totais Coletadas: <color=#FFB300>{data.generalStatistics.goldCollected}</color>";
        }
    }
}