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
    public class PuzzleBoard<TGridObject>
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
        private TGridObject[,] gridArray;
        private Func<PuzzleBoard<TGridObject>, int, int, int, int, TGridObject> createGridObject;

        public PuzzleBoard(int width,
            int height,
            float cellSize,
            Vector3 originPosition,
            Func<PuzzleBoard<TGridObject>, int, int, int, int, TGridObject> createGridObject,
            int[] widths,
            int[] heights)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;
            this.createGridObject = createGridObject;

            gridArray = new TGridObject[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridArray[x, y] = createGridObject(this, x, y, widths[x], heights[y]);
                }
            }
        }

        public TGridObject GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                return gridArray[x, y];
            }
            else
            {
                return default(TGridObject);
            }
        }


        public List<TGridObject> GetAll()
        {
            var gridObjects = new List<TGridObject>();
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