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

        [SerializeField] private TimeManager _timeManager;

        [SerializeField] private TMP_Text _timeText;

        [Header("HP")]
        [SerializeField] private Slider _hpSlider;
        [Tooltip("플레이어 월드 위치 기준 오프셋 (월드 단위), 플레이어 바로 아래")]
        [SerializeField] private Vector3 _hpWorldOffset = new Vector3(0f, -1f, 0f);
        [Tooltip("Canvas 내 UI 위치 추가 오프셋 (픽셀)")]
        [SerializeField] private Vector2 _hpUiOffset = Vector2.zero;

        [Header("Score")]
        [SerializeField] private TMP_Text _scoreText;

        [Header("Item Buff")]
        [SerializeField] private TMP_Text _itemText;
        private Vector2 _itemTextPosition;
        private float _pickupDuration = 0.9f;
        private Vector2 _pickupMove = new Vector2(0f, 40f);

        private RectTransform _hpSliderRect;
        private RectTransform _canvasRect;
        private Camera _uiCamera;

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
            if (_scoreText != null)
                _scoreText.text = "SCORE: 0";

            if (_itemText != null)
                _itemTextPosition = _itemText.rectTransform.anchoredPosition;

            if (_hpSlider != null)
                _hpSliderRect = _hpSlider.GetComponent<RectTransform>();

            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _canvasRect = canvas.GetComponent<RectTransform>();
                _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
            }

            // 이벤트 구독
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
                // 초기 표시
                _scoreText.text = $"SCORE: {ScoreManager.Instance.CurrentScore}";
            }

            if (_timeManager != null)
            {
                _timeManager.OnSecondTick += HandleSecondTick;
                // 초기 표시
                _timeText.text = $"TIME: {Mathf.CeilToInt(_timeManager.RemainingTime)}";
            }

            if (PlayerController.Instance != null && _hpSlider != null)
            {
                PlayerController.Instance.OnHpChanged += HandleHpChanged;
                // 초기 표시
                _hpSlider.value = PlayerController.Instance.CurrentHp / PlayerController.Instance.MaxHp;
            }
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;

            if (_timeManager != null)
                _timeManager.OnSecondTick -= HandleSecondTick;

            if (PlayerController.Instance != null)
                PlayerController.Instance.OnHpChanged -= HandleHpChanged;
        }

        private void HandleScoreChanged(int newScore)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"SCORE: {newScore}";
            }
        }

        private void HandleSecondTick(int secondsRemaining)
        {
            if (_timeText != null)
            {
                _timeText.text = $"TIME: {secondsRemaining}";
            }
        }

        private void HandleHpChanged(int cur, int max)
        {
            if (_hpSlider != null)
            {
                _hpSlider.value = max > 0 ? (float)cur / max : 0f;
            }
        }

        private void FixedUpdate()
        {
            UpdateHpSliderPosition();
        }

        private void UpdateHpSliderPosition()
        {
            if (_hpSliderRect == null || _canvasRect == null || PlayerController.Instance == null)
                return;

            Vector3 worldPos = (Vector3)PlayerController.Instance.Position + _hpWorldOffset;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_uiCamera, worldPos);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPoint, _uiCamera, out var localPoint))
            {
                _hpSliderRect.anchoredPosition = localPoint + _hpUiOffset;
            }
        }

        public void ShowPickup(string text)
        {
            if (_itemText == null) return;

            _itemText.text = text;
            _itemText.alpha = 1f;
            StartCoroutine(PopupRoutine(_itemText));
        }

        private IEnumerator PopupRoutine(TMP_Text label)
        {
            float t = 0f;
            Vector2 start = label.rectTransform.anchoredPosition;
            Color baseColor = label.color;

            while (t < _pickupDuration)
            {
                t += Time.deltaTime;
                float progress = t / _pickupDuration;

                label.rectTransform.anchoredPosition = Vector2.Lerp(start, start + _pickupMove, progress);

                float a = Mathf.Lerp(1f, 0f, progress);
                label.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);

                yield return null;
            }

            if (label != null)
            {
                label.rectTransform.anchoredPosition = _itemTextPosition;
            }
        }
    }
}

