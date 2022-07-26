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

    [SerializeField] private TextAsset _puzzleMakerJsonOutput;
    [SerializeField] private List<PuzzlePiece> _puzzlePieces;
    [SerializeField]private PuzzleInfo _puzzleInfo;

    private Camera cam;
    [SerializeField] BoxCollider2D _collider2D;

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
        var upleft = new Vector2(_collider2D.bounds.min.x, _collider2D.bounds.max.y);
        var downright = new Vector2(_collider2D.bounds.max.x, _collider2D.bounds.min.y);
        foreach (var puzzlePiece in _puzzlePieces)
        {
            puzzlePiece.Initialize(upleft,downright);
        }
    }

    public void SetInfo(PuzzleInfo puzzleInfo)
    {
        _puzzleInfo = puzzleInfo;
    }

    
}
