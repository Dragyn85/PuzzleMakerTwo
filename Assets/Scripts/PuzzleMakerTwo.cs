using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace PuzzleMakerTwo
{

    public class PuzzleMakerTwo : MonoBehaviour
    {
        [Header("1. Select sprite for puzzle")] [SerializeField]
        private Sprite _puzzleImageSprite;



        [Header("Decide, puzzle size with amount of rows/columns and pixels for the pieces")]

        [SerializeField] private int _columns = 3;
        [SerializeField] private int _rows = 3;

        [Header("Decide knob design, only symmetrical are supported.")] [SerializeField]
        private Texture2D _knob;

        [SerializeField] private int _knobSize = 32;

        [Header("Adjust puzzle overall size")] [SerializeField]
        private float _pixelsPerUnit = 100;

        [Header("SavingPaths")] [SerializeField]
        string _savePath = "MyPuzzleMaker/Output";

        [SerializeField] private string _puzzleName = "mypuzzle";


        [SerializeField] private PuzzlePiece _prefab;
        [SerializeField] private SpriteRenderer _puzzlePicture;


        private Grid<PuzzlePieceInit> _puzzleGrid;
        private const string PrefabOutputPrefix = "PiecePrefab";



        
        
        //[ContextMenu("Creat Puzzle")]
        [MenuItem("MyMenu/Puzzle Maker 2/Creat Puzzle")]
        public static void FindAndCreatPuzzle()
        {
            FindObjectOfType<PuzzleMakerTwo>().CreatPuzzle();
        }

        int sumToIndexArray(int[] array,int index)
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
            var width = _puzzleImageSprite.texture.width;
            var height = _puzzleImageSprite.texture.height;
            int[] puzzlePieceWidths = new int[_columns];
            int[] puzzlePieceHeights = new int[_rows];

            DividePixels(width, puzzlePieceWidths);
            DividePixels(height,puzzlePieceHeights);

            foreach (var pieceWidth in puzzlePieceWidths)
            {
                Debug.Log(pieceWidth);
            }

            var total = sumToIndexArray(puzzlePieceWidths, puzzlePieceWidths.Length - 1);
            Debug.Log("TOTAL pixels = " + total);
            
            
            //CODE BELLOW IS NOT REMADE 20/7 -22


            PuzzlePieceInit CreateGridObject(Grid<PuzzlePieceInit> g, int x, int y,int width, int height)
            {
                return new PuzzlePieceInit(g, x, y,height,width);
            }


            _puzzleGrid = new Grid<PuzzlePieceInit>(_columns, _rows, 1,
                Vector3.zero, CreateGridObject,puzzlePieceWidths,puzzlePieceHeights);
            
            var allPuzzlePieces = _puzzleGrid.GetAll();


            for (int i = 0; i < allPuzzlePieces.Count; i++)
            {
                allPuzzlePieces[i].SetKnobs();
            }

            var totalPieceSizeWidth =  + _knobSize + _knobSize;
            var totalPieceSizeHeight = _puzzlePieceSize + _knobSize + _knobSize;
            Texture2D rightKnobTextureMale = MakeKnobTexture();
            rightKnobTextureMale.Apply();

            var downKnobTextureMale = TextureRotate.rotateTexture(rightKnobTextureMale, true);
            var leftKnobTextureMale = TextureRotate.rotateTexture(downKnobTextureMale, true);
            var upperKnobTextureMale = TextureRotate.rotateTexture(rightKnobTextureMale, false);

            Texture2D leftKnobTextureFemale = SpriteMerger.InvertMask(rightKnobTextureMale);

            var upperKnobTextureFemale = TextureRotate.rotateTexture(leftKnobTextureFemale, true);
            var rightKnobTextureFemale = TextureRotate.rotateTexture(upperKnobTextureFemale, true);
            var downKnobTextureFemale = TextureRotate.rotateTexture(leftKnobTextureFemale, false);


            Texture2D maskTexture = new Texture2D(totalPieceSize, totalPieceSize);
            for (int x = 0; x < maskTexture.width; x++)
            for (int y = 0; y < maskTexture.height; y++)
                maskTexture.SetPixel(x, y, Color.clear);
            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(maskTexture);
            Texture2D mainBody = new Texture2D(_puzzlePieceSize, _puzzlePieceSize);
            for (int x = 0; x < mainBody.width; x++)
            {
                for (int y = 0; y < mainBody.height; y++)
                {
                    mainBody.SetPixel(x, y, Color.white);
                }
            }

            mainBody = SpriteMerger.InsertTextureWithOffset(maskTexture, mainBody, new Vector2(_knobSize, _knobSize));
            textures.Add(mainBody);
            var finalPiecePre = SpriteMerger.MergePuzzleMaskTexture(textures.ToArray());
            PuzzlePieceInit tempPieceInit;

            foreach (var puzzlePiece in _puzzleGrid.GetAll())
            {
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
                            new Vector2(maskTexture.width - _knobSize,
                                puzzlePiece.GetKnobs().Right.pos * _puzzlePieceSize + _knobSize / 2));
                    //new Vector2(maskTexture.width - _knobSize , maskTexture.height / 2 - _knobSize / 2));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            rightKnobTextureFemale,
                            new Vector2(maskTexture.width - _knobSize * 2,
                                puzzlePiece.GetKnobs().Right.pos * _puzzlePieceSize + _knobSize / 2));
                }

                if (puzzlePiece.HasNeighbourTop)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.up);
                    if (!tempPieceInit.IsKnobMale(Vector2.down))
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            upperKnobTextureMale,
                            new Vector2(puzzlePiece.GetKnobs().Top.pos * _puzzlePieceSize + _knobSize / 2,
                                maskTexture.height - _knobSize));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            upperKnobTextureFemale,
                            new Vector2(puzzlePiece.GetKnobs().Top.pos * _puzzlePieceSize + _knobSize / 2,
                                maskTexture.height - _knobSize * 2));
                }

                if (puzzlePiece.HasNeighbourLeft)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.left);
                    if (!tempPieceInit.IsKnobMale(Vector2.right))
                    {
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            leftKnobTextureMale,
                            new Vector2(0,
                                puzzlePiece.GetKnobs().Left.pos * _puzzlePieceSize + _knobSize / 2));
                    }
                    else
                    {
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            leftKnobTextureFemale,
                            new Vector2(_knobSize,
                                puzzlePiece.GetKnobs().Left.pos * _puzzlePieceSize + _knobSize / 2));
                    }
                }

                if (puzzlePiece.HasNeighbourDown)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.down);
                    if (!tempPieceInit.IsKnobMale(Vector2.up))
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            downKnobTextureMale,
                            new Vector2(puzzlePiece.GetKnobs().Down.pos * _puzzlePieceSize + (_knobSize / 2),
                                0));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            downKnobTextureFemale,
                            new Vector2(puzzlePiece.GetKnobs().Down.pos * _puzzlePieceSize + (_knobSize / 2),
                                _knobSize));
                }

                finalMask.Apply();
                puzzlePiece.SetTexture(finalMask);





            }

            int count = 0;


            Vector3 puzzleOrigin = new Vector3();
            PuzzleInfo puzzleInfo = new PuzzleInfo();
            puzzleInfo.path = Path.Combine(Application.dataPath, _savePath + "/info.txt");
            foreach (var piece in allPuzzlePieces)
            {
                count++;
                var mask = piece.GetMask();
                var spriteMask = Sprite.Create(mask, new Rect(0, 0, mask.width, mask.height), new Vector2(0.5f, 0.5f),
                    _pixelsPerUnit);
                spriteMask.name = $"X{piece.X} Y{piece.Y}";


                var prefab = Instantiate(_prefab,
                    new Vector3(piece.X * _puzzlePieceSize / _pixelsPerUnit,
                        piece.Y * _puzzlePieceSize / _pixelsPerUnit, 0),
                    quaternion.identity);
                if (piece.X == 0 && piece.Y == 0)
                    puzzleOrigin = prefab.transform.position;

                var worldCoordinatesOffset = prefab.transform.position - puzzleOrigin;
                var newPuzzlePieceData = new puzzlePieceData(worldCoordinatesOffset.x, worldCoordinatesOffset.y, count);
                //puzzleInfo.positions.Add(worldCoordinatesOffset);
                puzzleInfo.PuzzlePieceDatas.Add(newPuzzlePieceData);
                //prefab.SetPosition(worldCoordinatesOffset);



                var path = _savePath + "/" + _puzzleName + "mask" + count + ".png";
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var savedSprite = SaveSpriteAsAsset(spriteMask, path);
                prefab.SetMask(savedSprite);



                var pictureInstance = Instantiate(_puzzlePicture, prefab.transform, true);

                pictureInstance.sprite = _puzzleImageSprite;
                pictureInstance.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

                var prefabPath = Path.Combine(Application.dataPath,
                    _savePath + "/" + _puzzleName + PrefabOutputPrefix + count + ".prefab");
                PrefabUtility.SaveAsPrefabAsset(prefab.gameObject, prefabPath);

                DestroyImmediate(prefab.gameObject);

                AssetDatabase.SaveAssets();

            }

            AssetDatabase.Refresh();
            
            printPuzzleInfo(puzzleInfo);
        }

        private void DividePixels(int amount, int[] TargetArray)
        {
            var first = (int)Mathf.Floor(amount / _columns);

            // Calculater individual pieces width
            TargetArray[0] = first;
            for (int i = 1; i < TargetArray.Length - 1; i++)
            {
                var currentBaseXpos = ((float)amount / _columns) * (i + 1);
                int temp = sumToIndexArray(TargetArray, i - 1) + first;

                if (currentBaseXpos - temp >= 1)
                    TargetArray[i] = first + 1;
                else
                    TargetArray[i] = first;
            }

            var currentSum = sumToIndexArray(TargetArray, TargetArray.Length - 1);
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

        void printPuzzleInfo(PuzzleInfo puzzleInfo)
        {

            var json = JsonUtility.ToJson(puzzleInfo);
            System.IO.File.WriteAllText(puzzleInfo.path, json);
            System.IO.File.WriteAllText(puzzleInfo.path, JsonConvert.SerializeObject(puzzleInfo));

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

    [Serializable]
    public class puzzlePieceData
    {
        public int PieceNumber { get; private set; }
        public float x;
        public float y;


        public puzzlePieceData(float x, float y, int pieceNumber)
        {
            PieceNumber = pieceNumber;
            this.x = x;
            this.y = y;
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