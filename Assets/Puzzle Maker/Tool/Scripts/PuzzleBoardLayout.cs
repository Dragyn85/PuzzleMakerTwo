/*
 * Based off on CodeMonkey's Grid class
 * https://unitycodemonkey.com/
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleMakerTwo.Creator
{
    public class PuzzleBoardLayout
    {
        public class OnGridObjectChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        private int _width;
        private int _height;
        private int[] _widths;
        private int[] _heights;
        private PuzzlePieceCreationTool[,] _gridArray;

        public int[] PieceHeights => _heights;
        public int[] PieceWidths => _widths;


        public PuzzleBoardLayout(int width, int height, int[] widths, int[] heights, int knobSize)
        {
            this._width = width;
            this._height = height;
            _heights = heights;
            _widths = widths;

            _gridArray = new PuzzlePieceCreationTool[width, height];
            int number = 0;
            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {

                    _gridArray[x, y] = CreatePuzzlePiece(this, x, y, widths[x], heights[y], number, knobSize);
                    number++;
                }
            }

            foreach (var piece in _gridArray)
                piece.SetKnobs();
        }

        PuzzlePieceCreationTool CreatePuzzlePiece(PuzzleBoardLayout g, int x, int y, int width, int height, int id, int knobSize)
        {
            return new PuzzlePieceCreationTool(g, x, y, height, width, id, knobSize);
        }

        public PuzzlePieceCreationTool GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                return _gridArray[x, y];
            }
            else
            {
                return default(PuzzlePieceCreationTool);
            }
        }


        public List<PuzzlePieceCreationTool> GetAllPieces()
        {
            var gridObjects = new List<PuzzlePieceCreationTool>();
            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    gridObjects.Add(_gridArray[x, y]);
                }
            }

            return gridObjects;
        }
    }
}
#endif