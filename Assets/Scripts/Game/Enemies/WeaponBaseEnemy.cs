using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponBaseEnemy : WeaponBase
{
    public GameObject   AttackPrefab;
    public Text         MovesText;
    public Text         PowerText;
    public int          AttackInterval;
    protected int       _movesToAttack; //TODO set this parameter to 1, 2, 3 for each enemy at start of level?

    //private Canvas _canvas;

    //private void Awake()
    //{
        //_canvas = GetComponent<Canvas>();
        //_canvas.worldCamera = Camera.main;
        //_canvas.transform.localScale = new Vector3(0.01f, 0.01f, 1);
    //}

    public override void InitWeapon()
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

    public override void SetPower(int power)
    {
        Power = power;
        if (power <= 0)
        {
            PowerText.gameObject.SetActive(false);
        } else
        {
            PowerText.gameObject.SetActive(true);
            PowerText.text = power.ToString();
        }
    }

    public override bool IsReady()
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

    public override void ResetWeapon()
    {
        _movesToAttack = AttackInterval;
        SetMoves(_movesToAttack);
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
                if (!slot.IsEmpty() && slot.Pipe.IsCharacter() && !slot.Pipe.GetComponent<Pipe_Character>().IsDead())
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
                slotsInColumn.Add(board.GetSlot(col, 0));
            }
            slots.Add(slotsInColumn);
        }
        return slots;
    }
}