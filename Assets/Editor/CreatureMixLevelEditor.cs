using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(CreatureMixLevelData))]
public class CreatureMixLevelEditor : Editor
{
	private const int                       PIPE_TYPE_HOLE_ID = 27; //32 for LAST_COLOR_PARAM == 5
	private const int                       PIPE_TYPE_NONE_ID = 26; //31 for LAST_COLOR_PARAM == 5
	private const int                       PIPE_TYPE_BLOCKER_ID = 25; // 30 for LAST_COLOR_PARAM == 5
	private const int                       LAST_COLOR_PARAM = 4; // = Consts.MAX_COLORS_LEVEL - 1

    private int 					        SLOT_SIZE = 60;
	private int 					        _w = 5;
	private int 					        _h = 5;
	private List<List<SSlotData>>	        _neededStates = new List<List<SSlotData>>();

    // current brush
    private EPipeType                       _carrentBrushType = EPipeType.None;
    private int                             _carrentBrushColor = -1;
    private int                             _carrentBrushParam = -1;
    private int                             _intBrush = PIPE_TYPE_NONE_ID;
    private int                             _indexStart = -1;

	private EMoveType				        _drawState = EMoveType.None;
    // 

    private GUIStyle 				        guiStyle = new GUIStyle();

	private Texture[] 				        _statesTexs;
	private Texture[] 				        _textures;

    CreatureMixLevelData 			        _myLevel = null;

    // queue part
    private const int                       QUEUE_ELEMENTS_IN_ROW = 10;
    private GUIStyle                        guiStyleBlack = new GUIStyle();
    private GUIStyle                        guiStyleRed = new GUIStyle();
    private Dictionary<string, int>         _elementsNames = new Dictionary<string, int>();
    private List<Texture>                   _queueTextures;
    private List<QueueElement>              _queue = new List<QueueElement>();
    private Texture                         _textureError;
    private List<Texture>                   _colorSticksTextures = new List<Texture>();
    private int                             _queueIndex = -1;
    private int                             _currentQueueIndex = -1;
    private List<int>                       _indexesFilter = new List<int>();
    private Dictionary<string, Texture>     _awailableTextures = new Dictionary<string, Texture>();
    private List<string>                    _awailableElements = new List<string>();
    private string                          _currentNameFilter = "";
    private
    //


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

        // queue part
        List<string> elementsNamesList = new List<string>(); //TODO from some file or find all prefabs in folder or from scriptable data about enemies!
        elementsNamesList.Add("none");
        elementsNamesList.Add("enemy_1");
        elementsNamesList.Add("enemy_2");
        elementsNamesList.Add("enemy_3");
        _elementsNames = new Dictionary<string, int>();
        _awailableElements = new List<string>();
        _queueTextures = new List<Texture>();
        string elementName = "";

        for (int i = 0; i < elementsNamesList.Count; ++i)
        {
            elementName = elementsNamesList[i];
            Texture text = EditorGUIUtility.Load("queue/elements/qe_" + elementName + ".png") as Texture;
            _queueTextures.Add(text);
            _awailableTextures.Add(elementName, _queueTextures[i]);
            _elementsNames.Add(elementName, i);
            _awailableElements.Add(elementName);
        }

        _textureError = EditorGUIUtility.Load("queue/ERROR.png") as Texture;

        for (int i = 0; i < 5; ++i)
        {
            _colorSticksTextures.Add(EditorGUIUtility.Load("queue/color_stick_" + i.ToString() + ".png") as Texture);
        }

        guiStyleBlack.fontSize = 20;
        guiStyleBlack.fontStyle = FontStyle.Bold;
        guiStyleBlack.alignment = TextAnchor.MiddleLeft;
        guiStyleRed.normal.textColor = Color.black;

        guiStyleRed.fontSize = 20;
        guiStyleRed.fontStyle = FontStyle.Bold;
        guiStyleRed.alignment = TextAnchor.MiddleLeft;
        guiStyleRed.normal.textColor = Color.red;

        //
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

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("RECREATE"))
            {
                RecreateLevel();
            }
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
                    if (_carrentBrushType == EPipeType.Colored)
                    {
                        if (_carrentBrushColor == 4)
                        {
                            Debug.LogError("Dont use this color!");
                            return;
                        } else
                        if (_carrentBrushParam == LAST_COLOR_PARAM)
                        {
                            Debug.LogError("Dont use last pipes!");
                            return;
                        }
                    }
                    _neededStates[xx1][yy1].pt = _carrentBrushType;
                    _neededStates[xx1][yy1].c = _carrentBrushColor;
                    _neededStates[xx1][yy1].p = _carrentBrushParam;
                    _textures[indexNeeded] = _statesTexs[_intBrush];
                }
            }
			GUILayout.EndHorizontal();

            // queue part
            GUILayout.Label("----- QUEUE -----------------------------------------------------------------------------------------------------------", guiStyleBlack);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("LEFT"))
            {
                OnLeftArrowClick();
            }
            if (GUILayout.Button("RIGHT"))
            {
                OnRightArrowClick();
            }
            if (GUILayout.Button("UP"))
            {
                OnUpArrowClick();
            }
            if (GUILayout.Button("DOWN"))
            {
                OnDownArrowClick();
            }
            if (GUILayout.Button("CLEAR QUEUE"))
            {
                ClearQueue();
            }
            if (GUILayout.Button("ADD ELEMENT"))
            {
                AddElementToQueue();
            }
            GUILayout.EndHorizontal();

            // REMOVE-INSERT-APPLY EPISODE->
            if (_queueIndex >= 0) // when selected episode
            {
                if (GUILayout.Button("REMOVE EPISODE"))
                {
                    _queue.RemoveAt(_queueIndex);
                    if (_queueIndex > 0)
                    {
                        --_queueIndex;
                    }
                    else
                    if (_queue.Count > 0)
                    {
                        _queueIndex = 0;
                    }
                    else
                    {
                        _queueIndex = -1;
                    }
                }
                if (GUILayout.Button("INSERT ELEMENT"))
                {
                    _queue.Insert(_queueIndex, new QueueElement("HZ", 0, ""));
                }
                // applying
                GUILayout.BeginVertical();
                for (int ii = 0; ii < _awailableElements.Count; ii += 5)
                {
                    GUILayout.BeginHorizontal();
                    string objectName = "";
                    for (int jj = ii; jj < ii + 5; ++jj)
                    {
                        if (jj < _awailableElements.Count)
                        {
                            if (GUILayout.Button(_awailableElements[jj]))
                            {
                                objectName = _awailableElements[jj];
                                //!!!QueueObject obj = CreateObjectTemporarly(objectName).GetComponent<QueueObject>();
                                QueueElement newData = _queue[_queueIndex];
                                newData.Name = objectName;
                                newData.Parameters = "";
                                //!!!newData = obj.GeneratePipesQueueElementData(newData);
                                //!!!GameObject.DestroyImmediate(obj.gameObject);
                                //!!!obj = null;
                                if (_queueIndex > 0)
                                {
                                    newData.Delay = GameData.GetDelayAfterQueueElement(_queue[_queueIndex - 1].Name);
                                } else
                                {
                                    newData.Delay = 2;
                                }
                                _queue[_queueIndex] = newData;// new PipesQueueElement(_awailableElements[jj], _queue[_queueIndex].Delay, _queue[_queueIndex].Angle, (int)pos.x, (int)pos.y, _queue[_queueIndex].Parameters);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            // REMOVE-INSERT-APPLY EPISODE <-

            // GRAPH ->
            GUILayout.Label("----------------------------------------------------------------------------------------------------------------", guiStyleBlack);
            GUILayout.Space(15);
            int startX = 15;
            int startY = 451;
            int stickWidth = 10;
            for (int i = 0; i < _queue.Count; ++i)
            {
                float stickHeight = 1 * _queue[i].Delay;
                int stickColor = 0;
                //if (_queue[i].Name.StartsWith("p_"))
                //{
                //    stickColor = 0;
                //} else
                //if (_queue[i].Name.StartsWith("w_"))
                //{
                //    stickColor = 1;
                //}
                Rect rect = new Rect();
                rect.x = startX + i * (stickWidth + 2);
                rect.y = startY - stickHeight;
                rect.width = stickWidth;
                rect.height = stickHeight;
                GUI.DrawTexture(rect, _colorSticksTextures[stickColor]);
            }
            if (_queueIndex >= 0)
            {
                Rect rect2 = new Rect();
                rect2.x = startX + _queueIndex * (stickWidth + 2);
                rect2.y = startY + 2;
                rect2.width = stickWidth;
                rect2.height = 5;
                GUI.DrawTexture(rect2, _colorSticksTextures[1]);
            }
            GUILayout.Label("----------------------------------------------------------------------------------------------------------------", guiStyleBlack);
            // GRAPH <-

            // DRAW ->
            GUILayout.BeginHorizontal();
            int startId = _queueIndex / QUEUE_ELEMENTS_IN_ROW * QUEUE_ELEMENTS_IN_ROW;
            for (int i = 0; i < QUEUE_ELEMENTS_IN_ROW; ++i)
            {
                int aid = i + startId;
                if (aid >= _queue.Count)
                {
                    break;
                }
                int asize = 60;
                //GUIStyle guiStyle = guiStyleBlack;
                //if (_queueIndex == aid)
                //{
                //    asize = 80;
                //    guiStyle = guiStyleRed;
                //}
                Texture atexture = _textureError;
                if (_awailableTextures.ContainsKey(_queue[aid].Name))
                {
                    atexture = _awailableTextures[_queue[aid].Name];
                }
                GUILayout.BeginVertical();
                GUILayout.Label(aid.ToString());
                GUILayout.Label(_queue[aid].Name);
                //
                GUILayout.Label("delay");
                string prevDelay = _queue[aid].Delay.ToString();
                string newDelay = GUILayout.TextField(prevDelay, 3, guiStyleRed);
                if (newDelay != prevDelay)
                {
                    int newIdelay = 0;
                    if (int.TryParse(newDelay, out newIdelay))
                    {
                        QueueElement qElement = _queue[aid];
                        qElement.Delay = newIdelay;
                        _queue[aid] = qElement;
                    }
                }
                //
                GUILayout.Label("parameters"); // divided by '|'
                string prevParameters = _queue[aid].Parameters;
                string newParameters = GUILayout.TextField(prevParameters, 20, guiStyleRed);
                if (newParameters != prevParameters)
                {
                    QueueElement qElement = _queue[aid];
                    qElement.Parameters = newParameters;
                    _queue[aid] = qElement;
                }
                //
                if (GUILayout.Button(atexture, GUILayout.Width(asize), GUILayout.Height(asize)))
                {
                    if (_queueIndex == aid)
                    {
                        _queueIndex = -1;
                    } else
                    {
                        _queueIndex = aid;
                    }
                    UpdateQueueIndexes();
                }
                GUILayout.EndVertical();
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
        // queue
        _queue.Clear();
        for (int i = 0; i < _myLevel.Queue.Count; ++i)
        {
            _queue.Add(_myLevel.Queue[i]);
        }
        if (_queue.Count > 0)
        {
            _currentQueueIndex = 0;
        } else
        {
            _currentQueueIndex = -1;
        }
        //
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
        // queue
        if (_myLevel.Queue == null)
        {
            _myLevel.Queue = new List<QueueElement>();
        }
        _myLevel.Queue.Clear();
        for (int i = 0; i < _queue.Count; ++i)
        {
            _myLevel.Queue.Add(_queue[i]);
        }
        //
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

    // queue
    private void OnLeftArrowClick()
    {
        if (_queueIndex > 0)
        {
            --_queueIndex;
        }
    }

    private void OnRightArrowClick()
    {
        if (_queue.Count > _queueIndex + 1)
        {
            ++_queueIndex;
        }
    }

    private void OnUpArrowClick()
    {
        if (_queueIndex >= QUEUE_ELEMENTS_IN_ROW)
        {
            _queueIndex -= QUEUE_ELEMENTS_IN_ROW;
        }
    }

    private void OnDownArrowClick()
    {
        if (_queue.Count > _queueIndex + QUEUE_ELEMENTS_IN_ROW)
        {
            _queueIndex += QUEUE_ELEMENTS_IN_ROW;
        }
    }

    private void ClearQueue()
    {
        _queue.Clear();
        _currentQueueIndex = -1;
    }

    private void AddElementToQueue()
    {
        QueueElement element = new QueueElement("HZ", 0, "");
        _queue.Add(element);
    }

    private void RemoveElementFromQueue()
    {
        _queue.RemoveAt(_queueIndex);
        if (_queueIndex > 0)
        {
            --_queueIndex;
        }
        else
        if (_queue.Count == 0)
        {
            _queueIndex = -1;
        }
    }

    private void UpdateQueueIndexes()
    {
        _indexesFilter.Clear();
        for (int i = 0; i < _awailableElements.Count; ++i)
        {
            if (_currentNameFilter == "" || _awailableElements[i].Contains(_currentNameFilter))
            {
                _indexesFilter.Add(i);
            }
        }
        if (_indexesFilter.Count > 0)
        {
            _currentQueueIndex = 0;
        }
        else
        {
            _currentQueueIndex = -1;
        }
    }

    //private GameObject CreateObjectTemporarly(string objName)
    //{
    //    Object prefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Objects/" + objName + ".prefab", typeof(GameObject));
    //    return ((GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity));
    //}

    private int PipesNameToEditorsInt(string pipesName)
    {
        return _elementsNames[pipesName];
    }
}