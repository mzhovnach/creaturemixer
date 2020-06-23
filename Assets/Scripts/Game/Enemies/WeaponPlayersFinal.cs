using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPlayersFinal : WeaponBase
{
    public override IEnumerator AttackCoroutine(GameBoard board, SSlot slot, int pipeColor, int attackPower)
    {
        OnStartAttack();
        yield return new WaitForSeconds(CreateAttack(board, slot, pipeColor, attackPower));
    }

    private float CreateAttack(GameBoard board, SSlot slot, int pipeColor, int attackPower)
    {
        EnemySlot enemySlot = board.AEnemies.Slots[slot.X]; // find enemies slot in front of slot
        Enemy enemy = enemySlot.GetEnemy();
        if (!enemy)
        {
            board.BreakePipeInSlot(slot, (slot.Pipe as Pipe_Colored).GetExplodeEffectPrefab());
            OnEndAttack();
            return 0;
        }
        SPipe pipe = slot.TakePipe();

        //Time.timeScale = 0.1f;
        Vector3 fromPos = pipe.transform.position;
        Vector3 toPos = enemySlot.transform.position;

        float distance = Mathf.Abs(fromPos.y - toPos.y);
        float speed = 0.05f; // per unit
        float flyTime = distance * speed;

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
        LeanTween.value(pipe.gameObject, 0.0f, 1.0f, flyTime)
            .setOnUpdate((float norm) =>
            {
                Vector3 pos = spline.point(norm);
                pipe.transform.position = pos;
            })
            .setOnComplete(() =>
            {
                ApplyAttack(board, enemy, pipeColor, attackPower);
                //Destroy(trailEffect, Consts.ADD_POINTS_EFFECT_TIME + 0.2f);
                pipe.transform.localScale = Vector3.one;
                pipe.transform.localScale = Vector3.zero;
                pipe.gameObject.SetActive(false);
                //pipe.PlayHideAnimation();
            })
            .setEaseInOutSine();
        LeanTween.rotateX(pipe.gameObject, startAngle.x + 40, (flyTime - 0.05f) / 2.0f)
            .setLoopPingPong(1)
            .setEase(LeanTweenType.easeInOutSine);
        LeanTween.rotateY(pipe.gameObject, startAngle.y + 40, (flyTime - 0.05f) / 2.0f)
            .setLoopPingPong(1)
            .setEase(LeanTweenType.easeInOutSine);
        float scale = 0.85f;
        LeanTween.scale(pipe.gameObject, new Vector3(scale, scale, scale), flyTime - 0.1f);
        return flyTime;
    }

    private void ApplyAttack(GameBoard board, Enemy enemy, int acolor, int power) // attack after each match to opposite enemy slot
    {
        OnEndAttack();
        if (!enemy)
        {
            // no enemy to attack
            return;
        } else
        if (enemy.IsDead())
        {
            Debug.LogError("EnemyIsDead!");
            return;
        }
        board.AEnemies.DealDamageToEnemy(enemy, acolor, power);
    }
}