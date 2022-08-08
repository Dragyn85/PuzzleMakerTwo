using System;
using PuzzleMakerTwo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SpriteRenderer))]
public class PuzzlePiece : MonoBehaviour
{
    
    private SpriteRenderer _spriteRenderer;
    private PuzzleGame _puzzleGame;
    [SerializeField] private puzzlePieceData _pieceData;
    private RectTransform _rectTransform;
    private Vector3 _lastPos;


    public Vector2 CorrectPos => _pieceData.CorrectPos;

    public void SetSprite(Sprite newSprite)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = newSprite;
        
    }

    private void OnMouseDown()
    {
        _lastPos = transform.position;
        PuzzleGame.Instance.HeldOffset = GetMouseWorldPos() - transform.position;
    }
    

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var worldpos = new Vector3(mousePos.x, mousePos.y, transform.position.z);
        return worldpos;
    }

    private void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos();// + PuzzleGame.Instance.HeldOffset;
    }

    private void OnMouseUp()
    {
        if(PuzzleGame.Instance)
            PuzzleGame.Instance.DropedPiece(this);
    }

    public void SetBoardPosition(int pieceX, int pieceY)
    {
        _pieceData.x = pieceX;
        _pieceData.y = pieceY;
    }
    
    public void MoveToPos(Vector3 pos)
    {
        transform.position = pos;
    }

    public void SetCornerStart(int startPixelX, int startPixelY)
    {
        _pieceData.puzzlePixelStartCorner = new Vector2(startPixelX, startPixelY);
    }

    public void ReturnToTakenPos()
    {
        transform.position = _lastPos;
    }

    public void SetCorrectPosition(float correctX, float correctY)
    {
        _pieceData.CorrectPos = new Vector2(correctX, correctY);
    }
}

[Serializable]
public class puzzlePieceData
{
    public float x;
    public float y;
    public Vector2 puzzlePixelStartCorner;


    public puzzlePieceData(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2 CorrectPos;
}