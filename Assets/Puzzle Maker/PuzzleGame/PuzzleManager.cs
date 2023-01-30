using PuzzleMakerTwo.GameExample;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace PuzzleMakerTwo.GameExample
{
    public class PuzzleManager : MonoBehaviour
    {
        [SerializeField] List<Puzzle> _puzzlePrefabs;
        [SerializeField] PuzzleSelectButton _buttonPrefab;
        [SerializeField] AvailablePiecesArea _availablePiecesArea;
        [SerializeField] Transform _puzzlesPreviewParent;
        [SerializeField] PuzzleArea _puzzleArea;
        [SerializeField] CanvasGroup _menuCanvasGroup;
        [SerializeField] List<CanvasGroup> _gameCanvasGroups;

        private void Awake()
        {
            PuzzleSelectButton.AnyPuzzleStarted += HandlePuzzleStart;
            foreach (var puzzle in _puzzlePrefabs)
            {
                var newbutton = Instantiate(_buttonPrefab, _puzzlesPreviewParent);
                newbutton.SetPuzzle(puzzle);
            }
            foreach (var canvasGroup in _gameCanvasGroups)
            {
                SetCanvasGroupShow(canvasGroup, false);
            }
        }

        private void HandlePuzzleStart(Puzzle puzzle)
        {
            var newPuzzle = Instantiate(puzzle, _puzzleArea.transform);
            var targetSize = _puzzleArea.GetPuzzleAreaSize();

            _availablePiecesArea.AddPieces(newPuzzle.GetPuzzlePieces());
            newPuzzle.Initialize();
            newPuzzle.SetUniscaledSize(targetSize);
            newPuzzle.transform.position = _puzzleArea.transform.position;

            SetCanvasGroupShow(_menuCanvasGroup, false);
            foreach (var canvasGroup in _gameCanvasGroups)
            {
                SetCanvasGroupShow(canvasGroup, true);
            }

            Puzzle.PuzzleCompleted += HandleCompletedPuzzle;
        }

        private void HandleCompletedPuzzle(Puzzle puzzle)
        {
            Puzzle.PuzzleCompleted -= HandleCompletedPuzzle;
            SetCanvasGroupShow(_menuCanvasGroup, true);
            foreach (var canvasGroup in _gameCanvasGroups)
            {
                SetCanvasGroupShow(canvasGroup, false);
            }
        }

        private void SetCanvasGroupShow(CanvasGroup canvasGroup, bool show)
        {
            canvasGroup.alpha = show ? 1 : 0;
            canvasGroup.blocksRaycasts = show;
            canvasGroup.interactable = show;
        }
    }
}