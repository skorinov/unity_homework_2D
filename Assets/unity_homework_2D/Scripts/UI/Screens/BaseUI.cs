using Constants;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseUI : MonoBehaviour
    {
        [SerializeField] private float fadeSpeed = GameConstants.UI_FADE_SPEED;

        private CanvasGroup _canvasGroup;

        private CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        protected virtual void Awake()
        {
            InitializeCanvasGroup();
        }

        private void InitializeCanvasGroup()
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            ClearButtonStates();
        }

        public virtual void Hide()
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }

        public virtual void FadeIn(float duration = -1f)
        {
            gameObject.SetActive(true);
            float speed = duration > 0 ? duration : fadeSpeed;
            StartCoroutine(FadeCanvasGroup(CanvasGroup, 0f, 1f, speed, true));
        }

        public virtual void FadeOut(float duration = -1f, System.Action onComplete = null)
        {
            float speed = duration > 0 ? duration : fadeSpeed;
            StartCoroutine(FadeCanvasGroup(CanvasGroup, 1f, 0f, speed, false, onComplete));
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration,
            bool setInteractable, System.Action onComplete = null)
        {
            float elapsed = 0f;
            cg.alpha = start;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
                yield return null;
            }

            cg.alpha = end;
            cg.interactable = setInteractable;
            cg.blocksRaycasts = setInteractable;

            if (!setInteractable && end == 0f)
                gameObject.SetActive(false);

            onComplete?.Invoke();
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