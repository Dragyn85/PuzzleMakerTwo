using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvailablePiecesArea : MonoBehaviour
{
    private HashSet<PuzzlePiece> _availablePieces = new HashSet<PuzzlePiece>();
    
    private bool dragging;
    private Vector3 lastPos;
    [SerializeField] private float _moveSpeed = 0.2f;
    [SerializeField] private float _distnance= 1f;

    private void OnMouseDrag()
    {
        if (!dragging)
        { 
            lastPos = Input.mousePosition;
            dragging = true;
        }
        else
        {
            var deltaY = lastPos.y - Input.mousePosition.y;
            lastPos = Input.mousePosition;

            foreach (var puzzlePiece in _availablePieces)
            {
                puzzlePiece.transform.position = Vector3.up * deltaY * _moveSpeed;
            }
        }
    }
    
    private void OnMouseUp()
    {
        dragging = false;
    }

    public void AddPieces(List<PuzzlePiece> piecesToAdd)
    {
        foreach (PuzzlePiece puzzlePiece in piecesToAdd)
        {
            _availablePieces.Add(puzzlePiece);
            puzzlePiece.transform.SetParent(this.transform);
        }
        ArrangePieces();
    }

    public void AddPiece(PuzzlePiece pieceToAdd)
    {
        _availablePieces.Add(pieceToAdd);
        pieceToAdd.transform.SetParent(this.transform);
        ArrangePieces();
    }
    void ArrangePieces()
    {
        var count = 0;
        foreach (var piece in _availablePieces)
        {
            piece.transform.localPosition += Vector3.down * _distnance *count;
            count++;
        }
    }
}
