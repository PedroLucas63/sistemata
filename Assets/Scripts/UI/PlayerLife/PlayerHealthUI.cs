using UnityEngine;
using UnityEngine.UI;
using Sistemata.Common;

namespace Sistemata.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] private EntityHealth playerHealth;
        [SerializeField] private Image[] hearts;

        private void Start()
        {
            playerHealth.OnHealthChanged += UpdateHearts;
            UpdateHearts(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHearts;
            }
        }

        private void UpdateHearts(float currentHealth, float maxHealth)
        {
            if (hearts.Length == 0) return;

            if (maxHealth <= 0) return;

            float healthPerHeart = maxHealth / hearts.Length;

            for (int i = 0; i < hearts.Length; i++)
            {
                float healthForThisHeart = Mathf.Clamp(currentHealth - (i * healthPerHeart), 0, healthPerHeart);

                hearts[i].fillAmount = healthForThisHeart / healthPerHeart;
            }
        }
    }
}