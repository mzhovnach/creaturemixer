using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatsCustom : CheatsBase
{
    private int level = 0;
    private bool pinLevel = false;

    public override void DisplayCheats()
    {
        GUILayout.Label("--------------------------------------------------------------");
        GUILayout.BeginHorizontal("pin level");
        {
            if (pinLevel)
            {
                DisplayButtonCheat("UNPIN LEVEL", () =>
                {
                    pinLevel = false;
                    GameManager.Instance.Game.PinLevel(pinLevel);
                });
            } else
            {
                DisplayButtonCheat("PIN LEVEL", () => 
                {
                    pinLevel = true;
                    GameManager.Instance.Game.PinLevel(pinLevel);
                });
            }
        }
        GUILayout.EndHorizontal();

        if (GameManager.Instance.CurrentMenu == UISetType.LevelBattle || GameManager.Instance.CurrentMenu == UISetType.LevelCollect)
        {
            if (GameManager.Instance.Game.IsPlayersTurn())
            {
                GUILayout.BeginHorizontal("winloose");
                {
                    DisplayButtonCheat("Win Game", () => GameManager.Instance.Game.OnLevelCompleted());
                    DisplayButtonCheat("Loose Game", () => GameManager.Instance.Game.OnLoose());
                }
                GUILayout.EndHorizontal();

                if (GameManager.Instance.CurrentMenu == UISetType.LevelBattle)
                {
                    GUILayout.BeginHorizontal("mana");
                    {
                        DisplayButtonCheat("Refill Mana", () => GameManager.Instance.Game.RefillMana());
                        DisplayButtonCheat("Remove Mana", () => GameManager.Instance.Game.RemoveMana());
                    }
                    GUILayout.EndHorizontal();
                }
            }
        } else
        if (GameManager.Instance.CurrentMenu == UISetType.MainMenu)
        {
            GUILayout.BeginVertical("levelselection");
            {
                float flevel = GUILayout.HorizontalSlider(level, 0, Consts.LEVELS_COUNT - 1);
                level = (int)flevel;
                GUILayout.Label("Level : " + level.ToString());
                DisplayButtonCheat("ApplyLevel", () => { GameManager.Instance.Player.CreatureMixLevel = level; });
            }
            GUILayout.EndVertical();
        }
    }
}
