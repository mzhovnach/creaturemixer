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
    public int Power;
    public int Color;
    public EAttackTarget TargetType;
    public GameObject TargetObject;
    public bool DestroyOnComplete;
}
