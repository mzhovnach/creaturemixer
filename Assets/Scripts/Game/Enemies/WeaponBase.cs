using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponBase : MonoBehaviour
{
    public int          Power;
    public int          MaxPower = 1;
    protected bool      _attacking = false;

    public virtual void InitWeapon()
    {
        _attacking = false;
        SetPower(MaxPower);
    }

    public virtual void SetPower(int power)
    {
        Power = power;
    }

    public virtual bool IsReady()
    {
        return true;
    }

    public bool IsAttacking()
    {
        return _attacking;
    }

    protected void OnStartAttack()
    {
        _attacking = true;
    }

    protected void OnEndAttack()
    {
        _attacking = false;
    }

    public virtual void ResetWeapon()
    {

    }

    // for enemies attacks
    public virtual IEnumerator AttackCoroutine(GameBoard board, Enemy enemy, bool last)
    {
        ResetWeapon();
        yield return new WaitForSeconds(0.1f);
    }

    // for players attacks
    public virtual IEnumerator AttackCoroutine(GameBoard board, SSlot slot, int pipeColor, int attackPower)
    {
        ResetWeapon();
        yield return new WaitForSeconds(0.1f);
    }

}