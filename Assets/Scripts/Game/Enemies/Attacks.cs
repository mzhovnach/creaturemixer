using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacks : MonoBehaviour
{
	private const float			SIMPLE_ATTACK_SPEED = 0.05f; // per unit
    private List<Attack>        _attacks = new List<Attack>();
    private SuperSimplePool     _pool;
    private Enemies             _enemies;
    private int                 _incompletedAttacksCount = 0;
	public Transform 			ObjectsContainer;

    private void Awake()
    {
        _pool = GetComponent<SuperSimplePool>();
        _enemies = GameManager.Instance.Game.AEnemies;
    }

    public void ClearAllAttacks()
    {
        _incompletedAttacksCount = 0;
        for (int i = _attacks.Count - 1; i >= 0; --i)
        {
            GameObject.Destroy(_attacks[i].gameObject);
        }
        _attacks.Clear();
    }

    private void AddAttack(Attack attack)
    {
        ++_incompletedAttacksCount;
        _attacks.Add(attack);
    }

    public void DecreaseAttacksCount()
    {
        --_incompletedAttacksCount;
        if (_incompletedAttacksCount < 0)
        {
            Debug.LogError("_incompletedAttacksCount = " + _incompletedAttacksCount);
        }
        if (_incompletedAttacksCount < _attacks.Count)
        {
            Debug.LogError("_incompletedAttacksCount < _attacks.Count");
        }
    }

    public bool IsAttacking()
    {
        return _incompletedAttacksCount > 0; //_attacks.Count > 0;
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
        GameObject attackObject = _pool.InstantiateObject("simple_attack_" + pipeColor.ToString(), ObjectsContainer, pos);
        Attack attack = attackObject.AddComponent<Attack>();
        attack.DestroyOnComplete = true;
        attack.Color = pipeColor;
        attack.Power = attackPower;
        attack.TargetType = EAttackTarget.EnemySlot;
        attack.TargetObject = enemySlot.gameObject;
        AddAttack(attack);
        // fly to slot
        Vector3 finalPos = enemySlot.transform.position;
        finalPos.z = -7;
        float distance = Mathf.Abs(finalPos.y - pos.y);
		float flyTime = distance * SIMPLE_ATTACK_SPEED;
        LeanTween.move(attackObject, finalPos, flyTime)
            .setEaseOutSine()
            .setOnComplete(() => {
                ApplyAttack(attack);
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
        AddAttack(attack);

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
                ApplyAttack(attack);
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

    private void ApplyAttack(Attack attack)
    {
        if (_enemies.ApplyAttack(attack) <= 0)
        {
            // no enemy attacked
            RemoveAttackObject(attack);
            DecreaseAttacksCount();
        } else
        {
            // enemy attacked
            RemoveAttackObject(attack);
        }
        
    }

    private void RemoveAttackObject(Attack attack)
    {
        _attacks.Remove(attack);
        if (attack.DestroyOnComplete)
        {
            // destroy attack object
            GameObject.Destroy(attack.gameObject);
        } else
        {
            // just remove Attack component from attack object
            GameObject.Destroy(attack);
        }
    }
}
