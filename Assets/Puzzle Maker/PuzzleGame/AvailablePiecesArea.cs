using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class AvailablePiecesArea : MonoBehaviour
{
    private HashSet<PuzzlePiece> _availablePieces = new HashSet<PuzzlePiece>();
    public event Action AvailablePiecesChanged;
    
    private bool dragging;
    private Vector3 lastPos;
    [SerializeField] private float _moveSpeed = 0.2f;
    [SerializeField] private float _distnance= 1f;
    [SerializeField] private Transform _pieceHolder;
    private float _maxScrollPos;
    private float _minScrollPos;
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
            var deltaY = (lastPos.y - Input.mousePosition.y)*-1;
            lastPos = Input.mousePosition;
            var newYpos = _pieceHolder.position.y + _moveSpeed * deltaY;
            newYpos = Mathf.Clamp(newYpos, _minScrollPos,_maxScrollPos);
            _pieceHolder.position = new Vector3(_pieceHolder.position.x, newYpos);
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
                transform.position.y - _distnance * count,
                0);
            count++;
        }
        CalculateScrollArea();
    }

    public void SetDistance(float heightOfPieces)
    {
        _distnance = heightOfPieces;
    }

    public void RemovePiece(PuzzlePiece puzzlePiece)
    {
        _availablePieces.Remove(puzzlePiece);
        ArrangePieces();
        AvailablePiecesChanged?.Invoke();
        
    }

    private void CalculateScrollArea()
    {
        _minScrollPos = 0;
        _maxScrollPos = _availablePieces.Count * _distnance;
    }
}
