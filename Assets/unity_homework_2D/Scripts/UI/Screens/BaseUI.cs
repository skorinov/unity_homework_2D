using Constants;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    public abstract class BaseUI : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
            ClearButtonStates();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        private void ClearButtonStates()
        {
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                button.OnDeselect(null);
                if (button.transition == Selectable.Transition.ColorTint && button.targetGraphic)
                {
                    button.targetGraphic.color = button.colors.normalColor;
                }
            }
        }
    }
}