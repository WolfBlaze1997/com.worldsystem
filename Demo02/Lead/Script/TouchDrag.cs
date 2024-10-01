using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace MobileController
{
    public class TouchDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerMoveHandler
    {
        public enum InitialState { normal, disable };
        public enum TouchDragState { normal, touch, disable };


        [Header("Behaviour")]
        public InitialState initialState;
        [Space(15)]

        [Header("Events")]
        [Space(5)]
        public UnityEvent TouchStartEvent;
        public UnityEvent TouchEndEvent;
        public UnityEvent EnableEvent;
        public UnityEvent DisableEvent;


        private Vector2 touchStartPosition = Vector2.zero;
        private Vector2 movementFromTouch = Vector2.zero;
        private TouchDragState touchDragState;


        /**/


        #region Setup

        private void Awake()
        {
            // update touchDrag
            if (initialState == InitialState.normal) touchDragState = TouchDragState.normal;
            else touchDragState = TouchDragState.disable;
        }

        #endregion


        #region Pointer Events

        public void OnPointerUp(PointerEventData eventData) { PointerUp(); }
        public void OnPointerExit(PointerEventData eventData) { PointerUp(); }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (touchDragState == TouchDragState.normal)
            {
                // record the starting position
                touchStartPosition = eventData.position;

                TouchStartEvent.Invoke();
                touchDragState = TouchDragState.touch;
            }
        }


        public void OnPointerMove(PointerEventData eventData)
        {
            if (touchDragState == TouchDragState.touch)
            {
                // calculate distance
                movementFromTouch = eventData.position - touchStartPosition;
            }
        }


        private void PointerUp()
        {
            if (touchDragState == TouchDragState.touch)
            {
                TouchEndEvent.Invoke();
                touchDragState = TouchDragState.normal;

                touchStartPosition = Vector2.zero;
                movementFromTouch = Vector2.zero;
            }
        }

        #endregion


        #region Getters / Setters

        public Vector2 GetTouchDragMovement() { return movementFromTouch; }
        public TouchDragState GetTouchDragState() { return touchDragState; }


        public void Enable()
        {
            EnableEvent.Invoke();
            touchDragState = TouchDragState.normal;

            touchStartPosition = Vector2.zero;
            movementFromTouch = Vector2.zero;
        }


        public void Disable()
        {
            DisableEvent.Invoke();
            touchDragState = TouchDragState.disable;

            touchStartPosition = Vector2.zero;
            movementFromTouch = Vector2.zero;
        }

        #endregion
    }
}