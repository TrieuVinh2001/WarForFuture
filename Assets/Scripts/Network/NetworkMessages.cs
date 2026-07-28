using UnityEngine;
using WarForFuture.Data;

namespace WarForFuture.Network
{
    // Client -> Server Messages
    public struct MoveInputMsg
    {
        public Vector2 moveDir;
        public bool jumpPressed;
        public uint clientTick;

        public MoveInputMsg(Vector2 moveDir, bool jumpPressed, uint clientTick)
        {
            this.moveDir = moveDir;
            this.jumpPressed = jumpPressed;
            this.clientTick = clientTick;
        }
    }

    public struct AttackInputMsg
    {
        public byte weaponSlot;
        public Vector2 attackTarget;
        public uint clientTick;

        public AttackInputMsg(byte weaponSlot, Vector2 attackTarget, uint clientTick)
        {
            this.weaponSlot = weaponSlot;
            this.attackTarget = attackTarget;
            this.clientTick = clientTick;
        }
    }

    public struct InteractMsg
    {
        public int targetNetId;

        public InteractMsg(int targetNetId)
        {
            this.targetNetId = targetNetId;
        }
    }

    public struct CraftRequestMsg
    {
        public int recipeId;

        public CraftRequestMsg(int recipeId)
        {
            this.recipeId = recipeId;
        }
    }

    public struct BuildRequestMsg
    {
        public int buildingId;
        public Vector2Int gridPos;

        public BuildRequestMsg(int buildingId, Vector2Int gridPos)
        {
            this.buildingId = buildingId;
            this.gridPos = gridPos;
        }
    }

    // Server -> Client Messages
    public struct StateCorrectionMsg
    {
        public Vector2 position;
        public Vector2 velocity;
        public uint ackTick;

        public StateCorrectionMsg(Vector2 position, Vector2 velocity, uint ackTick)
        {
            this.position = position;
            this.velocity = velocity;
            this.ackTick = ackTick;
        }
    }

    public struct HpUpdateMsg
    {
        public int netId;
        public int currentHp;
        public int maxHp;

        public HpUpdateMsg(int netId, int currentHp, int maxHp)
        {
            this.netId = netId;
            this.currentHp = currentHp;
            this.maxHp = maxHp;
        }
    }

    public struct InventoryDeltaMsg
    {
        public ItemType itemType;
        public int deltaAmount;

        public InventoryDeltaMsg(ItemType itemType, int deltaAmount)
        {
            this.itemType = itemType;
            this.deltaAmount = deltaAmount;
        }
    }

    public struct WaveStateMsg
    {
        public int dayNumber;
        public DayPhase phase;
        public int waveIndex;
        public int enemiesRemaining;
        public float timeToNextPhase;

        public WaveStateMsg(int dayNumber, DayPhase phase, int waveIndex, int enemiesRemaining, float timeToNextPhase)
        {
            this.dayNumber = dayNumber;
            this.phase = phase;
            this.waveIndex = waveIndex;
            this.enemiesRemaining = enemiesRemaining;
            this.timeToNextPhase = timeToNextPhase;
        }
    }

    public struct BossStateMsg
    {
        public int bossNetId;
        public int phase;
        public int currentHp;
        public int maxHp;

        public BossStateMsg(int bossNetId, int phase, int currentHp, int maxHp)
        {
            this.bossNetId = bossNetId;
            this.phase = phase;
            this.currentHp = currentHp;
            this.maxHp = maxHp;
        }
    }
}
