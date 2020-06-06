using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackData
{
    public enum EAttackState
    {
        Fly = 0,        // анімація польоту до монстра
        Applied = 1,    // атака примінена, грає анімація атаки
    }
    public Attack AAttack = null;
    public EAttackState State = EAttackState.Fly;

    public bool RemoveAttack(Attack attack)
    {
        if (attack != AAttack)
        {
            return false;
        }
        AAttack = null;
        if (attack.DestroyOnComplete)
        {
            // destroy attack object
            Debug.Log("RemoveAttackObject " + attack.name + " with Object");
            GameObject.Destroy(attack.gameObject);
        } else
        {
            // just remove Attack component from attack object
            Debug.Log("RemoveAttackObject " + attack.name + " with Component");
            GameObject.Destroy(attack);
        }
        return true;
    }

    public void Clear()
    {
        RemoveAttack(AAttack);
    }
}

public class Attacks : MonoBehaviour
{
	private const float			SIMPLE_ATTACK_SPEED = 0.05f; // per unit
    private List<AttackData>    _attacksData = new List<AttackData>();
    private Enemies             _enemies;
	public Transform 			ObjectsContainer;

    private void Awake()
    {
        _enemies = GameManager.Instance.Game.AEnemies;
    }

    public void ClearAllAttacks()
    {
        for (int i = _attacksData.Count - 1; i >= 0; --i)
        {
            _attacksData.Clear();
        }
        _attacksData.Clear();
    }

    private AttackData AddAttack(Attack attack)
    {
        AttackData attackData = new AttackData();
        attackData.AAttack = attack;
        attackData.State = AttackData.EAttackState.Fly;
        _attacksData.Add(attackData);
        Debug.Log("AddAttack " + attack.name + " / " + GetAttackingAttacksCount());
        return attackData;
    }

    public void OnAttackApplied(AttackData attackData)
    {
        _attacksData.Remove(attackData);
        Debug.Log("AttackRemoved, left " + GetAttackingAttacksCount());
    }

    public bool IsAttacking()
    {
        for (int i = _attacksData.Count - 1; i >= 0; --i)
        {
            if (_attacksData[i].State == AttackData.EAttackState.Fly)
            {
                return true;
            }
        }
        if (!Consts.MINIMIZE_DELAY_ON_PLAYER_ATTACKS)
        {
            for (int i = _attacksData.Count - 1; i >= 0; --i)
            {
                if (_attacksData[i].State == AttackData.EAttackState.Applied)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public int GetAttackingAttacksCount()
    {
        int res = 0;
        for (int i = _attacksData.Count - 1; i >= 0; --i)
        {
            if (_attacksData[i].State == AttackData.EAttackState.Fly)
            {
                ++res;
            }
        }
        if (!Consts.MINIMIZE_DELAY_ON_PLAYER_ATTACKS)
        {
            for (int i = _attacksData.Count - 1; i >= 0; --i)
            {
                if (_attacksData[i].State == AttackData.EAttackState.Applied)
                {
                    ++res;
                }
            }
        }
        return res;
    }

    public IEnumerator PlayersAttackCoroutine()
    {
        do
        {
            yield return new WaitForSeconds(0.005f);
        } while (IsAttacking());
    }

    public void CreateSimpleAttack(SSlot slot, int pipeColor, int attackPower) // attack after each match to opposite enemy slot
    {
        // find enemies slot in front of slot
        EnemySlot enemySlot = _enemies.Slots[slot.X];
        // instantiate attack beam
		Vector3 pos = slot.transform.position;
		pos.z = -7;
        GameObject attackObject = GameManager.Instance.Game.GetPool().InstantiateObject("simple_attack_" + pipeColor.ToString(), ObjectsContainer, pos);
        Attack attack = attackObject.AddComponent<Attack>();
        attack.DestroyOnComplete = true;
        attack.Color = pipeColor;
        attack.Power = attackPower;
        attack.TargetType = EAttackTarget.EnemySlot;
        attack.TargetObject = enemySlot.gameObject;
        AttackData attackData = AddAttack(attack);
        // fly to slot
        Vector3 finalPos = enemySlot.transform.position;
        finalPos.z = -7;
        float distance = Mathf.Abs(finalPos.y - pos.y);
		float flyTime = distance * SIMPLE_ATTACK_SPEED;
        LeanTween.move(attackObject, finalPos, flyTime)
            .setEaseOutSine()
            .setOnComplete(() => {
                ApplyAttack(attackData);
            });
        // 
    }

    public void CreateFinalAttack(SSlot slot, int pipeColor, int attackPower)
    {
        EnemySlot enemySlot = _enemies.Slots[slot.X]; // find enemies slot in front of slot
        SPipe pipe = slot.TakePipe();

        Attack attack = pipe.gameObject.AddComponent<Attack>();
        attack.DestroyOnComplete = false;
        attack.Color = pipeColor;
        attack.Power = attackPower;
        attack.TargetType = EAttackTarget.EnemySlot;
        attack.TargetObject = enemySlot.gameObject;
        AttackData attackData = AddAttack(attack);

        //Time.timeScale = 0.1f;
        Vector3 fromPos = pipe.transform.position;
        Vector3 toPos = enemySlot.transform.position;

        Vector3[] pathPoints = new Vector3[5];
        // from
        Vector3 p1 = fromPos;
        p1.z = -11;
        pathPoints[1] = p1;
        pipe.transform.position = p1;
        // to
        Vector3 p3 = toPos;
        p3.z = -11;
        pathPoints[3] = p3;
        // first liverage
        Vector3 p0 = p1;
        p0.x += UnityEngine.Random.Range(-4.0f, 4.0f);
        p0.y += UnityEngine.Random.Range(-2.0f, -4.0f);
        pathPoints[0] = p0;
        // second liverage
        Vector3 p4 = p3;
        p4.x += UnityEngine.Random.Range(-4.0f, 4.0f);
        p4.y += UnityEngine.Random.Range(2.0f, 4.0f);
        pathPoints[4] = p4;
        // middle point
        Vector3 p2 = (p1 + p3) / 2.0f;
        p2.x += UnityEngine.Random.Range(-3.0f, 3.0f);
        pathPoints[2] = p2;
        //
        LTSpline spline = new LTSpline(pathPoints);
        //
        //GameObject trailEffect = GameObject.Instantiate(FlyEffectPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //trailEffect.transform.SetParent(pipe.transform, false);
        //trailEffect.transform.localPosition = Vector3.zero;
        ////trailEffect.transform.position = fromPos;
        ////trailEffect.transform.localScale = new Vector3(0, 0, 1);
        ////LeanTween.scale(trailEffect, Vector3.one, Consts.FINAL_ATTACK_TIME / 2.0f)
        ////    .setEase(LeanTweenType.easeInOutSine)
        ////    .setLoopPingPong(1)
        Vector3 startAngle = pipe.transform.eulerAngles;
        LeanTween.value(pipe.gameObject, 0.0f, 1.0f, Consts.FINAL_ATTACK_TIME)
            .setOnUpdate((float norm) =>
            {
                Vector3 pos = spline.point(norm);
                pipe.transform.position = pos;
            })
            .setOnComplete(() =>
            {
                //Destroy(trailEffect, Consts.ADD_POINTS_EFFECT_TIME + 0.2f);
                pipe.transform.localScale = Vector3.one;
                pipe.transform.localScale = Vector3.zero;
                ApplyAttack(attackData);
                pipe.gameObject.SetActive(false);
                //pipe.PlayHideAnimation();
            })
            .setEaseInOutSine();
        LeanTween.rotateX(pipe.gameObject, startAngle.x + 40, (Consts.FINAL_ATTACK_TIME - 0.05f) / 2.0f)
            .setLoopPingPong(1)
            .setEase(LeanTweenType.easeInOutSine);
        LeanTween.rotateY(pipe.gameObject, startAngle.y + 40, (Consts.FINAL_ATTACK_TIME - 0.05f) / 2.0f)
            .setLoopPingPong(1)
            .setEase(LeanTweenType.easeInOutSine);
        float scale = 0.85f;
        LeanTween.scale(pipe.gameObject, new Vector3(scale, scale, scale), Consts.FINAL_ATTACK_TIME - 0.1f);
    }

    private void ApplyAttack(AttackData attackData)
    {
        if (_enemies.ApplyAttack(attackData) <= 0)
        {
            // no enemy attacked
            attackData.Clear();
            _attacksData.Remove(attackData);
        } else
        {
            // enemy attacked
            attackData.Clear();
        }
    }
}
