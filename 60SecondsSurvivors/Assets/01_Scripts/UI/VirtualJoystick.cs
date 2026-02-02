using UnityEngine;
using UnityEngine.EventSystems;

namespace _60SecondsSurvivors.UI
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 50f; // 핸들이 움직일 최대 픽셀 반경
        [SerializeField, Range(0f, 1f)] private float deadZone = 0.1f;
        private bool hideWhenIdle = true;

        private Vector2 input = Vector2.zero;
        private int activePointerId = -1;

        public Vector2 Direction => input.sqrMagnitude > (deadZone * deadZone) ? input.normalized : Vector2.zero;
        public float Magnitude => input.magnitude;

        private void Reset()
        {
            if (background == null)
                background = GetComponent<RectTransform>();

            if (handle == null && background != null && background.childCount > 0)
                handle = background.GetChild(0) as RectTransform;
        }

        private void Awake()
        {
            if (background == null)
                Reset();

            if (hideWhenIdle && background != null)
            {
                background.gameObject.SetActive(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != -1) return;
            activePointerId = eventData.pointerId;

            if (background == null || handle == null) return;

            if (hideWhenIdle)
                background.gameObject.SetActive(true);

            var parentRect = background.parent as RectTransform;
            if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out var localPointInParent))
            {
                background.anchoredPosition = localPointInParent;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId) return;
            if (background == null || handle == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out var localPoint))
                return;

            Vector2 size = background.sizeDelta * 0.5f;
            Vector2 raw = new Vector2(size.x != 0f ? localPoint.x / size.x : 0f, size.y != 0f ? localPoint.y / size.y : 0f);
            Vector2 clamped = Vector2.ClampMagnitude(raw, 1f);

            input = clamped;

            handle.anchoredPosition = clamped * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId) return;
            activePointerId = -1;

            input = Vector2.zero;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;

            if (hideWhenIdle && background != null)
                background.gameObject.SetActive(false);
        }
    }
}