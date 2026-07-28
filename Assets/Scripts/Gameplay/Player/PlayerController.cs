using System;
using UnityEngine;
using UnityEngine.InputSystem;
using WarForFuture.Data;
using WarForFuture.Gameplay.Combat;
using WarForFuture.Network;

namespace WarForFuture.Gameplay.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int maxMana = 100;
        [SerializeField] private int maxFood = 100;

        [Header("Map Movement Boundaries")]
        [SerializeField] private Vector2 minMapBounds = new Vector2(-20f, -20f);
        [SerializeField] private Vector2 maxMapBounds = new Vector2(20f, 20f);

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private GameObject weaponHolderObj;
        private SpriteRenderer weaponSpriteRenderer;

        private Vector2 moveInput;
        private Vector2 lastLookDir = Vector2.right;

        private int currentHp;
        private int currentMana;
        private int currentFood;

        private byte currentWeaponSlot = 0; // 0 = Tool/Melee, 1 = Sword, 2 = Bow
        private uint tickCounter = 0;
        private Vector3 respawnPoint;
        private float lastHungerTickTime;
        private float lastManaRegenTime;

        public byte CurrentWeaponSlot => currentWeaponSlot;
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public int EffectiveMaxHp => maxHp + (PlayerEquipmentManager.Instance != null ? PlayerEquipmentManager.Instance.GetTotalHpBonus() : 0);
        public int CurrentMana => currentMana;
        public int MaxMana => maxMana;
        public int CurrentFood => currentFood;
        public int MaxFood => maxFood;

        public event Action<int, int> OnHpChanged;
        public event Action<int, int> OnManaChanged;
        public event Action<int, int> OnFoodChanged;
        public event Action OnPlayerDied;
        public event Action<byte> OnWeaponSlotChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Sprite loadedSprite = UnityEngine.Resources.Load<Sprite>("Art/player");
                if (loadedSprite != null)
                {
                    spriteRenderer.sprite = loadedSprite;
                    transform.localScale = new Vector3(0.08f, 0.08f, 1f);
                }
                else if (spriteRenderer.sprite == null)
                {
                    spriteRenderer.sprite = CreatePlaceholderPlayerSprite();
                }
            }

            SetupWeaponVisualHolder();

            currentHp = maxHp;
            currentMana = maxMana;
            currentFood = maxFood;
            respawnPoint = transform.position;
        }

        private void SetupWeaponVisualHolder()
        {
            if (weaponHolderObj == null)
            {
                weaponHolderObj = new GameObject("WeaponHolder");
                weaponHolderObj.transform.SetParent(transform);
                weaponHolderObj.transform.localPosition = new Vector3(4f, 0f, -0.1f);
                weaponSpriteRenderer = weaponHolderObj.AddComponent<SpriteRenderer>();
                weaponSpriteRenderer.sortingOrder = 10;
            }
            UpdateWeaponVisual();
        }

        private void UpdateWeaponVisual()
        {
            if (weaponSpriteRenderer == null) return;

            if (currentWeaponSlot == 1)
            {
                Sprite sword = UnityEngine.Resources.Load<Sprite>("Art/sword");
                weaponSpriteRenderer.sprite = sword;
                weaponHolderObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                weaponSpriteRenderer.flipX = false;
            }
            else if (currentWeaponSlot == 2)
            {
                Sprite bow = UnityEngine.Resources.Load<Sprite>("Art/bow");
                weaponSpriteRenderer.sprite = bow;
                weaponHolderObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                weaponSpriteRenderer.flipX = true;
            }
            else
            {
                weaponSpriteRenderer.sprite = null;
            }

            OnWeaponSlotChanged?.Invoke(currentWeaponSlot);
        }

        private void Start()
        {
            if (PlayerEquipmentManager.Instance != null)
            {
                PlayerEquipmentManager.Instance.OnEquipmentChanged += RecalculateStats;
            }
            RecalculateStats();
        }

        public void RecalculateStats()
        {
            int bonusHp = PlayerEquipmentManager.Instance != null ? PlayerEquipmentManager.Instance.GetTotalHpBonus() : 0;
            int effectiveMaxHp = maxHp + bonusHp;

            currentHp = Mathf.Min(effectiveMaxHp, currentHp + bonusHp);
            OnHpChanged?.Invoke(currentHp, effectiveMaxHp);
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnFoodChanged?.Invoke(currentFood, maxFood);
        }

        private void Update()
        {
            float moveX = 0f;
            float moveY = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;

                byte oldSlot = currentWeaponSlot;
                if (Keyboard.current.digit1Key.wasPressedThisFrame) currentWeaponSlot = 0;
                if (Keyboard.current.digit2Key.wasPressedThisFrame) currentWeaponSlot = 1;
                if (Keyboard.current.digit3Key.wasPressedThisFrame) currentWeaponSlot = 2;

                if (oldSlot != currentWeaponSlot)
                {
                    UpdateWeaponVisual();
                }
            }

            moveInput = new Vector2(moveX, moveY).normalized;

            Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
            Vector3 mouseWorldPos = Camera.main != null ? Camera.main.ScreenToWorldPoint(mouseScreenPos) : transform.position + Vector3.right;
            mouseWorldPos.z = 0f;

            Vector2 aimDir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
            if (aimDir.sqrMagnitude > 0.01f)
            {
                lastLookDir = aimDir;
            }

            if (weaponHolderObj != null)
            {
                float angle = Mathf.Atan2(lastLookDir.y, lastLookDir.x) * Mathf.Rad2Deg;
                weaponHolderObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                weaponHolderObj.transform.localPosition = (Vector3)lastLookDir.normalized * 4f;
            }

            bool attackTriggered = false;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) attackTriggered = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && GUIUtility.hotControl == 0) attackTriggered = true;

            if (attackTriggered)
            {
                ExecuteAttack();
            }

            if (Time.time - lastManaRegenTime >= 1.0f)
            {
                lastManaRegenTime = Time.time;
                if (currentMana < maxMana)
                {
                    currentMana = Mathf.Min(maxMana, currentMana + 3);
                    OnManaChanged?.Invoke(currentMana, maxMana);
                }
            }

            if (Time.time - lastHungerTickTime >= 3.0f)
            {
                lastHungerTickTime = Time.time;
                if (currentFood > 0)
                {
                    currentFood -= 1;
                    OnFoodChanged?.Invoke(currentFood, maxFood);
                }
                else
                {
                    TakeDamage(2);
                }
            }

            tickCounter++;
        }

        private void FixedUpdate()
        {
            float speedMultiplier = 1.0f + (PlayerEquipmentManager.Instance != null ? PlayerEquipmentManager.Instance.GetTotalSpeedBonus() * 0.1f : 0f);
            rb.linearVelocity = moveInput * (moveSpeed * speedMultiplier);

            Vector2 clampedPos = rb.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, minMapBounds.x, maxMapBounds.x);
            clampedPos.y = Mathf.Clamp(clampedPos.y, minMapBounds.y, maxMapBounds.y);
            rb.position = clampedPos;
        }

        private void ExecuteAttack()
        {
            AttackInputMsg attackMsg = new AttackInputMsg(currentWeaponSlot, lastLookDir, tickCounter);

            if (currentWeaponSlot == 2)
            {
                if (currentMana >= 5)
                {
                    currentMana -= 5;
                    OnManaChanged?.Invoke(currentMana, maxMana);
                    CombatSystem.Instance?.PerformRangedAttack(transform.position, lastLookDir);
                }
            }
            else
            {
                CombatSystem.Instance?.PerformMeleeAttack(transform.position, lastLookDir);
            }
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || currentHp <= 0) return;

            int defense = PlayerEquipmentManager.Instance != null ? PlayerEquipmentManager.Instance.GetTotalDefenseBonus() : 0;
            int actualDamage = Mathf.Max(1, damage - defense);

            currentHp -= actualDamage;
            Debug.Log($"Player took {actualDamage} damage! Current HP: {currentHp}/{maxHp}");

            int bonusHp = PlayerEquipmentManager.Instance != null ? PlayerEquipmentManager.Instance.GetTotalHpBonus() : 0;
            OnHpChanged?.Invoke(currentHp, maxHp + bonusHp);

            FloatingTextManager.Instance?.SpawnDamageText(transform.position, actualDamage, true);

            if (currentHp <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || currentHp <= 0) return;
            int bonusHp = PlayerEquipmentManager.Instance != null ? PlayerEquipmentManager.Instance.GetTotalHpBonus() : 0;
            int effectiveMaxHp = maxHp + bonusHp;

            currentHp = Mathf.Min(effectiveMaxHp, currentHp + amount);
            Debug.Log($"Player healed {amount} HP! Current HP: {currentHp}/{effectiveMaxHp}");
            OnHpChanged?.Invoke(currentHp, effectiveMaxHp);

            FloatingTextManager.Instance?.SpawnHealText(transform.position, amount);
        }

        public void EatFood(int foodAmount, int healAmount)
        {
            currentFood = Mathf.Min(maxFood, currentFood + foodAmount);
            OnFoodChanged?.Invoke(currentFood, maxFood);
            Heal(healAmount);
        }

        private void Die()
        {
            Debug.Log("Player died! Respawning...");
            OnPlayerDied?.Invoke();
            Respawn();
        }

        public void Respawn()
        {
            transform.position = respawnPoint;
            RecalculateStats();
        }

        private Sprite CreatePlaceholderPlayerSprite()
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.cyan;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
