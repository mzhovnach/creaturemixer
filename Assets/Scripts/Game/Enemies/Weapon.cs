using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public Text         MovesText;
    public Text         PowerText;
    public GameObject   AttackPrefab;
    public int          Power;
    public int          AttackInterval;
    public int          MaxPower = 1;
    protected int       _movesToAttack; //TODO set this parameter to 1, 2, 3 for each enemy at start of level?

    //private Canvas _canvas;

    //private void Awake()
    //{
        //_canvas = GetComponent<Canvas>();
        //_canvas.worldCamera = Camera.main;
        //_canvas.transform.localScale = new Vector3(0.01f, 0.01f, 1);
    //}

    public void InitWeapon()
    {
        _movesToAttack = AttackInterval;
        SetPower(MaxPower);
        SetMoves(AttackInterval);
    }

    public void SetMoves(int moves)
    {
        MovesText.text = moves.ToString();
        if (moves <= 1)
        {
            // ready to attack
            MovesText.transform.localScale = new Vector3(1.3f, 1.3f, 1.0f);
        } else
        {
            MovesText.transform.localScale = Vector3.one;
        }
    }

    public void SetPower(int damage)
    {
        if (damage <= 0)
        {
            PowerText.gameObject.SetActive(false);
        } else
        {
            PowerText.gameObject.SetActive(true);
            PowerText.text = damage.ToString();
        }
    }

    public bool IsReady()
    {
        return _movesToAttack <= 0;
    }

    public void DecreaseMovesToAttack()
    {
        if (_movesToAttack > 0)
        {
            --_movesToAttack;
            SetMoves(_movesToAttack);
        }
    }

    public void ResetWeapon()
    {
        _movesToAttack = AttackInterval;
        SetMoves(_movesToAttack);
    }

    public virtual IEnumerator AttackCoroutine(Enemy enemy, bool last, Attacks attacks)
    {
        ResetWeapon();
        //
        GameBoard board = GameManager.Instance.Game;
        List<List<SSlot>> slots = GetSlotsWithCharactersInFront(board, enemy, true);
        for (int i = 0; i < slots.Count; ++i)
        {
            Vector3 finalPos = Vector3.zero;
            SSlot firstSlot = slots[i][0];
            bool attackCharacter = false;
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
            }
            int slotX = firstSlot.X;
            Vector3 startPos = board.AEnemies.Slots[slotX].transform.position;
            // instantiate attack beam
            finalPos.z = -7;
            startPos.z = -7;
            GameObject attackObject = (GameObject)GameObject.Instantiate(AttackPrefab, startPos, Quaternion.identity, attacks.ObjectsContainer);
            Attack attack = attackObject.AddComponent<Attack>();
            attack.DestroyOnComplete = true;
            attack.Color = enemy.Color;
            attack.Power = Power;
            if (attackCharacter)
            {
                attack.TargetType = EAttackTarget.Character;
                attack.TargetObject = firstSlot.gameObject;
            } else
            {
                attack.TargetType = EAttackTarget.LivesPanel;
                attack.TargetObject = null;
            }
            AttackData attackData = attacks.AddAttack(attack);
            // fly to slot
            finalPos.z = -7;
            float distance = Mathf.Abs(finalPos.y - startPos.y);
            float flyTime = 0.25f; //distance * SIMPLE_ATTACK_SPEED;
            LeanTween.move(attackObject, finalPos, flyTime)
                .setEaseOutSine()
                .setOnComplete(() => {
                    attacks.ApplyAttack(attackData);
                });
            // 
        }
        //GameManager.Instance.Game.ALivesPanel.AttackByEnemy(enemy);
        yield return StartCoroutine(attacks.WaitEndAttacksCoroutine());
    }

    protected List<List<SSlot>> GetSlotsWithCharactersInFront(GameBoard board, Enemy enemy, bool justFirst)
    {
        List<List<SSlot>> slots = new List<List<SSlot>>();
        List<int> columsToAttack = board.AEnemies.GetSlotsIdsWithEnemy(enemy);
        for (int i = 0; i < columsToAttack.Count; ++i)
        {
            List<SSlot> slotsInColumn = new List<SSlot>();
            int col = columsToAttack[i];
            bool characterFound = false;
            for (int j = GameBoard.HEIGHT - 1; j >= 0; --j)
            {
                SSlot slot = board.GetSlot(col, j);
                if (!slot.IsEmpty() && slot.Pipe.IsCharacter()) // && !slot.Pipe.GetComponent<Pipe_Character>().IsDead())
                {
                    characterFound = true;
                    slotsInColumn.Add(slot);
                    if (justFirst)
                    {
                        break;
                    }
                }
            }
            if (!characterFound)
            {
                slotsInColumn.Add(null);
                slotsInColumn.Add(board.GetSlot(i, 0));
            }
            slots.Add(slotsInColumn);
        }

        return slots;
    }
}