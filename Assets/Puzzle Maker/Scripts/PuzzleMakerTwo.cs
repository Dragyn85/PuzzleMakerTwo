#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace PuzzleMakerTwo
{

    public class PuzzleMakerTwo : EditorWindow
    {
        private Sprite _puzzleImageSprite;
        private Sprite _tempSprite;
        private Texture2D _knob;
        private int _columns = 3;
        private int _rows = 3;
        private int _knobSize = 32;

        string _savePath = "MyPuzzleMaker/Output";
        private string _puzzleName = "mypuzzle";
        private PuzzlePiece _prefab;
        private PuzzleGame _puzzleGamePrefab;
        private PuzzleBoard<PuzzlePieceInit> _puzzleBoard;
        private const string PREFIX = "PuzzleMakerValue";



        [MenuItem("Puzzle Maker Two/Open puzzle maker")]
        public static void ShowWindow()
        {
            var window = GetWindow<PuzzleMakerTwo>();
            
        }
        
        

        private void OnGUI()
        {
            
            
            EditorGUILayout.LabelField("Puzzle Maker Window");
            EditorGUILayout.BeginHorizontal();
            
            //First Column
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Puzzle settings");
            _tempSprite = (Sprite)EditorGUILayout.ObjectField(_tempSprite, typeof(Sprite));
            _columns = EditorGUILayout.IntField("How many columns",_columns);
            _rows = EditorGUILayout.IntField("How many columns",_rows);
            EditorGUILayout.LabelField("Knobs settings");
            _knob = (Texture2D)EditorGUILayout.ObjectField(_knob, typeof(Texture2D));
            _knobSize = EditorGUILayout.IntField("Knob size",_knobSize);

            _puzzleGamePrefab = (PuzzleGame)EditorGUI.ObjectField(Rect.zero, _puzzleGamePrefab, typeof(PuzzleGame));
            _prefab = (PuzzlePiece)EditorGUI.ObjectField(Rect.zero, _prefab, typeof(PuzzlePiece));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            
            if (GUILayout.Button("Creat puzzle"))
                CreatPuzzle();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void OnEnable()
        {
            _knobSize = EditorPrefs.GetInt(PREFIX + "KnobSize");
            _columns = EditorPrefs.GetInt(PREFIX + "Columns");
            _rows = EditorPrefs.GetInt(PREFIX+"Rows");
            
            var spritePath = EditorPrefs.GetString(PREFIX + "SpritePath");
            if (!string.IsNullOrEmpty(spritePath))
                _puzzleImageSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            
            var knobPath = EditorPrefs.GetString(PREFIX + "KnobPath");
            if (!string.IsNullOrEmpty(knobPath))
                _knob = AssetDatabase.LoadAssetAtPath<Texture2D>(knobPath);
            
            
        }

        void OnDestroy()
        {
            EditorPrefs.SetInt(PREFIX+"KnobSize",_knobSize);
            EditorPrefs.SetInt(PREFIX+"Columns",_columns);
            EditorPrefs.SetInt(PREFIX+"Rows",_rows);

            if (_puzzleImageSprite != null)
            {
                var SpritePath =AssetDatabase.GetAssetPath(_puzzleImageSprite);
                EditorPrefs.SetString(PREFIX+"SpritePath",SpritePath);
            }
            if (_puzzleImageSprite != null)
            {
                var knobPath =AssetDatabase.GetAssetPath(_knob);
                EditorPrefs.SetString(PREFIX+"KnobPath",knobPath);
            }


        }

        int sumOfIntsToIndexArray(int[] array,int index)
        {

            
            
            var sum = 0;
            for (int i = 0; i <= index; i++)
            {
                sum += array[i];
            }

            return sum;
        }

        public void CreatPuzzle()
        {
            _puzzleImageSprite = Sprite.Create(_tempSprite.texture,_tempSprite.rect,_tempSprite.pivot,_tempSprite.pixelsPerUnit);
            var puzzleGames = MyEditorTools.Tools.FindAssetsWithExtension<PuzzleGame>(".prefab");
            _puzzleGamePrefab = puzzleGames.FirstOrDefault(t => t.name == "PuzzleMakerPuzzle - GamePrefab");
            if (_puzzleGamePrefab == null)
            {
                Debug.LogError("Can not find PuzzleMakerPuzzle - GamePrefab, ");
                return;
            }

            var puzzlePieces = MyEditorTools.Tools.FindAssetsWithExtension<PuzzlePiece>(".prefab");
            _prefab = puzzlePieces.FirstOrDefault(t => t.name == "PuzzleMakerPuzzle - PiecePrefab");
            if (_prefab == null)
            {
                Debug.LogError("Can not find PuzzleMakerPuzzle - PiecePrefab");
                return;
            }
            
            Texture2D puzzleTexture = CopyTexture2D(_puzzleImageSprite.texture);
            var width = _puzzleImageSprite.texture.width;
            var height = _puzzleImageSprite.texture.height;
            int[] puzzlePieceWidths = new int[_columns];
            int[] puzzlePieceHeights = new int[_rows];
            

            DividePixels(width, puzzlePieceWidths);
            DividePixels(height,puzzlePieceHeights);

            var total = sumOfIntsToIndexArray(puzzlePieceWidths, puzzlePieceWidths.Length - 1);
            
            //Method for creating puzzlePieces passed to the PuzzleBoard
            PuzzlePieceInit CreatePuzzlePiece(PuzzleBoard<PuzzlePieceInit> g, int x, int y,int width, int height)
            {
                return new PuzzlePieceInit(g, x, y,height,width);
            }

            //Creat PuzzleBoard
            _puzzleBoard = new PuzzleBoard<PuzzlePieceInit>(_columns, _rows, 1,
                Vector3.zero, CreatePuzzlePiece,puzzlePieceWidths,puzzlePieceHeights);

            
            var allPuzzlePieces = _puzzleBoard.GetAll();

            
            for (int i = 0; i < allPuzzlePieces.Count; i++)
            {
                allPuzzlePieces[i].SetKnobs();
            }

            //Creats Male and Female Knobs in all direction in the correct size.
            Texture2D rightKnobTextureMale = MakeKnobTexture();
            rightKnobTextureMale.Apply();

            var downKnobTextureMale = TextureRotate.rotateTexture(rightKnobTextureMale, true);
            var leftKnobTextureMale = TextureRotate.rotateTexture(downKnobTextureMale, true);
            var upperKnobTextureMale = TextureRotate.rotateTexture(rightKnobTextureMale, false);

            Texture2D leftKnobTextureFemale = SpriteMerger.InvertMask(rightKnobTextureMale);

            var upperKnobTextureFemale = TextureRotate.rotateTexture(leftKnobTextureFemale, true);
            var rightKnobTextureFemale = TextureRotate.rotateTexture(upperKnobTextureFemale, true);
            var downKnobTextureFemale = TextureRotate.rotateTexture(leftKnobTextureFemale, false);

            
            foreach (var puzzlePiece in allPuzzlePieces)
            {
                List<Texture2D> textures = new List<Texture2D>();
                Texture2D maskTexture2D = new Texture2D(puzzlePiece.Width+_knobSize*2, puzzlePiece.Height+_knobSize*2);
                for (int x = 0; x < maskTexture2D.width; x++)
                    for (int y = 0; y < maskTexture2D.height; y++)
                        maskTexture2D.SetPixel(x, y, Color.clear);
                
                textures.Add(maskTexture2D);
                
                Texture2D mainBody = new Texture2D(puzzlePiece.Width, puzzlePiece.Height);
                for (int x = 0; x < mainBody.width; x++)
                    for (int y = 0; y < mainBody.height; y++)
                        mainBody.SetPixel(x, y, Color.white);
                
                mainBody = SpriteMerger.InsertTextureWithOffset(maskTexture2D, mainBody, new Vector2(_knobSize, _knobSize));
                textures.Add(mainBody);
                
                var finalPiecePre = SpriteMerger.MergePuzzleMaskTexture(textures.ToArray());
                PuzzlePieceInit tempPieceInit;
                
                var finalMask = new Texture2D(finalPiecePre.width, finalPiecePre.height);
                var size = finalMask.height;
                var colors = finalPiecePre.GetPixels();
                finalMask.SetPixels(colors);
                
                if (puzzlePiece.HasNeighbourRight)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.right);
                    if (!tempPieceInit.IsKnobMale(Vector2.left))
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            rightKnobTextureMale,
                            new Vector2(maskTexture2D.width - _knobSize,
                                puzzlePiece.GetKnobs().Right.pos * puzzlePiece.Height + _knobSize / 2));
                    //new Vector2(maskTexture.width - _knobSize , maskTexture.height / 2 - _knobSize / 2));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            rightKnobTextureFemale,
                            new Vector2(maskTexture2D.width - _knobSize * 2,
                                puzzlePiece.GetKnobs().Right.pos * puzzlePiece.Height + _knobSize / 2));
                }
                
                if (puzzlePiece.HasNeighbourTop)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.up);
                    if (!tempPieceInit.IsKnobMale(Vector2.down))
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            upperKnobTextureMale,
                            new Vector2(puzzlePiece.GetKnobs().Top.pos * puzzlePiece.Width + _knobSize / 2,
                                maskTexture2D.height - _knobSize));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            upperKnobTextureFemale,
                            new Vector2(puzzlePiece.GetKnobs().Top.pos * puzzlePiece.Width + _knobSize / 2,
                                maskTexture2D.height - _knobSize * 2));
                }

                if (puzzlePiece.HasNeighbourLeft)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.left);
                    if (!tempPieceInit.IsKnobMale(Vector2.right))
                    {
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            leftKnobTextureMale,
                            new Vector2(0,
                                puzzlePiece.GetKnobs().Left.pos * puzzlePiece.Height + _knobSize / 2));
                    }
                    else
                    {
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            leftKnobTextureFemale,
                            new Vector2(_knobSize,
                                puzzlePiece.GetKnobs().Left.pos * puzzlePiece.Height + _knobSize / 2));
                    }
                }

                if (puzzlePiece.HasNeighbourDown)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.down);
                    if (!tempPieceInit.IsKnobMale(Vector2.up))
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            downKnobTextureMale,
                            new Vector2(puzzlePiece.GetKnobs().Down.pos * puzzlePiece.Width + (_knobSize / 2),
                                0));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            downKnobTextureFemale,
                            new Vector2(puzzlePiece.GetKnobs().Down.pos * puzzlePiece.Width + (_knobSize / 2),
                                _knobSize));
                }
                

                finalMask.Apply();
                puzzlePiece.SetTexture(finalMask);
            }

            int count = 0;


            var ppu = _puzzleImageSprite.pixelsPerUnit;
            PuzzleGame parent = Instantiate(_puzzleGamePrefab,
                Vector3.zero, Quaternion.identity);
            parent.name = _puzzleName;
            var puzzleWidthWorldSpace = _puzzleImageSprite.texture.width / ppu;
            var puzzleHeightWorldSpace = _puzzleImageSprite.texture.height / ppu;
            
            foreach (var piece in allPuzzlePieces)
            {
                count++;
                var mask = piece.GetMask();
                var spriteMask = Sprite.Create(mask, new Rect(0, 0, mask.width, mask.height), new Vector2(0.5f, 0.5f),
                    ppu);
                spriteMask.name = $"X{piece.X} Y{piece.Y}";
                
                var correctX = ((sumOfIntsToIndexArray(puzzlePieceWidths ,((int)piece.X-1))   / ppu) + ( piece.Width / ppu) / 2) 
                    - (puzzleWidthWorldSpace/2);
                var correctY = ((sumOfIntsToIndexArray(puzzlePieceHeights,((int)piece.Y-1))   / ppu) + (piece.Height / ppu) / 2)
                    - (puzzleHeightWorldSpace/2);
                
                var prefab = Instantiate(_prefab,
                    new Vector3(
                        correctX,
                        correctY,
                        0),
                    quaternion.identity,
                    parent.transform);
                
                var correctPositionOriginLowerLeft = prefab.transform.localPosition;
                
                
                //prefab.SetPosition(new Vector2(correctX,correctY));
                prefab.name = $"PuzzlePiece {count}";

                var newTexture = new Texture2D(mask.width, mask.height);
                var startPixelX = sumOfIntsToIndexArray(puzzlePieceWidths, (int)piece.X-1)-_knobSize;
                var startPixelY = sumOfIntsToIndexArray(puzzlePieceHeights, (int)piece.Y-1)-_knobSize;
                
                for (int x = 0; x < mask.width; x++)
                    for (int y = 0; y < mask.height; y++)
                        if (mask.GetPixel(x, y) == Color.white)
                            newTexture.SetPixel(x,y,puzzleTexture.GetPixel(x + startPixelX, y + startPixelY));
                        else
                            newTexture.SetPixel(x,y,Color.clear);
                        
                var texturepath = _savePath + "/" + _puzzleName + count + ".png";
                
                if(Directory.Exists(Path.GetDirectoryName(texturepath)))
                    Debug.Log("Ask to overwrite");
                Directory.CreateDirectory(Path.GetDirectoryName(texturepath));
                var textureSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f),
                    ppu);
                var savedSpriteaswell = SaveSpriteAsAsset(textureSprite, texturepath);
                prefab.SetSprite(savedSpriteaswell);
                //prefab.SetPosition(new Vector2(correctX,correctY));
                prefab.SetCorrectPosition(correctX, correctY);
                prefab.SetBoardPosition(piece.X, piece.Y);
                prefab.SetCornerStart(sumOfIntsToIndexArray(puzzlePieceWidths, piece.X - 1),
                    sumOfIntsToIndexArray(puzzlePieceHeights, piece.Y - 1));
                
                var puzzleMoveGrabCollider = prefab.gameObject.AddComponent<BoxCollider2D>();
                puzzleMoveGrabCollider.isTrigger = false;
                puzzleMoveGrabCollider.size =
                    new Vector2((newTexture.width - 2 * _knobSize)/ppu, (newTexture.height - 2 * _knobSize)/ppu);

                AssetDatabase.SaveAssets();

            }
            var parentPrefabPath = Path.Combine(Application.dataPath,
                _savePath + "/" + _puzzleName + ".prefab");
            parent.FindPieces();
            parent.SetBackGround(_puzzleImageSprite);
            var heightOfPieces = (float)height / _rows + _knobSize * 2;
            parent.SetPiecesDistance(heightOfPieces/ppu);
            var parentPrefab = PrefabUtility.SaveAsPrefabAsset(parent.gameObject,parentPrefabPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            DestroyImmediate(parent.gameObject);
        }

        

        private Texture2D CopyTexture2D(Texture2D texture)
        {
            Texture2D copy = new Texture2D(texture.width, texture.height);
            copy.SetPixels(texture.GetPixels());
            return copy;
        }

        private void DividePixels(int amount, int[] TargetArray)
        {
            var first = (int)Mathf.Floor(amount / TargetArray.Length);

            // Calculater individual pieces width
            TargetArray[0] = first;
            for (int i = 1; i < TargetArray.Length - 1; i++)
            {
                var currentBaseXpos = ((float)amount / _columns) * (i + 1);
                int temp = sumOfIntsToIndexArray(TargetArray, i - 1) + first;

                if (currentBaseXpos - temp >= 1)
                    TargetArray[i] = first + 1;
                else
                    TargetArray[i] = first;
            }

            var currentSum = sumOfIntsToIndexArray(TargetArray, TargetArray.Length - 1);
            TargetArray[TargetArray.Length - 1] = amount - currentSum;
        }


        Sprite SaveSpriteAsAsset(Sprite sprite, string proj_path)
        {
            var abs_path = Path.Combine(Application.dataPath, proj_path);
            proj_path = Path.Combine("Assets", proj_path);

            Directory.CreateDirectory(Path.GetDirectoryName(abs_path));
            File.WriteAllBytes(abs_path, ImageConversion.EncodeToPNG(sprite.texture));

            AssetDatabase.Refresh();


            var ti = AssetImporter.GetAtPath(proj_path) as TextureImporter;
            ti.spritePixelsPerUnit = sprite.pixelsPerUnit;
            ti.mipmapEnabled = false;
            ti.textureType = TextureImporterType.Sprite;

            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(proj_path);

        }


        private Texture2D MakeKnobTexture()
        {
            var newTexture = new Texture2D(_knob.width, _knob.height);
            for (int x = 0; x < newTexture.width; x++)
            {
                for (int y = 0; y < newTexture.height; y++)
                {
                    newTexture.SetPixel(x, y, _knob.GetPixel(x, y));
                }
            }

            newTexture.Apply();
            TextureScale.Scale(newTexture, _knobSize, _knobSize);


            return newTexture;
        }
    }



    [Serializable]
    public class PuzzleInfo
    {
        public List<puzzlePieceData> PuzzlePieceDatas;

        //public List<Vector2> positions;
        public string path;
        public string PieceCountPrefix;

        public PuzzleInfo()
        {
            PuzzlePieceDatas = new List<puzzlePieceData>();
        }
    }

    public struct Knobs
    {
        public knob Top;
        public knob Left;
        public knob Right;
        public knob Down;
    }

    public struct knob
    {
        public PuzzlePieceInit PuzzlePieceInit;
        public bool male;
        public float pos;
    }

}
#endif