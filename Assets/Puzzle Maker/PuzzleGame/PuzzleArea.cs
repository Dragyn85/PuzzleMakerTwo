using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleArea : MonoBehaviour
{
    [SerializeField] GameObject puzzleArea;

    public Vector2 GetPuzzleAreaSize()
    {
        var rectTransform = puzzleArea.GetComponent<RectTransform>();
        Vector3[] fourCorners = new Vector3[4];
        if (rectTransform != null)
        {
            rectTransform.GetWorldCorners(fourCorners);

            float height = 0;
            float width = 0;

            foreach (var corner in fourCorners)
            {
                if (height == 0 && width == 0)
                {
                    height = corner.y;
                    width = corner.x;
                }
                else if (height != corner.y && width != corner.x)
                {
                    height = Mathf.Abs(height - corner.y);
                    width = Mathf.Abs(width - corner.x);
                }
            }
            return new Vector2(width, height);
        }
        return Vector2.zero;
    }
}
