using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using PuzzleMakerTwo;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;

namespace PuzzleMakerTwo.GameExample
{
    public class AvailablePiecesArea : MonoBehaviour
    {
        [SerializeField] Transform _scrollRectContent;
        [SerializeField] ScrollRect _scrollRect;
        [SerializeField] AvailablePiecePreview _piecePreviewPrefab;


        public void AddPieces(List<PuzzlePiece> pieces)
        {
            
            List<AvailablePiecePreview> newPreviews = new List<AvailablePiecePreview>();
            foreach (var piece in pieces)
            {
                //var newPreview = Instantiate(_piecePreviewPrefab, _scrollRectContent);
                var newPreview = Instantiate(_piecePreviewPrefab);
                newPreview.AddPiece(piece);
                newPreviews.Add(newPreview);
            }
            newPreviews.Shuffle();
            foreach (var piecePreview in newPreviews)
            {
                piecePreview.transform.SetParent(_scrollRectContent);
            }
        }
        private void Awake()
        {
            AvailablePiecePreview.OnAnyPieceSelected += HandleSelection;
            AvailablePiecePreview.OnAnyPieceReleased += HandleDeselection;
            _scrollRect.horizontal = false;
        }
        private void OnDestroy()
        {
            AvailablePiecePreview.OnAnyPieceSelected -= HandleSelection;
            AvailablePiecePreview.OnAnyPieceReleased -= HandleDeselection;
        }

        private void HandleDeselection()
        {
            _scrollRect.vertical = true;
        }

        private void HandleSelection()
        {
            _scrollRect.vertical = false;
        }
    }
}