using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuzzleMakerTwo.GameExample
{
    public class Puzzle : MonoBehaviour
    {
        public static event Action<Puzzle> PuzzleCompleted;

        [SerializeField] private List<PuzzlePiece> _puzzlePieces;
        [SerializeField] private PuzzleBoard _puzzleBoard;
        [SerializeField, HideInInspector] int numberOfPieces;

        public Sprite PuzzleSprite => _puzzleBoard.GetSprite();
        public int NumberOfPieces => GetComponentsInChildren<PuzzlePiece>().Length;

        private void Awake()
        {
            _puzzlePieces = _puzzleBoard.GetComponentsInChildren<PuzzlePiece>().ToList();
            PuzzlePiece.AnyPiecePlacedCorrectly += HandleCorrectlyPlacedPiece;
        }
        private void OnDestroy()
        {
            PuzzlePiece.AnyPiecePlacedCorrectly -= HandleCorrectlyPlacedPiece;
        }
        private void HandleCorrectlyPlacedPiece()
        {
            bool allPiecesInPlace = true;
            foreach (var item in _puzzlePieces)
            {
                if(!item.IsPlacedCorrectly)
                {
                    allPiecesInPlace = false;
                }
            }
            if (allPiecesInPlace)
            {
                PuzzleCompleted?.Invoke(this);
                Destroy(gameObject);
            }
        }

        private void OnValidate()
        {
            numberOfPieces = _puzzleBoard.GetComponentsInChildren<PuzzlePiece>().Count();
        }

        public void SetUniscaledSize(Vector2 targetSize)
        {
            var originalHeight = _puzzleBoard.GetUnscaledSize().y;
            var originalWidth = _puzzleBoard.GetUnscaledSize().x;
            var factorY = targetSize.y/ originalHeight;
            var factorX = targetSize.x/ originalWidth;

            var factor = factorX < factorY ? factorX : factorY;

            transform.localScale = new Vector3(factor, factor, 1);
        }

        public void SetBoardBackground(Sprite puzzleImageSprite)
        {
            _puzzleBoard.SetBackground(puzzleImageSprite);
        }

        internal void AddPiece(PuzzlePiece prefab)
        {
            prefab.transform.parent = _puzzleBoard.transform;
        }

        internal List<PuzzlePiece> GetPuzzlePieces()
        {
            if (_puzzlePieces == null)
            {
                _puzzlePieces = GetComponents<PuzzlePiece>().ToList();
            }

            return _puzzlePieces;
        }

        internal void Initialize()
        {
            if(_puzzlePieces != null)
            {
                foreach (var piece in _puzzlePieces)
                {
                    piece.Hide();
                }
            }
        }
    }
}
