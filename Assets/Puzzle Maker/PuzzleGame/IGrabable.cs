using System;
using UnityEngine;

namespace PuzzleMakerTwo.GameExample
{
    public interface IGrabable
    {
        //static Action<IGrabable> HoveringOver;
        //static Action<IGrabable> HoveringOut;

        Sprite GetIcon();
        void OnGrab(Vector2 pos);
        void OnRelease(Vector2 pos);
    }
}