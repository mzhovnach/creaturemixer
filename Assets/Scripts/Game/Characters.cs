using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Characters : MonoBehaviour
{
    public GameObject               CollectManaEffect; //TODO colorize effect or different prefabs
    private List<Pipe_Character>    _characters = new List<Pipe_Character>();
    private Pipe_Character          _selectedCharacter = null;
    private ZActionWorker           _worker;

    private void Awake()
    {
        _worker = new ZActionWorker();
    }

    public void ClearCharacters()
    {
        _characters.Clear();
    }

    public void AddCharacters(GameBoard board, List<SSlot> emptySlots)
    {
        _selectedCharacter = null;
        _characters.Clear();
        emptySlots = Helpers.ShuffleList<SSlot>(emptySlots);
        AddCharacter("Character_0", board, emptySlots);
        AddCharacter("Character_1", board, emptySlots);
        AddCharacter("Character_2", board, emptySlots);
    }

    private void AddCharacter(string characterId, GameBoard board, List<SSlot> emptySlots)
    {
        if (emptySlots.Count == 0)
        {
            Debug.LogError("NO SLOTS FOR CHARACTER");
            return;
        }
        GameObject obj = board.GetPool().GetObjectFromPool(characterId, board.SlotsContainer);
        Pipe_Character character = obj.GetComponent<Pipe_Character>();
        SSlot slot = emptySlots[0];
        emptySlots.RemoveAt(0);
        slot.SetPipe(character);
        character.InitCharacter(1);
        character.PlayAddAnimation();
        _characters.Add(character);
    }

    public void OnCharacterClick(Pipe_Character character)
    {
        if (GameManager.Instance.Game.GetGameState() == EGameState.PlayersTurn)
        {
            if (_selectedCharacter)
            {
                if (_selectedCharacter == character)
                {
                    character.Unselect();
                    _selectedCharacter = null;
                    return;
                } else
                {
                    _selectedCharacter.Unselect();
                    _selectedCharacter = null;
                }
            }

            if (character.Mana.IsFull())
            {
                if (!character.IsSelectable())
                {
                    character.TryApplyPowerup();
                } else
                {
                    character.Select();
                    _selectedCharacter = character;
                }
            }
        }
    }

    public int AddManaForBump(SSlot slot, SPipe pipe, int mana, int color)
    {
        for (int i = 0; i < _characters.Count; ++i)
        {
            int addedMana = _characters[i].AddMana(mana, color);
            if (addedMana > 0)
            {
                mana -= addedMana;
                Transform container = slot.transform.parent;
                Vector3 startPos = slot.transform.position;
                Vector3 endPos = _characters[i].transform.position;
                GameObject effect = GameObject.Instantiate(CollectManaEffect, Vector3.zero, Quaternion.identity) as GameObject;
                effect.transform.SetParent(transform.parent.transform, false);
                effect.transform.localPosition = startPos;
                List<Vector3> path = GameManager.Instance.GameData.XMLSplineData[String.Format("chip_get_{0}", UnityEngine.Random.Range(1, 4))];
                MoveSplineAction splineMover = new MoveSplineAction(effect, path, startPos, endPos, Consts.ADD_POINTS_EFFECT_TIME);
                _worker.AddParalelAction(splineMover);
                GameObject.Destroy(effect, Consts.ADD_MANA_EFFECT_TIME + 0.1f);
            }
            if (mana <= 0)
            {
                break;
            }
        }
        return mana;
    }

    void Update()
    {
        if (_worker != null)
        {
            _worker.UpdateActions(Time.deltaTime);
        }
    }

    public EPowerupType GetSelectedPowerupType()
    {
        if (_selectedCharacter)
        {
            return _selectedCharacter.GetPowerupType();
        } else
        {
            return EPowerupType.None;
        }
    }

    public bool IsCanApply()
    {
        for (int i = 0; i < _characters.Count; ++i)
        {
            if (_characters[i].Mana.IsFull() && _characters[i].IsCanApply())
            {
                return true;
            }
        }
        return false;
    }

    public bool OnSlotTouched(SSlot slot)
    {
        if (_selectedCharacter)
        {
            if (_selectedCharacter == slot.Pipe)
            {
                _selectedCharacter.Unselect();
                _selectedCharacter = null;
            } else
            {
                bool applied = _selectedCharacter.TryApplyPowerup(slot);
                if (applied)
                {
                    _selectedCharacter.Unselect();
                    _selectedCharacter = null;
                }
            }
            return true; // can't swipe if powerup selected
        }
        else
        {
            return false;
        }
    }

    public bool IsAllDead()
    {
        //for (int i = 0; i < _characters.Count; ++i)
        //{
        //    if (!_characters[i].IsDead())
        //    {
        //        return false;
        //    }
        //}
        //return true;
        return _characters.Count == 0;
    }

    public void OnCharacterDied(Pipe_Character character)
    {
        if (_selectedCharacter == character)
        {
            character.Unselect();
            _selectedCharacter = null;
        }
        _characters.Remove(character);
    }

    public void AddLivesToAll(int lives)
    {
        for (int i = 0; i < _characters.Count; ++i)
        {
            if (!_characters[i].IsDead())
            {
                _characters[i].AddLives(lives);
            }
        }
    }

    public bool IsSomebodyWounded()
    {
        for (int i = 0; i < _characters.Count; ++i)
        {
            if (!_characters[i].IsDead() && !_characters[i].Lives.IsFull())
            {
                return true;
            }
        }
        return false;
    }

    public bool IsSomebodyCanApplyPowerup()
    {
        for (int i = 0; i < _characters.Count; ++i)
        {
            if (!_characters[i].IsDead() && _characters[i].IsCanApply())
            {
                return true;
            }
        }
        return false;
    }
}
