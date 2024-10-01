using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace MobileController
{
    [RequireComponent(typeof(Animator))]
    public class Swap : MonoBehaviour
    {
        public enum InitialState { normal, disable };
        public enum SwapState { normal, disable };


        [Header("Behaviour")]
        public InitialState initialState;
        public bool isCyclical = false;
        [Space(15)]
        
        public List<SwapContent> items;
        [Min(0)] public int startIndex = 0;
        [Space(15)]

        [Header("References")]
        public Swapper swapperPrev;
        public Swapper swapperNext;
        public Image iconImage;
        [Space(15)]

        [Header("Events")]
        [Space(5)]
        public UnityEvent SwapEvent;
        public UnityEvent EnableEvent;
        public UnityEvent DisableEvent;


        private SwapState swapState;
        private Animator anim;

        private int currentIndex = 0;


        /**/


        #region Setup

        private void Start()
        {
            // setup animator
            anim = this.GetComponent<Animator>();

            // handle edge cases
            if (startIndex <= items.Count - 1) currentIndex = startIndex;
            else currentIndex = 0;


            // update state
            if (initialState == InitialState.normal)
            {
                anim.SetTrigger("Normal");
                swapState = SwapState.normal;

                UpdateSwappers();
            }
            else
            {
                anim.SetTrigger("Disable");
                swapState = SwapState.disable;

                swapperPrev.SetState(false);
                swapperNext.SetState(false);
            }

            // update content
            UpdateContent();
        }

        #endregion


        #region Swap

        public void OnPrev()
        {
            if (swapState != SwapState.disable)
            {
                // do nothing if already at index 0
                if (currentIndex == 0 && !isCyclical) return;

                // handle the index
                currentIndex--;
                if (currentIndex < 0 && isCyclical) currentIndex = items.Count - 1;

                // update
                SwapEvent.Invoke();
                anim.SetTrigger("Swap");

                UpdateContent();
                UpdateSwappers();
            }
        }


        public void OnNext()
        {
            if (swapState != SwapState.disable)
            {
                // do nothing if at max index
                if (currentIndex == items.Count - 1 && !isCyclical) return;

                // handle the index
                currentIndex++;
                if (currentIndex > items.Count - 1 && isCyclical) currentIndex = 0;

                // update
                SwapEvent.Invoke();
                anim.SetTrigger("Swap");

                UpdateContent();
                UpdateSwappers();
            }
        }

        #endregion


        #region Update

        private void UpdateContent()
        {
            iconImage.sprite = items[currentIndex].contentImage;
        }


        private void UpdateSwappers()
        {
            if (!isCyclical)
            {
                if (currentIndex == 0)
                {
                    swapperPrev.SetState(false);
                    swapperNext.SetState(true);
                }
                else if (currentIndex == items.Count - 1)
                {
                    swapperPrev.SetState(true);
                    swapperNext.SetState(false);
                }
                else
                {
                    swapperPrev.SetState(true);
                    swapperNext.SetState(true);
                }
            }
            else
            {
                swapperPrev.SetState(true);
                swapperNext.SetState(true);
            }
        }

        #endregion


        #region Getters / Setters
        
        public SwapContent GetCurrentItem() { return items[currentIndex]; }
        public int GetCurrentIndex() { return currentIndex; }
        public SwapState GetSwapState() { return swapState; }


        public void Enable()
        {
            EnableEvent.Invoke();
            anim.SetTrigger("Normal");
            swapState = SwapState.normal;

            UpdateSwappers();
        }


        public void Disable()
        {
            DisableEvent.Invoke();
            anim.SetTrigger("Disable");
            swapState = SwapState.disable;

            swapperPrev.SetState(false);
            swapperNext.SetState(false);
        }


        public void SetActiveIndex(int _index)
        {
            currentIndex = _index;
            anim.SetTrigger("Swap");

            UpdateContent();
            UpdateSwappers();
        }

        #endregion
    }
}