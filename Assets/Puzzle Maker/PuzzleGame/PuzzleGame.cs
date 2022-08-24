using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleGame : MonoBehaviour
{
    public event Action PuzzleCompleted;
    public Vector3 HeldOffset { get; set; }

    [SerializeField] private List<PuzzlePiece> _puzzlePieces;
    [SerializeField] private AvailablePiecesArea _availablePiecesArea;
    [SerializeField] private PuzzleBoard _puzzleBoard;
    

    private Camera cam;

   

    public void FindPieces()
    {
        _puzzlePieces = GetComponentsInChildren<PuzzlePiece>().ToList();
    }

    private void Start()
    {
        FindPieces();
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

    public void SetBackGround(Sprite sprite)
    {
        _puzzleBoard.SetBackground(sprite);
    }

    public void SetPiecesDistance(float heightOfPieces)
    {
        _availablePiecesArea.SetDistance(heightOfPieces);
    }
}
