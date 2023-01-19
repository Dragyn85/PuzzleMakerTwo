using PuzzleMakerTwo.GameExample;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzleSelectButton : MonoBehaviour, IPointerClickHandler
{
    public static event Action<Puzzle> AnyPuzzleStarted = delegate { };

    [SerializeField] Image _previewImage;
    [SerializeField] TMP_Text _numberOfPieces;
    Puzzle _puzzle;
    Action startPuzzle;
    

    public void OnPointerClick(PointerEventData eventData)
    {
        startPuzzle?.Invoke();
        AnyPuzzleStarted.Invoke(_puzzle);
    }


    public void SetPuzzle(Puzzle puzzle, Action puzzleStartDelegate)
    {
        _puzzle = puzzle;
        _previewImage.sprite = puzzle.PuzzleSprite;
        startPuzzle = puzzleStartDelegate;
        _numberOfPieces.SetText(_puzzle.NumberOfPieces.ToString());
    }

    
}
