using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    //TODO
    // клас тримає поточних ворогів на полі
    // вороги в залежності від розмірів займають 1 - 3 слоти
    // якщо немає місця, то новий ворог з черги не з"являється
    private const float MIN_DISTANCE_TO_MOVE_ENEMY = 0.25f;
    public const float ENEMY_SLIDE_TIME = 0.25f;
    public static int SLOTS_COUNT = 7;
    public static float ENEMIES_Z = -2f;
    public const float ATTACK_BEAM_FLY_TIME = 0.25f;
    public List<EnemySlot> Slots; // для простоти зараз 7 слотів в ряд
    [HideInInspector]
    public List<EnemyParams> EnemiesDefaultParams;
    public Transform ObjectsContainer;

    private List<Enemy> _enemies = new List<Enemy>();
    private Dictionary<string, EnemyParams> _enemiesDefaultParams;

    private void Awake()
    {
        SLOTS_COUNT = Slots.Count;
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
        Debug.Log("Left " + _enemies.Count + " enemies on field");
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
            GameObject enemyObj = Pools.GetObjectFromPool(enemyData.Name, ObjectsContainer);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            _enemies.Add(enemy);
            SetEnemyToSlots(slotsForEnemy, enemy, true);
            enemy.InitEnemy();
            Debug.Log("EnemyAdded, total " + _enemies.Count);
            return true;
        }
    }

    private void SetEnemyToSlots(List<EnemySlot> slotsForEnemy, Enemy enemy, bool force)
    {
        Vector3 centerPos = Vector3.zero;
        for (int i = 0; i < slotsForEnemy.Count; ++i)
        {
            slotsForEnemy[i].SetEnemy(enemy);
            centerPos += slotsForEnemy[i].transform.position;
        }
        centerPos = centerPos / slotsForEnemy.Count;
        centerPos.z = ENEMIES_Z;
        if (force)
        {
            enemy.transform.position = centerPos;
        } else
        {
            LeanTween.move(enemy.gameObject, centerPos, ENEMY_SLIDE_TIME);
        }
    }

    private List<EnemySlot> FindSlotsForEnemy(QueueElement enemyData)
    {
        List<EnemySlot> slots = new List<EnemySlot>();
        int enemySize = GetEnemySize(enemyData.Name);
        if (enemyData.Slot > 0)
        {
            // prioritized slot selected
            int slotReal = enemyData.Slot; // from 0
            if (slotReal + enemySize < SLOTS_COUNT)
            {
                for (int i = slotReal; i < slotReal + enemySize; ++i)
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

    public IEnumerator EnemiesAttackCoroutine(GameBoard board)
    {
        // find enemies to attack with
        List<Enemy> attackingEnemies = new List<Enemy>();
        for (int i = 0; i < _enemies.Count; ++i)
        {
            if (_enemies[i].IsDead())
            {
                Debug.LogError("EnemyIsDEAD!");
            }
            _enemies[i].DecreaseMovesToAttack();
            if (_enemies[i].IsReadyToAttack())
            {
                attackingEnemies.Add(_enemies[i]);
            }
        }
        //reorder from left to right, maybe change to attack in order of appearing at field?
        for (int i = 0; i < attackingEnemies.Count - 1; ++i)
        {
            for (int j = i + 1; j < attackingEnemies.Count; ++j)
            {
                if (attackingEnemies[i].transform.position.x > attackingEnemies[j].transform.position.x)
                {
                    Enemy temp = attackingEnemies[i];
                    attackingEnemies[i] = attackingEnemies[j];
                    attackingEnemies[j] = temp;
                }
            }
        }
        //
        for (int i = 0; i < attackingEnemies.Count; ++i)
        {
            Enemy enemy = attackingEnemies[i];
            bool last = i == attackingEnemies.Count - 1;
            enemy.PlayAttackAnimation();
            yield return StartCoroutine(enemy.Weapon.AttackCoroutine(board, enemy, last));
        }
        yield return StartCoroutine(board.AAttacks.WaitEndAttacksCoroutine());
    }

    public bool TryToMoveEnemyBySlide(Enemy enemy, Vector2 startPos, Vector2 endPos, bool mouseUp)
    {
        if (enemy.Weight > GameManager.Instance.Player.PlayersPower)
        {
            return false;
        }
        //TODO move more that 1 enemy (limited to PlayersPower parameter)
        bool wasSlide = false;
        if (Mathf.Abs(endPos.x - startPos.x) >= MIN_DISTANCE_TO_MOVE_ENEMY)
        {
            if (endPos.x > startPos.x)
            {
                // slide to right
                wasSlide = TryToMoveEnemy(enemy, 1);
            } else
            {
                // slide to right
                wasSlide = TryToMoveEnemy(enemy, -1);
            }
        }
        if (mouseUp && !wasSlide)
        {
            //TODO sound that wrong move
        }
        return wasSlide;
    }

    public List<EnemySlot> GetSlotsWithEnemy(Enemy enemy)
    {
        List<EnemySlot> res = new List<EnemySlot>();
        for (int i = 0; i < Slots.Count; ++i)
        {
            if (Slots[i].GetEnemy() == enemy)
            {
                res.Add(Slots[i]);
            }
        }
        return res;
    }

    public List<int> GetSlotsIdsWithEnemy(Enemy enemy)
    {
        List<int> res = new List<int>();
        for (int i = 0; i < Slots.Count; ++i)
        {
            if (Slots[i].GetEnemy() == enemy)
            {
                res.Add(Slots[i].Id);
            }
        }
        return res;
    }

    private bool TryToMoveEnemy(Enemy enemy, int side)
    {
        // TODO можливо слайдити як і пайпи дол упору?
        int enemySize = enemy.Size;
        List<EnemySlot> slotsMoveFrom = GetSlotsWithEnemy(enemy);
        List<EnemySlot> slotsMoveInTo = new List<EnemySlot>();
        int leftSlot = slotsMoveFrom[0].Id;
        int rightSlot = slotsMoveFrom[slotsMoveFrom.Count - 1].Id;
        if (side > 0)
        {
            // to the right
            if (rightSlot + 1 >= Slots.Count)
            {
                // move out of slots
                return false;
            }
            if (!Slots[rightSlot + 1].IsEmpty())
            {
                // slot is occupied
                return false;
            }
            for (int i = leftSlot + 1; i <= rightSlot + 1; ++i)
            {
                slotsMoveInTo.Add(Slots[i]);
            }
        } else
        {
            // to the left
            if (leftSlot - 1 < 0)
            {
                // move out of slots
                return false;
            }
            if (!Slots[leftSlot - 1].IsEmpty())
            {
                // slot is occupied
                return false;
            }
            for (int i = leftSlot - 1; i <= rightSlot - 1; ++i)
            {
                slotsMoveInTo.Add(Slots[i]);
            }
        }
        for (int i = 0; i < slotsMoveFrom.Count; ++i)
        {
            slotsMoveFrom[i].SetEnemy(null);
        }
        SetEnemyToSlots(slotsMoveInTo, enemy, false);
        return true;
    }

    public Enemy GetEnemyByPos(Vector2 pos)
    {
        Vector3 pos3 = new Vector3(pos.x, pos.y, Slots[0].Collider.bounds.center.z);
        for (int i = 0; i < Slots.Count; ++i)
        {
            if (Slots[i].Collider.bounds.Contains(pos3))
            {
                return Slots[i].GetEnemy();
            }
        }
        return null;
    }

    public bool DealDamageToEnemy(Enemy enemy, int acolor, int power)
    {
        if (enemy.DealDamage(power, acolor))
        {
            FreeSlotsFromEnemy(enemy);
            _enemies.Remove(enemy);
            return true;
        }
        return false;
    }

    public bool IsSomeAttack()
    {
        for (int i = 0; i < _enemies.Count; ++i)
        {
            if (_enemies[i].Weapon.IsAttacking())
            {
                return true;
            }
        }
        return false;
    }
}