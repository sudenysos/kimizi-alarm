using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Menü Panelleri (CanvasGroup Eklenmiş Olmalı)")]
    public CanvasGroup pauseAnaKonteyner; 
    public CanvasGroup pauseMenuUI;       
    public CanvasGroup settingsMenuUI;    

    [Header("Geçiş Ayarları")]
    public float fadeSuresi = 0.25f;      

    // YENİ EKLENEN: Menü Müziği Hoparlörü
    [Header("Arka Plan Müziği")]
    public AudioSource pauseMuzigi; 

    private bool isPaused = false;
    private bool isSettingsOpen = false;
    private Coroutine fadeCoroutine;      

    void Start()
    {
        // YENİ EKLENEN: Müziğin oyun dursa bile çalabilmesi için izin ver
        if (pauseMuzigi != null)
        {
            pauseMuzigi.ignoreListenerPause = true;
        }

        if(pauseAnaKonteyner != null)
        {
            pauseAnaKonteyner.alpha = 0f;
            pauseAnaKonteyner.interactable = false;
            pauseAnaKonteyner.blocksRaycasts = false;
        }

        if(pauseMenuUI != null) PanelGoster(pauseMenuUI);
        if(settingsMenuUI != null) PanelGizle(settingsMenuUI);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (isSettingsOpen) AyarlardanGeriDon(); 
                else DevamEt(); 
            }
            else Duraklat(); 
        }
    }

    public void Duraklat()
    {
        isPaused = true;
        isSettingsOpen = false;
        
        Time.timeScale = 0f; 
        AudioListener.pause = true; 
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // YENİ EKLENEN: Pause menüsü açılınca müziği başlat
        if (pauseMuzigi != null && !pauseMuzigi.isPlaying)
        {
            pauseMuzigi.Play();
        }

        PanelGoster(pauseMenuUI);
        PanelGizle(settingsMenuUI);

        pauseAnaKonteyner.interactable = true;
        pauseAnaKonteyner.blocksRaycasts = true;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeUygula(pauseAnaKonteyner, pauseAnaKonteyner.alpha, 1f, false));
    }

    public void DevamEt()
    {
        isPaused = false;
        isSettingsOpen = false;

        AudioListener.pause = false; 

        // YENİ EKLENEN: Oyuna dönünce müziği durdur
        if (pauseMuzigi != null)
        {
            pauseMuzigi.Stop();
        }

        pauseAnaKonteyner.interactable = false;
        pauseAnaKonteyner.blocksRaycasts = false;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeUygula(pauseAnaKonteyner, pauseAnaKonteyner.alpha, 0f, true));
    }

    public void AyarlariAc()
    {
        PanelGizle(pauseMenuUI);
        PanelGoster(settingsMenuUI);
        isSettingsOpen = true; 
    }

    public void AyarlardanGeriDon()
    {
        PanelGizle(settingsMenuUI);
        PanelGoster(pauseMenuUI);
        isSettingsOpen = false; 
    }

    public void MenuyeDon()
    {
        Time.timeScale = 1f; 
        AudioListener.pause = false; 
        SceneManager.LoadScene("Scene0"); 
    }

    private void PanelGoster(CanvasGroup panel)
    {
        if (panel == null) return;
        panel.alpha = 1f;               
        panel.interactable = true;      
        panel.blocksRaycasts = true;    
    }

    private void PanelGizle(CanvasGroup panel)
    {
        if (panel == null) return;
        panel.alpha = 0f;               
        panel.interactable = false;     
        panel.blocksRaycasts = false;   
    }

    private IEnumerator FadeUygula(CanvasGroup cg, float baslangic, float hedef, bool oyunuDevamEttir)
    {
        float gecenSure = 0f;

        while (gecenSure < fadeSuresi)
        {
            gecenSure += Time.unscaledDeltaTime; 
            cg.alpha = Mathf.Lerp(baslangic, hedef, gecenSure / fadeSuresi);
            yield return null; 
        }

        cg.alpha = hedef; 

        if (oyunuDevamEttir)
        {
            Time.timeScale = 1f;
        }
    }
}