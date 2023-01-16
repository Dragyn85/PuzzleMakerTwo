/*
 * Based off on CodeMonkey's Grid class
 * https://unitycodemonkey.com/
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleMakerTwo
{
    public class PuzzleBoardLayout
    {

        public class OnGridObjectChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        private int width;
        private int height;
        private PuzzlePieceCreationTool[,] gridArray;
        

        public PuzzleBoardLayout(int width,
            int height,
            int[] widths,
            int[] heights)
        {
            this.width = width;
            this.height = height;

            gridArray = new PuzzlePieceCreationTool[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridArray[x, y] = CreatePuzzlePiece(this, x, y, widths[x], heights[y]);
                }
            }

            foreach (var piece in gridArray)
                piece.SetKnobs();
        }
        
        PuzzlePieceCreationTool CreatePuzzlePiece(PuzzleBoardLayout g, int x, int y,int width, int height)
        {
            return new PuzzlePieceCreationTool(g, x, y,height,width);
        }

        public PuzzlePieceCreationTool GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                return gridArray[x, y];
            }
            else
            {
                return default(PuzzlePieceCreationTool);
            }
        }


        public List<PuzzlePieceCreationTool> GetAll()
        {
            var gridObjects = new List<PuzzlePieceCreationTool>();
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridObjects.Add(gridArray[x, y]);
                }
            }

            return gridObjects;
        }
    }
}
#endif