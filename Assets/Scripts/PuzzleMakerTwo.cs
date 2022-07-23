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

        [Header("SavingPaths")] [SerializeField]
        string _savePath = "MyPuzzleMaker/Output";

        [SerializeField] private string _puzzleName = "mypuzzle";


        [SerializeField] private PuzzlePiece _prefab;
        [SerializeField] private SpriteRenderer _puzzlePicture;
        [SerializeField] private PuzzleGame _puzzleGamePrefab;


        private Grid<PuzzlePieceInit> _puzzleGrid;
        private const string PrefabOutputPrefix = "PiecePrefab";



        
        
        //[ContextMenu("Creat Puzzle")]
        [MenuItem("MyMenu/Creat Puzzle")]
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
            Texture2D puzzleTexture = CopyTexture2D(_puzzleImageSprite.texture);
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

            //var totalPieceSizeWidth = _ + _knobSize + _knobSize;
            //var totalPieceSizeHeight = _puzzlePieceSize + _knobSize + _knobSize;
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


            Vector3 puzzleOrigin = Vector3.zero;
            PuzzleInfo puzzleInfo = new PuzzleInfo();
            puzzleInfo.path = Path.Combine(Application.dataPath, _savePath + "/info.txt");
            var ppu = _puzzleImageSprite.pixelsPerUnit;
            PuzzleGame parent = Instantiate(_puzzleGamePrefab,
                Vector3.zero, Quaternion.identity);
            parent.name = _puzzleName;
            
            foreach (var piece in allPuzzlePieces)
            {
                count++;
                var mask = piece.GetMask();
                var spriteMask = Sprite.Create(mask, new Rect(0, 0, mask.width, mask.height), new Vector2(0.5f, 0.5f),
                    ppu);
                spriteMask.name = $"X{piece.X} Y{piece.Y}";

/*
                var prefab = Instantiate(_prefab,
                    Vector3.zero, 
                    quaternion.identity,
                    parent.transform);
                */
                
                 var prefab = Instantiate(_prefab,
                    new Vector3(
                        (sumToIndexArray(puzzlePieceWidths,((int)piece.X-1)) / ppu)+(piece.Width/ppu)/2,
                        (sumToIndexArray(puzzlePieceHeights,((int)piece.Y-1)) / ppu)+(piece.Height/ppu)/2,
                        0),
                    quaternion.identity,
                    parent.transform);
                
                
                //if (piece.X == 0 && piece.Y == 0)
                //puzzleOrigin = prefab.transform.position;

                
                var correctPosition = prefab.transform.localPosition;
                //var newPuzzlePieceData = new puzzlePieceData(correctPosition.x, correctPosition.y, count);
                prefab.SetPosition(correctPosition);
                //puzzleInfo.positions.Add(worldCoordinatesOffset);
                puzzleInfo.PuzzlePieceDatas.Add(prefab.GetData());
                //prefab.SetPosition(worldCoordinatesOffset);

                var newTexture = new Texture2D(mask.width, mask.height);
                var startPixelX = sumToIndexArray(puzzlePieceWidths, (int)piece.X-1)-_knobSize;
                var startPixelY = sumToIndexArray(puzzlePieceHeights, (int)piece.Y-1)-_knobSize;
                
                for (int x = 0; x < mask.width; x++)
                {
                    for (int y = 0; y < mask.height; y++)
                    {
                        if (mask.GetPixel(x, y) == Color.white)
                        {
                            if(y==300)
                                Debug.Log("Bigger then 400");
                            newTexture.SetPixel(x,y,puzzleTexture.GetPixel(x + startPixelX, y + startPixelY));
                        }
                        else
                        {
                            newTexture.SetPixel(x,y,Color.clear);
                        }
                    }
                    
                }
                var path = _savePath + "/" + _puzzleName + "mask" + count + ".png";
                var texturepath = _savePath + "/" + _puzzleName + count + ".png";
                
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var textureSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f),
                    ppu);
                var savedSpriteaswell = SaveSpriteAsAsset(textureSprite, texturepath);
                var savedSprite = SaveSpriteAsAsset(spriteMask, path);
                prefab.SetMask(savedSpriteaswell);
                



                var pictureInstance = Instantiate(_puzzlePicture,new Vector3(width/2/ppu,height/2/ppu),Quaternion.identity,prefab.transform);

                pictureInstance.sprite = _puzzleImageSprite;
                pictureInstance.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

                var prefabPath = Path.Combine(Application.dataPath,
                    _savePath + "/" + _puzzleName + PrefabOutputPrefix + count + ".prefab");
                PrefabUtility.SaveAsPrefabAsset(prefab.gameObject, prefabPath);

                //DestroyImmediate(prefab.gameObject);

                AssetDatabase.SaveAssets();

            }

            AssetDatabase.Refresh();
            
            printPuzzleInfo(puzzleInfo);
            parent.FindPieces();
            parent.SetInfo(puzzleInfo);
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