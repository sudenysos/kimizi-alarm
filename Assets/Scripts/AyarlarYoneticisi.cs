using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AyarlarYoneticisi : MonoBehaviour
{
    [Header("Ses Karıştırıcısı (Audio Mixer)")]
    public AudioMixer anaMixer;

    [Header("Arayüz Sliderları (Opsiyonel)")]
    [Tooltip("Bu sahnede slider varsa atayın, yoksa boş bırakabilirsiniz.")]
    public Slider muzikSlider;
    public Slider sfxSlider;

    void Start()
    {
        // 1. Kayıtlı verileri çek (Yoksa varsayılan değer 1 yani %100)
        float kayitliMuzik = PlayerPrefs.GetFloat("MuzikAyar", 1f);
        float kayitliSFX = PlayerPrefs.GetFloat("SFXAyar", 1f);

        // 2. Sahnedeki Slider'ların pozisyonunu kayıtlı değere eşitle
        if (muzikSlider != null) muzikSlider.value = kayitliMuzik;
        if (sfxSlider != null) sfxSlider.value = kayitliSFX;

        // 3. Sahne açılır açılmaz sesi doğrudan uygula (Slider olmasa bile Mixer ayarlanır)
        SetMuzikVolume(kayitliMuzik);
        SetSFXVolume(kayitliSFX);

        // 4. Slider hareketlerini dinlemeye başla
        if (muzikSlider != null) muzikSlider.onValueChanged.AddListener(SetMuzikVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMuzikVolume(float deger)
    {
        // 0-1 arası değeri Logaritmik Desibele (dB) çevir (-80 ile 0 arası)
        float db = Mathf.Log10(Mathf.Clamp(deger, 0.0001f, 1f)) * 20f;
        
        anaMixer.SetFloat("MuzikVolume", db);
        PlayerPrefs.SetFloat("MuzikAyar", deger);
    }

    public void SetSFXVolume(float deger)
    {
        float db = Mathf.Log10(Mathf.Clamp(deger, 0.0001f, 1f)) * 20f;
        
        anaMixer.SetFloat("SFXVolume", db);
        PlayerPrefs.SetFloat("SFXAyar", deger);
    }

    void OnDisable()
    {
        // Obje kapandığında (veya sahne değiştiğinde) ayarları kalıcı olarak kaydet
        PlayerPrefs.Save();
    }
}