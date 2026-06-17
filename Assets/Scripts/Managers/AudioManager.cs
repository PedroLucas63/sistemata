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
            StartCoroutine(LoadVolumesNextFrame());
        }

        private IEnumerator LoadVolumesNextFrame()
        {
            yield return null;

            // Se não houver valor salvo, o PlayerPrefs.GetFloat retorna 0 por padrão.
            // Para não começar mudo na primeira vez, podemos passar um valor padrão (ex: 0.75f)
            float masterVol = PlayerPrefs.GetFloat("MasterVol", 0.75f);
            float bgmVol = PlayerPrefs.GetFloat("BGMVol", 0.45f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.75f);

            ApplyVolumeToMixer("MasterVol", masterVol);
            ApplyVolumeToMixer("BGMVol", bgmVol);
            ApplyVolumeToMixer("SFXVol", sfxVol);
        }

        private void ApplyVolumeToMixer(string parameterName, float linearValue)
        {
            float decibelValue = Mathf.Log10(Mathf.Clamp(linearValue, 0.0001f, 1f)) * 20;

            mainMixer.SetFloat(parameterName, decibelValue);
        }

        private void SetupAudioSources()
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.outputAudioMixerGroup = bgmGroup;
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;

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

        // ===================================================================================
        // NOVOS MÉTODOS DE SFX AQUI
        // ===================================================================================

        /// <summary>
        /// Procura um AudioSource livre no pool. Se todos estiverem ocupados, cria um novo.
        /// </summary>
        private AudioSource GetAvailableSFXSource()
        {
            for (int i = 0; i < sfxPool.Count; i++)
            {
                if (!sfxPool[i].isPlaying)
                {
                    return sfxPool[i];
                }
            }

            // Pool cheio! Expande dinamicamente.
            return CreateNewPoolSource(sfxPool[0].transform.parent.gameObject);
        }

        /// <summary>
        /// Toca um som Espacial (3D) no mundo. Útil para monstros, tiros e impactos.
        /// </summary>
        public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource source = GetAvailableSFXSource();

            source.outputAudioMixerGroup = sfxGroup; // Garante que está no grupo de Efeitos
            source.transform.position = position;
            source.spatialBlend = 1f; // Transforma em som 3D
            source.minDistance = 5f;  // Distância onde o som começa a perder força
            source.maxDistance = 40f; // Distância máxima para ouvir
            source.pitch = Random.Range(0.9f, 1.1f); // Adiciona variação de +-10% para não enjoar!
            source.volume = volume;
            source.clip = clip;
            source.Play();
        }

        /// <summary>
        /// Toca um efeito sonoro 2D (Centralizado).
        /// </summary>
        public void PlaySFX2D(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource source = GetAvailableSFXSource();

            source.outputAudioMixerGroup = sfxGroup; // Vai para o Mixer de SFX!
            source.spatialBlend = 0f; // 2D puro, som focado no meio da cabeça
            source.pitch = Random.Range(0.9f, 1.1f);
            source.volume = volume;
            source.clip = clip;
            source.Play();
        }

        /// <summary>
        /// Toca um som Plano (2D) para a Interface. Independe da câmera.
        /// </summary>
        public void PlayUISFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource source = GetAvailableSFXSource();

            source.outputAudioMixerGroup = uiGroup; // Joga para o grupo de UI!
            source.spatialBlend = 0f; // Som 2D puro
            source.pitch = 1f; // Sem variação na UI
            source.volume = volume;
            source.clip = clip;
            source.Play();
        }

        // ===================================================================================
        // MÚSICA DE FUNDO
        // ===================================================================================

        public void ChangeBGM(AudioClip newTrack, float fadeDuration = 1.5f)
        {
            if (bgmSource.clip == newTrack) return;

            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeBGMTurn(newTrack, fadeDuration));
        }

        private IEnumerator FadeBGMTurn(AudioClip newTrack, float duration)
        {
            float currentTime = 0;
            float startVolume = bgmSource.volume;

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