using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPlayersSimple : WeaponBase
{
    public override IEnumerator AttackCoroutine(GameBoard board, SSlot slot, int pipeColor, int attackPower)
    {
        OnStartAttack();
        yield return new WaitForSeconds(CreateAttack(board, slot, pipeColor, attackPower));
    }

    private float CreateAttack(GameBoard board, SSlot slot, int pipeColor, int attackPower) // attack after each match to opposite enemy slot
    {
        // find enemies slot in front of slot
        EnemySlot enemySlot = board.AEnemies.Slots[slot.X];
        Enemy enemy = enemySlot.GetEnemy();
        if (!enemy)
        {
            OnEndAttack();
            return 0;
        }
        // instantiate attack beam
        Vector3 pos = slot.transform.position;
        pos.z = -7;
        GameObject attackObject = board.GetPool().InstantiateObject("simple_attack_" + pipeColor.ToString(), board.AAttacks.ObjectsContainer, pos);
        // fly to slot
        Vector3 finalPos = enemySlot.transform.position;
        finalPos.z = -7;
        float distance = Mathf.Abs(finalPos.y - pos.y);
        float speed = 0.02f; // per unit
        float flyTime = distance * speed;
        LeanTween.move(attackObject, finalPos, flyTime)
            .setEaseOutSine()
            .setOnComplete(() => {
                GameObject.Destroy(attackObject);
                ApplyAttack(board, enemy, pipeColor, attackPower);
            });
        return flyTime;
    }

    private void ApplyAttack(GameBoard board, Enemy enemy, int acolor, int power) // attack after each match to opposite enemy slot
    {
        OnEndAttack();
        if (!enemy)
        {
            // no enemy to attack
            OnEndAttack();
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