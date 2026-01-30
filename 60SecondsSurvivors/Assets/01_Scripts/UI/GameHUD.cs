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
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Slider hpSlider;

        [Header("Item Popup")]
        [SerializeField] private TMP_Text itemText;
        private Vector2 itemTextPosition;
        private float pickupDuration = 0.9f;
        private Vector2 pickupMove = new Vector2(0f, 40f);

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
            itemTextPosition = itemText.rectTransform.anchoredPosition;
        }

        private void Update()
        {
            if (timeManager != null && timeText != null)
            {
                timeText.text = $"TIME: {Mathf.CeilToInt(timeManager.RemainingTime)}";
            }

            if (PlayerController.Instance != null && hpText != null)
            {
                hpText.text = $"{PlayerController.Instance.CurrentHp}/{PlayerController.Instance.MaxHp}";
            }

            if (PlayerController.Instance != null && hpSlider != null)
            {
                hpSlider.value = PlayerController.Instance.CurrentHp / PlayerController.Instance.MaxHp;
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
            Vector2 startPosition = label.rectTransform.anchoredPosition;
            Color baseColor = label.color;

            while (t < pickupDuration)
            {
                t += Time.deltaTime;
                float progress = t / pickupDuration;

                label.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, startPosition + pickupMove, progress);

                float a = Mathf.Lerp(1f, 0f, progress);
                label.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);

                yield return null;
            }

            if (label != null)
            {
                label.alpha = 0;
                label.rectTransform.anchoredPosition = itemTextPosition;
            }
        }
    }
}

