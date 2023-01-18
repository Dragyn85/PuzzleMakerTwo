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
using PuzzleMakerTwo.GameExample;


namespace PuzzleMakerTwo.Creator
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
        private float _pixelsPerUnit;
        private float puzzleWidthWorldSpace;
        private float puzzleHeightWorldSpace;

        public void CreatPuzzle(Sprite puzzleImageSprite, int columns, int rows, Texture2D knobTexture2D, int knobSize, string savePath, string puzzleName, bool createGame)
        {
            if (createGame && !TryGetGamePrefabs())
            {
                return;
            }

            _columns = columns;
            _rows = rows;
            _knobSize = knobSize;
            _knobTexture2D = knobTexture2D;

            string pathAndNameCombined = $"{savePath}/{puzzleName}";

            Texture2D puzzleTexture = CopyTexture2D(puzzleImageSprite.texture);

            var width = puzzleImageSprite.texture.width;
            var height = puzzleImageSprite.texture.height;

            

            //Calculate puzzle piece widths and heights as even as possible
            int[] puzzlePieceEvenWidths = DivideIntsEvenly(width, columns);
            int[] puzzlePieceEvenHeights = DivideIntsEvenly(height, rows);

            //Creat PuzzleBoard
            puzzleBoardLayout = new PuzzleBoardLayout(columns, rows,
                puzzlePieceEvenWidths, puzzlePieceEvenHeights, knobSize);

            //Catch all puzzle piece creation tool
            List<PuzzlePieceCreationTool> allPuzzlePiecesUnderConstruction = puzzleBoardLayout.GetAllPieces();

            CreatKnobTexturesFromOriginal();

            CreateMasksForPuzzlePieces(puzzleBoardLayout.GetAllPieces());
            _pixelsPerUnit = puzzleImageSprite.pixelsPerUnit;
            puzzleWidthWorldSpace = puzzleImageSprite.texture.width / _pixelsPerUnit;
            puzzleHeightWorldSpace = puzzleImageSprite.texture.height / _pixelsPerUnit;

            List<Sprite> sprites = CreatePuzzleSprites(pathAndNameCombined, puzzleTexture, puzzleBoardLayout.GetAllPieces());

            if (createGame)
            {
                CreateGame(puzzleImageSprite, puzzleName, pathAndNameCombined, puzzleBoardLayout.GetAllPieces(), _pixelsPerUnit, sprites);
            }

        }

        private List<Sprite> CreatePuzzleSprites(string pathAndNameCombined, Texture2D puzzleTexture, List<PuzzlePieceCreationTool> allPuzzlePiecesUnderConstruction)
        {
            List<Sprite> sprits = new List<Sprite>();

            foreach (var piece in allPuzzlePiecesUnderConstruction)
            {

                Sprite puzzlePieceSprite = CreatePuzzlePieceSprite(pathAndNameCombined, puzzleTexture, puzzleBoardLayout.PieceWidths, puzzleBoardLayout.PieceHeights, _pixelsPerUnit, piece);

                sprits.Add(puzzlePieceSprite);

                AssetDatabase.SaveAssets();
            }

            return sprits;
        }

        private void CreateGame(Sprite puzzleImageSprite, string puzzleName, string pathAndNameCombined, List<PuzzlePieceCreationTool> allPuzzlePiecesUnderConstruction, float ppu, List<Sprite> sprits)
        {
            PuzzleGame parent = GameObject.Instantiate(_puzzleGamePrefab,
                                    Vector3.zero,
                                    Quaternion.identity);

            parent.name = puzzleName;

            for (int i = 0; i < sprits.Count; i++)
            {
                CreatePuzzlePiecePrefabs(ppu, parent, allPuzzlePiecesUnderConstruction[i], sprits[i]);
            }

            CreatePuzzleGamePrefab(puzzleImageSprite, pathAndNameCombined, parent);

            if (parent != null)
                GameObject.DestroyImmediate(parent.gameObject);
        }

        private void CreatePuzzleGamePrefab(Sprite puzzleImageSprite, string relativePath, PuzzleGame parent)
        {
            var parentPrefabPath = Path.Combine(Application.dataPath,
                                relativePath + ".prefab");
            parent.SetBoardBackground(puzzleImageSprite);

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

            var background = new Texture2D((int)size.x + knobSize, (int)size.y + knobSize);
            background = ChangeAllPixels(background, Color.clear);

            var body = new Texture2D((int)size.x, (int)size.y);
            body = ChangeAllPixels(body, Color.white);
            background = SpriteMerger.InsertTextureWithOffset(background, body, Vector2.zero);
            background = SpriteMerger.InsertTextureWithOffset(background, _rightKnobTextureMale, new Vector2(size.x, size.y / 2 - knobSize / 2));
            background = SpriteMerger.InsertTextureWithOffset(background, _upperKnobTextureMale, new Vector2(size.x / 2 - knobSize / 2, size.y));
            background = SpriteMerger.InsertMask(background, _leftKnobTextureFemale, new Vector2(0, size.y / 2 - knobSize / 2));
            background = SpriteMerger.InsertMask(background, _downKnobTextureFemale, new Vector2(size.x / 2 - knobSize / 2, 0));

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

        private Sprite CreatePuzzlePieceSprite(string fullPath, Texture2D puzzleTexture, int[] puzzlePieceWidths, int[] puzzlePieceHeights, float ppu, PuzzlePieceCreationTool piece)
        {

            var mask = piece.GetMask();
            var spriteMask = Sprite.Create(mask, new Rect(0, 0, mask.width, mask.height), new Vector2(0.5f, 0.5f),
                ppu);
            spriteMask.name = $"X{piece.X} Y{piece.Y}";

            var newTexture = new Texture2D(mask.width, mask.height);
            var startPixelX = SumOfIntsToIndexArray(puzzlePieceWidths, (int)piece.X - 1) - piece.KnobSize;
            var startPixelY = SumOfIntsToIndexArray(puzzlePieceHeights, (int)piece.Y - 1) - piece.KnobSize;

            for (int x = 0; x < mask.width; x++)
                for (int y = 0; y < mask.height; y++)
                    if (mask.GetPixel(x, y) == Color.white)
                        newTexture.SetPixel(x, y, puzzleTexture.GetPixel(x + startPixelX, y + startPixelY));
                    else
                        newTexture.SetPixel(x, y, Color.clear);

            var texturepath = fullPath + piece.ID;

            if (Directory.Exists(Path.GetDirectoryName(texturepath)))
                Directory.CreateDirectory(Path.GetDirectoryName(texturepath));
            var textureSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f),
                ppu);
            Sprite exportedSprite = SaveSpriteAsPNG(textureSprite, texturepath);
            return exportedSprite;
        }

        private void CreatePuzzlePiecePrefabs(float ppu, PuzzleGame parent, PuzzlePieceCreationTool piece, Sprite pieceSprite)
        {

            var sumOfPiecesWidth = SumOfIntsToIndexArray(puzzleBoardLayout.PieceWidths, piece.X - 1);
            var correctX = ((sumOfPiecesWidth / ppu) + (piece.Width / ppu) / 2)
                                    - (puzzleWidthWorldSpace / 2);

            var sumOfPiecesHeight = SumOfIntsToIndexArray(puzzleBoardLayout.PieceHeights, piece.Y - 1);
            var correctY = ((sumOfPiecesHeight / ppu) + (piece.Height / ppu) / 2)
                - (puzzleHeightWorldSpace / 2);


            var prefab = GameObject.Instantiate(_puzzlePiecePrefab, new Vector3(correctX, correctY, 0), quaternion.identity, parent.transform);

            prefab.name = $"PuzzlePiece {piece.ID}";
            prefab.SetSprite(pieceSprite);
            //prefab.SetCorrectPosition(correctX, correctY);
            prefab.SetBoardPosition(piece.X, piece.Y);
            prefab.SetCornerStart(sumOfPiecesWidth, sumOfPiecesHeight);

            var puzzleMoveGrabCollider = prefab.gameObject.AddComponent<BoxCollider2D>();
            puzzleMoveGrabCollider.isTrigger = false;
            puzzleMoveGrabCollider.size =
                new Vector2((pieceSprite.texture.width - 2 * piece.KnobSize) / ppu, (pieceSprite.texture.height - 2 * piece.KnobSize) / ppu);
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


        int SumOfIntsToIndexArray(int[] array, int index)
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
                int temp = SumOfIntsToIndexArray(intsEvenly, i - 1) + first;

                if (currentBaseXpos - temp >= 1)
                    intsEvenly[i] = first + 1;
                else
                    intsEvenly[i] = first;
            }

            var currentSum = SumOfIntsToIndexArray(intsEvenly, intsEvenly.Length - 1);
            intsEvenly[intsEvenly.Length - 1] = AmountToDivide - currentSum;
            return intsEvenly;
        }

        Sprite SaveSpriteAsPNG(Sprite sprite, string relativePath)
        {
            var pathWithLineEnding = relativePath + ".png";
            var abs_path = Path.Combine(Application.dataPath, pathWithLineEnding);
            relativePath = Path.Combine("Assets", pathWithLineEnding);

            Directory.CreateDirectory(Path.GetDirectoryName(abs_path));
            File.WriteAllBytes(abs_path, ImageConversion.EncodeToPNG(sprite.texture));

            AssetDatabase.Refresh();


            var ti = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            ti.spritePixelsPerUnit = sprite.pixelsPerUnit;
            ti.mipmapEnabled = false;
            ti.textureType = TextureImporterType.Sprite;

            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);

        }


        private Texture2D MakeKnobTexture()
        {
            var newTexture = CopyTexture2D(_knobTexture2D);

            newTexture.Apply();
            TextureScale.Scale(newTexture, _knobSize, _knobSize);
            newTexture.Apply();


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