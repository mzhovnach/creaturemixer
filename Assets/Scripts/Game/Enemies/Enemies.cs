using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    //TODO
    // клас тримає поточних ворогів на полі
    // вороги в залежності від розмірів займають 1 - 3 слоти
    // якщо немає місця, то новий ворог з черги не з"являється
    public static int                       SLOTS_COUNT = 5;
    public static float                     ENEMIES_Z = -2f;
    public const float                      ATTACK_BEAM_FLY_TIME = 0.25f;
    public List<EnemySlot>                  Slots; // для простоти зараз 5 слотів в ряд
    [HideInInspector]
    public List<EnemyParams>                EnemiesDefaultParams;
    public Transform                        ObjectsContainer;

    private SuperSimplePool                 _pool;
    private List<Enemy>                     _enemies = new List<Enemy>();
    private Dictionary<string, EnemyParams> _enemiesDefaultParams;

    private void Awake()
    {
        _pool = GetComponent<SuperSimplePool>();
        _enemiesDefaultParams = new Dictionary<string, EnemyParams>();
        for (int i = 0; i < EnemiesDefaultParams.Count; ++i)
        {
            _enemiesDefaultParams.Add(EnemiesDefaultParams[i].Name, EnemiesDefaultParams[i]);
        }
    }

    public void ClearEnemiesForce()
    {
        for (int i = _enemies.Count - 1; i >= 0; --i)
        {
            Enemy enemy = _enemies[i];
            FreeSlotsFromEnemy(enemy);
            enemy.HideForce();
        }
        _enemies.Clear();
    }

    public float ClearEnemies()
    {
        float time = 0;
        if (_enemies.Count == 0)
        {
            return time;
        }
        for (int i = _enemies.Count - 1; i >= 0; --i)
        {
            Enemy enemy = _enemies[i];
            FreeSlotsFromEnemy(enemy);
            time = Mathf.Max(enemy.PlayHideAnimation(), time);
        }
        _enemies.Clear();
        return time;
    }

    private void FreeSlotsFromEnemy(Enemy enemy)
    {
        for (int i = 0; i < SLOTS_COUNT; ++i)
        {
            Slots[i].RemoveEnemy(enemy);
        }
    }

    public bool NoEnemiesLeft()
    {
        return _enemies.Count == 0;
    }

    public bool TryToAddEnemy(QueueElement enemyData)
    {
        List<EnemySlot> slotsForEnemy = FindSlotsForEnemy(enemyData);
        if (slotsForEnemy.Count == 0)
        {
            // no free slots
            return false;
        } else
        {
            // add enemy
            GameObject enemyObj = _pool.GetObjectFromPool(enemyData.Name, ObjectsContainer);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            _enemies.Add(enemy);
            // enemy position
            Vector3 centerPos = Vector3.zero;
            for (int i = 0; i < slotsForEnemy.Count; ++i)
            {
                slotsForEnemy[i].SetEnemy(enemy);
                centerPos += slotsForEnemy[i].transform.position;
            }
            centerPos = centerPos / slotsForEnemy.Count;
            centerPos.z = ENEMIES_Z;
            //
            enemyObj.transform.position = centerPos;
            enemy.InitEnemy();
            return true;
        }
    }

    private List<EnemySlot> FindSlotsForEnemy(QueueElement enemyData)
    {
        List<EnemySlot> slots = new List<EnemySlot>();
        if (enemyData.Slot > 0)
        {
            // prioritized slot selected
            int slotReal = enemyData.Slot - 1; // from 0
            if (slotReal + GetEnemySize(enemyData.Name) > SLOTS_COUNT)
            {
                for (int i = 0; i < SLOTS_COUNT; ++i)
                {
                    if (Slots[i].IsEmpty())
                    {
                        // empty slot
                        slots.Add(Slots[i]);
                    } else
                    {
                        // slot occupied
                        slots.Clear();
                        break;
                    }
                }
            } else
            {
                Debug.LogError("ENEMY " + enemyData.Name + " TOO LARGE FOR SLOT " + enemyData.Slot);
            }
        }
        if (slots.Count > 0)
        {
            // preffered slots is ok
            return slots;
        }
        // find random awailable
        List<int> awailableStartSlots = new List<int>();
        int enemySize = GetEnemySize(enemyData.Name);
        for (int i = 0; i <= SLOTS_COUNT - enemySize; ++i)
        {
            bool free = true;
            for (int s = 0; s < enemySize; ++s)
            {
                if (!Slots[i + s].IsEmpty())
                {
                    free = false;
                    break;
                }
            }
            if (free)
            {
                awailableStartSlots.Add(i);
            }
        }
        if (awailableStartSlots.Count > 0)
        {
            // awailable slots
            int rnd = UnityEngine.Random.Range(0, awailableStartSlots.Count);
            int slotStart = awailableStartSlots[rnd];
            for (int i = 0; i < enemySize; ++i)
            {
                slots.Add(Slots[slotStart + i]);
            }
        }
        return slots;
    }

    public int GetEnemySize(string enemyName)
    {
        return _enemiesDefaultParams[enemyName].Size;
    }

    public Color GetEnemyColor(string enemyName)
    {
        return GameManager.Instance.Game.GetPipeColor(_enemiesDefaultParams[enemyName].Color);
    }

    public int GetEnemyColorId(string enemyName)
    {
        return _enemiesDefaultParams[enemyName].Color;
    }


    public float ApplyAttack(Attack attack)
    {
        float delay = 0;
        Enemy enemy = null;
        if (attack.TargetType == EAttackTarget.EnemySlot)
        {
            EnemySlot slot = attack.TargetObject.GetComponent<EnemySlot>();
            enemy = slot.GetEnemy();
        } else
        {
            enemy = attack.TargetObject.GetComponent<Enemy>();
        }

        if (!enemy || enemy.IsDead())
        {
            // no enemy to attack
            return 0;
        }

        delay = enemy.GainDamage(attack.Power, attack.Color);
        if (enemy.IsDead())
        {
            FreeSlotsFromEnemy(enemy);
            _enemies.Remove(enemy);
        }

        return delay;
    }
}