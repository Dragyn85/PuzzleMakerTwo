using PuzzleMakerTwo;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
public class PuzzlePiece : MonoBehaviour, IPointerClickHandler,IPointerDownHandler
{
    
    private SpriteRenderer _spriteRenderer;
    private PuzzleGame _puzzleGame;
    [SerializeField]private puzzlePieceData _pieceData;


    public Vector2 correctPos => _pieceData.puzzlePixelStartCorner / _spriteRenderer.sprite.pixelsPerUnit;

    public void SetSprite(Sprite newSprite)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = newSprite;
        
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

    public void gameUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            
            if (hit.transform.GetComponent<PuzzlePiece>() == this)
            {
                Debug.Log("Hitting " + gameObject);
            }
        }   
    }

    public puzzlePieceData GetData()
    {
        return _pieceData;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.pointerClick.gameObject == gameObject)
        {
            Debug.Log("Clicked this :" +gameObject.name);
            return;
        }

        Debug.Log("Clicked something");


    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void SetBoardPosition(int pieceX, int pieceY)
    {
        _pieceData.x = pieceX;
        _pieceData.y = pieceY;
    }

    public void SetCornerStart(int startPixelX, int startPixelY)
    {
        _pieceData.puzzlePixelStartCorner = new Vector2(startPixelX, startPixelY);
    }
}