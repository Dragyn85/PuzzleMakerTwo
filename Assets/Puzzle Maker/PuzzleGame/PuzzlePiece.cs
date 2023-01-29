using System;
using UnityEngine;

namespace PuzzleMakerTwo.GameExample
{

    [RequireComponent(typeof(SpriteRenderer))]
    public class PuzzlePiece : MonoBehaviour
    {
        public static event Action AnyPiecePlacedCorrectly;

        [SerializeField] private puzzlePieceData _pieceData;
        [SerializeField] private float _hideAlpha = 0.1f;

        private SpriteRenderer _spriteRenderer;
        private bool _isPlacedCorrectly;

        public Vector2 CorrectPos => _pieceData.CorrectPos *transform.lossyScale.x;

        public bool IsPlacedCorrectly => _isPlacedCorrectly;

        public void SetSprite(Sprite newSprite)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = newSprite;
        }

        public void SetBoardPosition(int pieceX, int pieceY)
        {
            _pieceData.x = pieceX;
            _pieceData.y = pieceY;
        }

        public void MoveToPos(Vector3 pos)
        {
            transform.position = pos;
        }

        public void SetCornerStart(int startPixelX, int startPixelY)
        {
            _pieceData.puzzlePixelStartCorner = new Vector2(startPixelX, startPixelY);
        }

        public void SetCorrectPosition(float correctX, float correctY)
        {
            _pieceData.CorrectPos = new Vector2(correctX, correctY);
        }

        internal Sprite GetSprite()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
                return _spriteRenderer.sprite;
            else return null;
        }

        internal void Hide()
        {
            Color color =_spriteRenderer.color;
            color.a = _hideAlpha;
            _spriteRenderer.color = color;
        }
        public void Show()
        {
            Color color = _spriteRenderer.color;
            float colorAlpha = 1;
            color.a = colorAlpha;
            _spriteRenderer.color = color;
        }

        internal void CheckPosition(Vector2 pos, Action<bool> value)
        {
            if(Vector3.Distance(pos,transform.position)< 1)
            {
                value(true);
                Show();
                _isPlacedCorrectly=true;
                AnyPiecePlacedCorrectly?.Invoke();
            }
            else
            {
                value(false);
            }
        }
    }

    [Serializable]
    public class puzzlePieceData
    {
        public float x;
        public float y;
        public Vector2 puzzlePixelStartCorner;


        public puzzlePieceData(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 CorrectPos;
    }

    public class PuzzleInputs
    {
        public static Vector2 GetMouseWorldPos()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var worldpos = new Vector2(mousePos.x, mousePos.y);
            return worldpos;
        }
    }
}