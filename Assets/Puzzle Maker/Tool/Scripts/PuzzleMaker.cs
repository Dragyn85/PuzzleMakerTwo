#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TextureEditing;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace PuzzleMakerTwo
{
    public class PuzzleMaker
    {
        private PuzzleGame _puzzleGamePrefab;
        private PuzzlePiece _puzzlePiecePrefab;
        private PuzzleBoard<PuzzlePieceInit> _puzzleBoard;
        private int _columns;
        private int _rows;
        private int _knobSize;
        private Texture2D _knobTexture2D;
        private const string PREFIX = "PuzzleMakerValue";

        
       

        int sumOfIntsToIndexArray(int[] array,int index)
        {

            
            
            var sum = 0;
            for (int i = 0; i <= index; i++)
            {
                sum += array[i];
            }

            return sum;
        }

        public void CreatPuzzle(Sprite puzzleImageSprite, int columns, int rows,Texture2D knobTexture2D, int knobSize, string savePath,string puzzleName)
        {
            _columns = columns;
            _rows = rows;
            _knobSize = knobSize;
            _knobTexture2D = knobTexture2D;
            var puzzleGames = MyEditorTools.Tools.FindAssetsWithExtension<PuzzleGame>(".prefab");
            _puzzleGamePrefab = puzzleGames.FirstOrDefault(t => t.name == "PuzzleMakerPuzzle - GamePrefab");
            if (_puzzleGamePrefab == null)
            {
                Debug.LogError("Can not find PuzzleMakerPuzzle - GamePrefab, ");
                return;
            }

            var puzzlePieces = MyEditorTools.Tools.FindAssetsWithExtension<PuzzlePiece>(".prefab");
            _puzzlePiecePrefab = puzzlePieces.FirstOrDefault(t => t.name == "PuzzleMakerPuzzle - PiecePrefab");
            if (_puzzlePiecePrefab == null)
            {
                Debug.LogError("Can not find PuzzleMakerPuzzle - PiecePrefab");
                return;
            }
            
            Texture2D puzzleTexture = CopyTexture2D(puzzleImageSprite.texture);
            var width = puzzleImageSprite.texture.width;
            var height = puzzleImageSprite.texture.height;
            int[] puzzlePieceWidths = new int[columns];
            int[] puzzlePieceHeights = new int[rows];
            

            DividePixels(width, puzzlePieceWidths);
            DividePixels(height,puzzlePieceHeights);

            var total = sumOfIntsToIndexArray(puzzlePieceWidths, puzzlePieceWidths.Length - 1);
            
            //Method for creating puzzlePieces passed to the PuzzleBoard
            PuzzlePieceInit CreatePuzzlePiece(PuzzleBoard<PuzzlePieceInit> g, int x, int y,int width, int height)
            {
                return new PuzzlePieceInit(g, x, y,height,width);
            }

            //Creat PuzzleBoard
            _puzzleBoard = new PuzzleBoard<PuzzlePieceInit>(columns, rows, 1,
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
                Texture2D maskTexture2D = new Texture2D(puzzlePiece.Width+knobSize*2, puzzlePiece.Height+knobSize*2);
                for (int x = 0; x < maskTexture2D.width; x++)
                    for (int y = 0; y < maskTexture2D.height; y++)
                        maskTexture2D.SetPixel(x, y, Color.clear);
                
                textures.Add(maskTexture2D);
                
                Texture2D mainBody = new Texture2D(puzzlePiece.Width, puzzlePiece.Height);
                for (int x = 0; x < mainBody.width; x++)
                    for (int y = 0; y < mainBody.height; y++)
                        mainBody.SetPixel(x, y, Color.white);
                
                mainBody = SpriteMerger.InsertTextureWithOffset(maskTexture2D, mainBody, new Vector2(knobSize, knobSize));
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
                            new Vector2(maskTexture2D.width - knobSize,
                                puzzlePiece.GetKnobs().Right.pos * puzzlePiece.Height + knobSize / 2));
                    //new Vector2(maskTexture.width - _knobSize , maskTexture.height / 2 - _knobSize / 2));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            rightKnobTextureFemale,
                            new Vector2(maskTexture2D.width - knobSize * 2,
                                puzzlePiece.GetKnobs().Right.pos * puzzlePiece.Height + knobSize / 2));
                }
                
                if (puzzlePiece.HasNeighbourTop)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.up);
                    if (!tempPieceInit.IsKnobMale(Vector2.down))
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            upperKnobTextureMale,
                            new Vector2(puzzlePiece.GetKnobs().Top.pos * puzzlePiece.Width + knobSize / 2,
                                maskTexture2D.height - knobSize));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            upperKnobTextureFemale,
                            new Vector2(puzzlePiece.GetKnobs().Top.pos * puzzlePiece.Width + knobSize / 2,
                                maskTexture2D.height - knobSize * 2));
                }

                if (puzzlePiece.HasNeighbourLeft)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.left);
                    if (!tempPieceInit.IsKnobMale(Vector2.right))
                    {
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            leftKnobTextureMale,
                            new Vector2(0,
                                puzzlePiece.GetKnobs().Left.pos * puzzlePiece.Height + knobSize / 2));
                    }
                    else
                    {
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            leftKnobTextureFemale,
                            new Vector2(knobSize,
                                puzzlePiece.GetKnobs().Left.pos * puzzlePiece.Height + knobSize / 2));
                    }
                }

                if (puzzlePiece.HasNeighbourDown)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.down);
                    if (!tempPieceInit.IsKnobMale(Vector2.up))
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            downKnobTextureMale,
                            new Vector2(puzzlePiece.GetKnobs().Down.pos * puzzlePiece.Width + (knobSize / 2),
                                0));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            downKnobTextureFemale,
                            new Vector2(puzzlePiece.GetKnobs().Down.pos * puzzlePiece.Width + (knobSize / 2),
                                knobSize));
                }
                

                finalMask.Apply();
                puzzlePiece.SetTexture(finalMask);
            }

            int count = 0;


            var ppu = puzzleImageSprite.pixelsPerUnit;
            PuzzleGame parent = GameObject.Instantiate(_puzzleGamePrefab,
                Vector3.zero, Quaternion.identity);
            parent.name = puzzleName;
            var puzzleWidthWorldSpace = puzzleImageSprite.texture.width / ppu;
            var puzzleHeightWorldSpace = puzzleImageSprite.texture.height / ppu;
            
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
                
                var prefab = GameObject.Instantiate(_puzzlePiecePrefab,
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
                var startPixelX = sumOfIntsToIndexArray(puzzlePieceWidths, (int)piece.X-1)-knobSize;
                var startPixelY = sumOfIntsToIndexArray(puzzlePieceHeights, (int)piece.Y-1)-knobSize;
                
                for (int x = 0; x < mask.width; x++)
                    for (int y = 0; y < mask.height; y++)
                        if (mask.GetPixel(x, y) == Color.white)
                            newTexture.SetPixel(x,y,puzzleTexture.GetPixel(x + startPixelX, y + startPixelY));
                        else
                            newTexture.SetPixel(x,y,Color.clear);
                        
                var texturepath = savePath + "/" + puzzleName + count + ".png";
                
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
                    new Vector2((newTexture.width - 2 * knobSize)/ppu, (newTexture.height - 2 * knobSize)/ppu);

                AssetDatabase.SaveAssets();

            }
            var parentPrefabPath = Path.Combine(Application.dataPath,
                savePath + "/" + puzzleName + ".prefab");
            parent.FindPieces();
            parent.SetBackGround(puzzleImageSprite);
            var heightOfPieces = (float)height / rows + knobSize * 2;
            parent.SetPiecesDistance(heightOfPieces/ppu);
            var parentPrefab = PrefabUtility.SaveAsPrefabAsset(parent.gameObject,parentPrefabPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            GameObject.DestroyImmediate(parent.gameObject);
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
            var newTexture = new Texture2D(_knobTexture2D.width, _knobTexture2D.height);
            for (int x = 0; x < newTexture.width; x++)
            {
                for (int y = 0; y < newTexture.height; y++)
                {
                    newTexture.SetPixel(x, y, _knobTexture2D.GetPixel(x, y));
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