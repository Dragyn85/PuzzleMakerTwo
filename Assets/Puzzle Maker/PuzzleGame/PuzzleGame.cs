using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PuzzleMakerTwo;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

public class PuzzleGame : MonoBehaviour
{
    public static PuzzleGame Instance { get; private set; }
    public Vector3 HeldOffset { get; set; }

    [SerializeField] private List<PuzzlePiece> _puzzlePieces;
    [SerializeField] private AvailablePiecesArea _availablePiecesArea;
    [SerializeField] private PuzzleBoard _puzzleBoard;
    

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Second PuzzleGames tried to initialize by " + gameObject.name);
        }
    }

    

    public void FindPieces()
    {
        _puzzlePieces = GetComponentsInChildren<PuzzlePiece>().ToList();
    }

    private void Start()
    {
        FindPieces();
        _availablePiecesArea.AddPiecesInRandomOrder(_puzzlePieces);
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
