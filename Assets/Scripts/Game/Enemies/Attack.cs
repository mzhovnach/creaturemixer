using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EAttackTarget
{
    EnemySlot = 0,
    Enemy = 1,
    Character = 2,
    LivesPanel,
    //Column - to attack whole column
}

public class Attack : MonoBehaviour
{
    public EAttackTarget    TargetType;
    public GameObject       TargetObject;
    public WeaponBase       Weapon;
    public bool             DestroyOnComplete;
}
