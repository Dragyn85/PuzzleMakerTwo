#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private PuzzleBoardLayout puzzleBoardLayout;
        private int _columns;
        private int _rows;
        private int _knobSize;
        private Texture2D _knobTexture2D;
        private Texture2D _rightKnobTextureMale;
        private Texture2D _downKnobTextureMale;
        private Texture2D _leftKnobTextureMale;
        private Texture2D _upperKnobTextureMale;
        private Texture2D _upperKnobTextureFemale;
        private Texture2D _rightKnobTextureFemale;
        private Texture2D _downKnobTextureFemale;
        private Texture2D _leftKnobTextureFemale;

        public void CreatPuzzle(Sprite puzzleImageSprite, int columns, int rows,Texture2D knobTexture2D, int knobSize, string savePath,string puzzleName,bool createGame)
        {
            _columns = columns;
            _rows = rows;
            _knobSize = knobSize;
            _knobTexture2D = knobTexture2D;

            Texture2D puzzleTexture = CopyTexture2D(puzzleImageSprite.texture);
            var width = puzzleImageSprite.texture.width;
            var height = puzzleImageSprite.texture.height;


            if (createGame && !TryGetGamePrefabs()) 
            {
                return;
            }

            //Calculate puzzle piece widths and heights as even as possible
            int[] puzzlePieceEvenWidths = DivideIntsEvenly(width, columns);
            int[] puzzlePieceEvenHeights = DivideIntsEvenly(height, rows);

            //Creat PuzzleBoard
            puzzleBoardLayout = new PuzzleBoardLayout(columns, rows,
                puzzlePieceEvenWidths, puzzlePieceEvenHeights);

            //Catch all puzzlePieces
            List<PuzzlePieceCreationTool> allPuzzlePiecesUnderConstruction = puzzleBoardLayout.GetAll();

            CreatKnobTexturesFromOriginal();

            CreateMasksForPuzzlePieces(allPuzzlePiecesUnderConstruction);


            int count = 0;
            var ppu = puzzleImageSprite.pixelsPerUnit;
            PuzzleGame parent = null;
            if (createGame)
            {
                parent = GameObject.Instantiate(_puzzleGamePrefab,
                Vector3.zero, Quaternion.identity);
                parent.name = puzzleName;
            }
            var puzzleWidthWorldSpace = puzzleImageSprite.texture.width / ppu;
            var puzzleHeightWorldSpace = puzzleImageSprite.texture.height / ppu;



            foreach (var piece in allPuzzlePiecesUnderConstruction)
            {
                count++;
                Sprite puzzlePieceSprite = CreatePuzzlePieceSprite(knobSize, savePath, puzzleName, puzzleTexture, puzzlePieceEvenWidths, puzzlePieceEvenHeights, count, ppu, piece);

                if (createGame)
                {
                    CreatePuzzleGame(knobSize, puzzlePieceEvenWidths, puzzlePieceEvenHeights, count, ppu, parent, puzzleWidthWorldSpace, puzzleHeightWorldSpace, piece, puzzlePieceSprite.texture, puzzlePieceSprite);
                }

                AssetDatabase.SaveAssets();
            }
            if (createGame)
            {
                CreateGamePrefabs(puzzleImageSprite, rows, knobSize, savePath, puzzleName, height, parent);
            }
            if (parent != null)
                GameObject.DestroyImmediate(parent.gameObject);
        }

        private static void CreateGamePrefabs(Sprite puzzleImageSprite, int rows, int knobSize, string savePath, string puzzleName, int height, PuzzleGame parent)
        {
            var parentPrefabPath = Path.Combine(Application.dataPath,
                                savePath + "/" + puzzleName + ".prefab");
            var heightOfPieces = (float)height / rows + knobSize * 2;
            parent.Initialize(puzzleImageSprite, heightOfPieces);

            var parentPrefab = PrefabUtility.SaveAsPrefabAsset(parent.gameObject, parentPrefabPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public Texture2D GetPreviewPiece(Sprite image, int columns, int rows, Texture2D knobTexture2D, int knobSize)
        {
            _columns = columns;
            _rows = rows;
            _knobSize = knobSize;
            _knobTexture2D = knobTexture2D;

            CreatKnobTexturesFromOriginal();
            var size = new Vector2(image.texture.width / _columns, image.texture.height / _rows);
            size.x = MathF.Floor(size.x);
            size.y = MathF.Floor(size.y);

            var background = new Texture2D((int)size.x+knobSize, (int)size.y+knobSize);
            background = ChangeAllPixels(background,Color.clear);

            var body = new Texture2D((int)size.x, (int)size.y);
            body = ChangeAllPixels(body,Color.white);
            background = SpriteMerger.InsertTextureWithOffset(background, body, Vector2.zero);
            background = SpriteMerger.InsertTextureWithOffset(background, _rightKnobTextureMale,new Vector2(size.x,size.y/2-knobSize/2));
            background = SpriteMerger.InsertTextureWithOffset(background, _upperKnobTextureMale, new Vector2(size.x / 2 - knobSize / 2, size.y));
            background = SpriteMerger.InsertMask (background, _leftKnobTextureFemale, new Vector2(0, size.y / 2 - knobSize / 2));
            background = SpriteMerger.InsertMask (background, _downKnobTextureFemale, new Vector2(size.x / 2 - knobSize / 2, 0));
            
            return background;

        }

        private Texture2D ChangeAllPixels(Texture2D targetTexture, Color newColor)
        {
            var results = CopyTexture2D(targetTexture);
            var pixels = results.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = newColor;
            results.SetPixels(pixels);
            return results;
        }

        private Sprite CreatePuzzlePieceSprite(int knobSize, string savePath, string puzzleName, Texture2D puzzleTexture, int[] puzzlePieceWidths, int[] puzzlePieceHeights, int pieceNumber, float ppu, PuzzlePieceCreationTool piece)
        {
            
            var mask = piece.GetMask();
            var spriteMask = Sprite.Create(mask, new Rect(0, 0, mask.width, mask.height), new Vector2(0.5f, 0.5f),
                ppu);
            spriteMask.name = $"X{piece.X} Y{piece.Y}";

            var newTexture = new Texture2D(mask.width, mask.height);
            var startPixelX = sumOfIntsToIndexArray(puzzlePieceWidths, (int)piece.X - 1) - knobSize;
            var startPixelY = sumOfIntsToIndexArray(puzzlePieceHeights, (int)piece.Y - 1) - knobSize;

            for (int x = 0; x < mask.width; x++)
                for (int y = 0; y < mask.height; y++)
                    if (mask.GetPixel(x, y) == Color.white)
                        newTexture.SetPixel(x, y, puzzleTexture.GetPixel(x + startPixelX, y + startPixelY));
                    else
                        newTexture.SetPixel(x, y, Color.clear);

            var texturepath = savePath + "/" + puzzleName + pieceNumber;

            if (Directory.Exists(Path.GetDirectoryName(texturepath)))
            Directory.CreateDirectory(Path.GetDirectoryName(texturepath));
            var textureSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f),
                ppu);
            Sprite exportedSprite = SaveSpriteAsPNG(textureSprite, texturepath);
            return exportedSprite;
        }

        private void CreatePuzzleGame(int knobSize, int[] puzzlePieceWidths, int[] puzzlePieceHeights, int count, float ppu, PuzzleGame parent, float puzzleWidthWorldSpace, float puzzleHeightWorldSpace, PuzzlePieceCreationTool piece, Texture2D newTexture, Sprite savedSpriteaswell)
        {
            var correctX = ((sumOfIntsToIndexArray(puzzlePieceWidths, ((int)piece.X - 1)) / ppu) + (piece.Width / ppu) / 2)
                                    - (puzzleWidthWorldSpace / 2);
            var correctY = ((sumOfIntsToIndexArray(puzzlePieceHeights, ((int)piece.Y - 1)) / ppu) + (piece.Height / ppu) / 2)
                - (puzzleHeightWorldSpace / 2);


            var prefab = GameObject.Instantiate(_puzzlePiecePrefab,
            new Vector3(
                correctX,
                correctY,
                0),
            quaternion.identity,
            parent.transform);
            var correctPositionOriginLowerLeft = prefab.transform.localPosition;
            prefab.name = $"PuzzlePiece {count}";
            prefab.SetSprite(savedSpriteaswell);
            prefab.SetCorrectPosition(correctX, correctY);
            prefab.SetBoardPosition(piece.X, piece.Y);
            prefab.SetCornerStart(sumOfIntsToIndexArray(puzzlePieceWidths, piece.X - 1),
                sumOfIntsToIndexArray(puzzlePieceHeights, piece.Y - 1));

            var puzzleMoveGrabCollider = prefab.gameObject.AddComponent<BoxCollider2D>();
            puzzleMoveGrabCollider.isTrigger = false;
            puzzleMoveGrabCollider.size =
                new Vector2((newTexture.width - 2 * knobSize) / ppu, (newTexture.height - 2 * knobSize) / ppu);
        }


        /// <summary>
        /// Creates a mask with knobs for all of the puzzle pieces.
        /// </summary>
        /// <param name="allPuzzlePieces"></param>
        private void CreateMasksForPuzzlePieces(List<PuzzlePieceCreationTool> allPuzzlePieces)
        {
            foreach (var puzzlePiece in allPuzzlePieces)
            {
                List<Texture2D> allTexturesForPuzzlePiece = new List<Texture2D>();

                Texture2D wholePuzzlePieceTexture = new Texture2D(puzzlePiece.Width + _knobSize * 2, puzzlePiece.Height + _knobSize * 2);
                for (int x = 0; x < wholePuzzlePieceTexture.width; x++)
                    for (int y = 0; y < wholePuzzlePieceTexture.height; y++)
                        wholePuzzlePieceTexture.SetPixel(x, y, Color.clear);

                allTexturesForPuzzlePiece.Add(wholePuzzlePieceTexture);

                Texture2D mainBody = new Texture2D(puzzlePiece.Width, puzzlePiece.Height);
                for (int x = 0; x < mainBody.width; x++)
                    for (int y = 0; y < mainBody.height; y++)
                        mainBody.SetPixel(x, y, Color.white);

                mainBody = SpriteMerger.InsertTextureWithOffset(wholePuzzlePieceTexture, mainBody, new Vector2(_knobSize, _knobSize));
                allTexturesForPuzzlePiece.Add(mainBody);

                var finalPiecePre = SpriteMerger.MergePuzzleMaskTexture(allTexturesForPuzzlePiece.ToArray());
                PuzzlePieceCreationTool tempPieceInit;

                var finalMask = new Texture2D(finalPiecePre.width, finalPiecePre.height);
                var size = finalMask.height;
                var colors = finalPiecePre.GetPixels();
                finalMask.SetPixels(colors);

                if (puzzlePiece.HasNeighbourRight)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.right);
                    if (!tempPieceInit.IsKnobMale(Vector2.left))
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            _rightKnobTextureMale,
                            new Vector2(wholePuzzlePieceTexture.width - _knobSize,
                                puzzlePiece.GetKnobs().Right.pos * puzzlePiece.Height + _knobSize / 2));
                    //new Vector2(maskTexture.width - _knobSize , maskTexture.height / 2 - _knobSize / 2));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            _rightKnobTextureFemale,
                            new Vector2(wholePuzzlePieceTexture.width - _knobSize * 2,
                                puzzlePiece.GetKnobs().Right.pos * puzzlePiece.Height + _knobSize / 2));
                }

                if (puzzlePiece.HasNeighbourTop)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.up);
                    if (!tempPieceInit.IsKnobMale(Vector2.down))
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            _upperKnobTextureMale,
                            new Vector2(puzzlePiece.GetKnobs().Top.pos * puzzlePiece.Width + _knobSize / 2,
                                wholePuzzlePieceTexture.height - _knobSize));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            _upperKnobTextureFemale,
                            new Vector2(puzzlePiece.GetKnobs().Top.pos * puzzlePiece.Width + _knobSize / 2,
                                wholePuzzlePieceTexture.height - _knobSize * 2));
                }

                if (puzzlePiece.HasNeighbourLeft)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.left);
                    if (!tempPieceInit.IsKnobMale(Vector2.right))
                    {
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            _leftKnobTextureMale,
                            new Vector2(0,
                                puzzlePiece.GetKnobs().Left.pos * puzzlePiece.Height + _knobSize / 2));
                    }
                    else
                    {
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            _leftKnobTextureFemale,
                            new Vector2(_knobSize,
                                puzzlePiece.GetKnobs().Left.pos * puzzlePiece.Height + _knobSize / 2));
                    }
                }

                if (puzzlePiece.HasNeighbourDown)
                {
                    tempPieceInit = puzzlePiece.GetNeighbour(Vector2.down);
                    if (!tempPieceInit.IsKnobMale(Vector2.up))
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            _downKnobTextureMale,
                            new Vector2(puzzlePiece.GetKnobs().Down.pos * puzzlePiece.Width + (_knobSize / 2),
                                0));
                    else
                        finalMask = SpriteMerger.InsertMask(finalMask,
                            _downKnobTextureFemale,
                            new Vector2(puzzlePiece.GetKnobs().Down.pos * puzzlePiece.Width + (_knobSize / 2),
                                _knobSize));
                }


                finalMask.Apply();
                puzzlePiece.SetTexture(finalMask);
            }
        }

        private void CreatKnobTexturesFromOriginal()
        {
            //Creats Male and Female Knobs in all direction in the correct size.
            _rightKnobTextureMale = MakeKnobTexture();
            _rightKnobTextureMale.Apply();

            _downKnobTextureMale = TextureRotate.rotateTexture(_rightKnobTextureMale, true);
            _leftKnobTextureMale = TextureRotate.rotateTexture(_downKnobTextureMale, true);
            _upperKnobTextureMale = TextureRotate.rotateTexture(_rightKnobTextureMale, false);

            _leftKnobTextureFemale = SpriteMerger.InvertMask(_rightKnobTextureMale);

            _upperKnobTextureFemale = TextureRotate.rotateTexture(_leftKnobTextureFemale, true);
            _rightKnobTextureFemale = TextureRotate.rotateTexture(_upperKnobTextureFemale, true);
            _downKnobTextureFemale = TextureRotate.rotateTexture(_leftKnobTextureFemale, false);
        }

        //Method for creating puzzlePieces passed to the PuzzleBoard
        

        int sumOfIntsToIndexArray(int[] array,int index)
        {

            
            
            var sum = 0;
            for (int i = 0; i <= index; i++)
            {
                sum += array[i];
            }

            return sum;
        }

        /// <summary>
        /// Tries to find Puzzle Game dependancies prefabs
        /// </summary>
        /// <returns>true if dependancies are found</returns>
        private bool TryGetGamePrefabs()
        {
            var puzzleGames = MyEditorTools.Tools.FindAssetsWithExtension<PuzzleGame>(".prefab");
            _puzzleGamePrefab = puzzleGames.FirstOrDefault(t => t.name == "PuzzleMakerPuzzle - GamePrefab");
            if (_puzzleGamePrefab == null)
            {
                Debug.LogError("Can not find PuzzleMakerPuzzle - GamePrefab");
                return false;
            }

            var puzzlePieces = MyEditorTools.Tools.FindAssetsWithExtension<PuzzlePiece>(".prefab");
            _puzzlePiecePrefab = puzzlePieces.FirstOrDefault(t => t.name == "PuzzleMakerPuzzle - PiecePrefab");
            if (_puzzlePiecePrefab == null)
            {
                Debug.LogError("Can not find PuzzleMakerPuzzle - PiecePrefab");
                return false;
            }

            return true;
        }


        private Texture2D CopyTexture2D(Texture2D texture)
        {
            Texture2D copy = new Texture2D(texture.width, texture.height);
            copy.SetPixels(texture.GetPixels());
            return copy;
        }

        /// <summary>
        /// Takes an integer and to be divided as equally as possible
        /// </summary>
        /// <param name="AmountToDivide"></param>
        /// <param name="divideWith"></param>
        /// <returns>Array of ints, each holding a divided part of the Input integer</returns>
        private int[] DivideIntsEvenly(int AmountToDivide, int divideWith)
        {
            int[] intsEvenly = new int[divideWith];
            var first = (int)Mathf.Floor(AmountToDivide / intsEvenly.Length);

            // Calculater individual pieces width
            intsEvenly[0] = first;
            for (int i = 1; i < intsEvenly.Length - 1; i++)
            {
                var currentBaseXpos = ((float)AmountToDivide / _columns) * (i + 1);
                int temp = sumOfIntsToIndexArray(intsEvenly, i - 1) + first;

                if (currentBaseXpos - temp >= 1)
                    intsEvenly[i] = first + 1;
                else
                    intsEvenly[i] = first;
            }

            var currentSum = sumOfIntsToIndexArray(intsEvenly, intsEvenly.Length - 1);
            intsEvenly[intsEvenly.Length - 1] = AmountToDivide - currentSum;
            return intsEvenly;
        }

        /// <summary>
        /// Exports a png and returns the sprite
        /// </summary>
        /// <param name="sprite">Sprite to be Exported</param>
        /// <param name="proj_path">Path to be exported to</param>
        /// <returns></returns>
        Sprite SaveSpriteAsPNG(Sprite sprite, string proj_path)
        {
            var pathWithLineEnding = proj_path + ".png";
            var abs_path = Path.Combine(Application.dataPath, pathWithLineEnding);
            proj_path = Path.Combine("Assets", pathWithLineEnding);

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

        /// <summary>
        /// returns a copy and scaled _knobTexture2D to the height and width of the _knobs
        /// </summary>
        /// <returns></returns>
        private Texture2D MakeKnobTexture()
        {
            var newTexture = CopyTexture2D(_knobTexture2D);

            newTexture.Apply();
            TextureScale.Scale(newTexture, _knobSize, _knobSize);


            return newTexture;
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
        public PuzzlePieceCreationTool PuzzlePieceInit;
        public bool male;
        public float pos;
    }

}
#endif