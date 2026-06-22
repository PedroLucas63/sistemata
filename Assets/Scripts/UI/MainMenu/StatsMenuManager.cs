using System;
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
        [SerializeField] private TextMeshProUGUI txtGenXP;
        [SerializeField] private TextMeshProUGUI txtGenCoins;

        private void OnEnable()
        {
            LoadStats();
        }
        
        private void LoadStats()
        {
            // ---------------------------------------------------------
            // DADOS FALSOS (DUMMIES) PARA TESTE DE LAYOUT
            // Futuramente, substitua estas variáveis pelo seu sistema de save
            // Exemplo: int recMonstros = SaveSystem.CarregarDados().maxMonstrosMortos;
            // ---------------------------------------------------------

            // Recordes
            var recTempo = "32m 14s";
            var recMonstros = 450;
            var recNivel = 25;
            var recDificuldade = "Seca Severa";

            // Geral
            var gerTempo = "12h 45m";
            var gerRodadasJogadas = 34;
            var gerRodadasCompletas = 12;
            var gerMonstros = 8450;
            var gerXP = 154000;
            var gerMoedas = 3250;

            // ---------------------------------------------------------
            // APLICANDO OS VALORES NA INTERFACE
            // ---------------------------------------------------------

            // Atualizando a Coluna de Recordes
            txtRecTime.text = $"Tempo Vivido: <color=#FFB300>{recTempo}</color>";
            txtRecMonsters.text = $"Monstros Mortos: <color=#FFB300>{recMonstros}</color>";
            txtRecLevel.text = $"Nível: <color=#FFB300>{recNivel}</color>";
            txtRecDifficulty.text = $"Dificuldade Máxima Concluída: <color=#FFB300>{recDificuldade}</color>";

            // Atualizando a Coluna Geral
            txtGenTime.text = $"Tempo Vivido Geral: <color=#FFB300>{gerTempo}</color>";
            txtGenRounds.text = $"Rodadas (Concluídas): <color=#FFB300>{gerRodadasJogadas}</color> (<color=#FFB300>{gerRodadasCompletas}</color>)";
            txtGenMonsters.text = $"Número de Monstros Mortos: <color=#FFB300>{gerMonstros}</color>";
            txtGenXP.text = $"XP Total Coletado: <color=#FFB300>{gerXP}</color>";
            txtGenCoins.text = $"Moedas Totais Coletadas: <color=#FFB300>{gerMoedas}</color>";
        }
    }
}