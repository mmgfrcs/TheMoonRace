using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Database/Skill", order = 0)]
    public class Skill : ScriptableObject
    {
        public string skillName;
        [TextArea] public string description;
        public Sprite sprite;
        public int energyCost;
        public GameObject projectile;
        public int projectileAmount = 1;
        public float projectileFireRate;
        
        public float damage, heal;
        
    }
}