using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyParams
{
    public string Name;
    public int Size;
    public int Lives;
    public int Damage;
    public int Color;
    public int AttackInterval;

    public void Init(string name, Enemy enemy)
    {
        Name = name;
        Size = enemy.Size;
        Lives = enemy.MaxLives;
        Damage = enemy.AWeapon.MaxPower;
        Color = enemy.Color;
        AttackInterval = enemy.AWeapon.AttackInterval;
    }
}

public enum EEnemyAnimState
{
    Normal = 0,
    GainDamage = 1,
    Death = 2,
    Attack = 3
}

public class Enemy : MonoBehaviour
{
    //TODO
    // Базовий клас ворога - містить свій колір, життя, спец-здібності, анімації атаки, смерті і можливо появи...
    [Range(1, 5)]
    public int Size = 1; // how many slots
    public GameObject ScaleObject; // should has local scale = (1, 1, 1)
    public GameObject ShakeObject; // should has local position = (0, 0, 0)
    public int MaxLives = 1;
    [Range(-1, 4)]
    public int Color = -1; // default color

    public Weapon       AWeapon;
    public ProgressView Lives;
    protected bool _dead = false;

    EEnemyAnimState _animState = EEnemyAnimState.Normal;
    public const float ENEMY_APPEAR_TIME = 0.15f;
    public List<AttackData> _attacksApplied = new List<AttackData>();

    public virtual void InitEnemy()
    {
        _dead = false;
        _animState = EEnemyAnimState.Normal;
        Lives.InitCounter(MaxLives, MaxLives);
        AWeapon.InitWeapon();
        LeanTween.cancel(ShakeObject);
        ShakeObject.transform.localPosition = Vector3.zero;
        //LeanTween.cancel(ScaleObject);
        //ScaleObject.transform.localScale = Vector3.one;
        UpdateLivesView();
        PlayAppearAnimation();
    }

    public virtual float ApplyAttack(AttackData attackData)
    {
        if (_dead)
        {
            Debug.LogError("DEAD!");
            return 0;
        }
        _attacksApplied.Add(attackData);
        Lives.SetAmount(Lives.GetAmount() - attackData.AAttack.Power);
        _dead = Lives.IsEmpty();
        return PlayGainDamageAnimation();
    }

    protected virtual float PlayGainDamageAnimation()
    {
        _animState = EEnemyAnimState.GainDamage;
        UpdateLivesView();
        float time = 0.25f;
        LeanTween.cancel(ShakeObject);
        float shakePower = 0.2f;
        LeanTween.value(ShakeObject, 0.0f, 1.0f, time)
            .setOnUpdate((float val) =>
            {
                Vector2 dxy = UnityEngine.Random.insideUnitCircle;
                ShakeObject.transform.localPosition = new Vector3(shakePower * dxy.x, shakePower * dxy.y, 0);
            })
            .setOnComplete(() =>
            {
                ShakeObject.transform.localPosition = Vector3.zero;
                if (_dead)
                {
                    PlayDeathAnimation();
                } else
                {
                    RemoveAppliedAttacks();
                    _animState = EEnemyAnimState.Normal;
                }
            });
        return time;
    }

    protected virtual void PlayDeathAnimation()
    {
        //TODO through alpha or other animation + sound
        _animState = EEnemyAnimState.Death;
        float time = 0.15f;
        LeanTween.cancel(ScaleObject);
        float scaleMin = 0f;
        LeanTween.scale(ScaleObject, new Vector3(scaleMin, scaleMin, 1), time)
            .setEaseInOutSine()
            .setOnComplete(() =>
            {
                RemoveAppliedAttacks();
                HideForce();
            });
    }

    private void RemoveAppliedAttacks()
    {
        for (int i = 0; i < _attacksApplied.Count; ++i)
        {
            GameManager.Instance.Game.AAttacks.OnAttackApplied(_attacksApplied[i]);
        }
        _attacksApplied.Clear();
    }

    public virtual float PlayAttackAnimation() // returnes time of animation
    {
        float time = 0.25f;
        _animState = EEnemyAnimState.Attack;
        LeanTween.cancel(ScaleObject);
        float scaleMax = 1.2f;
        LeanTween.scale(ScaleObject, new Vector3(scaleMax, scaleMax, 1), time / 2.0f)
            .setEaseInOutSine()
            .setOnComplete(() =>
            {
                LeanTween.scale(ScaleObject, Vector3.one, time / 2.0f)
                    .setEaseInOutSine()
                    .setOnUpdate((Vector3 val) =>
                    {
                        ScaleObject.transform.localScale = val;
                    })
                    .setOnComplete(()=>{
                        //GameManager.Instance.Game.AEnemies.DecreaseAttacksCount();
                        _animState = EEnemyAnimState.Normal;
                    });
            });
        return time;
    }

    public void HideForce()
    {
        gameObject.SetActive(false);
    }

    public virtual float PlayHideAnimation()
    {
        float time = 0.15f;
        LeanTween.cancel(ScaleObject);
        float scaleMin = 0f;
        LeanTween.scale(ScaleObject, new Vector3(scaleMin, scaleMin, 1), time)
            .setEaseInOutSine()
            .setOnComplete(() =>
            {
                HideForce();
            });
        return time;
    }

    public virtual float PlayAppearAnimation()
    {
        float time = ENEMY_APPEAR_TIME;
        LeanTween.cancel(ScaleObject);
        ScaleObject.transform.localScale = new Vector3(0, 0, 1);
        float scaleMax = 1f;
        LeanTween.scale(ScaleObject, new Vector3(scaleMax, scaleMax, 1), time)
            .setEaseInOutSine();
        return time;
    }

    public virtual void UpdateLivesView()
    {
        //TODO change art according to lives
        //TODO прогресс-бар здоров"я як мінімум, так само для мани ворогів-кастерів
    }

    public bool IsDead()
    {
        return _dead;
    }

    public void DecreaseMovesToAttack()
    {
        AWeapon.DecreaseMovesToAttack();
    }

    public bool IsReadyToAttack()
    {
        return AWeapon.IsReady();
    }

    public void ResetWeapon()
    {
        AWeapon.ResetWeapon();
    }
}
