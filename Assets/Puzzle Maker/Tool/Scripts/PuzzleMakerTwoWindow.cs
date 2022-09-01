#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PuzzleMakerTwo
{
    public class PuzzleMakerTwoWindow : EditorWindow
    {
        private Sprite _tempSprite;
        private Texture2D _knob;
        private int _columns = 3;
        private int _rows = 3;
        private int _knobSize = 32;

        string _savePath = "MyPuzzleMaker/Output";
        private string _puzzleName = "mypuzzle";
        private PuzzlePiece _prefab;
        private PuzzleGame _puzzleGamePrefab;
        private PuzzleBoardLayout puzzleBoardLayout;
        private const string PREFIX = "PuzzleMakerValue";

        private Dictionary<string, Condition> _conditions =
            new Dictionary<string, Condition>();
        private bool _createGame;

        [MenuItem("Puzzle Maker Two/Open puzzle maker")]
        public static void ShowWindow()
        {
            var window = GetWindow<PuzzleMakerTwoWindow>();
        }

        private void OnGUI()
        {

            EditorGUILayout.LabelField("Puzzle Maker Window");
            EditorGUILayout.BeginHorizontal();

            //First Column
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Puzzle settings");

            if (_tempSprite != null && !_tempSprite.texture.isReadable)
            {
                var precolor = GUI.color;
                GUI.color = Color.red;
                _tempSprite = (Sprite)EditorGUILayout.ObjectField(_tempSprite, typeof(Sprite),false);
                EditorGUILayout.LabelField("Enable read/write access in for this sprite");
                GUI.color = precolor;
                _conditions[nameof(_tempSprite)].SetCondition(false);
            }
            else if (_tempSprite == null)
            {
                _conditions[nameof(_tempSprite)].SetCondition(false);
                _tempSprite = (Sprite)EditorGUILayout.ObjectField(_tempSprite, typeof(Sprite),false);
            }
        else

        {
                _tempSprite = (Sprite)EditorGUILayout.ObjectField(_tempSprite, typeof(Sprite),false);
                _conditions[nameof(_tempSprite)].SetCondition(true);
            }
            
            _columns = EditorGUILayout.IntField("How many columns",_columns);
            _conditions[nameof(_columns)].SetCondition(_columns > 1);
            _rows = EditorGUILayout.IntField("How many columns",_rows);
            _conditions[nameof(_rows)].SetCondition(_rows > 1);
            EditorGUILayout.LabelField("Knobs settings");
            
            if (_knob != null && !_knob.isReadable)
            {
                var precolor = GUI.color;
                GUI.color = Color.red;
                _knob = (Texture2D)EditorGUILayout.ObjectField(_knob, typeof(Sprite),false);
                EditorGUILayout.LabelField("Enable read/write access in for this Texture");
                GUI.color = precolor;
                _conditions[nameof(_knob)].SetCondition(false);
            }
            else if (_knob == null)
            {
                _conditions[nameof(_knob)].SetCondition(false);
            }
            else
            {
                _knob = (Texture2D)EditorGUILayout.ObjectField(_knob, typeof(Texture2D),false);
                _conditions[nameof(_knob)].SetCondition(true);
            }

            _knobSize = EditorGUILayout.IntField("Knob size",_knobSize);
            _conditions[nameof(_knobSize)].SetCondition(_knobSize > 0);

            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            _savePath = EditorGUILayout.TextField(_savePath);
            if (GUILayout.Button("Select folder"))
            {
                _savePath = EditorUtility.OpenFolderPanel("Select save folder", Application.dataPath, "");
                if (_savePath.StartsWith(Application.dataPath))
                    _savePath = _savePath.Substring(Application.dataPath.Length + 1);
            }
            
            EditorGUILayout.EndHorizontal();
            _puzzleName = EditorGUILayout.TextField(_puzzleName);
            var prefabs = MyEditorTools.Tools.FindAssetsWithExtension<PuzzleGame>(".prefab");
            var prefab = prefabs.FirstOrDefault(t => t.name == _puzzleName);
            
            _conditions[nameof(_puzzleName)].SetCondition(prefab == null);

            var isMissingCondition = _conditions.Values.Where(t => t.Accepted() == false).ToList();

            _createGame = EditorGUILayout.Toggle("Create Game",_createGame);
            if (isMissingCondition.Count > 0)
            {
                GUI.enabled = false;
                GUI.color = Color.magenta;
                
            }
            
            if (GUILayout.Button("Creat puzzle"))
            {
                var puzzleMakerTwo = new PuzzleMaker();
                puzzleMakerTwo.CreatPuzzle(_tempSprite, _columns, _rows, _knob, _knobSize, _savePath, _puzzleName,_createGame);
            }

            foreach (var condition in isMissingCondition)
            {
                EditorGUILayout.LabelField(condition.ErrorMessage);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

       

        private string GetPrefabPath(string dataPath, string prefabPath)
        {
            
            var combinedPath = $"{dataPath}/{prefabPath}.prefab";
            
            return combinedPath;

        }

        private void OnEnable()
        {
            LoadSettings();
            _conditions.Add(nameof(_columns),new Condition("You need at least 2 columns"));
            _conditions.Add(nameof(_rows), new Condition("You need at least 2 rows"));
            _conditions.Add(nameof(_knobSize), new Condition("Knob size needs to be 1 or greater"));
            _conditions.Add(nameof(_knob),new Condition("Select a knob with read/write access"));
            _conditions.Add(nameof(_tempSprite),new Condition("Select a Sprite with read/write access"));
            _conditions.Add(nameof(_puzzleName),new Condition("A prefab with that name already exists in that directory"));
        }

        

        void OnDestroy()
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            EditorPrefs.SetInt(PREFIX + "KnobSize", _knobSize);
            EditorPrefs.SetInt(PREFIX + "Columns", _columns);
            EditorPrefs.SetInt(PREFIX + "Rows", _rows);
            EditorPrefs.SetString(PREFIX + "path", _savePath);
            EditorPrefs.SetString(PREFIX + "puzzleName", _puzzleName);
            EditorPrefs.SetBool(PREFIX + "CreatGame", _createGame);

            if (_tempSprite != null)
            {
                var SpritePath = AssetDatabase.GetAssetPath(_tempSprite);
                EditorPrefs.SetString(PREFIX + "SpritePath", SpritePath);
            }

            if (_knob != null)
            {
                var knobPath = AssetDatabase.GetAssetPath(_knob);
                EditorPrefs.SetString(PREFIX + "KnobPath", knobPath);
            }
        }
        private void LoadSettings()
        {
            _knobSize = EditorPrefs.GetInt(PREFIX + "KnobSize");
            _columns = EditorPrefs.GetInt(PREFIX + "Columns");
            _rows = EditorPrefs.GetInt(PREFIX + "Rows");
            _createGame = EditorPrefs.GetBool(PREFIX + "CreatGame");
            var path = EditorPrefs.GetString(PREFIX + "path");
            if (!string.IsNullOrWhiteSpace(path))
                _savePath = path;
            var name = EditorPrefs.GetString(PREFIX + "puzzleName");
            if (!string.IsNullOrWhiteSpace(name))
                _puzzleName = name;

            var spritePath = EditorPrefs.GetString(PREFIX + "SpritePath");
            if (!string.IsNullOrEmpty(spritePath))
                _tempSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            var knobPath = EditorPrefs.GetString(PREFIX + "KnobPath");
            if (!string.IsNullOrEmpty(knobPath))
                _knob = AssetDatabase.LoadAssetAtPath<Texture2D>(knobPath);
        }
    }

    public class Condition
    {
        private bool _accepted;
        private string _errorMessage;

        public string ErrorMessage => _errorMessage;

        public Condition(string errorMessage)
        {
            _errorMessage = errorMessage;
        }

        public bool Accepted()
        {
            return _accepted;
        }
        public void SetCondition(bool b)
        {
            _accepted = b;
        }
    }
}
#endif