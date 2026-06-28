using UnityEngine;

namespace sistemata.UI.Deck
{
    [CreateAssetMenu(fileName = "NovaCartaAliado", menuName = "Deck/Carta de Aliado")]
    public class CardData : ScriptableObject
    {
        public string cardName;
        public Sprite cardImage;
        public CardCategory category;
        [TextArea(3, 5)] public string description;

        [Header("Gameplay")]
        public GameObject allyPrefab;
        public float specificCooldown = 10f; // Tempo que leva para recarregar após o aliado morrer
    }

    public enum CardCategory
    {
        Melee,
        Ranged,
        Support,
        item,
    }
}