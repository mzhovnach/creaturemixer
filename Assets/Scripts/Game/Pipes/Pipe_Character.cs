using UnityEngine;
using System.Collections.Generic;

public enum ECharacterAnimState
{
    Normal = 0,
    GainDamage = 1,
    Death = 2,
    Attack = 3
}

public class Pipe_Character : SPipe
{
    protected Powerup           _powerup;
    public GameObject           Art;
    public GameObject           Selection;
    public ProgressView         Mana;
    public ProgressView         Lives;
    public GameObject           ScaleObject; // should has local scale = (1, 1, 1)
    public GameObject           ShakeObject; // should has local position = (0, 0, 0)
    public GameObject           DeadObject;
    public GameObject           ReadyObject;
    public EnemyUI              UI;

    protected bool              _selected = false;
    protected bool              _dead = false;
    ECharacterAnimState         _animState = ECharacterAnimState.Normal;
    //public List<AttackData>     _attacksApplied = new List<AttackData>();

    protected void Awake()
    {
        _powerup = GetComponent<Powerup>();
    }

    public virtual void InitCharacter(int level)
    {
        _animState = ECharacterAnimState.Normal;
        UI.gameObject.SetActive(true);
        _powerup.InitPowerup(level);
        Lives.InitCounter(30, 30);  // TODO according to level
        Mana.InitCounter(0, 15);    // TODO according to level
        UpdateReadyMark();
        _destroyed = false;
        _movable = true;
        _dead = false;
        DeadObject.SetActive(false);
        SetValueForce(level);
        UnselectForce();
    }

	public void SetValueForce(int param)
	{
        Param = param;
        UpdateSkin();
    }
		
	public override void UpdateSkin()
	{
        //TODO different images on different level
	}
    public bool RemoveMana(int manaToRemove)
    {
        int mana = Mana.GetAmount() - manaToRemove;
        Mana.SetAmount(mana);
        return Mana.IsFull();
    }

    public bool AddMana(int manaToAdd)
    {
        int mana = Mana.GetAmount() + manaToAdd;
        Mana.SetAmount(mana);
        UpdateReadyMark();
        return Mana.IsFull();
    }

    public int AddMana(int manaToAdd, int color)
    {
        if (Mana.IsFull())
        {
            return 0;
        }
        if (color < 0 || color == _powerup.GetColor())
        {
            int manaToAddReal = Mathf.Min(Mana.GetMaxAmount() - Mana.GetAmount(), manaToAdd);
            int mana = Mana.GetAmount() + manaToAddReal;
            Mana.SetAmount(mana);
            UpdateReadyMark();
            return manaToAddReal;
        }
        return 0;
    }

    public bool IsSelected()
    {
        return _selected;
    }

    public void Select()
    {
        _selected = true;
        Selection.SetActive(true);
    }

    public void UnselectForce()
    {
        _selected = false;
        Selection.SetActive(false);
    }

    public void Unselect()
    {
        _selected = false;
        Selection.SetActive(false);
    }

    public bool TryApplyPowerup()
    {
        if (!IsCanApply())
        {
            return false;
        }
        Mana.SetAmount(0);
        UpdateReadyMark();
        _powerup.ApplyPowerup();
        return true;
    }

    public bool TryApplyPowerup(SSlot slot)
    {
        if (!IsCanApply(slot))
        {
            return false;
        }
        Mana.SetAmount(0);
        UpdateReadyMark();
        _powerup.ApplyPowerup(slot);
        return true;
    }

    public bool IsCanApply()
    {
        return _powerup.IsCanApply();
    }

    public bool IsCanApply(SSlot slot)
    {
        return _powerup.IsCanApply(slot);
    }

    public bool IsSelectable()
    {
        return _powerup.IsSelectable();
    }

    public EPowerupType GetPowerupType()
    {
        return _powerup.GetPowerupType();
    }

    public virtual bool DealDamage(SSlot slot, int acolor, int power)
    {
        if (_dead)
        {
            Debug.LogError("DEAD!");
            return false;
        }
        //TODO paper/scissors/stone!!!
        int lives = Lives.GetAmount() - power;
        Lives.SetAmount(lives);
        _dead = Lives.GetAmount() == 0;
        if (_dead)
        {
            slot.TakePipe();
            GameManager.Instance.Game.ACharacters.OnCharacterDied(this);
            PlayDeathAnimation();
        } else
        {
            PlayGainDamageAnimation();
        }
        return _dead;
    }

    public void AddLives(int livesToAdd)
    {
        if (!Lives.IsFull())
        {
            int lives = Lives.GetAmount() + livesToAdd;
            Lives.SetAmount(lives);
        }
    }

    protected virtual float PlayGainDamageAnimation()
    {
        _animState = ECharacterAnimState.GainDamage;
        //UpdateLivesView();
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
                _animState = ECharacterAnimState.Normal;
            });
        return time;
    }

    protected virtual void PlayDeathAnimation()
    {
        _animState = ECharacterAnimState.Death;
        float time = 0.15f;
        LeanTween.cancel(ScaleObject);
        float scaleMin = 0f;
        UI.gameObject.SetActive(false);
        LeanTween.scale(ScaleObject, new Vector3(scaleMin, scaleMin, 1), time)
            .setEaseInOutSine()
            .setOnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        LeanTween.cancel(DeadObject);
        DeadObject.transform.localScale = new Vector3(0, 0, 1);
        DeadObject.SetActive(true);
        LeanTween.scale(DeadObject, Vector3.one, time)
            .setEaseInOutSine();
    }

    public bool IsDead()
    {
        return _dead;
    }

    protected void UpdateReadyMark()
    {
        //TODO move to counter view!!!!, particles, hide progress bar through canvas group
        ReadyObject.SetActive(Mana.IsFull());
    }
}