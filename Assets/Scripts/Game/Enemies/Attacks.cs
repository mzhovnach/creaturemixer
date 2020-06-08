using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacks : MonoBehaviour
{
    private GameBoard           _board;
	public Transform 			ObjectsContainer;

    private void Awake()
    {
        _board = GameManager.Instance.Game;
    }

    public bool IsSomeAttack()
    {
        if (_board.SimpleWeapon.IsAttacking() || _board.FinalWeapon.IsAttacking())
        {
            return true;
        }
        if (_board.AEnemies.IsSomeAttack())
        {
            return true;
        }
        return false;
    }

    public IEnumerator WaitEndAttacksCoroutine()
    {
        do
        {
            yield return new WaitForSeconds(0.005f);
        } while (IsSomeAttack());
    }
}
