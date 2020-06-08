using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public enum EAddingType
{
    EachXMoves = 0,
    OnNoMatch = 1
}
public enum EGameState
{
    Pause = -1,
    Loose = 0,
    Win = 1,
    PlayersTurn = 2,
    PlayerUsedPowerup = 3,
    PlayersAttack = 4, // match was made or powerup used that made match
    EnemiesAttack = 5,
    EnemyAppear = 6,
    MoveEnemy = 7
}

public enum ESlideType
{
	None = 0,
	Move = 1,
	Consum = 2,
	Match = 3
}

public struct SlideData
{
	public BoardPos 		PosSlideFrom;
	public BoardPos 		PosAforMatchSlide;
	public BoardPos 		FinalPosForSlide;
	public SSlot 			Slot;
	public SSlot 			Slot2;				// slot in which we arrive
	public SPipe 			Pipe;
	public SPipe 			Pipe2;				// pipe at slot Slot2
	public int 				DirX;
	public int 				DirY;
	public int 				DistX;
	public int 				DistY;
	public ESlideType 		SlideType;
    public int              NewParam;
}

public class GameBoard : MonoBehaviour
{
	public struct MatchHintData
	{
		public int XA;
		public int YA;
		public int XB;
		public int YB;
		public bool IsMatch;
	}

    private struct ChainInfo
    {
        public int X;
        public int Y;
        public int Color;
        public int Param;
        public int Id;
        public EPipeType PipeType;
    }

    public const int 							WIDTH 					= 7;
	public const int 							HEIGHT 					= 7;
    public static Vector2                       DXDY = Vector2.zero; // зміщення боарда, щоб потрапив в поле зору камери
    public static float							SlotZ                 	= 0.3f;
    public static float             			PipeZ                	= 0.26f;
	public static float             			PipeZForMatch			= -2.0f;
    public static float             			PipeDragZ             	= -0.3f;
    public static float             			SlotSize              	= 1.86f;
	public static float             			ImpulseSpeed       	  	= 30.0f;                            		// speed of moving pipe when it slide after impulse
    public static EAddingType                   AddingType              = EAddingType.EachXMoves;

    public float             					ImpulseDistance	  		= 0.5f;//TODO в опшнси винести!				// how far need slide to pull pipe
	public SSlot[,] 			    			Slots { get; set; }
	public int                   				MaxX { get; set; }
	public int                   				MinX { get; set; }
	public int                   				MaxY { get; set; }
	public int                   				MinY { get; set; }

	public GameObject							BumpShakeObject;
	private float								_shakeDx = 0;
	private float								_shakeDy = 0;

	private Vector3								_cameraPos;
    private Vector3                             _slotsContainerPos;

    public WeaponPlayersSimple                  SimpleWeapon;
    public WeaponPlayersFinal                   FinalWeapon;

    public QueuePanel                           AQueuePanel;
	public SequencePanel                        ASequencePanel;
	public MovesPanel							AMovesPanel;
	public StarsPanel							AStarsPanel;
	public LevelPanel							ALevelPanel;
    public LivesPanel                           ALivesPanel;
    public PowerupsPanel                        APowerupsPanel;
    public Characters                           ACharacters;

    public Enemies                              AEnemies;
    public EnemiesQueue                         AEnemiesQueue;
    public Attacks                              AAttacks;

    // sprites
    private Dictionary<string, Sprite>          _sprites = new Dictionary<string, Sprite>();
    public List<Sprite>                         Sprites;
    
    public List<Color>                          Colors; // colors of pipes and enemiesб use GetPipeColor() to get!

    public List<GameObject>               		PipesPrefabs;														// prefabs for pipes
	public List<GameObject>						ColoredPipesPrefabs;
	// pool
    public GameObject               			SSlotPrefab;
    private SuperSimplePool                     _pool;
	//
    public GameObject               			Selection;                                                			// selection for pipe that we move

	public Transform							SlotsContainer;
    
    //private Vector2               				_dragDxDy             			= new Vector2();
	protected Vector2               			_startPos               		= new Vector2();                    // position when we click on pipe

	//
//    private int                   				_prevXin;
//    private int                   				_prevYin;
//    private float                 				_prevXpos;
//    private float                 				_prevYpos;

	public GameObject                           BreakeEffectPrefab;
    public GameObject                           ChainEffectPrefab;
	public GameObject							MatchEffectPrefab;

    private Dictionary<int, ChainInfo>          _chainInfos = new Dictionary<int, ChainInfo>();
    private Dictionary<int, ChainInfo>          _checkedChainInfos = new Dictionary<int, ChainInfo>();
	private int 								_maxColoredLevels = 9;

	private float 								_hintTimer = 0;
    private float                               _tutor2Timer = 0;
    private NewHintScript				    	_hint;
	public GameObject							HintPrefab;
    private int                                 _startSequenceState = 0;
    private MatchHintData                       _startTutorHintData;

    public GameMenuUIController                 GameMenuUIController;
    public AimPanel                             AAimPanel;
    [SerializeField] UpgradesManager            _upgradesManager;
    [SerializeField] CreaturesManager           _creaturesManager;

    private Camera _camera;
    private Canvas _canvas;
    public Canvas ACanvas
    {
        set
        {
            _canvas = value;
        }

        get
        {
            if (_canvas == null)
            {
                _canvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
            }
            return _canvas;
        }
    }

	private int                               _currentTouchId = -1; // for EInputType.UsingPositions
	public Material[] ColoredMaterials;
    public Material[] ColoredMaterialsFill_0;
    public Material[] ColoredMaterialsFill_1;
    public Material[] ColoredMaterialsFill_2;
    public Material[] ColoredMaterialsFill_3;
    public Material[] ColoredMaterialsFill_4;

    // GameData
    private List<long> _resources;
    private long _pointsForSequences;
    private EGameState _gameState = EGameState.Pause;
    public float TimePlayed;
    public SSlot DragSlot;
    public Enemy DragEnemy = null;
    private Vector2 _startDragEnemyPos = Vector2.zero;
    private int _movesToNextPipe;           // for turn base game
    public int _allTurns;                   // for all game types
    private int _pipesAdded;
    private int _pipesToNextBlocker;
    // leveled
    public int MovesLeft;
    // slots to check aims
    List<BoardPos> _slotsToCheckAims = new List<BoardPos>();
    BoardPos _lastSlotWithMatch = new BoardPos(-1, -1);
    private List<int> _possibleColors = new List<int>();
    bool _addNewPipes = false;

    void Awake()
    {
        // limiting FPS
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = Consts.MAX_FPS;
        //
        _camera = Camera.main;
        _pool = GetComponent<SuperSimplePool>();
        _pool.InitPool();
        //Game Board Data
        _movesToNextPipe = 0;
        if (GameBoard.AddingType != EAddingType.OnNoMatch)
        {
            _movesToNextPipe = Consts.TURNS_TO_NEXT_PIPE;
        }
        _allTurns = 0;
        _pipesAdded = 0;
        _pipesToNextBlocker = Consts.PIPES_TO_NEXT_BLOCKER;
        _pointsForSequences = 0;
        _resources = new List<long>();
        for (int i = 0; i < Consts.COLORS.Length; ++i)
        {
            _resources.Add(0);
            //SetResourceForce(0, i);
        }
        SetGameState(EGameState.Pause, "Awake");
        TimePlayed = 0;
        DragSlot = null;
        DragEnemy = null;
        GameManager.Instance.Game = this;
        //
        //EventManager.OnTransitToMenu += OnTransitToMenu;
        //EventManager.OnStartPlayPressedEvent += CallOnStartPlayPressed;

        DXDY = new Vector2(-WIDTH * SlotSize / 2.0f + SlotSize / 2.0f, -HEIGHT * SlotSize / 2.0f + SlotSize / 2.0f);
        DXDY.y += 0.54f;
        DXDY.x += transform.localPosition.x;
        DXDY.y += transform.localPosition.y;

		CreateSlots();
        // 
        for (int i = 0; i < Sprites.Count; ++i)
        {
            _sprites.Add(Sprites[i].name, Sprites[i]);
        }
        //
        _slotsToCheckAims.Add(new BoardPos(1, 4));
        _slotsToCheckAims.Add(new BoardPos(2, 4));
        _slotsToCheckAims.Add(new BoardPos(3, 4));
        //
        _upgradesManager.Reset();
        _slotsContainerPos = SlotsContainer.transform.position;
        _creaturesManager.Reset();
    }

    void OnDestroy()
    {
        //EventManager.OnTransitToMenu -= OnTransitToMenu;
		//EventManager.OnStartPlayPressedEvent -= CallOnStartPlayPressed;
    }
    // Use this for initialization
    void Start () 
	{
		//_camera.transform.position = new Vector3(0, 0.2f, _camera.transform.position.z);
        //Invoke("PlayGame", 0.15f);
		//PlayGame();
    }

    //void CallOnStartPlayPressed(EventData e)
    //{
    //	PlayGame();
    //}

    // Update is called once per frame
    void Update () 
	{
        if (GameManager.Instance.CurrentMenu != UISetType.ClassicGame && GameManager.Instance.CurrentMenu != UISetType.LeveledGame)
		{
			return;
		}
        Cheats.CheckMatchCheats(this);
        //camera shakes	
        Vector3 realPos = _slotsContainerPos; //_cameraPos;
        realPos.x -= _shakeDx;
        realPos.y -= _shakeDy;
        //_camera.transform.position = realPos;
        SlotsContainer.position = realPos;
        // update drag of slot
        if (IsPause() || IsLoose())
		{
			return;
		}
        TryShowHint();
        TryShowTutor2();

        UpdateInput();
    }

  //  void OnTransitToMenu(EventData e)
  //  {
		//CancelInvoke();
  //  }

	///////////
	public static Vector3 SlotPos(int x, int y)
	{
		return new Vector3(DXDY.x + x * SlotSize, DXDY.y + y * SlotSize, SlotZ);
	}

	public static Vector2 SlotPos(Vector2 posin)
	{
		return SlotPos((int)posin.x, (int)posin.y);
	}

	public static Vector3 PipePos(int x, int y)
	{
		Vector3 res = SlotPos(x, y);
		res.z = PipeZ;
		return res;
	}

	public static Vector2 PipePos(Vector2 posin)
	{
		Vector3 res = SlotPos((int)posin.x, (int)posin.y);
		res.z = PipeZ;
		return res;
	}

	public static BoardPos SlotPosIn(float x, float y)
	{
		BoardPos res;
		float resx = (x - DXDY.x) / SlotSize - 0.5f;
		float resy = (y - DXDY.y) / SlotSize - 0.5f;
		res.x = Mathf.CeilToInt(resx);
		res.y = Mathf.CeilToInt(resy);
		return res;
	}

	public static BoardPos SlotPosIn(Vector2 pos)
	{
		return SlotPosIn(pos.x, pos.y);
	}

	public bool IsSlotInBoard(int i, int j)
	{
		if (i >= WIDTH || i < 0 || j >= HEIGHT || j < 0) 
		{
			return false;
		}
		return true;
	}

	public bool IsSlotInBoard(BoardPos posInd)
	{
		return IsSlotInBoard(posInd.x, posInd.y);
	}

	public SSlot GetSlot (int i, int j) 
	{
		return Slots[i,j];
	}

	public SSlot GetSlot (BoardPos posInd) 
	{
		return GetSlot(posInd.x, posInd.y);
	}

	public void SetSlot (int i, int j, SSlot slot) 
	{
		slot.X = i;
		slot.Y = j;
		Slots[i,j] = slot;
		slot.transform.position = SlotPos(i, j);
	}
	
	////////////
	protected void CreateSlots()
	{
		// create empty invissible slots
		Slots = new SSlot[WIDTH, HEIGHT];
		for (int i = 0; i < WIDTH; ++i)
		{
			for (int j = 0; j < HEIGHT; ++j)
			{
                GameObject slotObj = _pool.InstantiateObject("SSlot", SlotsContainer, Vector3.zero);
                SSlot slot = slotObj.GetComponent<SSlot>();
				slotObj.transform.position = SlotPos(i, j);
				slot.InitSlot(i, j);
				Slots[i, j] = slot;
			}
		}
        //
        AddSlotDoubles();
	}

    private void AddSlotDoubles()
    {
        // adding CellDoubles
        for (int i = 0; i < GameManager.Instance.Player.SlotsDoubles.Count; ++i)
        {
            Vector2 pos = GameManager.Instance.Player.SlotsDoubles[i];
            int x = (int)pos.x;
            int y = (int)pos.y;
            Slots[x, y].AddSlotDouble();
        }
    }

	protected SPipe CreatePipe(EPipeType pType, int parameter, int color)
	{
		SPipe res = null;
		GameObject pipeObj = null;
		if (pType == EPipeType.Colored && color < 0)
		{
			color = GetRandomColor();
		}
		pipeObj = GetPipeFromPool(pType, color);

		pipeObj.transform.SetParent(SlotsContainer, false);
		SPipe pipe = pipeObj.GetComponent<SPipe>();
		pipe.InitPipe(parameter, color);
		//pipe.transform.parent = transform;
		res = pipe;
		res.transform.localScale = new Vector3(1, 1, 1);
		LeanTween.cancel(pipeObj);
		return res;
	}

    public void ClearBoardForce()
    {
        if (Slots == null) return;
        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                SPipe pipe = Slots[i, j].TakePipe();
                if (pipe) pipe.gameObject.SetActive(false);
            }
        }
        ACharacters.ClearCharacters();
        AEnemies.ClearEnemiesForce();
    }

    private void ClearBoardQuick()
    {
        if (Slots == null) return;

        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                SPipe pipe = Slots[i, j].TakePipe();
                if (pipe)
                {
                    pipe.PlayHideAnimation();
                }
            }
        }
        ACharacters.ClearCharacters();
    }

    protected IEnumerator ClearBoard()
    {
        if (Slots == null) yield return null;
        float waitTime = AEnemies.ClearEnemies();
        float waitedTime = 0;
        float waitOnEachPipe = 0.05f;
        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                SPipe pipe = Slots[i, j].TakePipe();
                if (pipe)
                {
                    pipe.PlayHideAnimation();
                    waitedTime += waitOnEachPipe;
                    yield return new WaitForSeconds(waitOnEachPipe);
                }
            }
        }
        waitTime -= waitedTime;
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }
        ACharacters.ClearCharacters();
    }

    protected IEnumerator CreateLevel(LevelData levelData) 
	{
        _currentTouchId = -1;
        DragSlot = null;
        DragEnemy = null;
        HideSelection();
        _addNewPipes = levelData.AddNewPipes;
        //
        _possibleColors.Clear();
        for (int i = 0; i < levelData.Colors.Count; ++i)
        {
            _possibleColors.Add(levelData.Colors[i]);
        }
        //
        AQueuePanel.LoadPanel(levelData.QueueState);
        //AAimPanel.InitPanel(levelData);
        ALivesPanel.InitPanel(100, 100);

        List<PowerupData> powerupsData = new List<PowerupData>(); //TODO select before level
        powerupsData.Add(new PowerupData(EPowerupType.AddLives, 0));
        powerupsData.Add(new PowerupData(EPowerupType.Reshaffle, 0));
        powerupsData.Add(new PowerupData(EPowerupType.DestroyPiece, 0));
        APowerupsPanel.InitPanel(powerupsData);
        for (int i = 0; i < WIDTH; ++i)
		{
			for (int j = 0; j < HEIGHT; ++j)
			{
				Slots[i, j].SetAsNotHole();
			}
		}

		TimePlayed = levelData.timePlayed;
		for (int i = 0; i < levelData.Resources.Count; ++i)
		{
			SetResourceForce(levelData.Resources[i], i);
		}

        yield return new WaitForSeconds(Consts.DARK_SCREEN_SHOW_HIDE_TIME);
        yield return StartCoroutine(ClearBoard());
        AEnemiesQueue.InitQueue(levelData.EnemiesQueue);
        // create pipes force
        for (int i = 0; i < levelData.Slots.Count; ++i)
        {
            int x = levelData.Slots[i].x;
            int y = levelData.Slots[i].y;
            Slots[x, y].InitSavedSlot(levelData.Slots[i]);
            // pipe
            EPipeType pType = (EPipeType)levelData.Slots[i].pt;
            if (pType != EPipeType.None)
            {
                // create pipe
                SPipe pipe = CreatePipe(pType, levelData.Slots[i].p, levelData.Slots[i].c);
                Slots[x, y].SetPipe(pipe);
                pipe.PlayAddAnimation();
                yield return new WaitForSeconds(0.025f);
            }
        }
        yield return new WaitForSeconds(0.15f);
        ACharacters.AddCharacters(this, GetEmptySSlots());
        StartPlayersTurn();
    }
		
	public void PlayGame()
    {
		_maxColoredLevels = GetMaxColoredLevels();
		ResetHint();
        Reset();
        _cameraPos = _camera.transform.position;
		_cameraPos.x = 0;
		_cameraPos.y = 0.2f;
        _lastSlotWithMatch.x = -1; // no matches were made
		_camera.transform.position = _cameraPos;
		ShowDarkScreenForce();
		Selection.SetActive(false);

        SetGameState(EGameState.Pause, "PlayGame");

        LevelData levelData = null; //GameManager.Instance.Player.SavedGame; поки без сейва, бо треба сейвити інфу про ворогів на полі і чергу ворогів
		//if (levelData == null || levelData.Slots.Count == 0)
		//{
            //levelData = GameManager.Instance.GameData.StartLevelData;
            int level = 0; //TODO uncomment GameManager.Instance.Player.CreatureMixLevel;
            string path = "CreatureMixLevels/cmlevel_" + level.ToString();
            CreatureMixLevelData cmlevelData = (CreatureMixLevelData)Resources.Load<CreatureMixLevelData>(path);
            if (cmlevelData)
            {
                levelData = LevelData.ConvertToLevelData(cmlevelData, level);
            } else
            {
                levelData = LevelData.GenerateCreatureMixLevel(level);
            }
        //} else
		//{
		//	GameManager.Instance.Player.SavedGame = null;
		//	GameManager.Instance.Settings.Save();
		//}
        // complete aims with level 1
        for (int i = 0; i < levelData.Aims.Count; ++i)
        {
            if (levelData.Aims[i].y == 1)
            {
                Vector3Int aim = levelData.Aims[i];
                aim.z = 1; // complete force
                levelData.Aims[i] = aim;
            }
        }
        //
        StartCoroutine(CreateLevel(levelData));
    }

    //protected LevelData GetLevelToSave()
    //{
    //    LevelData res = new LevelData();
    //	res.timePlayed = TimePlayed;
    //	for (int i = 0; i < Consts.COLORS.Length; ++i)
    //	{
    //		res.Resources[i] = GetResourceAmount(i);
    //	}
    //    // pipes
    //    for (int i = 0; i < WIDTH; ++i)
    //    {
    //        for (int j = 0; j < HEIGHT; ++j)
    //        {
    //            SSlot slotScript = Slots[i, j];
    //			SPipe pipeScript = slotScript.Pipe;
    //			if (pipeScript != null)
    //			{
    //				SSlotData slotData = new SSlotData();
    //				slotData.x = slotScript.X;
    //				slotData.y = slotScript.Y;
    //				slotData.pt = pipeScript.PipeType;
    //				slotData.c = pipeScript.AColor;
    //				slotData.p = pipeScript.Param;
    //				res.Slots.Add(slotData);
    //			}
    //        }
    //    }
    //    // queue state
    //    res.QueueState = AQueuePanel.GetStateToSave();
    //    //
    //    res.ReshufflePowerups = PowerUps[GameData.PowerUpType.Reshuffle];
    //    res.BreakePowerups = PowerUps[GameData.PowerUpType.Breake];
    //    res.ChainPowerups = PowerUps[GameData.PowerUpType.Chain];
    //    res.DestroyColorsPowerups = PowerUps[GameData.PowerUpType.DestroyColor];
    //    res.AddsViewed = AddsViewed;
    //    res.Aims = AAimPanel.GetDataToSave();
    //
    //    for (int i = 0; i < _possibleColors.Count; ++i)
    //    {
    //        res.Colors.Add(_possibleColors[i]);
    //    }
    //    res.AddNewPipes = _addNewPipes;
    //    return res;
    //}

	protected void ShowDarkScreenForce()
    {
//		EventData eventData = new EventData("OnWindowNeededEvent");
//		eventData.Data["name"] = "DarkScreen";
//		eventData.Data["isforce"] = true;
//		GameManager.Instance.EventManager.CallOnWindowNeededEvent(eventData);
	}
	
    protected void ShowDarkScreen()
    {
//		EventData eventData = new EventData("OnWindowNeededEvent");
//		eventData.Data["name"] = "DarkScreen";
//		eventData.Data["isforce"] = false;
//		GameManager.Instance.EventManager.CallOnWindowNeededEvent(eventData);
	}
		
	public GameObject GetPipeFromPool(EPipeType pType, int color = -1)
	{
		int pid = (int)pType;
		string sid = pid.ToString();
        GameObject obj = null;
		if (pType == EPipeType.Colored)
		{
            obj = _pool.GetObjectFromPool("PipeColored", SlotsContainer);
		} else
		{
            obj = _pool.GetObjectFromPool("Pipe_" + sid, SlotsContainer);
		}
		return obj;
	}

    // <-----------

    private void FindMinXToSlide(ref SlideData slideData)
	{
		for (int i = slideData.PosSlideFrom.x - 1; i >= 0; --i)
	    {
			SSlot slot2 = Slots[i, slideData.PosSlideFrom.y];
			SPipe pipe2 = slot2.Pipe;
			if (slot2.IsEmpty())
			{
	
			} else
			{
				slideData.Slot2 = slot2;
				slideData.Pipe2 = pipe2;
				slideData.SlideType = CheckPipesForCooperation(slideData.Pipe, slideData.Pipe2, slideData.Slot2);
				if (slideData.SlideType == ESlideType.Match)
				{
					slideData.FinalPosForSlide.x = i;
					slideData.PosAforMatchSlide.x = slideData.FinalPosForSlide.x + 1;
				} else
				{
					slideData.FinalPosForSlide.x = i + 1;
					slideData.Slot2 = GetSlot(slideData.FinalPosForSlide);
					slideData.Pipe2 = slideData.Slot2.Pipe;
				}
				return;
			}
	    }
		slideData.SlideType = ESlideType.Move;
		slideData.FinalPosForSlide.x = 0;
		slideData.PosAforMatchSlide.x = slideData.FinalPosForSlide.x;
		slideData.Slot2 = GetSlot(slideData.FinalPosForSlide);
		slideData.Pipe2 = slideData.Slot2.Pipe;
	}

	private void FindMaxXToSlide(ref SlideData slideData)
	{
		for (int i = slideData.PosSlideFrom.x + 1; i < WIDTH; ++i)
		{
			SSlot slot2 = Slots[i, slideData.PosSlideFrom.y];
			SPipe pipe2 = slot2.Pipe;
			if (slot2.IsEmpty())
			{

			} else
			{
				slideData.Slot2 = slot2;
				slideData.Pipe2 = pipe2;
				slideData.SlideType = CheckPipesForCooperation(slideData.Pipe, slideData.Pipe2, slideData.Slot2);
				if (slideData.SlideType == ESlideType.Match)
				{
					slideData.FinalPosForSlide.x = i;
					slideData.PosAforMatchSlide.x = slideData.FinalPosForSlide.x - 1;
				} else
				{
					slideData.FinalPosForSlide.x = i - 1;
					slideData.Slot2 = GetSlot(slideData.FinalPosForSlide);
					slideData.Pipe2 = slideData.Slot2.Pipe;
				}
				return;
			}
		}
		slideData.SlideType = ESlideType.Move;
		slideData.FinalPosForSlide.x = WIDTH - 1;
		slideData.PosAforMatchSlide.x = slideData.FinalPosForSlide.x;
		slideData.Slot2 = GetSlot(slideData.FinalPosForSlide);
		slideData.Pipe2 = slideData.Slot2.Pipe;
	}
		
	private void FindMinYToSlide(ref SlideData slideData)
	{
		for (int i = slideData.PosSlideFrom.y - 1; i >= 0; --i)
		{
			SSlot slot2 = Slots[slideData.PosSlideFrom.x, i];
			SPipe pipe2 = slot2.Pipe;
			if (slot2.IsEmpty())
			{

			} else
			{
				slideData.Slot2 = slot2;
				slideData.Pipe2 = pipe2;
				slideData.SlideType = CheckPipesForCooperation(slideData.Pipe, slideData.Pipe2, slideData.Slot2);
				if (slideData.SlideType == ESlideType.Match)
				{
					slideData.FinalPosForSlide.y = i;
					slideData.PosAforMatchSlide.y = slideData.FinalPosForSlide.y + 1;
				} else
				{
					slideData.FinalPosForSlide.y = i + 1;
					slideData.Slot2 = GetSlot(slideData.FinalPosForSlide);
					slideData.Pipe2 = slideData.Slot2.Pipe;
				}
				return;
			}
		}
		slideData.SlideType = ESlideType.Move;
		slideData.FinalPosForSlide.y = 0;
		slideData.PosAforMatchSlide.y = slideData.FinalPosForSlide.y;
		slideData.Slot2 = GetSlot(slideData.FinalPosForSlide);
		slideData.Pipe2 = slideData.Slot2.Pipe;
	}

	private void FindMaxYToSlide(ref SlideData slideData)
	{
		for (int i = slideData.PosSlideFrom.y + 1; i < HEIGHT; ++i)
		{
			SSlot slot2 = Slots[slideData.PosSlideFrom.x, i];
			SPipe pipe2 = slot2.Pipe;
			if (slot2.IsEmpty())
			{

			} else
			{
				slideData.Slot2 = slot2;
				slideData.Pipe2 = pipe2;
				slideData.SlideType = CheckPipesForCooperation(slideData.Pipe, slideData.Pipe2, slideData.Slot2);
				if (slideData.SlideType == ESlideType.Match)
				{
					slideData.FinalPosForSlide.y = i;
					slideData.PosAforMatchSlide.y = slideData.FinalPosForSlide.y - 1;
				} else
				{
					slideData.FinalPosForSlide.y = i - 1;
					slideData.Slot2 = GetSlot(slideData.FinalPosForSlide);
					slideData.Pipe2 = slideData.Slot2.Pipe;
				}
				return;
			}
		}
		slideData.SlideType = ESlideType.Move;
		slideData.FinalPosForSlide.y = HEIGHT - 1;
		slideData.PosAforMatchSlide.y = slideData.FinalPosForSlide.y;
		slideData.Slot2 = GetSlot(slideData.FinalPosForSlide);
		slideData.Pipe2 = slideData.Slot2.Pipe;
	}

	private ESlideType CheckPipesForCooperation(SPipe pipe, SPipe pipe2, SSlot slot2)
	{
		if (!slot2.IsMovable() || pipe.PipeType != EPipeType.Colored)
		{
			// slot not movable or slide not colored pipe (blocker)
			return ESlideType.Move;
		}
		if (pipe.PipeType == EPipeType.Colored)
		{
			// slide color pipe, check type of pipe2
			EPipeType pType2 = pipe2.PipeType;
			if (pipe2.IsCanConsumeColoredPipes())
			{
				// base consum color pipes
				return ESlideType.Consum;
			} else
			{
                if (Consts.FILLER_VARIATION)
                {
                    if (pType2 == EPipeType.Colored && pipe.AColor == pipe2.AColor && _maxColoredLevels > pipe.Param)
                    {
                        // can match them
                        return ESlideType.Match;
                    } else
                    {
                        return ESlideType.Move;
                    }
                } else
                {
                    if (pType2 == EPipeType.Colored && pipe.AColor == pipe2.AColor && _maxColoredLevels > pipe.Param && pipe.Param == pipe2.Param)
                    {
                        // can match them
                        return ESlideType.Match;
                    } else
                    {
                        return ESlideType.Move;
                    }
                }
			}
		}
		return ESlideType.None;
	}

	private SlideData CreateSlideData(SSlot slot, int xDir, int yDir)
	{
		SlideData res;
		res.Slot2 = null;
		res.Pipe2 = null;
		res.Slot = slot;
		res.Pipe = slot.Pipe;
		res.DirX = xDir;
		res.DirY = yDir;
		res.SlideType = ESlideType.None;
		res.PosSlideFrom.x = res.Slot.X;
		res.PosSlideFrom.y = res.Slot.Y;
		res.FinalPosForSlide.x = res.PosSlideFrom.x;
		res.FinalPosForSlide.y = res.PosSlideFrom.y;
		res.PosAforMatchSlide.x = res.FinalPosForSlide.x;
		res.PosAforMatchSlide.y = res.FinalPosForSlide.y;
		res.DistX = 0;
		res.DistY = 0;
        res.NewParam = -1;

		if (res.DirX != 0)
		{
			// try horizontal impulse
			if (res.DirX > 0)
			{
				// impulse to right
				FindMaxXToSlide(ref res);
			} else
			{
				// impulse to left
				FindMinXToSlide(ref res);
			}
		} else
		{
			// try vertical impulse
			if (res.DirY > 0)
			{
				// impulse to top
				FindMaxYToSlide(ref res);
			} else
			{
				// impulse to bottom
				FindMinYToSlide(ref res);
			}
		}
        if (res.SlideType == ESlideType.Match)
        {
            if (Consts.FILLER_VARIATION)
            {
                res.NewParam = Mathf.Min(res.Pipe.Param + res.Pipe2.Param + 1, 9);
            } else
            {
                res.NewParam = res.Pipe.Param + 1;
            }
        }
		// find distances in slots
		res.DistX = Mathf.Abs(res.PosSlideFrom.x - res.FinalPosForSlide.x);
		res.DistY = Mathf.Abs(res.PosSlideFrom.y - res.FinalPosForSlide.y);
		//
		return res;
	}

	public void SlidePipe(SSlot slot, int xDir, int yDir)
    {
        if (slot.Pipe == null)
		{
			// nothing to slide
			return;
		}

        SlideData slideData = CreateSlideData(slot, xDir, yDir);
		if ((slideData.DistX == 0 && slideData.DistY == 0) || slideData.SlideType == ESlideType.None)
		{
			// cant slide
			return;
		}
        // start tutor
        if (_startSequenceState > 0 && ( _startTutorHintData.XA != slot.X || _startTutorHintData.YA != slot.Y || _startTutorHintData.XB != slideData.Slot2.X || _startTutorHintData.YB != slideData.Slot2.Y))
        {
            return;
        } else
        {
            if (_startSequenceState > 0)
            {
                HideHint();
                LeanTween.delayedCall(0.5f, () => { ForceToSwipe(); });
            }
        }
        // slide according to type
        if (slideData.SlideType == ESlideType.Match)
		{
			// match
			SlidePipeWithMatch(slideData);
		} else
		{
			// no match
			SlidePipeWithoutMatch(slideData);
		}
        ResetHint();
        ResetTutor2Timer();
	}

	private void SlidePipeWithMatch(SlideData slideData)
	{
		slideData.Slot2.WaitForPipe = true;
		SPipe pipe = slideData.Slot.TakePipe();
		Helpers.SetZ(pipe.transform, PipeZForMatch);
		// match
		if (slideData.DirX != 0)
		{
			float xPos = GetSlot(slideData.PosAforMatchSlide).transform.position.x;
			xPos += slideData.DirX * Consts.EXTRA_DX_DY_WHEN_MATCHING;
			float atime2 = (slideData.DistX - 1) * Consts.IMPULSE_SPEED;
            if (atime2 == 0.0f)
            {
                atime2 = 0.05f;
            }
            LeanTween.moveX(pipe.gameObject, xPos, atime2)
				.setOnComplete(() =>
					{
						OnPipeArrivedToSlotWithMatch(slideData);
					});
		} else
		//if (slideData.DirY != 0)
		{
			float yPos = GetSlot(slideData.PosAforMatchSlide).transform.position.y;
			yPos += slideData.DirY * Consts.EXTRA_DX_DY_WHEN_MATCHING;
			float atime2 = (slideData.DistY - 1) * Consts.IMPULSE_SPEED;
            if (atime2 == 0.0f)
            {
                atime2 = 0.05f;
            }
			LeanTween.moveY(pipe.gameObject, yPos, atime2)
				.setOnComplete(() =>
					{
						OnPipeArrivedToSlotWithMatch(slideData);
					});
		}
	}

	private void SlidePipeWithoutMatch(SlideData slideData)
	{
		slideData.Slot2.WaitForPipe = true;
		SPipe pipe = slideData.Slot.TakePipe();
		float atime = Consts.IMPULSE_SPEED * slideData.DistX + Consts.IMPULSE_SPEED * slideData.DistY;
		pipe.SetXY(slideData.FinalPosForSlide.x, slideData.FinalPosForSlide.y);
		if (slideData.DirX != 0)
		{
			// horizontal slide
			float xPos = slideData.Slot2.transform.position.x;
			LeanTween.moveX(pipe.gameObject, xPos, atime)
				.setOnComplete(() =>
					{
						OnPipeArrivedToSlotWithoutMatch(slideData);
					});
		} else
		if (slideData.DirY != 0)
		{
			// vertical slide
			float yPos = slideData.Slot2.transform.position.y;
			LeanTween.moveY(pipe.gameObject, yPos, atime)
				.setOnComplete(() =>
					{
						OnPipeArrivedToSlotWithoutMatch(slideData);
					});
		}
	}

	private void OnPipeArrivedToSlotWithMatch(SlideData slideData)
	{
        SSlot slot2 = slideData.Slot2;
        _lastSlotWithMatch = new BoardPos(slot2.X, slot2.Y);
        SPipe pipe2 = slideData.Pipe2;
		SPipe pipe = slideData.Pipe;
//		// rotate pipe
//		if (slideData.DirX != 0)
//		{
//			// horizontal move
//			if (slideData.DirX  < 0)
//			{
//				LeanTween.rotateLocal(pipe.CubeObject, new Vector3(pipe.CubeObject.transform.localEulerAngles.x, pipe.CubeObject.transform.localEulerAngles.y + 180, 0), rotateTime)
//					.setEase(LeanTweenType.easeOutSine);
//			} else
//			{
//				LeanTween.rotateLocal(pipe.CubeObject, new Vector3(pipe.CubeObject.transform.localEulerAngles.x, pipe.CubeObject.transform.localEulerAngles.y - 180, 0), rotateTime)
//					.setEase(LeanTweenType.easeOutSine);
//			}
//		} else
//		{
//			// vertical move
//			if (slideData.DirY < 0)
//			{
//				LeanTween.rotateLocal(pipe.CubeObject, new Vector3(pipe.CubeObject.transform.localEulerAngles.x + 180, pipe.CubeObject.transform.localEulerAngles.y, 0), rotateTime)
//					.setEase(LeanTweenType.easeOutSine);
//			} else
//			{
//				//LeanTween.rotateX(pipe.CubeObject, 180.0f, rotateTime)
//					//.setEase(LeanTweenType.easeOutSine);
//				LeanTween.rotateLocal(pipe.CubeObject, new Vector3(pipe.CubeObject.transform.localEulerAngles.x - 180, pipe.CubeObject.transform.localEulerAngles.y, 0), rotateTime)
//					.setEase(LeanTweenType.easeOutSine);
//			}
//		}

		// rase value and rotate animation
        slideData.Pipe.RaseCombineAnimation(slideData.NewParam, slideData.DirX, slideData.DirY);
		
        // points
        int multiplyer = 1;
        if (GameManager.Instance.Player.SlotsDoubles.Contains(new Vector2(slot2.X, slot2.Y)))
        {
            multiplyer = 2;
        }
        //LeanTween.delayedCall(Consts.MATCH_ROTATE_TIME, () =>
        //{
            AddResourceByLevelOfColoredPipe(pipe.Param, pipe.AColor, multiplyer, slot2.transform.position);
        //});
        // move new pipe to center of new slot
        Vector2 newPos = slot2.transform.position;
		LeanTween.move(pipe.gameObject, newPos, Consts.MATCH_ROTATE_TIME)
            .setEase(LeanTweenType.easeOutSine)
			.setOnComplete(()=>{
				Vector3 pos = slideData.Slot2.transform.position;
				pos.z = PipeZ;
				slideData.Pipe.transform.position = newPos;
				OnPipeLandAfterMatch(slideData);
			});
        //// move prev pipe slightly
        ////Vector3 prevPos = pipe2.transform.position;
        ////prevPos.z += 0.05f;
        ////LeanTween.move(pipe2.gameObject, new Vector3(prevPos.x + slideData.DirX * Consts.DX_DY_OF_PIPE_WHEN_PIPE_BUMPS_INTO_IT, prevPos.y + slideData.DirY * Consts.DX_DY_OF_PIPE_WHEN_PIPE_BUMPS_INTO_IT, prevPos.z), Consts.MATCH_ROTATE_TIME / 4.0f)
        ////	.setEase(LeanTweenType.easeOutSine)
        ////	.setOnComplete(() =>
        ////		{
        ////			LeanTween.move(pipe2.gameObject, prevPos, Consts.MATCH_ROTATE_TIME / 4.0f);
        ////		});
        //// bump

//        GameObject coloredObj = pipe2.RotateObject.transform.Find("Color_0").gameObject;
//        Pipe_Colored coloredPipe2 = (Pipe_Colored)pipe2;
//        Renderer pipe2Rend = coloredObj.GetComponent<Renderer>();
//        pipe2Rend.material = new Material(pipe2Rend.material);
//        Color prevColor = pipe2Rend.material.color;
//        LeanTween.value(pipe2.gameObject, pipe2Rend.material.color, new Color(0.4f, 0.4f, 0.4f), Consts.MATCH_ROTATE_TIME)
//                 .setDelay(0.05f)
//                 .setEase(LeanTweenType.easeOutSine)
//                 .setOnUpdate((Color col) =>
//                {
//                    pipe2Rend.material.color = col;
//                })
//                 .setOnComplete(() =>
//               {
//                   pipe2Rend.material.color = prevColor;
//               });
//        LeanTween.value(pipe2.gameObject, Color.white, Color.black, Consts.MATCH_ROTATE_TIME)
//                 .setDelay(0.05f)
//                 .setEase(LeanTweenType.easeOutSine)
//                 .setOnUpdate((Color col) =>
//                {
//                    coloredPipe2.SymbolSprites[0].color = col;
//                    coloredPipe2.SymbolSprites[1].color = col;
//                })
//                 .setOnComplete(() =>
//               {
//                    coloredPipe2.SymbolSprites[0].color = Color.white;
//                    coloredPipe2.SymbolSprites[1].color = Color.white;
//               });
        

		if (Consts.BUMP_ON_MATCH)
		{
			Bump(slideData);
		}
    }

	private void OnPipeLandAfterMatch(SlideData slideData)
	{
		MusicManager.playSound("Match");//"chip_hit"
		slideData.Pipe2.RemoveCombineAnimation();
		EventData eventData = new EventData("OnCombineWasMadeEvent");
		eventData.Data["acolor"] = slideData.Pipe.AColor;
		eventData.Data["double"] = slideData.Slot2.IsDoubleSlot;
		eventData.Data["param"] = slideData.Pipe.Param;
		slideData.Slot2.SetPipe(slideData.Pipe);
		if (slideData.Pipe.Param == _maxColoredLevels - 1 && Consts.MAX_COLORED_LEVEL_REMOVES)
		{
            // reached max pipe             
            BreakePipeInSlot(slideData.Slot2, (slideData.Pipe as Pipe_Colored).GetExplodeEffectPrefab()); //BreakeEffectPrefab);
            if (Consts.BAD_PIXEL_MACHANIC_IN_CLASSIC_GAME)
            {
                SPipe bPipe = GetPipeFromPool(EPipeType.Hole).GetComponent<SPipe>();
                bPipe.InitPipe(0, -1, false);
                slideData.Slot2.SetPipe(bPipe);
                bPipe.PlayAddAnimation();
            }
            EventData eventData2 = new EventData("OnReachMaxPipeLevelEvent");
            eventData2.Data["x"] = slideData.Pipe.transform.position.x;
            eventData2.Data["y"] = slideData.Pipe.transform.position.y;
            GameManager.Instance.EventManager.CallOnReachMaxPipeLevelEvent(eventData2);



            //			Vector3 pos = ConvertPositionFromLocalToScreenSpace(slideData.Pipe.transform.position);
            //			EventData e = new EventData("OnShowAddResourceEffect");
            //			e.Data["x"] = pos.x;
            //			e.Data["y"] = pos.y;
            //			e.Data["screenpos"] = pos;
            //
        } else
		{
            // players simple attack
            CreateSimpleAttack(slideData.Slot2, slideData.Pipe.AColor, slideData.NewParam);

            //GameObject effect = (GameObject)GameObject.Instantiate(MatchEffectPrefab, Vector3.zero, Quaternion.identity);
            //effect.transform.SetParent(SlotsContainer, false);
            //Vector3 pos = slideData.Slot2.transform.position;
            //pos.z = PipeZ + 0.05f;
            //effect.transform.position = pos;
            //GameObject.Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);
        }
		GameManager.Instance.EventManager.CallOnCombineWasMadeEvent(eventData);
		//
		slideData.Slot2.WaitForPipe = false;
		OnTurnWasMade(true, false);
	}

	private void OnPipeArrivedToSlotWithoutMatch(SlideData slideData)
	{
		// see how react with new slot
		if (slideData.SlideType == ESlideType.Move)
		{
			slideData.Slot2.SetPipe(slideData.Pipe);
			// bump
			Bump(slideData);
		} else
		//if (slideData.SlideType == ESlideType.Consum)
		{
			//if (slideData.Pipe2.IsCanConsumeColoredPipes())
			//{
				// play consum animation
				slideData.Pipe.BaseConsumAnimation(slideData.Pipe.Param, slideData.Pipe.AColor);
				slideData.Pipe.RemoveConsumAnimation();
			//}
		}
		slideData.Slot2.WaitForPipe = false;
		OnTurnWasMade(false, false);
	}

	private void Bump(SlideData slideData)
	{
		if (slideData.DistX != 0)
		{
			// horizontal bump
			float horizontalBump = slideData.DirX * Consts.BUMP_PER_SLOT * slideData.DistX + Consts.BUMP_EXTRA * slideData.DirX;
			BumpCameraHorizontal(horizontalBump, Consts.BUMP_TIME);
            if (slideData.Slot2.X == 0 || slideData.Slot2.X == WIDTH - 1)
            {
                AddManaForBump(slideData.Slot2, slideData.Pipe, slideData.DistX);
            }
		} else
		{
			// vertical bump
			float verticalBump = slideData.DirY * Consts.BUMP_PER_SLOT * slideData.DistY + Consts.BUMP_EXTRA * slideData.DirY;
			BumpCameraVertical(verticalBump, Consts.BUMP_TIME);
            if (slideData.Slot2.Y == 0 || slideData.Slot2.Y == HEIGHT - 1)
            {
                AddManaForBump(slideData.Slot2, slideData.Pipe, slideData.DistY);
            }
        }
	}
	
//	protected void DragedPipeToFinger()
//    {
//        Vector3 pos = m_drag.transform.position;
//        pos.x = m_dragDxDy.x + m_downGamePos.x;
//        pos.y = m_dragDxDy.y + m_downGamePos.y;
//        m_drag.transform.position = pos;
//    }
	
	public void ShowSelection(Vector2 pos)
    {
        Selection.SetActive(true);
		Helpers.SetXY(Selection, pos);
    }

    public void HideSelection()
    {
		Selection.SetActive(false);
    }

	private SSlot TryGetSlot(int x, int y)
	{
		if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT)
		{
			return null;
		} else
		{
			return Slots[x, y];
		}
	}

	private List<SSlot> GetEmptySSlotsNearPos(int x, int y)
	{
		List<SSlot> res = new List<SSlot>();
		// from left
		SSlot slot0 = TryGetSlot(x - 1, y);
		if (slot0.IsEmpty()) { res.Add(slot0); }
		// from right
		SSlot slot1 = TryGetSlot(x + 1, y);
		if (slot1.IsEmpty()) { res.Add(slot1); }
		// from bottom
		SSlot slot2 = TryGetSlot(x, y - 1);
		if (slot2.IsEmpty()) { res.Add(slot2); }
		// from top
		SSlot slot3 = TryGetSlot(x, y + 1);
		if (slot3.IsEmpty()) { res.Add(slot3); }
		return res;
	}

	public int GeneratePipesNearSlot(int x, int y)
	{
		List<SSlot> slots = GetEmptySSlotsNearPos(x, y);
		for (int i = 0; i < slots.Count; ++i)
		{
			// add colored pipe to slot
			int color = GetRandomColor();
			SPipe cPipe = GetPipeFromPool(EPipeType.Colored, color).GetComponent<SPipe>();
			cPipe.InitPipe(0, color, false);
			slots[i].SetPipe(cPipe);
			cPipe.PlayAddAnimation();
		}
		return slots.Count;
	}

    //without queue
    //public void AddRandomPipe(bool needBlocker)
    //{
    //	List<SSlot> slots = GetEmptySSlots();
    //	if (slots.Count == 0)
    //	{
    //           Debug.LogError("NO FREE SLOT!");
    //       } else
    //	{
    //		SSlot slot = slots[UnityEngine.Random.Range(0, slots.Count)];
    //           if (needBlocker)
    //           {
    //               // add blocker
    //               SPipe bPipe = GetPipeFromPool(EPipeType.Blocker).GetComponent<SPipe>();
    //               bPipe.InitPipe(0, -1, false);
    //               slot.SetPipe(bPipe);
    //               bPipe.PlayAddAnimation();
    //           }
    //           else
    //           {
    //			// add colored pipe to slot
    //			SPipe cPipe = GetPipeFromPool(EPipeType.Colored).GetComponent<SPipe>();
    //			cPipe.InitPipe(0, GetRandomColor(), false);
    //			slot.SetPipe(cPipe);
    //			cPipe.PlayAddAnimation();
    //		}
    //	}
    //       if (slots.Count <= 1)
    //       {
    //           bool movesExists = CheckIfCanMatchSomethingInTheEnd();
    //           if (!movesExists)
    //           {
    //               //TODO we loose, send signal
    //               Debug.LogError("We Loose! Send Signal!");
    //               SetGameState(EGameState.Loose);
    //               MusicManager.playSound("Fart_1");
    //               Invoke("ToMainMenu", 4.0f);
    //           }
    //       }
    //   }

    public bool AddRandomPipe(EPipeType newPipeType)
    {
        List<SSlot> slots = GetEmptySSlots();
        if (slots.Count == 0)
        {
            Debug.LogError("NO FREE SLOT!");
            return false;
        }
        else
        {
            // get info about first pipe from queue
            EPipeType pipeType = AQueuePanel.GetNextType();
            int acolor = AQueuePanel.GetNextColor();
            int aparam = AQueuePanel.GetNextParam();
            // add new pipe to queue
            if (newPipeType == EPipeType.Blocker)
            {
                AQueuePanel.MoveQueue(newPipeType, -1, 0);
            } else
            if (newPipeType == EPipeType.Colored)
            {
                AQueuePanel.MoveQueue(newPipeType, GetRandomColor(), UnityEngine.Random.Range(0, Consts.MAX_COLORED_LEVEL_IN_QUEUE + 1));
            }
            //
            SSlot slot = slots[UnityEngine.Random.Range(0, slots.Count)];
            if (pipeType == EPipeType.Blocker)
            {
                // add blocker
                SPipe bPipe = GetPipeFromPool(EPipeType.Blocker).GetComponent<SPipe>();
                bPipe.InitPipe(0, -1, false);
                slot.SetPipe(bPipe);
                bPipe.PlayAddAnimation();
            } else
            if (pipeType == EPipeType.Colored)
            {
                // add colored pipe to slot
				SPipe cPipe = GetPipeFromPool(EPipeType.Colored, acolor).GetComponent<SPipe>();
                cPipe.InitPipe(aparam, acolor, false);
                slot.SetPipe(cPipe);
                cPipe.PlayAddAnimation();
            }
        }
        CheckIfOutOfMoves();
        return true;
    }

    public void ToMainMenu()
    {
        GameManager.Instance.GameFlow.TransitToScene(UIConsts.SCENE_ID.MAINMENU);
    }

    private bool CheckIfCanMatchSomethingInTheEnd()
    {
        // check if can match something when all board filled with pipes
        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                SPipe pipe = Slots[i, j].Pipe;
                if (pipe.PipeType == EPipeType.Colored)
                {
                    // check right
                    int ii = i + 1;
                    if (ii < WIDTH)
                    {
                        SPipe pipe2 = Slots[ii, j].Pipe;
                        if (pipe2.PipeType == EPipeType.Colored)
                        {
                            if (pipe.AColor == pipe2.AColor && pipe.Param == pipe2.Param && pipe.Param < _maxColoredLevels)
                            {
                                Debug.Log("You can match ---> " + pipe2.X + " / " + pipe2.Y + "  ...  " + +pipe.X + " / " + pipe.Y);
                                return true;
                            }
                        }
                    }
                    // check top
                    int jj = j + 1;
                    if (jj < HEIGHT)
                    {
                        SPipe pipe2 = Slots[i, jj].Pipe;
                        if (pipe2.PipeType == EPipeType.Colored)
                        {
                            if (pipe.AColor == pipe2.AColor && pipe.Param == pipe2.Param && pipe.Param < _maxColoredLevels)
                            {
                                Debug.Log("You can match ---> " + pipe2.X + " / " + pipe2.Y + "  ...  " + +pipe.X + " / " + pipe.Y);
                                return true;
                            }
                        }
                    }
                }

            }
        }
        // check powerups
        if (APowerupsPanel.IsCanApply())
        {
            // show notification
            EventData eventData = new EventData("OnShowNotificationEvent");
            eventData.Data["type"] = GameNotification.NotifyType.UsePowerup;
            GameManager.Instance.EventManager.CallOnShowNotificationEvent(eventData);
            return true;
        }
        return false;
    }

    public List<SSlot> GetEmptySSlots()
    {
        List<SSlot> slots = new List<SSlot>();
        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                if (Slots[i, j].IsEmpty())
                {
                    slots.Add(Slots[i, j]);
                }
            }
        }
        return slots;
    }

    public void BumpCameraHorizontal(float xpower, float time)
	{
        if (!GameManager.Instance.Player.Bump)
        {
            return;
        }
        xpower *= -1; // якщо шатаємо не камеру, а гейм обжект - інвертуємо і збільшуємо силу
        // shake it
        MusicManager.playSound("chip_hit");
        LeanTween.cancel(BumpShakeObject);
        LeanTween.value(BumpShakeObject, 0.0f, 1.0f, time)
			.setLoopPingPong(1)
	        //.setEase(UIConsts.SHOW_EASE)
		    //.setDelay(UIConsts.SHOW_DELAY_TIME)
			.setOnUpdate
			(
				(float val)=>
				{
					_shakeDx = val * xpower;
				}
			).setOnComplete
			(
                () =>
				{
					_shakeDx = 0;
					_shakeDy = 0;
				}
			);
	}

	public void BumpCameraVertical(float ypower, float time)
	{
        if (!GameManager.Instance.Player.Bump)
        {
            return;
        }
        ypower *= -1; // якщо шатаємо не камеру, а гейм обжект - інвертуємо і збільшуємо силу
        // shake it
        MusicManager.playSound("chip_hit");
        LeanTween.cancel(BumpShakeObject);
		LeanTween.value(BumpShakeObject, 0.0f, 1.0f, time)
			.setLoopPingPong(1)
			//.setEase(UIConsts.SHOW_EASE)
			//	.setDelay(UIConsts.SHOW_DELAY_TIME)
			.setOnUpdate
			(
				(float val)=>
				{
					_shakeDy = val * ypower;
				}
			).setOnComplete
			(
				() =>
				{
					_shakeDx = 0;
					_shakeDy = 0;
				}
			);
	}

	public void ShakeCamera(float xpower, float ypower, float time)
	{
        if (!GameManager.Instance.Player.Shake)
        {
            return;
        }
        // shake it
        float doublePowerX = xpower * 2;
		float doublePowerY = ypower * 2;
		LeanTween.cancel(BumpShakeObject);
		LeanTween.value(BumpShakeObject, 0, 1, time)
			//.setEase(UIConsts.SHOW_EASE)
			//	.setDelay(UIConsts.SHOW_DELAY_TIME)
			.setOnUpdate
				(
					(float val)=>
					{
						if (xpower > 0)
						{
							_shakeDx = UnityEngine.Random.Range(0, doublePowerX) - xpower;
						}
						if (ypower > 0)
						{
							_shakeDy = UnityEngine.Random.Range(0, doublePowerY) - ypower;
						}
					}
				).setOnComplete
				(
				() =>
				{
					_shakeDx = 0;
					_shakeDy = 0;
				}
			);
	}

	public void BreakePipeInSlot(SSlot slot, GameObject prefab) //, Vector3 startEffectPos)
	{
        MusicManager.playSound("chip_destroy");
        SPipe pipe = slot.TakePipe();
        // explosion effect
        GameObject effect = (GameObject)GameObject.Instantiate(prefab, Vector3.zero, prefab.transform.rotation);
        effect.transform.SetParent(SlotsContainer, false);
        effect.transform.position = slot.transform.position;
        effect.SetActive(true);
        GameObject.Destroy(effect, 3.0f);
        //
        pipe.RemoveConsumAnimation();
	}

    public void OnChainPowerupUsed(SSlot slot)
    {
        SetGameState(EGameState.PlayerUsedPowerup, "OnChainPowerupUsed");
		//
		EventData eventData = new EventData("OnPowerUpUsedEvent");
		eventData.Data["type"] = GameData.PowerUpType.Chain;
		GameManager.Instance.EventManager.CallOnPowerUpUsedEvent(eventData);
		//

        _chainInfos.Clear();
        _checkedChainInfos.Clear();
        ChainInfo info = new ChainInfo();
        info.X = slot.X;
        info.Y = slot.Y;
        info.Color = slot.Pipe.AColor;
        info.Param = slot.Pipe.Param;
        info.PipeType = slot.Pipe.PipeType;
        info.Id = info.X * 1000 + info.Y;
        _chainInfos.Add(info.Id, info);
        RemoveChainIteration();
    }

    private void ChainDestroyPipeAtInfo(ChainInfo info)
    {
        SSlot slot = Slots[info.X, info.Y];
        BreakePipeInSlot(slot, ChainEffectPrefab);
    }

    private void RemoveChainIteration()
    {
        List<int> dxs = new List<int>() { -1, 0, 1, 0 };
        List<int> dys = new List<int>() { 0, 1, 0, -1 };

        List<ChainInfo> tempList = new List<ChainInfo>(); 
        foreach (var chInfo in _chainInfos)
        {
            ChainInfo info = chInfo.Value;
            // breake it
            ChainDestroyPipeAtInfo(info);
            tempList.Add(info);
            _checkedChainInfos.Add(info.Id, info);
        }
        // try add next wave (neighbours)
        _chainInfos.Clear();
        foreach (var info in tempList)
        {
            for (int i = 0; i < 4; ++i)
            {
                int ax = info.X + dxs[i];
                int ay = info.Y + dys[i];
                int akey = ax * 1000 + ay;
                if (!_checkedChainInfos.ContainsKey(akey) && !_chainInfos.ContainsKey(akey))
                {
                    if (IsSlotInBoard(ax, ay))
                    {
                        SPipe pipe = Slots[ax, ay].Pipe;
                        if ((pipe != null) && (info.PipeType == pipe.PipeType) && (info.PipeType == EPipeType.Blocker || info.Color == pipe.AColor || (info.Param == pipe.Param)))
                        {
                            ChainInfo newInfo = new ChainInfo();
                            newInfo.X = ax;
                            newInfo.Y = ay;
                            newInfo.Color = pipe.AColor;
                            newInfo.Param = pipe.Param;
                            newInfo.PipeType = pipe.PipeType;
                            newInfo.Id = akey;
                            _chainInfos.Add(newInfo.Id, newInfo);
                        }
                    }
                }   
            }
        }
        tempList.Clear();

        if (_chainInfos.Count == 0)
        {
            _checkedChainInfos.Clear();
            // if no pipes left - add new pipe on board without move counting
            if (GetMovablePipesCount() == 0)
            {
                OnTurnWasMade(false, true);
            } else
            {
                SetGameState(EGameState.PlayersTurn, "Chain powerup completed");
            }
        } else
        {
            Invoke("RemoveChainIteration", Consts.PU__CHAIN_TIME_PER_ITERATION);
        }
    }

    public void OnDestroyColorPowerupUsed(SSlot slot)
    {
        SetGameState(EGameState.PlayerUsedPowerup, "OnDestroyColorPowerupUsed");
        // find all pipes with this color
        int colorToDestroy = slot.Pipe.AColor;
        List<SSlot> slots = new List<SSlot>();
        slots.Add(slot);
        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                SSlot aslot = Slots[i, j];
                SPipe apipe = aslot.Pipe;
                if (apipe != null && apipe.AColor == colorToDestroy && aslot != slot)
                {
                    slots.Add(aslot);
                }
            }
        }
        // destroy this pipes
        for (int i = 0; i < slots.Count; ++i)
        {
            SSlot dSlot = slots[i];
            LeanTween.delayedCall(dSlot.gameObject, i * Consts.PU__DESTROY_COLOR_TIME_PER_ITERATION, () => { BreakePipeInSlot(dSlot, (dSlot.Pipe as Pipe_Colored).GetExplodeEffectPrefab());  }); //BreakePipeInSlot(dSlot, BreakeEffectPrefab);
        }
        LeanTween.delayedCall(gameObject, slots.Count * Consts.PU__DESTROY_COLOR_TIME_PER_ITERATION, () => 
            {
                // if no pipes left - add new pipe on board without move counting
                if (GetMovablePipesCount() == 0)
                {
                     OnTurnWasMade(false, true);
                } else
                {
                    SetGameState(EGameState.PlayersTurn, "DestroyColorsPowerupciompleted");
                }
                //
            });
        slots.Clear();
        //
        EventData eventData = new EventData("OnPowerUpUsedEvent");
        eventData.Data["type"] = GameData.PowerUpType.DestroyColor;
        GameManager.Instance.EventManager.CallOnPowerUpUsedEvent(eventData);
    }

    public int GetPipesCount()
    {
        int res = 0;
        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                SSlot slot = Slots[i, j];
                SPipe pipe = slot.Pipe;
                if (pipe != null)
                {
                    ++res;
                }
            }
        }
        return res;
    }

    public int GetMovablePipesCount()
    {
        int res = 0;
        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                SSlot slot = Slots[i, j];
                SPipe pipe = slot.Pipe;
                if (pipe != null && slot.IsMovable() && pipe.IsMovable())
                {
                    ++res;
                }
            }
        }
        return res;
    }

    public bool IsMoreThenOnePipeLeft()
    {
        int count = 0;
        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                SSlot slot = Slots[i, j];
                SPipe pipe = slot.Pipe;
                if (pipe != null)
                {
                    ++count;
                    if (count == 2)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void UpdateSkin()
    {
        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                SPipe pipe = Slots[i, j].Pipe;
                if (pipe != null)
                {
					if (pipe.PipeType == EPipeType.Hole)
					{
						Slots[i, j].SetAsHole();
					} else
					{
                    	pipe.UpdateSkin();
					}
                }
            }
        }

        if (ASequencePanel != null)
        {
            ASequencePanel.UpdateSkins();
        }
        AQueuePanel.UpdateSkins();
    }
		

	public void RestartGame()
	{
	    PlayGame();
    }

	public void GoHome()
	{
        SaveGame();
		//MusicManager.PlayMainMenuTrack();
		GameManager.Instance.CurrentMenu = UISetType.MainMenu;
		EventData eventData = new EventData("OnUISwitchNeededEvent");
		eventData.Data["setid"] = UISetType.MainMenu;
		GameManager.Instance.EventManager.CallOnUISwitchNeededEvent(eventData);
		GameMenuUIController.UpdatePlayEndlessButton();
	}

    public void SaveGame()
    {
        GameManager.Instance.Player.SavedGame = null; // поки без сейвів (треба реалізація сейва/лоада стану ворогів на полі)
        //if (GameType == EGameType.Classic)
        //{
        //	if (GameState != EGameState.Loose)
        //  	{
        //  	    GameManager.Instance.Player.SaveLastGame(GetLevelToSave());
        //  	}
        //  	else
        //  	{
        //  	    GameManager.Instance.Player.SavedGame = null;
        //  	}
        //}
    }

    public bool CheckIfOutOfMoves()
    {
        List<SSlot> slots = GetEmptySSlots();
        if (slots.Count == 0)
        {
            bool movesExists = CheckIfCanMatchSomethingInTheEnd();
            if (!movesExists)
            {
                OnLoose();
                return true;
            }
        }
        return false;
    }

    public void OnLoose()
    {
        Debug.Log("You Loose!");
        MusicManager.playSound("level_lost");
        SetGameState(EGameState.Loose, "OnLoose");
        GameManager.Instance.Player.SavedGame = null;
        LeanTween.delayedCall(1.0f, () => {
            //EventData eventData = new EventData("OnOpenFormNeededEvent");
            //eventData.Data["form"] = UIConsts.FORM_ID.STATISTIC_WINDOW;
            //GameManager.Instance.EventManager.CallOnOpenFormNeededEvent(eventData);
            PlayGame();
        });
    }

	private List<MatchHintData> FindPossibleHints(bool withoutMatchToo)
	{
		List<MatchHintData> possibleHints = new List<MatchHintData>();
		for (int i = 0; i < WIDTH; ++i)
		{
			for (int j = 0; j < HEIGHT; ++j)
			{
				int xB = -1;
				int yB = -1;
				SPipe pipe = Slots[i, j].Pipe;
				if (pipe != null && pipe.IsMovable())
				{
					// try slide left
					xB = i;
					for (int xx = i - 1; xx >= 0; --xx)
					{
						SPipe pipeB = Slots[xx, j].Pipe;
						if (pipeB != null)
						{
							if (!CheckPipesforHint(pipe, pipeB, i, j, xx, j, ref possibleHints))
							{
								xB = i;
							}
							break;
						} else
						{
							xB = xx;
						}
					}
					if (withoutMatchToo && xB != i)
					{
						AddPossibleHintWithoutMatch(i, j, xB, j, ref possibleHints);
					}
					// try slide right
					xB = i;
					for (int xx = i + 1; xx < WIDTH; ++xx)
					{
						SPipe pipeB = Slots[xx, j].Pipe;
						if (pipeB != null)
						{
							if (!CheckPipesforHint(pipe, pipeB, i, j, xx, j, ref possibleHints))
							{
								xB = i;
							}
							break;
						} else
						{
							xB = xx;
						}
					}
					if (withoutMatchToo && xB != i)
					{
						AddPossibleHintWithoutMatch(i, j, xB, j, ref possibleHints);
					}
					// try slide up
					yB = j;
					for (int yy = j + 1; yy < HEIGHT; ++yy)
					{
						SPipe pipeB = Slots[i, yy].Pipe;
						if (pipeB != null)
						{
							if (!CheckPipesforHint(pipe, pipeB, i, j, i, yy, ref possibleHints))
							{
								yB = j;
							}
							break;
						} else
						{
							yB = yy;
						}
					}
					if (withoutMatchToo && yB != j)
					{
						AddPossibleHintWithoutMatch(i, j, i, yB, ref possibleHints);
					}
					//try slide down
					yB = j;
					for (int yy = j - 1; yy >= 0; --yy)
					{
						SPipe pipeB = Slots[i, yy].Pipe;
						if (pipeB != null)
						{
							if (!CheckPipesforHint(pipe, pipeB, i, j, i, yy, ref possibleHints))
							{
								yB = j;
							}
							break;
						} else
						{
							yB = yy;
						}
					}
					if (withoutMatchToo && yB != j)
					{
						AddPossibleHintWithoutMatch(i, j, i, yB, ref possibleHints);
					}
				}
			}
		}
		return possibleHints;
	}

	private bool CheckPipesforHint(SPipe pipeA, SPipe pipeB, int xA, int yA, int xB, int yB, ref List<MatchHintData> possibleHints)
	{
        if (!pipeA.IsColored() || !pipeB.IsColored())
        {
            return false;
        }
		if (pipeA.AColor == pipeB.AColor && pipeA.Param == pipeB.Param && pipeA.Param < _maxColoredLevels)
		{
			MatchHintData mhData;
			mhData.XA = xA;
			mhData.YA = yA;
			mhData.XB = xB;
			mhData.YB = yB;
            mhData.IsMatch = true;
			possibleHints.Add(mhData);
			return true;
		}
		return false;
	}

	private void AddPossibleHintWithoutMatch(int xA, int yA, int xB, int yB, ref List<MatchHintData> possibleHints)
	{
		MatchHintData mhData;
		mhData.XA = xA;
		mhData.YA = yA;
		mhData.XB = xB;
		mhData.YB = yB;
		mhData.IsMatch = false;
		possibleHints.Add(mhData);
	}

	private void TryShowHint()
	{
        if (GameManager.Instance.GameFlow.IsSomeWindow())
        {
            return;
        }
        if (_hintTimer <= 0)
        {
            return;
        }
		_hintTimer -= Time.deltaTime;
		if (_hintTimer <= 0)
		{
			//ResetHint(true); // to show random hints
			List<MatchHintData> mhData = FindPossibleHints(false);
			if (mhData.Count > 0)
			{
				//show random hint
				MatchHintData data = mhData[UnityEngine.Random.Range(0, mhData.Count)];
				GameObject obj = (GameObject)GameObject.Instantiate(HintPrefab, Vector3.zero, Quaternion.identity);
				_hint = obj.GetComponent<NewHintScript>();
				_hint.ShowHint(data, this);
			}
		}
	}


    //TUTOR_2
    private void TryShowTutor2()
    {
        if (_tutor2Timer <= 0 || GameManager.Instance.GameFlow.IsSomeWindow())
        {
            return;
        }
        //_tutor2Timer -= Time.deltaTime;
        //if (_tutor2Timer <= 0)
        //{
        //    if (!GameManager.Instance.Player.IsTutorialShowed("2"))
        //    {
        //        for (var powerup = GameData.PowerUpType.Reshuffle; powerup <= GameData.PowerUpType.DestroyColor; ++powerup)
        //        {
        //            if (PowerUps[powerup] > 0) // && GameManager.Instance.Player.PowerUpsState.ContainsKey(powerup) && GameManager.Instance.Player.PowerUpsState[powerup].Level > 0)
        //            {
        //                GameManager.Instance.ShowTutorial("2", new Vector3(0, 0, 0));
        //                return;
        //            }
        //        }
        //    }
        //}
    }

    private void ResetHint(bool resetByTime = false)
	{
		if (_hint != null)
		{
            HideHint();
            if (resetByTime)
			{
				_hintTimer = Consts.HINT_DELAY / 3.0f;
			} else
			{
				_hintTimer = Consts.HINT_DELAY;
			}
		} else
		{
			_hintTimer = Consts.HINT_DELAY;
		}
	}

    //TUTOR_2
    private void ResetTutor2Timer()
    {
        if (!GameManager.Instance.Player.IsTutorialShowed("2"))
        {
            _tutor2Timer = Consts.TUTOR_2_DELAY;
        }
    }

    public Vector3 ConvertPositionFromLocalToScreenSpace(Vector3 localPos)
    {
        Vector3 screenPos = _camera.WorldToViewportPoint(localPos);
        RectTransform canvasRect = ACanvas.GetComponent<RectTransform>();
        screenPos.x *= canvasRect.rect.width;
        screenPos.y *= canvasRect.rect.height;
        screenPos.x -= canvasRect.rect.width * canvasRect.pivot.x;
        screenPos.y -= canvasRect.rect.height * canvasRect.pivot.y;
        return screenPos;
    }

	public Sprite GetSprite(string id)
	{
		Sprite res = null;
		if (_sprites.TryGetValue(id, out res))
		{
			return res;
		}
        string path = "art\\Pipes\\" + id;
        res = Resources.Load<Sprite>(path);
		_sprites.Add(id, res);
		return res;
	}

    //public void OnLeveledGameCompleted()
    //{
    //	SetGameState(EGameState.Loose);
    //    LeanTween.delayedCall(1.0f, () =>
    //    {
    //        EventData eventData = new EventData("OnOpenFormNeededEvent");
    //        eventData.Data["form"] = UIConsts.FORM_ID.LEVELED_STATISTIC_WINDOW;
    //        GameManager.Instance.EventManager.CallOnOpenFormNeededEvent(eventData);
    //    });
    //}

    private IEnumerator OnCreatureMixGameCompleted()
    {
        SetGameState(EGameState.Loose, "Loose");
        int nextLevel = GameManager.Instance.Player.CreatureMixLevel + 1;
        _upgradesManager.SetLevel(nextLevel, false);
        GameManager.Instance.Player.CreatureMixLevel = nextLevel;
        yield return new WaitForSeconds(Consts.ADD_POINTS_EFFECT_TIME);
        // teleport
        MusicManager.playSound("Teleport");
        yield return new WaitForSeconds(1);
        //
        MusicManager.playSound("Mimishki_WIN");
        yield return new WaitForSeconds(0.7f);
        RestartGame();
    }

    private void TryStartStartTutorSequence()
    {
        _startSequenceState = 0;
        _startTutorHintData.XA = 0;
        _startTutorHintData.YA = 0;
        _startTutorHintData.XB = 0;
        _startTutorHintData.YB = 0;
        if (!GameManager.Instance.Player.IsTutorialShowed("start"))
        {
            GameManager.Instance.Player.SetTutorialShowed("start");
            ForceToSwipe();
        }
    }

    private void ForceToSwipe()
    {
        ++_startSequenceState;
        //TUTOR_5
        if (_startSequenceState >= 6)
        {
            _startSequenceState = 0;
            LeanTween.delayedCall(1.0f, ()=> { GameManager.Instance.ShowTutorial("5", new Vector3(0, 0, 0)); });
            return;
        }
        if (_startSequenceState == 1)
        {
            _startTutorHintData.XA = 4;
            _startTutorHintData.YA = 2;
            _startTutorHintData.XB = 1;
            _startTutorHintData.YB = 2;
        } else
        if (_startSequenceState == 2)
        {
            _startTutorHintData.XA = 1;
            _startTutorHintData.YA = 2;
            _startTutorHintData.XB = 1;
            _startTutorHintData.YB = 4;
        } else
        if (_startSequenceState == 3)
        {
            _startTutorHintData.XA = 1;
            _startTutorHintData.YA = 4;
            _startTutorHintData.XB = 4;
            _startTutorHintData.YB = 4;
        } else
        if (_startSequenceState == 4)
        {
            _startTutorHintData.XA = 4;
            _startTutorHintData.YA = 4;
            _startTutorHintData.XB = 4;
            _startTutorHintData.YB = 2;
        } else
        if (_startSequenceState == 5)
        {
            _startTutorHintData.XA = 4;
            _startTutorHintData.YA = 2;
            _startTutorHintData.XB = 0;
            _startTutorHintData.YB = 2;
        }
        // show hint
        ShowHint(_startTutorHintData);
    }

    private void ShowHint(MatchHintData mhData)
    {
        HideHint();
        GameObject obj = (GameObject)GameObject.Instantiate(HintPrefab, Vector3.zero, Quaternion.identity);
        _hint = obj.GetComponent<NewHintScript>();
        _hint.ShowHint(mhData, this);
    }

    private void HideHint()
    {
        if (_hint != null)
        {
            _hint.HideHint();
            _hint = null;
        }
    }

	protected void UpdateInput()
	{
		if (!Consts.IS_TOUCH_DEVICE && Input.touchCount > 0)
		{
			Consts.IS_TOUCH_DEVICE = true;
		}

		if (Consts.IS_TOUCH_DEVICE)
		{
            //use touches
            if (Input.touchCount == 0 && _currentTouchId != -1)
            {
                _currentTouchId = -1;
                DragSlot = null;
                DragEnemy = null;
                HideSelection();
            } else
            // only 1 touch - first touch (B real) for clicks on buttons and popups
            if (Input.touchCount > 0)
            {
                //GameObject uiObj = GameManager.Instance.GameFlow.GetFormFromID(UIConsts.FORM_ID.MAP_MAIN_MENU);
                //if (uiObj != null)
                //{
                //    BaseUIController ui = uiObj.GetComponent<BaseUIController>();
                //    if (ui != null)
                //    {
                //        if (ui.IsOver())
                //        {
                //            canClick = false;
                //        }
                //    }
                //}
                if (_currentTouchId == -1)
                {
                    if (Input.touches[0].phase == TouchPhase.Began)
                    {
                        Vector3 touchPosWorld = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
                        Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y);
                        // left down
                        //Debug.Log("left down");
                        _currentTouchId = Input.touches[0].fingerId;
                        LeftMouseDownByPosition(touchPosWorld2D);
                    }
                    else
                    if (Consts.START_SLIDE_ON_NO_MOUSE_DOWN)
                    {
                        Vector3 touchPosWorld = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
                        Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y);
                        // left down
                        //Debug.Log("left over-down");
                        _currentTouchId = Input.touches[0].fingerId;
                        LeftMouseDownByPosition(touchPosWorld2D);
                    }
                }
                else
                {
                    foreach (var touch in Input.touches)
                    {
                        if (touch.fingerId == _currentTouchId)
                        {
                            Vector2 downGamePosNew2 = Camera.main.ScreenToWorldPoint(touch.position);
                            //if (touch.phase == TouchPhase.Moved)
                            //{
                            //    OnMouseMove(downGamePosNew2);
                            //}
                            //else
                            if (touch.phase == TouchPhase.Ended)
                            {
                                //Debug.Log("left up");
                                _currentTouchId = -1;
                                LeftMouseUpByPosition(downGamePosNew2);
                            } else
                            if (Consts.SLIDE_WITHOUT_MOUSE_UP)
                            {
                                if (DragSlot != null)
                                {
                                    DragSlot.UpdateSlot(downGamePosNew2);
                                } else
                                if (DragEnemy != null)
                                {
                                    if (AEnemies.TryToMoveEnemyBySlide(DragEnemy, _startDragEnemyPos, downGamePosNew2, false))
                                    {
                                        SetGameState(EGameState.MoveEnemy, "move with slide2");
                                        LeanTween.delayedCall(Enemies.ENEMY_SLIDE_TIME, () =>
                                        {
                                            OnTurnWasMade(false, false);
                                        });
                                        DragEnemy = null;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }
			//MusicManager.playSound("horse");
			//MusicManager.playSound("unlock_upgrade");
		}
		else
		{
			// use mouse
			Vector2 downScreenPos = Input.mousePosition;
			Vector2 downGamePosNew = Camera.main.ScreenToWorldPoint(downScreenPos);
			if (Input.GetMouseButtonDown(0))
            {
                // left down
                //Debug.Log("over-left down");
                LeftMouseDownByPosition(downGamePosNew);
            } else
            if (Consts.START_SLIDE_ON_NO_MOUSE_DOWN && Input.GetMouseButton(0) && DragSlot == null && DragEnemy == null)
            {
                // left down
                //Debug.Log("left down");
                LeftMouseDownByPosition(downGamePosNew);
            }
            else
            if (Input.GetMouseButtonUp(0))
            {
                // left up
                //Debug.Log("left up");
                LeftMouseUpByPosition(downGamePosNew);
            }
            else
            if (Consts.SLIDE_WITHOUT_MOUSE_UP)
            {
                if (DragSlot != null)
                {
                    DragSlot.UpdateSlot(downGamePosNew);
                } else
                if (DragEnemy != null)
                {
                    if (AEnemies.TryToMoveEnemyBySlide(DragEnemy, _startDragEnemyPos, downGamePosNew, false))
                    {
                        SetGameState(EGameState.MoveEnemy, "move with slide (no mouse up)");
                        LeanTween.delayedCall(Enemies.ENEMY_SLIDE_TIME, () =>
                        {
                            OnTurnWasMade(false, false);
                        });
                        DragEnemy = null;
                    }
                }
            }
		}
	}

	protected virtual void LeftMouseDownByPosition(Vector2 downGamePos)
	{
		if (GameManager.Instance.CurrentMenu != UISetType.ClassicGame && GameManager.Instance.CurrentMenu != UISetType.LeveledGame)
		{
			return;
		}
		if (_gameState != EGameState.PlayersTurn)
		{
			// can't drag and use powrups
			return;
		}
        if (DragSlot != null)
        {
            Debug.LogError("DragSlot != null");
            return;
        }

        if (GameManager.Instance.GameFlow.IsSomeWindow())
		{
			return;
		}

		//if (Consts.IS_TOUCH_DEVICE)
		//{
		////We now raycast with this information. If we have hit something we can process it.
		//RaycastHit2D hitInformation = Physics2D.Raycast(downGamePos, Camera.main.transform.forward);
		//if (hitInformation.collider != null)
		//{
		//    SSlot touchObject = hitInformation.transform.GetComponent<SSlot>();
		//    if (touchObject != null)
		//    {
		//        //MusicManager.playSound("horse");
		//        touchObject.LeftMouseDownByPosition(downGamePos);
		//    }
		//}
		//}

		BoardPos slotPosIn = GameBoard.SlotPosIn(downGamePos);
		if (IsSlotInBoard(slotPosIn))
		{
			SSlot slot = GetSlot(slotPosIn);
			SPipe pipe = slot.Pipe;
			if (pipe)
			{
                if (APowerupsPanel.OnSlotTouched(slot))
                {
                    // used powerup from panel
                } else
                if (ACharacters.OnSlotTouched(slot))
                {
                    // used powerup of character
                } else
                {
                    // trying to slide or select pipe
                    slot.OnMouseDownByPosition(downGamePos);
                }
			}
        } else
        if (Consts.MOVE_ENEMIES_WITH_SLIDE)
        {
            DragEnemy = AEnemies.GetEnemyByPos(downGamePos);
            if (DragEnemy)
            {
                _startDragEnemyPos = downGamePos;
            }
        }
        if (Consts.START_SLIDE_ON_NO_MOUSE_DOWN && DragSlot == null && DragEnemy == null)
        {
            // we clicked on empty cell
            _currentTouchId = -1;
        }
    }

	protected virtual void LeftMouseUpByPosition(Vector2 downGamePos)
	{
		if (GameManager.Instance.CurrentMenu != UISetType.ClassicGame && GameManager.Instance.CurrentMenu != UISetType.LeveledGame)
		{
			return;
		}
		if (DragSlot != null)
		{
			if (DragSlot.OnMouseUpByPosition(downGamePos))
            {
                // impulse too short or tap on immovable character - tap
                if (!TryCreateFinalAttackByTouch(DragSlot) && DragSlot.Pipe.IsCharacter())
                {
                    ACharacters.OnCharacterClick(DragSlot.Pipe.GetComponent<Pipe_Character>());
                }
            }
            HideSelection();
            DragSlot = null;
        } else
        if (DragEnemy != null)
        {
            if (AEnemies.TryToMoveEnemyBySlide(DragEnemy, _startDragEnemyPos, downGamePos, true))
            {
                SetGameState(EGameState.MoveEnemy, "move with slide");
                LeanTween.delayedCall(Enemies.ENEMY_SLIDE_TIME, () =>
                {
                    OnTurnWasMade(false, false);
                });
            }
            DragEnemy = null;
        }
	}

	public Material GetMaterialForColoredPipe(int acolor, int param)
	{
		return ColoredMaterials[acolor * Consts.CLASSIC_GAME_COLORS + param];
	}

    public Material GetMaterialForFillPipeVariation(int acolor, int param)
    {
        switch (acolor)
        {
            case 0:
                {
                    return ColoredMaterialsFill_0[param];
                }
            case 1:
                {
                    return ColoredMaterialsFill_1[param];
                }
            case 2:
                {
                    return ColoredMaterialsFill_2[param];
                }
            case 3:
                {
                    return ColoredMaterialsFill_3[param];
                }
            case 4:
                {
                    return ColoredMaterialsFill_4[param];
                }
        }
        return ColoredMaterialsFill_0[param];
    }

    public void SetResourceForce(long amount, int color)
    {
        _resources[color] = amount;
        EventData eventData = new EventData("OnResourcesChangedEvent");
        eventData.Data["isforce"] = true;
        GameManager.Instance.EventManager.CallOnResourcesChangedEvent(eventData);
    }

    public void SetResource(long amount, int color)
    {
        _resources[color] = amount;
        EventData eventData = new EventData("OnResourcesChangedEvent");
        eventData.Data["isforce"] = false;
        GameManager.Instance.EventManager.CallOnResourcesChangedEvent(eventData);
    }

    //public void AddResource(long amount, int color)
    //{
    //	long sum = _resources[color] + amount;
    //	SetResource(sum, color);
    //}

    public void AddResource(long amount, int color, Vector3 pos)
    {
        long sum = _resources[color] + amount;
        pos = ConvertPositionFromLocalToScreenSpace(pos);
        EventData e = new EventData("OnShowAddResourceEffect");
        e.Data["x"] = pos.x;
        e.Data["y"] = pos.y;
        e.Data["amount"] = amount;
        e.Data["color"] = color;
        GameManager.Instance.EventManager.CallOnShowAddResourceEffect(e);
        if (Consts.SHOW_ADD_POINTS_ANIMATION)
        {
            LeanTween.delayedCall(Consts.ADD_POINTS_EFFECT_TIME, () => { SetResource(sum, color); });
        }
        else
        {
            SetResource(sum, color);
        }
    }

    public void RemoveResource(long amount, int color)
    {
        long sum = _resources[color] - amount;
        if (sum < 0)
        {
            sum = 0;
        }
        SetResource(sum, color);
    }

    public void AddResourceByLevelOfColoredPipe(int value, int color, int multiplyer, Vector3 pos) // by level of colored pipe
    {
        AddResource(Consts.POINTS[value] * multiplyer, color, pos);
        // TODO call to ResourcePanel to show "+points" animation
    }

    public long GetResourceAmount(int color)
    {
        return _resources[color];
    }

    public long GetTotalPoints()
    {
        long res = 0;
        for (int i = 0; i < _resources.Count; ++i)
        {
            res += _resources[i];
        }
        res += _pointsForSequences;
        return res;
    }

    public void AddPointsForSequence(long points)
    {
        _pointsForSequences += points;
        EventData eventData = new EventData("OnResourcesChangedEvent");
        eventData.Data["isforce"] = false;
        eventData.Data["x"] = 0;
        eventData.Data["y"] = 0;
        GameManager.Instance.EventManager.CallOnResourcesChangedEvent(eventData);
    }

    public void SetGameState(EGameState gameState, string reason)
    {
        Debug.Log(reason + " : " + _gameState.ToString() + " -> " + gameState);
        _gameState = gameState;
    }

    public EGameState GetGameState()
    {
        return _gameState;
    }

    public bool IsLoose()
    {
        return _gameState == EGameState.Loose;
    }

    public bool IsPause()
    {
        return _gameState == EGameState.Pause;
    }

    public int GetRandomColor()
    {
        //if (GameBoard.GameType == EGameType.Classic)
        //{
        if (_possibleColors.Count <= 1)
        {
            return UnityEngine.Random.Range(0, Consts.CREATURE_MIX_COLORS);
        } else
        {
            return _possibleColors[UnityEngine.Random.Range(0, _possibleColors.Count)];
        }
        //}
        //        else
        //        //if (GameBoard.GameType == EGameType.Inverse)
        //        {
        //            return UnityEngine.Random.Range(0, Consts.INVERSE_GAME_COLORS);
        //        }
    }

    public int GetColorsCount()
    {
        //if (GameBoard.GameType == EGameType.Classic)
        //{
        return Consts.CLASSIC_GAME_COLORS;
        //} 
        //		else
        //        //if (GameBoard.GameType == EGameType.Inverse)
        //        {
        //            return Consts.INVERSE_GAME_COLORS;
        //        }
    }

    public void UpdateTimer(float deltaTime)
    {
        TimePlayed += deltaTime;
    }

    public int GetMaxColoredLevels()
    {
        return Consts.MAX_COLORED_LEVELS;
    }

    private void AddPipesAfterTurn(bool wasMatch, bool justAddPipe)
    {
        //bool allAimsCompleted = false;
        bool aimComplited = false;
        if (Consts.CHECK_AIM_ON_COMBINE)
        {
            if (_lastSlotWithMatch.x >= 0)
            {
                //// variant with aims panels
                ////aimComplited = AAimPanel.CheckSlot(GetSlot(_lastSlotWithMatch));
                ////_lastSlotWithMatch.x = -1;
                ////allAimsCompleted = AAimPanel.IsAllAimsCompleted();
                //TryCreateFinalAttack(GetSlot(_lastSlotWithMatch));
                _lastSlotWithMatch.x = -1;
            }
        } else
        {
            //aimComplited = CheckAimsInSpecificSlots();
            //allAimsCompleted = AAimPanel.IsAllAimsCompleted();
        }
        bool pipeneeded = false;
        if (!aimComplited || justAddPipe)
        {
            if (!justAddPipe && GameBoard.AddingType == EAddingType.EachXMoves)
            {
                --_movesToNextPipe;
                ++_allTurns;
            }
            else
            {
                ++_allTurns;
            }

            if (justAddPipe)
            {
                pipeneeded = true;
            }
            else
            if (GameBoard.AddingType == EAddingType.EachXMoves)
            {
                if (_movesToNextPipe == 0)
                {
                    pipeneeded = true;
                }
            }
            else
            if (GameBoard.AddingType == EAddingType.OnNoMatch)
            {
                if (!wasMatch)
                {
                    pipeneeded = true;
                }
            }
            if (GetMovablePipesCount() == 0)
            {
                pipeneeded = true;
            }
        }
        else
        {
            ++_allTurns;
            if (GetMovablePipesCount() == 0)
            {
                pipeneeded = true;
            }
        }

        if (pipeneeded)
        {
            if (GameBoard.AddingType == EAddingType.EachXMoves && _movesToNextPipe == 0)
            {
                _movesToNextPipe = Consts.TURNS_TO_NEXT_PIPE;
            }
            bool needBlocker = false;
            if (GameBoard.AddingType != EAddingType.OnNoMatch || Consts.USE_BLOCKERS_ON_NO_MATCH_ADDING)
            {
                needBlocker = _pipesToNextBlocker == 0;
                if (needBlocker)
                {
                    _pipesToNextBlocker = Consts.PIPES_TO_NEXT_BLOCKER;
                }
                else
                {
                    --_pipesToNextBlocker;
                }
            }
            // add new pipe to queue and create new
            EPipeType pipeType = EPipeType.Colored;
            if (needBlocker)
            {
                pipeType = EPipeType.Blocker;
            }
            if (_addNewPipes && pipeneeded) // && (!allAimsCompleted)) поки все одно додаємо пайп
            {
                if (pipeneeded && AddRandomPipe(pipeType))
                {
                    ++_pipesAdded;
                }
                else
                {
                    if (pipeType == EPipeType.Blocker)
                    {
                        // add blocker on next turn
                        _pipesToNextBlocker = 0;
                    }
                }
            }
        }
        EventData eventData = new EventData("OnTurnWasMadeEvent");
        eventData.Data["tonextpipe"] = _movesToNextPipe;
        eventData.Data["turnsmade"] = _allTurns;
        eventData.Data["pipesadded"] = _pipesAdded;
        GameManager.Instance.EventManager.CallOnTurnWasMadeEvent(eventData);

        // Можливо панель сіквенсів теж можна заюзати, поки виграєм лише перемігши ворогів
        //if (allAimsCompleted)
        //{
        //    OnLevelCompleted();
        //}
    }

    public void OnTurnWasMade(bool wasMatch, bool justAddPipe)
    {
        if (_gameState == EGameState.MoveEnemy)
        {
            SetGameState(EGameState.PlayersAttack, "OnTurnWasMadeAfterMovedEnemy");
        } else
        {
            SetGameState(EGameState.PlayersAttack, "OnTurnWasMade");
            AddPipesAfterTurn(wasMatch, justAddPipe);
        }
        if (_gameState != EGameState.Loose && _gameState != EGameState.Win)
        {
            if (!justAddPipe)
            {
                StartCoroutine(OnTurnWasMadeCoroutine(wasMatch));
            } else
            {
                StartPlayersTurn();
            }
        }
    }

    private IEnumerator OnTurnWasMadeCoroutine(bool wasMatch)
    {
        yield return StartCoroutine(AAttacks.WaitEndAttacksCoroutine());
        if (!CheckWinConditions())
        {
            if (_gameState == EGameState.PlayerUsedPowerup)
            {
                SetGameState(EGameState.PlayersTurn, "after powerup"); // users turn, when powerup cleared board
            } else
            {
                if (Consts.ENEMIES_TURN_ON_EVERY_MATCH || !wasMatch)
                {
                    yield return StartCoroutine(AEnemies.EnemiesAttackCoroutine(this));
                }
                if (!CheckLooseConditions())
                {
                    // adding new enemies
                    if (Consts.ENEMIES_TURN_ON_EVERY_MATCH || !wasMatch)
                    {
                        if (AEnemiesQueue.OnTurnWasMade())
                        {
                            //yield return new WaitForSeconds(Enemy.ENEMY_APPEAR_TIME);
                        }
                    }
                    StartPlayersTurn();
                }
            }
        }
    }

    private bool IsAttackOver()
    {
        //TODO список атак кожна зі своєю затримкою. Наприклад після паверапа, коли знищаться багато фішок, непогано було б не атакувати одночасно всіма, а по черзі
        return true;
    }

    private void StartPlayersTurn()
    {
        SetGameState(EGameState.PlayersTurn, "StartPlayersTurn");
    }

    public int GetMovesToNextPipe()
    {
        return _movesToNextPipe;
    }

    public void Reset()
    {
        _movesToNextPipe = Consts.TURNS_TO_NEXT_PIPE;
        _allTurns = 0; // for leveled too
        _pipesAdded = 0;
        _pipesToNextBlocker = Consts.PIPES_TO_NEXT_BLOCKER;
        _pointsForSequences = 0;
        for (int i = 0; i < _resources.Count; ++i)
        {
            _resources[i] = 0;
        }
        SetGameState(EGameState.Pause, "Reset()");
        TimePlayed = 0;
        DragSlot = null;
        DragEnemy = null;

        _upgradesManager.SetLevel(GameManager.Instance.Player.CreatureMixLevel, true);

        //		// leveled
        //		StarsGained = 0;
        //		MovesLeft = 0;
        //		//

        //        // powerups
        //        EventData eventData = new EventData("OnPowerUpsResetNeededEvent");
        //        //eventData.Data["isforce"] = true;
        //        GameManager.Instance.EventManager.CallOnPowerUpsResetNeededEvent(eventData);
    }

    private bool CheckAimsInSpecificSlots()
    {
        bool completed = false;
        for (int i = 0; i < _slotsToCheckAims.Count; ++i)
        {
            SSlot slot = GetSlot(_slotsToCheckAims[i]);
            if (AAimPanel.CheckSlot(slot))
            {
                completed = true;
            }
        }
        return completed;
    }

    public void ClearProgress()
    {
        //GameManager.Instance.Player.SavedGame = null;
        //GameManager.Instance.Settings.Save();
        GameManager.Instance.Settings.ResetSettings();
        ClearBoardForce();
        _upgradesManager.Reset();
        _creaturesManager.Reset();
    }

    public Color GetPipeColor(int acolor)
    {
        return Colors[acolor + 1]; // зміщення через дефолтний нейтральний колір
    }

    public bool CheckLooseConditions()
    {
        if (ALivesPanel.IsDead()) // && ACharacters.IsAllDead())
        {
            OnLoose();
            return true;
        }
        return false;
    }

    private bool CheckWinConditions()
    {
        if (AEnemies.NoEnemiesLeft() && AEnemiesQueue.IsQueueEmpty())
        {
            OnLevelCompleted();
            return true;
        }
        return false;
    }

    private void OnLevelCompleted()
    {
        SetGameState(EGameState.Win, "Win");
        ClearBoardQuick();
        StartCoroutine(OnCreatureMixGameCompleted());
    }

    public void CreateSimpleAttack(SSlot slot, int pipeColor, int pipeParam)
    {
        int attackPower = pipeParam * 3;
        if (Consts.FILLER_VARIATION)
        {
            attackPower = pipeParam + 1;
        }
        StartCoroutine(SimpleWeapon.AttackCoroutine(this, slot, slot.Pipe.AColor, attackPower)); // TODO correct power of strike according to upgrades and balance
    }

    public void TryCreateFinalAttack(SSlot slot)
    {
        if (slot.Pipe.Param == _maxColoredLevels - 1)
        {
            int attackPower = slot.Pipe.Param * 3;
            StartCoroutine(FinalWeapon.AttackCoroutine(this, slot, slot.Pipe.AColor, attackPower)); // TODO correct power of strike according to upgrades and balance
        }
    }

    public bool TryCreateFinalAttackByTouch(SSlot slot)
    {
        if (!slot.Pipe || !slot.Pipe.IsColored())
        {
            return false;
        }
        if (slot.Pipe.Param == _maxColoredLevels)
        {
            StartCoroutine(FinalWeapon.AttackCoroutine(this, slot, slot.Pipe.AColor, slot.Pipe.Param * 3)); // TODO correct power of strike according to upgrades and balance
            SetGameState(EGameState.EnemiesAttack, "attacking by touch final");
            OnTurnWasMade(true, false);
            return true;
        }
        return false;
    }

    public SuperSimplePool GetPool()
    {
        return _pool;
    }

    public void AddManaForBump(SSlot slot, SPipe pipe, int distance)
    {
        int color = pipe.AColor;
        int aparam = pipe.Param;
        int mana = distance * (aparam + 1); // more distance - more mana
        mana = ACharacters.AddManaForBump(slot, pipe, mana, color);
        if (mana > 0)
        {
            mana = APowerupsPanel.AddManaForBump(slot, pipe, mana, color);
        }
    }
}


////винести базовий клас Чарактер.
////    від чарактера зробити Енемі і Пайп_чарактер.

//ГеймБоард тримає чарактерів для відслідковування кінця гри(поки панель життів теж є). Мертвий чарактер залишається на полі(на випадок воскресіння + заважає, можливо нерухомий крест)

//    Вороги атакують, але урон наноситься спершу чарактерам навпроти, а якщо чарактера на лінії удару немає - то в панель життів
//    Вороги теж атакують за допомогою Attack. У них буде клас-зброя (1-2 зброї на монстра), що раз в Х ходів атакує чи задіює спец вміння(фактично це паверапи).

//Фішку-блокери: 1. що розбивати матчами поряд, 2. розбивати бампами бампами, 3. дірка, в яку треба закинути певний пайп 


    //TODO 1 super simple pool in game board with list of folders to scan!!!!!!!!