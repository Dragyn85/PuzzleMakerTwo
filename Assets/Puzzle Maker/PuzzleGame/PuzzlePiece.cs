using System;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class PuzzlePiece : MonoBehaviour
{
    
    private SpriteRenderer _spriteRenderer;
    private PuzzleGame _puzzleGame;
    [SerializeField] private puzzlePieceData _pieceData;
    private RectTransform _rectTransform;
    private Vector3 _lastPos;
    

    private bool _isPlacedCorrectly;


    public Vector2 CorrectPos => _pieceData.CorrectPos;

    private void Awake()
    {
        _puzzleGame = GetComponentInParent<PuzzleGame>();
    }

    public void SetSprite(Sprite newSprite)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = newSprite;
        
    }

    private void OnMouseDown()
    {
        _lastPos = transform.position;
        _puzzleGame.HeldOffset = GetMouseWorldPos() - transform.position;
    }
    

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var worldpos = new Vector3(mousePos.x, mousePos.y, transform.position.z);
        return worldpos;
    }

    private void OnMouseDrag()
    {
        if(!_isPlacedCorrectly)
        {
            transform.position = GetMouseWorldPos();// + PuzzleGame.Instance.HeldOffset;
        }
    }

    private void OnMouseUp()
    {
        if(_puzzleGame)
            _puzzleGame.DropedPiece(this);
    }
    public void Reset()
    {
        _isPlacedCorrectly = false;
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

    public void AprovePosition()
    {
        _isPlacedCorrectly = true;
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