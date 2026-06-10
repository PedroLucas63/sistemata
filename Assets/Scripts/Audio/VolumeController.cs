using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace Sistemata.Audio
{
    public class VolumeController : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string mixerParameterName; // Ex: MasterVol, BGMVol, SFXVol

        private Slider slider;

        private void Awake()
        {
            slider = GetComponent<Slider>();
            slider.minValue = 0.0001f; // Nunca deixe 0 absoluto devido à matemática do Log10!
            slider.maxValue = 1f;

            slider.onValueChanged.AddListener(SetVolume);
        }

        private void Start()
        {
            // Desliga o listener temporariamente para não acionar o salvamento sem querer
            slider.onValueChanged.RemoveListener(SetVolume);

            // Pega o valor salvo na memória (ou 0.75 se for a primeira vez)
            float savedValue = PlayerPrefs.GetFloat(mixerParameterName, 0.75f);
            slider.value = savedValue;

            // Liga o listener de volta
            slider.onValueChanged.AddListener(SetVolume);
        }

        public void SetVolume(float sliderValue)
        {
            // Converte a escala linear (0.0001 a 1) para a escala logarítmica de decibéis (-80dB a 0dB)
            float decibelValue = Mathf.Log10(sliderValue) * 20;
            audioMixer.SetFloat(mixerParameterName, decibelValue);

            // Salva o valor do slider (de 0.0001 a 1) na memória do computador
            PlayerPrefs.SetFloat(mixerParameterName, sliderValue);
            PlayerPrefs.Save();
        }
    }
}