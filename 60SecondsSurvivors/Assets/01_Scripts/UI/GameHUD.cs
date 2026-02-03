using System.Collections;
using UnityEngine;
using TMPro;
using _60SecondsSurvivors.Core;
using _60SecondsSurvivors.Player;
using UnityEngine.UI;

namespace _60SecondsSurvivors.UI
{
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        [SerializeField] private TimeManager timeManager;

        [SerializeField] private TMP_Text timeText;

        [Header("HP")]
        [SerializeField] private Slider hpSlider;
        [Tooltip("플레이어 월드 위치 기준 오프셋 (월드 단위), 플레이어 바로 아래")]
        [SerializeField] private Vector3 hpWorldOffset = new Vector3(0f, -1f, 0f);
        [Tooltip("Canvas 내 UI 위치 추가 오프셋 (픽셀)")]
        [SerializeField] private Vector2 hpUiOffset = Vector2.zero;

        [Header("Score")]
        [SerializeField] private TMP_Text scoreText;

        [Header("Item Buff")]
        [SerializeField] private TMP_Text itemText;
        private Vector2 itemTextPosition;
        private float pickupDuration = 0.9f;
        private Vector2 pickupMove = new Vector2(0f, 40f);

        private RectTransform hpSliderRect;
        private RectTransform canvasRect;
        private Camera uiCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (scoreText != null)
                scoreText.text = "SCORE: 0";

            if (itemText != null)
                itemTextPosition = itemText.rectTransform.anchoredPosition;

            if (hpSlider != null)
                hpSliderRect = hpSlider.GetComponent<RectTransform>();

            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasRect = canvas.GetComponent<RectTransform>();
                uiCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
            }

            // 이벤트 구독
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
                // 초기 표시
                scoreText.text = $"SCORE: {ScoreManager.Instance.CurrentScore}";
            }

            if (timeManager != null)
            {
                timeManager.OnSecondTick += HandleSecondTick;
                // 초기 표시
                timeText.text = $"TIME: {Mathf.CeilToInt(timeManager.RemainingTime)}";
            }

            if (PlayerController.Instance != null && hpSlider != null)
            {
                PlayerController.Instance.OnHpChanged += HandleHpChanged;
                // 초기 표시
                hpSlider.value = PlayerController.Instance.CurrentHp / PlayerController.Instance.MaxHp;
            }
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;

            if (timeManager != null)
                timeManager.OnSecondTick -= HandleSecondTick;

            if (PlayerController.Instance != null)
                PlayerController.Instance.OnHpChanged -= HandleHpChanged;
        }

        private void HandleScoreChanged(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {newScore}";
            }
        }

        private void HandleSecondTick(int secondsRemaining)
        {
            if (timeText != null)
            {
                timeText.text = $"TIME: {secondsRemaining}";
            }
        }

        private void HandleHpChanged(int cur, int max)
        {
            if (hpSlider != null)
            {
                hpSlider.value = max > 0 ? (float)cur / max : 0f;
            }
        }

        private void FixedUpdate()
        {
            UpdateHpSliderPosition();
        }

        private void UpdateHpSliderPosition()
        {
            if (hpSliderRect == null || canvasRect == null || PlayerController.Instance == null)
                return;

            Vector3 worldPos = (Vector3)PlayerController.Instance.Position + hpWorldOffset;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCamera, out var localPoint))
            {
                hpSliderRect.anchoredPosition = localPoint + hpUiOffset;
            }
        }

        public void ShowPickup(string text)
        {
            if (itemText == null) return;

            itemText.text = text;
            itemText.alpha = 1f;
            StartCoroutine(PopupRoutine(itemText));
        }

        private IEnumerator PopupRoutine(TMP_Text label)
        {
            float t = 0f;
            Vector2 start = label.rectTransform.anchoredPosition;
            Color baseColor = label.color;

            while (t < pickupDuration)
            {
                t += Time.deltaTime;
                float progress = t / pickupDuration;

                label.rectTransform.anchoredPosition = Vector2.Lerp(start, start + pickupMove, progress);

                float a = Mathf.Lerp(1f, 0f, progress);
                label.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);

                yield return null;
            }

            if (label != null)
            {
                label.rectTransform.anchoredPosition = itemTextPosition;
            }
        }
    }
}

