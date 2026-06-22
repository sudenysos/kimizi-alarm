using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [Header("Menü Panelleri (Canvas Group Sürükleyin)")]
    public CanvasGroup mainMenu;
    public CanvasGroup levelSelection;
    public CanvasGroup settings;

    void Start()
    {
        // DroneController'dan gelen "Bölüm Seçimini Aç" notunu okuyoruz (Varsayılan değer 0'dır)
        if (PlayerPrefs.GetInt("BolumSecimiAc", 0) == 1)
        {
            // Notu okuduktan sonra sıfırlıyoruz ki oyunu kapatıp açınca sürekli bu ekran gelmesin
            PlayerPrefs.SetInt("BolumSecimiAc", 0);
            
            PanelKapat(mainMenu);
            PanelAc(levelSelection);
            PanelKapat(settings);
        }
        else
        {
            // Standart Başlangıç: Oyun normal olarak exe'den ilk defa açılıyorsa
            PanelAc(mainMenu);
            PanelKapat(levelSelection);
            PanelKapat(settings);
        }
    }

    void Update()
    {
        // Açık olan paneli kapatıp ana menüye dönmek için ESC kontrolü
        if (levelSelection != null && levelSelection.alpha > 0 && Input.GetKeyDown(KeyCode.Escape))
        {
            AnaMenuyeDon();
        }
        else if (settings != null && settings.alpha > 0 && Input.GetKeyDown(KeyCode.Escape))
        {
            AnaMenuyeDon();
        }
    }

    public void AnaMenuyeDon()
    {
        PanelKapat(levelSelection);
        PanelKapat(settings);
        PanelAc(mainMenu);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OynaButonu()
    {
        PanelKapat(mainMenu);
        PanelAc(levelSelection);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void AyarlarButonu()
    {
        PanelKapat(mainMenu);
        PanelAc(settings);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CikisButonu()
    {
        Debug.Log("Çıkış komutu tetiklendi!");
        Application.Quit();
    }

    public void BolumYukle(string sahneAdi)
    {
        EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene(sahneAdi);
    }

    // --- YENİ SİSTEM: GÖRÜNMEZLİK VE DOKUNULMAZLIK ---
    private void PanelAc(CanvasGroup panel)
    {
        panel.alpha = 1f;               
        panel.interactable = true;      
        panel.blocksRaycasts = true;    
    }

    private void PanelKapat(CanvasGroup panel)
    {
        panel.alpha = 0f;               
        panel.interactable = false;     
        panel.blocksRaycasts = false;   
    }
}