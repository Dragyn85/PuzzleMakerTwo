using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleMakerTwo.GameExample
{
    public class PuzzleBoard : MonoBehaviour
    {
        public Vector2 GetUnscaledSize()
        {
            var collider = GetComponent<BoxCollider2D>();
            if (collider != null )
            {
                return collider.size;
            }
            return Vector2.zero;
        }
        public bool CheckPosition(PuzzlePiece puzzlePiece)
        {
            var currentWorldPos = puzzlePiece.transform.position;
            var correctWorldPos = transform.position + (Vector3)puzzlePiece.CorrectPos;
            if (Vector3.Distance(currentWorldPos, correctWorldPos) < 1)
            {
                var correctPos = (Vector2)transform.position + puzzlePiece.CorrectPos;
                puzzlePiece.MoveToPos(correctPos);
                return true;
            }
            return false;
        }

        public void SetBackground(Sprite newBackGround)
        {
            GetComponent<SpriteRenderer>().sprite = newBackGround;
            var colliders = GetComponents<Collider2D>();
            foreach(var collider in colliders)
            {
                DestroyImmediate(collider);
            }
            gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
        }

        internal Sprite GetSprite()
        {
            return GetComponent<SpriteRenderer>().sprite;
        }
    }
}