using UnityEngine;
using TMPro; 
using System.Collections;
using UnityEngine.UI; 

public class TutorialManager : MonoBehaviour
{
    [Header("Kameralar")]
    public GameObject cinematicCamera;
    public GameObject droneCamera;

    [Header("Arayüz Panelleri")]
    public GameObject infoPanel;
    public TextMeshProUGUI infoPanelText;
    public GameObject controlsPanel; 
    public GameObject maddeControlsPanel; 
    
    [Header("Oyun İçi UI Elemanları")]
    public GameObject[] oyunIciUIElemanlari; 

    [Header("Geçiş (Fade) Ayarları")]
    public Image gecisEkrani; 
    public float gecisSuresi = 0.5f; 
    private bool gecisYapiliyor = false; 

    [Header("Dron Referansı")]
    public DroneController droneController; 

    [Header("Yangın Eğitim Hedefleri")]
    public GameObject[] yanginEfektleri;

    [Header("Eğitim İlerleme Ayarları")]
    // Sadece net bir sayı sayacı
    private int sondurulenHedefSayisi = 0; 

    [Header("Oyun Bitiş Ayarları")]
    public GameObject missionCompleteManager; 

    private int tutorialStep = 0;

    void Awake()
    {
        if (droneController != null)
        {
            droneController.kontrolEdilebilirMi = false;
        }
    }

    void Start()
    {
        tutorialStep = 0;
        sondurulenHedefSayisi = 0; // Oyun başında sayacı sıfırla
        gecisYapiliyor = false;
        
        if (gecisEkrani != null)
        {
            Color c = gecisEkrani.color;
            c.a = 0f;
            gecisEkrani.color = c;
            gecisEkrani.raycastTarget = false; 
            gecisEkrani.gameObject.SetActive(false); 
        }
        if (droneController != null)
        {
            droneController.gameObject.SetActive(false);
            droneController.maddeKullanimiAcik = false; 
        }

        foreach (GameObject ui in oyunIciUIElemanlari)
        {
            if (ui != null) ui.SetActive(false);
        }

        // YENİ GÜNCELLEME: Objeyi komple kapatmak yerine sadece partikülü ve sesi durdur
        foreach (GameObject ates in yanginEfektleri)
        {
            if (ates != null) 
            {
                // İçindeki partikül efektini bul ve durdur
                ParticleSystem alevEfekti = ates.GetComponentInChildren<ParticleSystem>();
                if (alevEfekti != null) alevEfekti.Stop();

                // Sesin kapalı olduğundan emin ol
                YanginSesiYoneticisi ses = ates.GetComponent<YanginSesiYoneticisi>();
                if (ses != null) ses.SesiDurdur();
            }
        }

        cinematicCamera.SetActive(true);
        if (droneCamera != null) droneCamera.SetActive(false);
        
        controlsPanel.SetActive(false);
        if (maddeControlsPanel != null) maddeControlsPanel.SetActive(false); 
        
        infoPanel.SetActive(true);
        infoPanelText.text = "Eğitime hoş geldin! Gerçek sahaya inmeden önce burada yeteneklerini test edeceğiz. İlk aşamada dron sürüş mekaniklerinde ustalaşmanı istiyoruz. Sonrasında ise yangın kaynağı ile söndürücü madde uyumunu çalışacağız; her ateşe su sıkılmaz, bunu burada öğreneceksin. Hazırsan, sürüş mekanikleri ile başlayalım.";
    }

    void Update()
    {
        if (infoPanel.activeSelf && Input.GetKeyDown(KeyCode.Space) && !gecisYapiliyor)
        {
            StartCoroutine(GecisRutini());
        }
    }

    IEnumerator GecisRutini()
    {
        gecisYapiliyor = true;

        if (gecisEkrani != null)
        {
            gecisEkrani.gameObject.SetActive(true); 
            gecisEkrani.raycastTarget = true; 
            Color c = gecisEkrani.color;
            float sure = 0f;
            while (sure < gecisSuresi)
            {
                sure += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, sure / gecisSuresi);
                gecisEkrani.color = c;
                yield return null;
            }
            c.a = 1f;
            gecisEkrani.color = c;
        }

        tutorialStep++;
        ArkaPlanDegisiklikleriniUygula();

        yield return new WaitForSeconds(0.2f);

        if (gecisEkrani != null)
        {
            Color c = gecisEkrani.color;
            float sure = 0f;
            while (sure < gecisSuresi)
            {
                sure += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, sure / gecisSuresi);
                gecisEkrani.color = c;
                yield return null;
            }
            c.a = 0f;
            gecisEkrani.color = c;
            gecisEkrani.raycastTarget = false; 
            gecisEkrani.gameObject.SetActive(false); 
        }

        gecisYapiliyor = false;
    }

    void ArkaPlanDegisiklikleriniUygula()
    {
        if (tutorialStep == 1)
        {
            cinematicCamera.SetActive(false);
            if (droneController != null) droneController.gameObject.SetActive(true);
            if (droneCamera != null) droneCamera.SetActive(true);

            foreach (GameObject ui in oyunIciUIElemanlari)
            {
                if (ui != null) ui.SetActive(true);
            }

            infoPanelText.text = "Kalkıştan önce arayüzü tanıyalım. Sol üstte kalan dron hakkımızı, sağ üstte ise merkezle olan sinyal gücümüzü göreceksin. Birazdan uçuşa başladığında sınırları test etmekten, uzaklara uçup sinyali koparmaktan korkma. Sistemin limitlerini ancak onları aşarak öğrenebilirsin!";
        }
        else if (tutorialStep == 2)
        {
            infoPanel.SetActive(false);
            if (maddeControlsPanel != null) maddeControlsPanel.SetActive(false);
            controlsPanel.SetActive(true); 

            if (droneController != null)
            {
                droneController.kontrolEdilebilirMi = true; 
            }
        }
        else if (tutorialStep == 3)
        {
            infoPanel.SetActive(false);
            
            if (controlsPanel != null) controlsPanel.SetActive(false); 
            if (maddeControlsPanel != null) maddeControlsPanel.SetActive(true); 

            if (droneController != null)
            {
                droneController.kontrolEdilebilirMi = true; 
                droneController.maddeKullanimiAcik = true; 
                droneController.EgitimiBaslatFizigiAc(); 
            }
        }
    }

    public void EgitimdeKazaYapildi()
    {
        if (tutorialStep == 3)
        {
            StartCoroutine(MaddeEgitimiKazaRutini());
        }
        else
        {
            StartCoroutine(KazaGecisRutini());
        }
    }

    IEnumerator MaddeEgitimiKazaRutini()
    {
        yield return new WaitForSeconds(0.5f);

        if (infoPanel != null) infoPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (maddeControlsPanel != null) maddeControlsPanel.SetActive(true); 

        if (droneController != null)
        {
            droneController.EgitimIcinMerkezeIsinla();
            
            if (droneCamera != null)
            {
                CameraFollow cf = droneCamera.GetComponent<CameraFollow>();
                if (cf != null) cf.ResetKameraKildi();
            }

            droneController.EgitimiBaslatFizigiAc();
            droneController.kontrolEdilebilirMi = true; 
        }
    }

    IEnumerator KazaGecisRutini()
    {
        gecisYapiliyor = true;

        if (gecisEkrani != null)
        {
            gecisEkrani.gameObject.SetActive(true); 
            gecisEkrani.raycastTarget = true; 
            Color c = gecisEkrani.color;
            float sure = 0f;
            while (sure < gecisSuresi)
            {
                sure += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, sure / gecisSuresi);
                gecisEkrani.color = c;
                yield return null;
            }
            c.a = 1f;
            gecisEkrani.color = c;
        }

        if (droneController != null)
        {
            droneController.EgitimIcinMerkezeIsinla();
            
            if (droneCamera != null)
            {
                CameraFollow cf = droneCamera.GetComponent<CameraFollow>();
                if (cf != null) cf.ResetKameraKildi();
            }
        }

        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (maddeControlsPanel != null) maddeControlsPanel.SetActive(false);
        
        if (infoPanel != null) infoPanel.SetActive(true);
        
        // YENİ GÜNCELLEME: Obje zaten açık, sadece partikülü ve sesi başlat
        foreach (GameObject ates in yanginEfektleri)
        {
            if (ates != null) 
            {
                // Sadece içindeki partikül efektini bul ve çalıştır
                ParticleSystem alevEfekti = ates.GetComponentInChildren<ParticleSystem>();
                if (alevEfekti != null) alevEfekti.Play();
                
                // Sesi başlat
                YanginSesiYoneticisi sesYoneticisi = ates.GetComponent<YanginSesiYoneticisi>();
                if (sesYoneticisi != null)
                {
                    sesYoneticisi.SesiBaslat();
                }
            }
        }
        
        infoPanelText.text = "Harika, sınırları test ettik! Şimdi yangın sınıflarına geçelim. Katı maddelerde suyu, sıvı ve yağda köpüğü, elektrikte karbondioksiti tam yangının kaynağına sıkmalısın. Unutma, yağa su sıkarsan alevler büyür. Burası bir eğitim simülasyonu, çekinmeden yanlış maddeleri deneyip hatalı müdahalelerin sonuçlarını tecrübe et. Hazırsan uçuşa geçebiliriz.";
        
        tutorialStep = 2; 

        yield return new WaitForSeconds(0.2f);

        if (gecisEkrani != null)
        {
            Color c = gecisEkrani.color;
            float sure = 0f;
            while (sure < gecisSuresi)
            {
                sure += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, sure / gecisSuresi);
                gecisEkrani.color = c;
                yield return null;
            }
            c.a = 0f;
            gecisEkrani.color = c;
            gecisEkrani.raycastTarget = false; 
            gecisEkrani.gameObject.SetActive(false); 
        }

        gecisYapiliyor = false;
    }

    public void BirHedefSonduruldu(string yananNesneninAdi)
    {
        sondurulenHedefSayisi++;

        Debug.Log("<color=orange>BİR YANGIN SÖNDÜ!</color> Tetikleyen Obje: <b>" + yananNesneninAdi + "</b> | Toplam Sönen Sayısı: " + sondurulenHedefSayisi);

        // EĞER 3 YANGININ 3'Ü DE SÖNDÜYSE
        if (sondurulenHedefSayisi >= 3)
        {
            Debug.Log("<color=green>3 YANGIN DA SÖNDÜ! SİNEMATİK TETİKLENİYOR!</color>");
            
            if (missionCompleteManager != null)
            {
                missionCompleteManager.SetActive(true); 

                MissionCompleteManager mcmScript = missionCompleteManager.GetComponent<MissionCompleteManager>();
                if (mcmScript != null)
                {
                    mcmScript.GoreviTamamla();
                }
                else
                {
                    Debug.LogError("DİKKAT: Atanan objede MissionCompleteManager scripti bulunamadı!");
                }
            }
            else
            {
                Debug.LogError("DİKKAT: TutorialManager içinde Mission Complete Manager Inspector'dan atanmamış!");
            }
        }
    }
}