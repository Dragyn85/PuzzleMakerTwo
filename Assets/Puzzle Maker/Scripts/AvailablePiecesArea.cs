using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class AvailablePiecesArea : MonoBehaviour
{
    private HashSet<PuzzlePiece> _availablePieces = new HashSet<PuzzlePiece>();
    
    private bool dragging;
    private Vector3 lastPos;
    [SerializeField] private float _moveSpeed = 0.2f;
    [SerializeField] private float _distnance= 1f;
    [SerializeField] private Transform _center;

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

            transform.position = new Vector3(transform.position.x, transform.position.y + _moveSpeed * deltaY);
            //transform.RotateAround(_center.position,Vector3.right, deltaY*_moveSpeed);
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
            piece.transform.position = new Vector3(
                piece.transform.parent.position.x+1,
                piece.transform.parent.position.y + _distnance * count,
                0);
            count++;
        }
    }
}
