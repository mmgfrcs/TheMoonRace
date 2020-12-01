using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class HealthShieldBar : MonoBehaviour
    {
        [SerializeField] private Image healthBar, shieldBar;
        [SerializeField] private TextMeshProUGUI healthText, shieldText;

        private float maxHealth, maxShield;

        private Color originalColor;
        private Tween shieldTween;

        public void SetCurrentHealth(float health)
        {
            if(health > maxHealth) SetMaxHealth(health);
            DOTween.To(() => maxHealth * healthBar.fillAmount, (x) =>
            {
                healthBar.fillAmount = x / maxHealth;
                healthText.text = x.ToString("N0");
            }, health, 0.5f);
        }
        
        public void SetMaxHealth(float health)
        {
            maxHealth = health;
        }
        
        public void SetCurrentShield(float shield)
        {
            if(shield > maxShield) SetMaxShield(shield);
            DOTween.To(() => maxShield * shieldBar.fillAmount, (x) =>
            {
                shieldBar.fillAmount = x / maxShield;
                shieldText.text = x.ToString("N0");
            }, shield, 0.5f);
        }

        public void ActivateShield()
        {
            shieldTween.Kill(true);
            shieldText.color = Color.white;
            shieldBar.color = originalColor;
        }

        public void DeactivateShield()
        {
            shieldText.color = Color.red;
            originalColor = shieldBar.color;
            var endColor = originalColor;
            endColor.a = 0.4f;

            shieldTween = shieldBar.DOColor(endColor, 1f).SetLoops(-1, LoopType.Yoyo);
        }
        
        public void SetMaxShield(float shield)
        {
            maxShield = shield;
        }
    }
}