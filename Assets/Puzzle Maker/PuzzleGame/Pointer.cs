using PuzzleMakerTwo.GameExample;
using System;
using UnityEngine;
namespace PuzzleMakerTwo.GameExample
{
    public class Pointer : MonoBehaviour
    {
        IGrabable _currentTarget;
        IGrabable _currentGrab;
        private SpriteRenderer _spriteRenderer;

        private void Update()
        {

            if (Input.GetMouseButtonDown(0) && _currentGrab == null && _currentTarget != null)
            {
                _currentTarget.OnGrab(transform.position);
                _spriteRenderer.sprite = _currentTarget.GetIcon();
                _spriteRenderer.enabled = true;
                _currentGrab = _currentTarget;
            }
            if (Input.GetMouseButtonUp(0) && _currentGrab != null)
            {
                _currentGrab.OnRelease(transform.position);
                _spriteRenderer.enabled = false;
                _currentGrab = null;
            }
            if (_currentGrab != null)
            {
                transform.position = PuzzleInputs.GetMouseWorldPos();
            }


        }


        private void HandleHoveringOut(IGrabable grabable)
        {
            if (_currentTarget == grabable)
            {
                _currentTarget = null;
            }
        }

        private void HandleHoveringStart(IGrabable grabable)
        {
            _currentTarget = grabable;
        }
        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.enabled = false;
            AvailablePiecePreview.HoveringOver += HandleHoveringStart;
            AvailablePiecePreview.HoveringOut += HandleHoveringOut;
        }
        private void OnDestroy()
        {
            AvailablePiecePreview.HoveringOver -= HandleHoveringStart;
            AvailablePiecePreview.HoveringOut -= HandleHoveringOut;
        }

    }
}