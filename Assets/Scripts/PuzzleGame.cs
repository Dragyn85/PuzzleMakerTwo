using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PuzzleMakerTwo;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

public class PuzzleGame : MonoBehaviour
{
    [SerializeField] private TextAsset _puzzleMakerJsonOutput;
    [SerializeField] private List<PuzzlePiece> _puzzlePieces;
    [SerializeField]private PuzzleInfo _puzzleInfo;
    
    public void FindPieces()
    {
        _puzzlePieces = GetComponentsInChildren<PuzzlePiece>().ToList();
    }

    public void SetInfo(PuzzleInfo puzzleInfo)
    {
        _puzzleInfo = puzzleInfo;
    }

    
}
