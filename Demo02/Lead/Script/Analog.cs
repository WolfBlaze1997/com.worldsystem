using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace MobileController
{
    [RequireComponent(typeof(Animator))]
    public class Analog : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public enum InitialState { normal, disable };
        public enum AnalogState { normal, disable };

        [Header("Behaviour")]
        public InitialState initialState;
        [Min(0)] public float analogRadious = 70f;
        [Min(0)] public float stickReturnSpeed = 10f;
        [Min(0)] public float stickFollowSpeed = 100f;
        public bool smoothStickOnMovement = true;
        public bool snapStickOnClick = false;
        public bool snapStickOnRelease = false;
        [Space(15)]

        [Header("References")]
        public RectTransform StickTarget;
        [Space(15)]

        [Header("Events")]
        [Space(5)]
        public UnityEvent stickTapEvent;
        public UnityEvent stickDragEvent;
        public UnityEvent stickExitEvent;
        public UnityEvent EnableEvent;
        public UnityEvent DisableEvent;


        private bool isDragging = false;
        private bool isStickInOrigin = false;
        private Vector2 originalStickPosition;
        private Vector2 pointerStartPosition;

        private AnalogState analogState;
        private Animator anim;


        /**/


        #region Setup

        private void Start()
        {
            // setup stick pos
            originalStickPosition = StickTarget.anchoredPosition;

            // setup animator
            anim = this.GetComponent<Animator>();

            // update state
            if (initialState == InitialState.normal)
            {
                anim.SetTrigger("Normal");
                analogState = AnalogState.normal;
            }
            else
            {
                anim.SetTrigger("Disable");
                analogState = AnalogState.disable;
            }
        }

        #endregion


        #region Stick

        private void Update()
        {
            if (!isDragging && !isStickInOrigin)
            {
                // smoothly return stick to initial position
                if (!snapStickOnRelease)
                {
                    StickTarget.anchoredPosition = Vector2.Lerp(StickTarget.anchoredPosition, originalStickPosition, stickReturnSpeed * Time.deltaTime);
                    if (Vector2.Distance(StickTarget.anchoredPosition, originalStickPosition) < 0.1f)
                    {
                        StickTarget.anchoredPosition = originalStickPosition;
                        isStickInOrigin = true;
                    }
                }

                // snap stick to initial position
                else
                {
                    StickTarget.anchoredPosition = originalStickPosition;
                    isStickInOrigin = true;
                }
            }
        }

        #endregion


        #region Pointer Events

        public void OnPointerDown(PointerEventData eventData)
        {
            // start dragging stick
            if (analogState != AnalogState.disable)
            {
                isDragging = true;
                isStickInOrigin = false;
                pointerStartPosition = eventData.position;

                // calculate offset from click position
                Vector2 offset = Vector2.zero;
                if (!snapStickOnClick) offset = (Vector2)StickTarget.position - (Vector2)eventData.position;
                StickTarget.position = pointerStartPosition + offset;

                stickTapEvent.Invoke();
            }
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            // determine stick position
            Vector2 pointerPosition = eventData.position;
            Vector2 direction = (pointerPosition - pointerStartPosition).normalized;
            float distance = Vector2.Distance(pointerPosition, pointerStartPosition);
            Vector2 newPosition = originalStickPosition + direction * Mathf.Clamp(distance, 0f, analogRadious);

            // assign stick smooth if option is enabled
            if (smoothStickOnMovement) StickTarget.anchoredPosition = Vector2.Lerp(StickTarget.anchoredPosition, newPosition, stickFollowSpeed * Time.deltaTime);
            else StickTarget.anchoredPosition = newPosition;

            stickDragEvent.Invoke();
        }


        public void OnPointerUp(PointerEventData eventData)
        {
            // drag state
            isDragging = false;
            stickExitEvent.Invoke();
        }

        #endregion


        #region Getters / Setters

        public Vector2 GetStickPosition() { return (StickTarget.anchoredPosition - originalStickPosition) / analogRadious; }
        public AnalogState GetAnalogState() { return analogState; }


        public void Enable()
        {
            EnableEvent.Invoke();
            anim.SetTrigger("Normal");
            analogState = AnalogState.normal;
        }


        public void Disable()
        {
            DisableEvent.Invoke();
            anim.SetTrigger("Disable");
            analogState = AnalogState.disable;
        }

        #endregion
    }
}