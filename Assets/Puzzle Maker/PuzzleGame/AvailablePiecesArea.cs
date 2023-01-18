using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using PuzzleMakerTwo;

namespace PuzzleMakerTwo.GameExample
{
    public class AvailablePiecesArea : MonoBehaviour
    {
        private HashSet<PuzzlePiece> _availablePieces = new HashSet<PuzzlePiece>();
        public event Action AvailablePiecesChanged;

        private bool dragging;
        private Vector3 lastPos;
        float piecesdistnance;

        [SerializeField] private float _moveSpeed = 0.2f;
        [SerializeField] private float spacing;
        [SerializeField] private Transform _pieceHolder;

        private float distance => spacing + piecesdistnance;
        public int PiecesLeft => _availablePieces.Count;

        private void OnMouseDrag()
        {
            if (!dragging)
            {
                lastPos = Input.mousePosition;
                dragging = true;
            }
            else
            {
                var deltaY = (lastPos.y - Input.mousePosition.y) * -1;
                lastPos = Input.mousePosition;

                _pieceHolder.position = new Vector3(_pieceHolder.position.x, (_pieceHolder.position.y + _moveSpeed * deltaY));
            }
        }

        private void OnMouseUp()
        {
            dragging = false;
        }

        public void AddPiecesInRandomOrder(List<PuzzlePiece> piecesToAdd)
        {
            piecesToAdd.Shuffle();
            foreach (PuzzlePiece puzzlePiece in piecesToAdd)
            {
                _availablePieces.Add(puzzlePiece);
                puzzlePiece.transform.SetParent(_pieceHolder);
            }
            ArrangePieces();
        }

        void ArrangePieces()
        {
            var count = 0;
            foreach (var piece in _availablePieces)
            {
                piece.transform.localPosition = new Vector3(
                    0,
                    transform.position.y - distance * count,
                    0);
                count++;
            }
        }

        public void SetDistance(float heightOfPieces)
        {
            piecesdistnance = heightOfPieces;
        }

        public void RemovePiece(PuzzlePiece puzzlePiece)
        {
            _availablePieces.Remove(puzzlePiece);
            ArrangePieces();
            AvailablePiecesChanged?.Invoke();
        }
    }
}