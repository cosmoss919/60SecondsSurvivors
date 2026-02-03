using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _60SecondsSurvivors.Core;

namespace _60SecondsSurvivors.UI
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Toggle musicMuteToggle;

        [Header("SFX")]
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle sfxMuteToggle;

        private void Start()
        {
            if (SoundManager.Instance == null) return;

            float mv = SoundManager.Instance.GetMusicVolume();
            float sv = SoundManager.Instance.GetSfxVolume();
            bool mm = SoundManager.Instance.IsMusicMuted();
            bool sm = SoundManager.Instance.IsSfxMuted();

            if (musicSlider != null)
            {
                musicSlider.value = mv;
                musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = sv;
                sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
            }

            if (musicMuteToggle != null)
            {
                musicMuteToggle.isOn = mm;
                musicMuteToggle.onValueChanged.AddListener(OnMusicMuteChanged);
            }

            if (sfxMuteToggle != null)
            {
                sfxMuteToggle.isOn = sm;
                sfxMuteToggle.onValueChanged.AddListener(OnSfxMuteChanged);
            }
        }

        private void OnEnable()
        {
            Time.timeScale = 0;
        }

        private void OnDisable()
        {
            Time.timeScale = 1;
        }

        public void OnMusicSliderChanged(float value)
        {
            SoundManager.Instance?.SetMusicVolume(value);
            if (musicMuteToggle != null && musicMuteToggle.isOn && value > 0f)
            {
                SoundManager.Instance?.SetMusicMuted(false);
                musicMuteToggle.isOn = false;
            }
        }

        public void OnSfxSliderChanged(float value)
        {
            SoundManager.Instance?.SetSfxVolume(value);
            if (sfxMuteToggle != null && sfxMuteToggle.isOn && value > 0f)
            {
                SoundManager.Instance?.SetSfxMuted(false);
                sfxMuteToggle.isOn = false;
            }
        }

        public void OnMusicMuteChanged(bool isMuted)
        {
            SoundManager.Instance?.SetMusicMuted(isMuted);
        }

        public void OnSfxMuteChanged(bool isMuted)
        {
            SoundManager.Instance?.SetSfxMuted(isMuted);
        }


        public void OnClose()
        {
            this.gameObject.SetActive(false);
        }
    }
}