using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float startingHealth = 1, fireRate = 1, damage = 10, scoreReward;
        [SerializeField] private GameObject bullet, explosionPrefab;
        [SerializeField] private Transform turretBase, ship;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AnimationCurve speedCurve;
        [SerializeField] private Slider healthBar;
        
        public float CurrentHealth { get; private set; }
        public float MaximumHealth { get; private set; }
        public float ScoreReward
        {
            get => scoreReward;
        }

        private float startTime;

        private void Start()
        {
            MaximumHealth = startingHealth;
            CurrentHealth = startingHealth;
            startTime = Time.time;
            StartCoroutine(ShootPlayer());
            ship = GameObject.Find("Ship").GetComponent<Transform>();
            healthBar.gameObject.SetActive(false);
            healthBar.maxValue = startingHealth;
            healthBar.value = startingHealth;
        }

        private void OnMouseOver()
        {
            if(Input.GetMouseButtonDown(0))
                GameManager.EnemyClick(this, 0);
            else if(Input.GetMouseButtonDown(1))
                GameManager.EnemyClick(this, 1);
        }

        private void Update()
        {
            turretBase.rotation = Quaternion.LookRotation(turretBase.forward,
                ship.position - turretBase.position);
            transform.Translate(Vector3.up * (Time.deltaTime * speedCurve.Evaluate(Time.time - startTime)));
        }

        private IEnumerator ShootPlayer()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f / fireRate);
                //Find Player
                if (ship.gameObject.activeSelf)
                {
                    muzzleFlash.Play();
                    Instantiate(bullet, turretBase.position, turretBase.rotation)
                        .GetComponent<Projectile>().SetupProjectile(gameObject, damage);
                }
                else break;
            }

            transform.Translate(Vector3.up * (Time.deltaTime * 2f));
            Destroy(gameObject, 3f);
        }

        public void Damage(float amt)
        {
            if(!healthBar.gameObject.activeSelf) healthBar.gameObject.SetActive(true);
            CurrentHealth -= amt;
            DOTween.To(() => healthBar.value, (val) => healthBar.value = val, CurrentHealth, 0.5f);
            
            if (CurrentHealth <= 0)
            {
                //Die
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                GameManager.AddScore(scoreReward);
                Destroy(gameObject);
            }
        }
    }
}