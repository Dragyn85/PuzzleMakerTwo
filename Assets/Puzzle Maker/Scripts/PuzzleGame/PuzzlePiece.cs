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


    public Vector2 CorrectPos => _pieceData.CorrectPos;

    public void SetSprite(Sprite newSprite)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = newSprite;
        
    }

    private void CalculateCorrectPosition()
    {
        _pieceData.CorrectPos = _pieceData.puzzlePixelStartCorner / _spriteRenderer.sprite.pixelsPerUnit;
    }

    public void SetPosition(Vector2 correctPos)
    {
        _pieceData.x = correctPos.x;
        _pieceData.y = correctPos.y;
    }
    
    public void Bind(puzzlePieceData pieceData)
    {
        _pieceData = pieceData;
    }
    
    public puzzlePieceData GetData()
    {
        return _pieceData;
    }

    private void OnMouseDown()
    {
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
        transform.position = GetMouseWorldPos() + PuzzleGame.Instance.HeldOffset;
    }

    private void OnMouseUp()
    {
        if(Vector3.Distance(_pieceData.CorrectPos,transform.localPosition) < 1)
            MoveToCorrectPos(); 
    }

    public void SetBoardPosition(int pieceX, int pieceY)
    {
        _pieceData.x = pieceX;
        _pieceData.y = pieceY;
    }
    
    void MoveToCorrectPos()
    {
        transform.localPosition = _pieceData.CorrectPos;
    }

    public void SetCornerStart(int startPixelX, int startPixelY)
    {
        _pieceData.puzzlePixelStartCorner = new Vector2(startPixelX, startPixelY);
    }

    

    public void Initialize(Vector2 upleft,Vector2 downright)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        CalculateCorrectPosition();
        var randX = Random.Range(upleft.x, downright.x);
        var randY = Random.Range(downright.y, upleft.y);
        transform.position = new Vector3(randX,randY,0);
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