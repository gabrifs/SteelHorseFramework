using UnityEngine;

namespace SteelHorse.Framework.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIButton : MonoBehaviour
    {
        [SerializeField] private bool _mobileButton = false;

        private CanvasGroup _canvasGroup;
        private CanvasGroup Canvas => _canvasGroup != null ? _canvasGroup : (_canvasGroup = GetComponent<CanvasGroup>());

        private void Awake()
        {
            if (_mobileButton)
                SetVisible(IsMobilePlatform());
        }

        private static bool IsMobilePlatform()
        {
            return Application.platform == RuntimePlatform.Android
                || Application.platform == RuntimePlatform.IPhonePlayer;
        }

        private void SetVisible(bool visible)
        {
            Canvas.alpha = visible ? 1f : 0f;
            Canvas.interactable = visible;
            Canvas.blocksRaycasts = visible;
        }
    }
}
