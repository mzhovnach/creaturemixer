using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSimple : WeaponBaseEnemy
{
    public override IEnumerator AttackCoroutine(GameBoard board, Enemy enemy, bool last)
    {
        ResetWeapon();
        OnStartAttack();
        yield return new WaitForSeconds(CreateAttack(board, enemy));
        OnEndAttack();
    }

    private float CreateAttack(GameBoard board, Enemy enemy)
    {
        List<List<SSlot>> slots = GetSlotsWithCharactersInFront(board, enemy, true);
        float maxTime = 0;
        for (int i = 0; i < slots.Count; ++i)
        {
            Vector3 finalPos = Vector3.zero;
            SSlot firstSlot = slots[i][0];
            bool attackCharacter = false;
            Pipe_Character character = null;
            if (!firstSlot)
            {
                // attack lives panel
                firstSlot = slots[i][1];
                finalPos = firstSlot.transform.position;
                finalPos.y -= 1.5f;
            } else
            {
                // attack character
                attackCharacter = true;
                finalPos = firstSlot.transform.position;
                character = firstSlot.Pipe.GetComponent<Pipe_Character>();
            }
            int slotX = firstSlot.X;
            Vector3 startPos = board.AEnemies.Slots[slotX].transform.position;
            // instantiate attack beam
            finalPos.z = -7;
            startPos.z = -7;
            GameObject attackObject = (GameObject)GameObject.Instantiate(AttackPrefab, startPos, Quaternion.identity, board.AAttacks.ObjectsContainer);
            // fly to slot
            finalPos.z = -7;
            float distance = Mathf.Abs(finalPos.y - startPos.y);
            float speed = 0.05f; // per unit
            float flyTime = distance * speed;
            if (maxTime < flyTime)
            {
                maxTime = flyTime;
            }
            LeanTween.move(attackObject, finalPos, flyTime)
                .setEaseOutSine()
                .setOnComplete(() => {
                    GameObject.Destroy(attackObject);
                    if (attackCharacter)
                    {
                        ApplyAttackOnCharacter(firstSlot, character, enemy.Color, Power);
                    } else
                    {
                        ApplyAttackOnLivesPanel(enemy.Color, Power);
                    }
                });
            // 
        }
        return maxTime;
    }

    private void ApplyAttackOnLivesPanel(int acolor, int power)
    {
        GameManager.Instance.Game.ALivesPanel.RemoveLives(power);
    }

    private void ApplyAttackOnCharacter(SSlot slot, Pipe_Character character, int acolor, int power)
    {
        character.DealDamage(slot, acolor, power);
    }
}