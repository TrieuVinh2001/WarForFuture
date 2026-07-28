using System;
using UnityEngine;
using WarForFuture.Network;

namespace WarForFuture.Gameplay.Enemies
{
    public class BossController : EnemyAI
    {
        public static BossController Instance { get; private set; }

        [Header("Boss Specifics")]
        [SerializeField] private int bossPhase = 1;
        private bool hasEnraged = false;

        public event Action<BossStateMsg> OnBossStateUpdated;

        protected override void Awake()
        {
            maxHp = 300;
            base.Awake();
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (spriteRenderer != null)
            {
                Sprite loadedBoss = UnityEngine.Resources.Load<Sprite>("Art/boss");
                if (loadedBoss != null)
                {
                    spriteRenderer.sprite = loadedBoss;
                    transform.localScale = new Vector3(0.22f, 0.22f, 1f); // Make boss larger
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            NotifyBossState();
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);

            // Phase 2 transition at 50% HP
            if (!hasEnraged && currentHp <= maxHp * 0.5f)
            {
                EnterPhase2();
            }

            NotifyBossState();
        }

        private void EnterPhase2()
        {
            bossPhase = 2;
            hasEnraged = true;
            moveSpeed *= 1.5f;
            attackDamage *= 2;
            attackCooldown *= 0.7f;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.6f, 0.0f, 0.0f); // Dark rage red tint
            }

            Debug.Log("BOSS ENTERED PHASE 2 RAGE MODE!");
        }

        protected override void Die()
        {
            NotifyBossState();
            base.Die();
            Debug.Log("VICTORY! BOSS HAS BEEN DEFEATED!");
        }

        private void NotifyBossState()
        {
            BossStateMsg msg = new BossStateMsg(GetHashCode(), bossPhase, currentHp, maxHp);
            OnBossStateUpdated?.Invoke(msg);
        }

        protected override Sprite CreatePlaceholderEnemySprite()
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.8f, 0.1f, 0.1f);
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
