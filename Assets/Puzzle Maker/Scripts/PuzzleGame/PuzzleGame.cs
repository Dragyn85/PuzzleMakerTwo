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
        foreach (var puzzlePiece in _puzzlePieces)
        {
            puzzlePiece.Initialize();
        }
        _availablePiecesArea.AddPieces(_puzzlePieces);
    }

    

    
}
