using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    public ProgressView LivesView;
    public Text         MovesText;
    public Text         DamageText;

    private Canvas      _canvas;
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = Camera.main;
        //_canvas.transform.localScale = new Vector3(0.01f, 0.01f, 1);
    }

    public void InitEnemyUI(Enemy enemy)
    {
        LivesView.InitCounter(enemy.GetLives(), enemy.MaxLives);
        SetDamage(enemy.GetDamage());
        SetMoves(enemy.GetMovesToAttack());
    }

    public void SetLives(int lives)
    {
        LivesView.SetAmount(lives);
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

    public void SetDamage(int damage)
    {
        if (damage <= 0)
        {
            DamageText.gameObject.SetActive(false);
        } else
        {
            DamageText.gameObject.SetActive(true);
            DamageText.text = damage.ToString();
        }
    }
}
