using PuzzleMakerTwo.GameExample;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace PuzzleMakerTwo.GameExample
{
    public class PuzzleSelectButton : MonoBehaviour, IPointerClickHandler
    {
        public static event Action<Puzzle> AnyPuzzleStarted = delegate { };

        [SerializeField] Image _previewImage;
        [SerializeField] TMP_Text _numberOfPieces;
        Puzzle _puzzle;


        public void OnPointerClick(PointerEventData eventData)
        {
            AnyPuzzleStarted.Invoke(_puzzle);
        }


        public void SetPuzzle(Puzzle puzzle)
        {
            _puzzle = puzzle;
            _previewImage.sprite = puzzle.PuzzleSprite;
            _numberOfPieces.SetText(_puzzle.NumberOfPieces.ToString());
        }


    }
}