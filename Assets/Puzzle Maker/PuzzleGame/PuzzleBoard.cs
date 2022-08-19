using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleBoard : MonoBehaviour
{
    public bool CheckPosition(PuzzlePiece puzzlePiece)
    {
        var currentWorldPos = puzzlePiece.transform.position;
        var correctWorldPos = transform.position + (Vector3)puzzlePiece.CorrectPos;
        if (Vector3.Distance(currentWorldPos,correctWorldPos) < 1)
        {
            var correctPos = (Vector2)transform.position + puzzlePiece.CorrectPos;
            puzzlePiece.MoveToPos(correctPos);
            return true;
        }
        return false;
    }

    public void SetBackground(Sprite newBackGround)
    {
        GetComponent<SpriteRenderer>().sprite = newBackGround;
    }
}
