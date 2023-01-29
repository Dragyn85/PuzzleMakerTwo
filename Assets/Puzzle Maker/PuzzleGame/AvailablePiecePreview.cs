using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleMakerTwo.GameExample
{
    public class AvailablePiecePreview : MonoBehaviour, IGrabable, IPointerEnterHandler,IPointerExitHandler
    {
        public static event Action OnAnyPieceSelected;
        public static event Action OnAnyPieceReleased;

        [SerializeField] Image _imagePreview;
        
        private PuzzlePiece _previewedPiece;

        public static event Action<IGrabable> HoveringOver;
        public static event Action<IGrabable> HoveringOut;

        public Sprite GetIcon()
        {
            return _previewedPiece.GetSprite();
        }

        public void OnGrab(Vector2 pos)
        {
            _imagePreview.enabled= false;
            OnAnyPieceSelected?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HoveringOver?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HoveringOut?.Invoke(this);
        }

        public void OnRelease(Vector2 pos)
        {
            OnAnyPieceReleased?.Invoke();
            _imagePreview.enabled = true;
            _previewedPiece.CheckPosition(pos, OnPosOk);
            void OnPosOk(bool posOK){
                _imagePreview.enabled = !posOK;
                if (posOK)
                {
                    Destroy(gameObject);
                }
            }
        }

        internal void AddPiece(PuzzlePiece piece)
        {
            _previewedPiece = piece;
            _imagePreview.sprite= piece.GetSprite();
            

        }
    }
}