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
        public Vector3 HeldOffset { get; set; }

        [SerializeField] private List<PuzzlePiece> _puzzlePieces;
        [SerializeField] private AvailablePiecesArea _availablePiecesArea;
        [SerializeField] private PuzzleBoard _puzzleBoard;

        public Sprite PuzzleSprite => _puzzleBoard.GetSprite();
        public int NumberOfPieces => GetComponentsInChildren<PuzzlePiece>().Length;
        private void FindPieces()
        {
            _puzzlePieces = GetComponentsInChildren<PuzzlePiece>().ToList();
        }

        public void SetUniscaledSize(Vector2 targetSize)
        {
            var originalHeight = _puzzleBoard.GetUnscaledSize().y;
            var originalWidth = _puzzleBoard.GetUnscaledSize().x;
            var factorY = originalHeight / targetSize.y;
            var factorX = originalWidth / targetSize.x;
            
            var factor = factorX < factorY? factorX : factorY;

            transform.localScale = new Vector3(factor, factor, 1);
        }

        private void Start()
        {
            FindPieces();

            if (_puzzlePieces.Count > 0)
            {
                var height = _puzzlePieces[0].GetComponent<BoxCollider2D>().size.y;
                _availablePiecesArea.SetDistance(height);

            }
            _availablePiecesArea.AddPiecesInRandomOrder(_puzzlePieces);
            _availablePiecesArea.AvailablePiecesChanged += HandleAvailablePiecesChanged;
        }

        private void OnDestroy()
        {
            _availablePiecesArea.AvailablePiecesChanged -= HandleAvailablePiecesChanged;
        }

        private void HandleAvailablePiecesChanged()
        {
            if (_availablePiecesArea.PiecesLeft <= 0)
            {
                PuzzleCompleted?.Invoke(this);
            }
        }

        public void DropedPiece(PuzzlePiece puzzlePiece)
        {
            if (_puzzleBoard.CheckPosition(puzzlePiece))
            {
                puzzlePiece.transform.parent = _puzzleBoard.transform;
                puzzlePiece.AprovePosition();
                _availablePiecesArea.RemovePiece(puzzlePiece);
            }
            else
            {
                puzzlePiece.ReturnToTakenPos();
            }
        }

        [ContextMenu("Reset Puzzle")]
        private void ResetPuzzle()
        {
            _availablePiecesArea.AddPiecesInRandomOrder(_puzzlePieces);
            foreach (var puzzlePiece in _puzzlePieces)
            {
                puzzlePiece.Reset();
            }
        }

        public void SetBoardBackground(Sprite puzzleImageSprite)
        {
            _puzzleBoard.SetBackground(puzzleImageSprite);
        }
    }
}
