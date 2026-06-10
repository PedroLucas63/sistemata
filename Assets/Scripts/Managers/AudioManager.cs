using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Sistemata.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Referências")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup uiGroup;

        [Header("Pool de SFX")]
        [SerializeField] private int initialPoolSize = 20;

        private AudioSource bgmSource;
        private List<AudioSource> sfxPool = new List<AudioSource>();
        private Coroutine bgmFadeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Inicia uma rotina para esperar a Unity carregar o Mixer completamente
            StartCoroutine(LoadVolumesNextFrame());
        }

        private IEnumerator LoadVolumesNextFrame()
        {
            yield return null; // Espera 1 frame

            float masterVol = PlayerPrefs.GetFloat("MasterVol");
            float bgmVol = PlayerPrefs.GetFloat("BGMVol");
            float sfxVol = PlayerPrefs.GetFloat("SFXVol");

            ApplyVolumeToMixer("MasterVol", masterVol);
            ApplyVolumeToMixer("BGMVol", bgmVol);
            ApplyVolumeToMixer("SFXVol", sfxVol);
        }

        private void ApplyVolumeToMixer(string parameterName, float linearValue)
        {
            // Mesma fórmula logarítmica que usamos nos Sliders
            float decibelValue = Mathf.Log10(Mathf.Clamp(linearValue, 0.0001f, 1f)) * 20;

            if (parameterName == "BGMVol")
                decibelValue -= 30f;

            mainMixer.SetFloat(parameterName, decibelValue);
        }

        private void SetupAudioSources()
        {
            // Configura o AudioSource dedicado para Música
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.outputAudioMixerGroup = bgmGroup;
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;

            // Cria o Pool inicial de SFX para evitar Instantiate em tempo de execução
            GameObject poolHolder = new GameObject("SFX_Pool");
            poolHolder.transform.SetParent(transform);

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewPoolSource(poolHolder);
            }
        }

        private AudioSource CreateNewPoolSource(GameObject parent)
        {
            GameObject sfxObj = new GameObject("Pooled_SFX");
            sfxObj.transform.SetParent(parent.transform);

            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxGroup;
            source.playOnAwake = false;

            sfxPool.Add(source);
            return source;
        }

        public void ChangeBGM(AudioClip newTrack, float fadeDuration = 1.5f)
        {
            if (bgmSource.clip == newTrack) return; // Já está tocando esta música

            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeBGMTurn(newTrack, fadeDuration));
        }

        private IEnumerator FadeBGMTurn(AudioClip newTrack, float duration)
        {
            float currentTime = 0;
            float startVolume = bgmSource.volume;

            // Fade Out
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.clip = newTrack;

            if (newTrack != null)
            {
                bgmSource.Play();
                currentTime = 0;

                // Fade In
                while (currentTime < duration)
                {
                    currentTime += Time.deltaTime;
                    bgmSource.volume = Mathf.Lerp(0f, 1f, currentTime / duration);
                    yield return null;
                }
            }
        }
    }
}