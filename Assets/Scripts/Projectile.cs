using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class Projectile : MonoBehaviour
    {
        public float speed = 25;

        internal float damage;
        internal GameObject owner;
        private bool isOwnerEnemy;
        public void SetupProjectile(GameObject owner, float dmg)
        {
            this.owner = owner;
            isOwnerEnemy = owner.CompareTag("Enemy");
            damage = dmg;
        }
        
        private void Start()
        {
            Destroy(gameObject, 4f);
        }

        private void Update()
        {
            transform.Translate(Vector3.up * (speed * Time.deltaTime));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //Don't collide with its owner and itself
            if (other.CompareTag("Projectile") || other.gameObject == owner) return;
            //Enemy bullets don't collide with other enemies
            if (isOwnerEnemy && other.gameObject.CompareTag("Enemy")) return;
            
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null) enemy.Damage(damage);
            else GameManager.ProjectileHit(this);
            Destroy(gameObject);
        }
    }
}