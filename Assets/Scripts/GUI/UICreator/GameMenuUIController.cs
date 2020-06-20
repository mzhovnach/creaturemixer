using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TObject.Shared;
using System;

public class GameMenuUIController : BaseUIController
{
    public GameObject BtnResetSettings;
    public GameObject ButtonPlayEndless;
    public GameObject ButtonPlayEndlessBlocked;
    public GameObject ButtonTurnOffAds;

    // Use this for initialization
    void Start()
    {
        ReInit();
        GameManager.Instance.CurrentMenu = UISetType.MainMenu;
        EventData eventData = new EventData("OnUISwitchNeededEvent");
        eventData.Data["setid"] = UISetType.MainMenu;
        eventData.Data["force"] = true;
        GameManager.Instance.EventManager.CallOnUISwitchNeededEvent(eventData);
    }

    // Update is called once per frame
    void Update()
    {
        //GameManager.Instance.Player.UpdateTimePlayed();
        if (GameManager.Instance == null || GameManager.Instance.Settings == null)
        {
            Debug.Log("ERROR_A --------------------------------------------" + (GameManager.Instance == null).ToString() + "   " + (GameManager.Instance.Settings == null).ToString());
            return;
        }
        //
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.GameFlow.EscapeOnClick())
            {

            }
            else
            if (GameManager.Instance.CurrentMenu == UISetType.MainMenu)
            {
                //EventData eventData = new EventData("OnOpenFormNeededEvent");
                //eventData.Data["form"] = UIConsts.FORM_ID.CONFIRM_WINDOW;
                //eventData.Data["type"] = "confirm_quit";
                //eventData.Data["scene"] = UISetType.MainMenu;
                //GameManager.Instance.EventManager.CallOnOpenFormNeededEvent(eventData);
                GameManager.Instance.GameFlow.ExitGame();
            }
            else
            if (GameBoard.IsInGame())
            {
                if (!GameManager.Instance.Game.IsLoose() && !GameManager.Instance.Game.IsPause())
                {
                    GameManager.Instance.Game.GoHome();
                }
            }
        }
    }

    public void ButtonMenuLeveledOnClick()
    {
        //
        //GameManager.Instance.HideTutorial("");
        //GameManager.Instance.CurrentMenu = "game_menu";
        EventData eventData = new EventData("OnOpenFormNeededEvent");
        eventData.Data["form"] = UIConsts.FORM_ID.OPTIONS_WINDOW;
        eventData.Data["scene"] = "game_menu";
        GameManager.Instance.EventManager.CallOnOpenFormNeededEvent(eventData);
    }

    public void ButtonMenuOnClick()
    {
        //if (!GameManager.Instance.Game.IsPlay())
        //{
        //    return;
        //}
        //
        //GameManager.Instance.HideTutorial("");
        //GameManager.Instance.CurrentMenu = "game_menu";
        EventData eventData = new EventData("OnOpenFormNeededEvent");
        eventData.Data["form"] = UIConsts.FORM_ID.OPTIONS_WINDOW;
        eventData.Data["scene"] = "game_menu";
        GameManager.Instance.EventManager.CallOnOpenFormNeededEvent(eventData);
    }

    public override void Show()
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition3D = UIConsts.STOP_POSITION;
        UpdatePlayEndlessButton();
        if (GameManager.Instance.Player.NoAds)
        {
            ButtonTurnOffAds.gameObject.SetActive(false);
        }
    }

    public void UpdatePlayEndlessButton()
    {
        ButtonPlayEndless.SetActive(true);
        ButtonPlayEndlessBlocked.SetActive(false);
        //bool unlocked = GameManager.Instance.Player.LevelsStates[Consts.UNLOCK_ENDLES_AFTER + 1].Unlocked;
        //ButtonPlayEndless.SetActive(unlocked);
        //ButtonPlayEndlessBlocked.SetActive(!unlocked);
    }

    public override void Hide()
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition3D = UIConsts.START_POSITION;
    }

    private Vector3 TransformPositionFromScreenToLocalUI(Vector3 startPos)
    {
        startPos -= transform.localPosition;
        return startPos;
    }

    public override void ReInit()
    {
        base.ReInit();
#if (PUBLISHING_PLATFORM_AMAZON)
        ButtonTurnOffAds.SetActive(false);
#endif
        //GameManager.Instance.Game.PlayGame();
    }

    protected override void OnDeactivate()
    {
        //transform.Find("ScoreDock").GetComponent<ScoreDock>().DeactivateTriggers();
    }

    protected override void OnActivate()
    {
        //transform.Find("ScoreDock").GetComponent<ScoreDock>().ActivateTriggers();
    }

    //Vector3 localToFixed(Vector3 pos)
    //{
    //    pos.x = ((UIConsts.DESIGN_RESOLUTION.x / 2 - pos.x) * -1);
    //    pos.y = ((UIConsts.DESIGN_RESOLUTION.y / 2 - pos.y));
    //    return pos;
    //}

    public void DisableMenu()
    {
        BroadcastMessage("Disable");
        OnDeactivate();
    }

    public void EnableMenu()
    {
        BroadcastMessage("Enable");
        OnActivate();
    }

    /////////////// MAIN_MENU
    //#if UNITY_STANDALONE
    //IEnumerator LoadLogo() 
    //{
    //	string path = Application.streamingAssetsPath + "/logo.png";
    //	string url = "file://" + path;
    //	WWW www = new WWW(url);
    //	yield return www;
    //	Sprite aSprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0)); // new Vector2(www.texture.width / 2, www.texture.height / 2));
    //	Image im = transform.Find("MGLogo").GetComponent<Image>();
    //	im.sprite = aSprite;
    //	im.SetNativeSize();
    //}
    //#else
    //void LoadLogo() 
    //{        
    //Sprite aSprite = Resources.Load<Sprite>("art/Splash/logo");
    //Image im = transform.Find("MGLogo").GetComponent<Image>();
    //im.sprite = aSprite;
    ////im.SetNativeSize();
    //}
    //#endif

    public void ButtonPlayEndlessOnClick()
    {
        //MusicManager.PlayGameTracks();

        //GameManager.Instance.Game.RestartGame(); no need for restart course this clears saved game!
        GameBoard.AddingType = EAddingType.OnNoMatch;
        GameManager.Instance.Game.ClearBoardForce();
        GameManager.Instance.Game.PlayGame();
    }

    public void ButtonPlayEndlessBlockedOnClick()
    {
        //TUTOR_1
        if (GameManager.Instance.Player.IsTutorialShowed("1"))
        {
            GameManager.Instance.Player.TutorialsShowed.Remove("1");
        }
        GameManager.Instance.ShowTutorial("1", new Vector3(0, 0, 0));
    }

    public void ButtonPlayLevelsOnClick()
    {
        EventData eventData = new EventData("OnOpenFormNeededEvent");
        eventData.Data["form"] = UIConsts.FORM_ID.LEVEL_SELECTION_WINDOW;
        eventData.Data["scene"] = UISetType.MainMenu;
        GameManager.Instance.EventManager.CallOnOpenFormNeededEvent(eventData);
    }

    //	public void ButtonPlayInverseOnClick()
    //	{
    //		MusicManager.PlayGameTracks();
    //		GameBoard.GameType = EGameType.Inverse;
    //		GameManager.Instance.GameFlow.TransitToScene(UIConsts.SCENE_ID.GAME_SCENE);
    //	}

    public void ButtonOptionsOnClick()
    {
        EventData eventData = new EventData("OnOpenFormNeededEvent");
        eventData.Data["form"] = UIConsts.FORM_ID.OPTIONS_WINDOW;
        eventData.Data["scene"] = "main_menu";
        GameManager.Instance.EventManager.CallOnOpenFormNeededEvent(eventData);
    }

    public void ButtonOptionsWideOnClick()
    {
        EventData eventData = new EventData("OnOpenFormNeededEvent");
        eventData.Data["form"] = UIConsts.FORM_ID.OPTIONS_WINDOW;
        eventData.Data["scene"] = "main_menu";
        GameManager.Instance.EventManager.CallOnOpenFormNeededEvent(eventData);
    }

    public void ButtonExitOnClick()
    {
        Debug.LogError("TODO");
        //EventData eventData = new EventData("OnOpenFormNeededEvent");
        //eventData.Data["form"] = UIConsts.FORM_ID.EXIT_GAME_WINDOW;
        //eventData.Data["scene"] = "main_menu";
        //GameManager.Instance.EventManager.CallOnOpenFormNeededEvent(eventData);
    }

    public void ButtonResetSettingsOnClick()
    {
        GameManager.Instance.Game.ClearProgress();
        UpdatePlayEndlessButton();
    }

    public void ButtonTrophiesOnClick()
    {
        //EventData eventData = new EventData("OnOpenFormNeededEvent");
        //eventData.Data["form"] = UIConsts.FORM_ID.TROPHIES_WINDOW;
        //eventData.Data["scene"] = "main_menu";
        //GameManager.Instance.EventManager.CallOnOpenFormNeededEvent(eventData);
    }

    public void ButtonRateGameOnClick()
    {

    }   

}
