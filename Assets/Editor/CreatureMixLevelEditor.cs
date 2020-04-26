using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(CreatureMixLevelData))]
public class CreatureMixLevelEditor : Editor
{
	private const int               PIPE_TYPE_HOLE_ID = 27; //32 for LAST_COLOR_PARAM == 5
	private const int               PIPE_TYPE_NONE_ID = 26; //31 for LAST_COLOR_PARAM == 5
	private const int               PIPE_TYPE_BLOCKER_ID = 25; // 30 for LAST_COLOR_PARAM == 5
	private const int               LAST_COLOR_PARAM = 4; // = Consts.MAX_COLORS_LEVEL - 1

    private int 					SLOT_SIZE = 60;
	private int 					_w = 5;
	private int 					_h = 5;
	private List<List<SSlotData>>	_neededStates = new List<List<SSlotData>>();

    // current brush
    private EPipeType               _carrentBrushType = EPipeType.None;
    private int                     _carrentBrushColor = -1;
    private int                     _carrentBrushParam = -1;
    private int                     _intBrush = PIPE_TYPE_NONE_ID;
    private int                     _indexStart = -1;

	private EMoveType				_drawState = EMoveType.None;
    // 

    private GUIStyle 				guiStyle = new GUIStyle();

	private Texture[] 				_statesTexs;
	private Texture[] 				_textures;

    CreatureMixLevelData 			_myLevel = null;


    void OnEnable()
	{
        Texture text_0_0 = EditorGUIUtility.Load("buttons/editor_0_0.png") as Texture;
        Texture text_0_1 = EditorGUIUtility.Load("buttons/editor_0_1.png") as Texture;
        Texture text_0_2 = EditorGUIUtility.Load("buttons/editor_0_2.png") as Texture;
        Texture text_0_3 = EditorGUIUtility.Load("buttons/editor_0_3.png") as Texture;
        //Texture text_0_4 = EditorGUIUtility.Load("buttons/editor_0_4.png") as Texture;
        Texture text_0_max = EditorGUIUtility.Load("buttons/editor_max_0.png") as Texture;

        Texture text_1_0 = EditorGUIUtility.Load("buttons/editor_1_0.png") as Texture;
        Texture text_1_1 = EditorGUIUtility.Load("buttons/editor_1_1.png") as Texture;
        Texture text_1_2 = EditorGUIUtility.Load("buttons/editor_1_2.png") as Texture;
        Texture text_1_3 = EditorGUIUtility.Load("buttons/editor_1_3.png") as Texture;
        //Texture text_1_4 = EditorGUIUtility.Load("buttons/editor_1_4.png") as Texture;
        Texture text_1_max = EditorGUIUtility.Load("buttons/editor_max_1.png") as Texture;

        Texture text_2_0 = EditorGUIUtility.Load("buttons/editor_2_0.png") as Texture;
        Texture text_2_1 = EditorGUIUtility.Load("buttons/editor_2_1.png") as Texture;
        Texture text_2_2 = EditorGUIUtility.Load("buttons/editor_2_2.png") as Texture;
        Texture text_2_3 = EditorGUIUtility.Load("buttons/editor_2_3.png") as Texture;
        //Texture text_2_4 = EditorGUIUtility.Load("buttons/editor_2_4.png") as Texture;
        Texture text_2_max = EditorGUIUtility.Load("buttons/editor_max_2.png") as Texture;

        Texture text_3_0 = EditorGUIUtility.Load("buttons/editor_3_0.png") as Texture;
        Texture text_3_1 = EditorGUIUtility.Load("buttons/editor_3_1.png") as Texture;
        Texture text_3_2 = EditorGUIUtility.Load("buttons/editor_3_2.png") as Texture;
        Texture text_3_3 = EditorGUIUtility.Load("buttons/editor_3_3.png") as Texture;
        //Texture text_3_4 = EditorGUIUtility.Load("buttons/editor_3_4.png") as Texture;
        Texture text_3_max = EditorGUIUtility.Load("buttons/editor_max_3.png") as Texture;

        Texture text_4_0 = EditorGUIUtility.Load("buttons/editor_4_0.png") as Texture;
        Texture text_4_1 = EditorGUIUtility.Load("buttons/editor_4_1.png") as Texture;
        Texture text_4_2 = EditorGUIUtility.Load("buttons/editor_4_2.png") as Texture;
        Texture text_4_3 = EditorGUIUtility.Load("buttons/editor_4_3.png") as Texture;
        //Texture text_4_4 = EditorGUIUtility.Load("buttons/editor_4_4.png") as Texture;
        Texture text_4_max = EditorGUIUtility.Load("buttons/editor_max_4.png") as Texture;

        Texture text_blocker = EditorGUIUtility.Load("buttons/editor_pBlocker.png") as Texture;
        Texture text_none = EditorGUIUtility.Load("buttons/editor_none.png") as Texture;
		Texture text_hole = EditorGUIUtility.Load("buttons/editor_hole.png") as Texture;

        _statesTexs = new Texture[]
		{
            text_0_0, text_0_1, text_0_2, text_0_3, //text_0_4, 
			text_0_max,
            text_1_0, text_1_1, text_1_2, text_1_3, //text_1_4, 
			text_1_max,
            text_2_0, text_2_1, text_2_2, text_2_3, //text_2_4, 
			text_2_max,
            text_3_0, text_3_1, text_3_2, text_3_3, //text_3_4, 
			text_3_max,
            text_4_0, text_4_1, text_4_2, text_4_3, //text_4_4, 
			text_4_max,
			text_blocker, text_none, text_hole
        };
		_myLevel = (CreatureMixLevelData)target;
		ReloadLevel();
	}

    public override void OnInspectorGUI()
    {
        this.DrawDefaultInspector();
        if (!Application.isPlaying)
        {
            guiStyle.fontSize = 20;
            guiStyle.fontStyle = FontStyle.Bold;
            guiStyle.alignment = TextAnchor.MiddleCenter;

			if (GUILayout.Button("RECREATE LEVEL"))
			{
				RecreateLevel();
			}

            GUILayout.BeginHorizontal();
            GUILayout.Label("PALETTE", guiStyle);
            GUILayout.Label("START", guiStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            // palette of type
			_intBrush = GUILayout.SelectionGrid(_intBrush, _statesTexs, LAST_COLOR_PARAM + 1, EditorStyles.miniButton, GUILayout.Width(SLOT_SIZE * 6), GUILayout.Height(SLOT_SIZE * 6));
            EditorsIntToPipeParams(_intBrush);
			GUILayout.Space (20);
            // needed
			int WIDTH = SLOT_SIZE * _w;
			int HEIGHT = SLOT_SIZE * _h;
			// receive clicks on needed buttons, draw buttons
			int indexNeeded = GUILayout.SelectionGrid(-1, _textures, _w, EditorStyles.miniButton, GUILayout.Width(WIDTH), GUILayout.Height(HEIGHT));
			if (indexNeeded >= 0)
			{
				int yy1 = GetSlotPosY(indexNeeded);
				int xx1 = GetSlotPosX(indexNeeded);
				if (indexNeeded >= 0 && IsOnBoard(xx1, yy1))
				{
                    _neededStates[xx1][yy1].pt = _carrentBrushType;
                    _neededStates[xx1][yy1].c = _carrentBrushColor;
                    _neededStates[xx1][yy1].p = _carrentBrushParam;
                    _textures[indexNeeded] = _statesTexs[_intBrush];
                }
            }
			GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("CHECK"))
			{
				CheckCurrentLevel();
			}
            if (GUILayout.Button("SAVE"))
            {
				if (CheckCurrentLevel())
				{
					SaveLevel();
				}
            }
            GUILayout.EndHorizontal();
        }
    }
    
    private int PipeParamsToEditorsInt(EPipeType brushType, int brushColor, int brushParam)
    {
        if (brushType == EPipeType.Blocker)
        {
            return PIPE_TYPE_BLOCKER_ID;
        } else
        if (brushType == EPipeType.None)
        {
            return PIPE_TYPE_NONE_ID;
        } else
		if (brushType == EPipeType.Hole)
		{
			return PIPE_TYPE_HOLE_ID;
		} else
        {
			return brushColor * (LAST_COLOR_PARAM + 1) + brushParam;
        }
    }
    
    private void EditorsIntToPipeParams(int editorsInt)
    {
        if (editorsInt == PIPE_TYPE_BLOCKER_ID)
        {
            _carrentBrushType = EPipeType.Blocker;
            _carrentBrushColor = -1;
            _carrentBrushParam = -1;
        } else
        if (editorsInt == PIPE_TYPE_NONE_ID)
        {
            _carrentBrushType = EPipeType.None;
            _carrentBrushColor = -1;
            _carrentBrushParam = -1;
        } else
		if (editorsInt == PIPE_TYPE_HOLE_ID)
		{
			_carrentBrushType = EPipeType.Hole;
			_carrentBrushColor = -1;
			_carrentBrushParam = -1;
		} else
        {
			_carrentBrushType = EPipeType.Colored;
			_carrentBrushColor = editorsInt / (LAST_COLOR_PARAM + 1);
			_carrentBrushParam = editorsInt % (LAST_COLOR_PARAM + 1);
        }
    }

    private bool IsOnBoard(int i, int j)
	{
		return (i >= 0 && j >= 0 && i < _w && j < _h);
	}

	private int GetSlotPosX(int index)
	{
		int i = index % _w;
		return i;
	}

	private int GetSlotPosY(int index)
	{
		int j = _h - 1 - index / _w ; //<-fix for vertical flip  //int j = index / _w ;
		return j;
	}

	private int GetSlotIndex(int i, int j)
	{
		return (_h - j - 1) * _w + i; //<-fix for vertical flip  // return j * _w + i;
	}

	private void RecreateLevel()
	{
        _carrentBrushType = EPipeType.None;
        _carrentBrushColor = -1;
        _carrentBrushParam = -1;
        _intBrush = PIPE_TYPE_NONE_ID;
        _indexStart = -1;
		_drawState = EMoveType.None;

        _textures = new Texture[_w * _h];
        _neededStates.Clear();
        for (int i = 0; i < _w; ++i)
        {
            List<SSlotData> rowN = new List<SSlotData>();
            for (int j = 0; j < _h; ++j)
            {
                int id = GetSlotIndex(i, j);
                _textures[id] = _statesTexs[PIPE_TYPE_NONE_ID];

                SSlotData neededSlot = new SSlotData();
                neededSlot.x = i;
                neededSlot.y = j;
                neededSlot.c = -1;
                neededSlot.p = -1;
                neededSlot.pt = EPipeType.None;
                rowN.Add(neededSlot);
            }
            _neededStates.Add(rowN);
        }
    }

	private void ReloadLevel()
	{
		if (_myLevel.NeededStates == null)
        {
            RecreateLevel();
            return;
        }
        _w = 5;
        _h = 5;
        _textures = new Texture[_w * _h];
		_neededStates = new List<List<SSlotData>>();

        for (int i = 0; i < _w; ++i)
        {
			List<SSlotData> rowN = new List<SSlotData>();
            for (int j = 0; j < _h; ++j)
            {
                int id = GetSlotIndex(i, j);

				SSlotData savedNeededSlot = _myLevel.NeededStates[id];
				SSlotData neededSlot = new SSlotData();
				neededSlot.x = i;
				neededSlot.y = j;
				neededSlot.c = savedNeededSlot.c;
				neededSlot.p = savedNeededSlot.p;
				neededSlot.pt = savedNeededSlot.pt;
				rowN.Add(neededSlot);
				int intNeededIndex = PipeParamsToEditorsInt(neededSlot.pt, neededSlot.c, neededSlot.p);
				_textures[id] = _statesTexs[intNeededIndex];
            }
			_neededStates.Add(rowN);
        }
    }

	private void SaveLevel()
	{
        if (_myLevel.NeededStates == null)
        {
            _myLevel.NeededStates = new List<SSlotData>();
        }
		_myLevel.NeededStates = new List<SSlotData>();

		//for (int j = 0; j < _h; ++j)
		//{
			//for (int i = 0; i < _w; ++i)
			//{
		for (int j = _h - 1; j >= 0; --j)
		{
			for (int i = 0; i < _w; ++i)
			{
				SSlotData neededData = _neededStates[i][j];
				SSlotData neededSlot = new SSlotData();
				neededSlot.x = i;
				neededSlot.y = j;
				neededSlot.c = neededData.c;
				neededSlot.p = neededData.p;
				neededSlot.pt = neededData.pt;
				_myLevel.NeededStates.Add(neededSlot);
			}
		}
		EditorUtility.SetDirty(_myLevel);
	}

	private bool CheckCurrentLevel()
	{
        if (_myLevel.Colors.Count == 1)
        {
            Debug.LogError("ADD MORE COLORS!!!");
        }
        if (_myLevel.Aims.Count == 0)
        {
            Debug.LogError("ADD AIMS!!!");
        } else
        {
            bool higherThan1 = false;
            for (int i = 0; i < _myLevel.Aims.Count; ++i)
            {
                if (_myLevel.Colors.Count > 0 && !_myLevel.Colors.Exists(x => x == _myLevel.Aims[i].x))
                {
                    Debug.LogError("NO COLOR TO COMPLETE AIM!!!");
                }
                if (_myLevel.Aims[i].y > 1)
                {
                    higherThan1 = true;
                }
                if (_myLevel.Aims[i].y < 1 || _myLevel.Aims[i].y > 4)
                {
                    Debug.LogError("AIM'S LEVEL SHOULD BE BETWEEN 1 AND 4");
                }
            }
            if (!higherThan1)
            {
                Debug.LogError("SHOULD BE AT LEAST ONE AIM WITH LEVEL > 1!!!");
            }
        }
        return true;
	}
}