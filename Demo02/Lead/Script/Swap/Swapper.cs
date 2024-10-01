using UnityEngine;
using UnityEngine.EventSystems;


namespace MobileController
{
    [RequireComponent(typeof(Animator))]
    public class Swapper : MonoBehaviour, IPointerDownHandler
    {
        public enum ScrollDirection { prev, next };

        [Header("References")]
        public ScrollDirection scrollDirection;
        public Swap buttonSwap;


        private Animator anim;


        /**/


        #region Setup

        private void Awake()
        {
            anim = this.GetComponent<Animator>();    
        }

        #endregion


        #region Pointer Events

        public void OnPointerDown(PointerEventData eventData)
        {
            if (scrollDirection == ScrollDirection.prev) buttonSwap.OnPrev();
            else buttonSwap.OnNext();
        }

        #endregion


        #region Getters / Setters

        public void SetState(bool _enable)
        {
            if (_enable) anim.SetTrigger("Normal");
            else anim.SetTrigger("Disable");
        }

        #endregion
    }
}