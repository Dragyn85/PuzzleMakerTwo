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

        public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;

        public class OnGridObjectChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        private int width;
        private int height;
        private float cellSize;
        private Vector3 originPosition;
        private PuzzlePieceInit[,] gridArray;
        

        public PuzzleBoardLayout(int width,
            int height,
            float cellSize,
            Vector3 originPosition,
            int[] widths,
            int[] heights)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;

            gridArray = new PuzzlePieceInit[width, height];

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
        
        PuzzlePieceInit CreatePuzzlePiece(PuzzleBoardLayout g, int x, int y,int width, int height)
        {
            return new PuzzlePieceInit(g, x, y,height,width);
        }

        public PuzzlePieceInit GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                return gridArray[x, y];
            }
            else
            {
                return default(PuzzlePieceInit);
            }
        }


        public List<PuzzlePieceInit> GetAll()
        {
            var gridObjects = new List<PuzzlePieceInit>();
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