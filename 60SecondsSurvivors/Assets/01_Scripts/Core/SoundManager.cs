using System.Collections;
using UnityEngine;

namespace _60SecondsSurvivors.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip playerShootClip;
        [SerializeField] private AudioClip enemyHitClip;
        [SerializeField] private AudioClip playerHitClip;
        [SerializeField] private AudioClip playerDeadClip;
        [SerializeField] private AudioClip itemSpawnClip;
        [SerializeField] private AudioClip itemPickupClip;
        [SerializeField] private AudioClip gameClearClip;
        [SerializeField] private AudioClip gameOverClip;

        [Header("BGM Clips (optional)")]
        [SerializeField] private AudioClip titleBgm;
        [SerializeField] private AudioClip gameBgm;
        [SerializeField] private AudioClip resultBgm;

        [Header("Volumes")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;

        private float defaultMusicFadeTime = 0.1f;

        private Coroutine _musicFadeCoroutine;
        private AudioClip _currentMusicClip;

        private const string PrefKeyMusicVolume = "60SS_MusicVolume";
        private const string PrefKeySfxVolume = "60SS_SfxVolume";
        private const string PrefKeyMusicMuted = "60SS_MusicMuted";
        private const string PrefKeySfxMuted = "60SS_SfxMuted";

        private bool musicMuted;
        private bool sfxMuted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // load saved settings
            musicVolume = PlayerPrefs.GetFloat(PrefKeyMusicVolume, musicVolume);
            sfxVolume = PlayerPrefs.GetFloat(PrefKeySfxVolume, sfxVolume);
            musicMuted = PlayerPrefs.GetInt(PrefKeyMusicMuted, 0) == 1;
            sfxMuted = PlayerPrefs.GetInt(PrefKeySfxMuted, 0) == 1;

            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
                sfxSource.mute = sfxMuted;
            }

            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
                musicSource.loop = true;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            PlayTitleBgm();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void PlayClip(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            if (sfxMuted) return;

            if (sfxSource != null)
                sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume * sfxVolume));
            else
                AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, Mathf.Clamp01(volume * sfxVolume));
        }

        // SFX API
        public void PlayPlayerShoot() => PlayClip(playerShootClip);
        public void PlayEnemyHit() => PlayClip(enemyHitClip);
        public void PlayPlayerHit() => PlayClip(playerHitClip);
        public void PlayPlayerDead() => PlayClip(playerHitClip);
        public void PlayItemSpawn() => PlayClip(itemSpawnClip);
        public void PlayItemPickup() => PlayClip(itemPickupClip);
        public void PlayGameClear()
        {
            StopMusic();
            PlayClip(gameClearClip);
        }
        public void PlayGameOver()
        {
            StopMusic();
            PlayClip(gameOverClip);
        }

        public void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = -1f)
        {
            if (musicSource == null)
            {
                if (clip == null) return;
                if (musicMuted) return;
                AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, musicVolume);
                return;
            }

            if (clip == _currentMusicClip && musicSource.isPlaying)
            {
                musicSource.loop = loop;
                musicSource.volume = musicVolume;
                return;
            }

            _currentMusicClip = clip;

            if (fadeTime < 0f) fadeTime = defaultMusicFadeTime;

            if (_musicFadeCoroutine != null)
                StopCoroutine(_musicFadeCoroutine);

            _musicFadeCoroutine = StartCoroutine(MusicCrossfadeCoroutine(clip, loop, fadeTime));
        }

        public void StopMusic(float fadeTime = -1f)
        {
            if (musicSource == null) return;
            if (fadeTime < 0f) fadeTime = defaultMusicFadeTime;

            if (_musicFadeCoroutine != null)
                StopCoroutine(_musicFadeCoroutine);

            _musicFadeCoroutine = StartCoroutine(MusicFadeOutCoroutine(fadeTime));
        }

        public void PlayTitleBgm(float fadeTime = -1f) => PlayMusic(titleBgm, true, fadeTime);
        public void PlayGameBgm(float fadeTime = -1f) => PlayMusic(gameBgm, true, fadeTime);
        public void PlayResultBgm(float fadeTime = -1f) => PlayMusic(resultBgm, true, fadeTime);

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume;
            PlayerPrefs.SetFloat(PrefKeyMusicVolume, musicVolume);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
            PlayerPrefs.SetFloat(PrefKeySfxVolume, sfxVolume);
            PlayerPrefs.Save();
        }

        public float GetMusicVolume() => musicVolume;
        public float GetSfxVolume() => sfxVolume;

        public void SetMusicMuted(bool muted)
        {
            musicMuted = muted;
            if (musicSource != null)
                musicSource.mute = musicMuted;
            PlayerPrefs.SetInt(PrefKeyMusicMuted, musicMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetSfxMuted(bool muted)
        {
            sfxMuted = muted;
            if (sfxSource != null)
                sfxSource.mute = sfxMuted;
            PlayerPrefs.SetInt(PrefKeySfxMuted, sfxMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool IsMusicMuted() => musicMuted;
        public bool IsSfxMuted() => sfxMuted;

        private IEnumerator MusicCrossfadeCoroutine(AudioClip newClip, bool loop, float fadeTime)
        {
            float startVol = musicSource.volume;
            float t = 0f;
            if (musicSource.isPlaying)
            {
                while (t < fadeTime)
                {
                    t += Time.deltaTime;
                    musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime) * musicVolume;
                    yield return null;
                }
                musicSource.Stop();
            }

            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();

            t = 0f;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, 1f, t / fadeTime) * musicVolume;
                yield return null;
            }

            musicSource.volume = musicVolume;
            _musicFadeCoroutine = null;
        }

        private IEnumerator MusicFadeOutCoroutine(float fadeTime)
        {
            float startVol = musicSource.volume;
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime) * musicVolume;
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = musicVolume;
            _musicFadeCoroutine = null;
        }
    }
}