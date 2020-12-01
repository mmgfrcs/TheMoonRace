using System;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Player1", menuName = "Player Config", order = 1)]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Player")] public float startingHealth = 400, startingShield = 400;
        public float shieldRegenRate;
        public float shieldRegenDelay;
        [Range(0, 1)] public float shieldActivateThreshold;
        [Header("Game")] public float gameDuration;
        public int startingEnergy, maximumEnergy;
        public GameConfig[] gameProgressions;
    }

    [Serializable]
    public class GameConfig
    {
        [Range(0, 1)] public float progressThreshold;
        public Color barColor;
        public int baseEnergyGainRate = 5, bonusEnergyGainRate = 10;
        public int minimumWordLength = 3, minimumBonusLength;
        public int numberOfLetters = 8;
    }
}