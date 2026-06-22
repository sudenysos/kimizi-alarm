using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; // Mixer'ı kullanabilmek için ekledik

public class ButonSesYoneticisi : MonoBehaviour
{
    [Header("Ses Dosyası")]
    public AudioClip clickSesi; 
    
    [Header("Mixer Kanalı")]
    public AudioMixerGroup arayuzKanalı; // Inspector'da yuva açacak
    
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = true; 
        
        // Sesin gideceği kanalı koda tanıtıyoruz
        if(arayuzKanalı != null)
        {
            audioSource.outputAudioMixerGroup = arayuzKanalı;
        }
        
        Button[] tumButonlar = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button btn in tumButonlar)
        {
            btn.onClick.AddListener(SesCal);
        }
    }

    void SesCal()
    {
        if (clickSesi != null) audioSource.PlayOneShot(clickSesi);
    }
}