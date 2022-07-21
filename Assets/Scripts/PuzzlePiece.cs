using PuzzleMakerTwo;
using UnityEngine;

[RequireComponent(typeof(SpriteMask))]
public class PuzzlePiece : MonoBehaviour
{
    
    private SpriteMask _mask;
    private PuzzleGame _puzzleGame;
    private puzzlePieceData _pieceData;


    public void SetMask(Sprite mask)
    {
        _mask = GetComponent<SpriteMask>();
        _mask.sprite = mask;
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
}