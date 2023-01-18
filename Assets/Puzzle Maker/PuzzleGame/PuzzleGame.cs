using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuzzleMakerTwo.GameExample
{
    public class PuzzleGame : MonoBehaviour
    {
        public event Action PuzzleCompleted;
        public Vector3 HeldOffset { get; set; }

        [SerializeField] private List<PuzzlePiece> _puzzlePieces;
        [SerializeField] private AvailablePiecesArea _availablePiecesArea;
        [SerializeField] private PuzzleBoard _puzzleBoard;

        private Camera cam;

        private void FindPieces()
        {
            _puzzlePieces = GetComponentsInChildren<PuzzlePiece>().ToList();
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
                PuzzleCompleted?.Invoke();
                StartCoroutine(RestartGameAfterDelay(3f));
            }
        }

        private IEnumerator RestartGameAfterDelay(float f)
        {

            yield return new WaitForSeconds(f);
            ResetPuzzle();

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
