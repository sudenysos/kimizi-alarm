using UnityEngine;
using System.Collections;

public class CinematicManager : MonoBehaviour
{
    [Header("Eğitim (Tutorial) Ayarı")]
    [Tooltip("Eğer sahne eğitim bölümüyse bunu işaretleyin. Space ile geçişi ve UI kapatmayı TutorialManager'a bırakır.")]
    public bool tutorialModu = false;

    [Header("Kamera Ayarları")]
    public GameObject cinematicCamera; 
    public GameObject mainCamera;       

    [Header("Sinematik Hareket (Klipler)")]
    public Transform[] kameraNoktalari; 
    public float noktaArasiGecisSuresi = 10f; 
    
    [Tooltip("Kameranın süzülürken sürekli bakacağı hedef (Örn: Yanan Elektrik Panosu)")]
    public Transform odakNoktasi; 

    [Header("Drone Ayarı")]
    public GameObject droneObjesi;     

    [Header("Sinematik UI Ayarları")]
    public GameObject infoPanel; 
    public float panelKaymaSuresi = 1.5f; 

    [Header("Oyun İçi UI (Başlayınca Açılacaklar)")]
    public GameObject[] oyunIciUIElemanlari; 

    [Header("İşçi Sahneleri (YENİ)")]
    public GameObject stage0; // Oyun başı ve içi sabit duran işçiler
    public GameObject stage1; // Outro sinematiğinde yürüyen işçiler

    private bool sinematikBitti = false;

    void Start()
    {
        // Oyun başlarken sabit işçiler sahnede olsun, yürüyenler gizlensin
        if (stage0 != null) stage0.SetActive(true);
        if (stage1 != null) stage1.SetActive(false);

        if (cinematicCamera != null) cinematicCamera.SetActive(true);
        if (infoPanel != null) infoPanel.SetActive(true);

        // NORMAL OYUN MODU (Tutorial değilse Drone'u ve oyun içi UI'ı gizle)
        // Tutorial modundaysak bunlara dokunmuyoruz, TutorialManager halledecek.
        if (!tutorialModu)
        {
            if (droneObjesi != null) droneObjesi.SetActive(false);
            if (mainCamera != null) mainCamera.SetActive(false);

            foreach (GameObject uiElemani in oyunIciUIElemanlari)
            {
                if (uiElemani != null) uiElemani.SetActive(false);
            }
        }

        if (kameraNoktalari.Length > 1 && cinematicCamera != null)
        {
            StartCoroutine(KameraHareketiRutini());
        }
    }

    void Update()
    {
        // EĞİTİM MODU KİLİDİ: Eğer tutorialModu aktifse Space tuşunu dinleme!
        if (!tutorialModu && !sinematikBitti && Input.GetKeyDown(KeyCode.Space))
        {
            sinematikBitti = true;
            StartCoroutine(OyunaGecisRutini());
        }
    }

    IEnumerator KameraHareketiRutini()
    {
        int suAnkiNokta = 0;
        
        cinematicCamera.transform.position = kameraNoktalari[0].position;
        
        if (odakNoktasi != null) cinematicCamera.transform.LookAt(odakNoktasi);
        else cinematicCamera.transform.rotation = kameraNoktalari[0].rotation;

        while (true)
        {
            int sonrakiNokta = (suAnkiNokta + 1) % kameraNoktalari.Length;
            Transform baslangic = kameraNoktalari[suAnkiNokta];
            Transform hedef = kameraNoktalari[sonrakiNokta];
            
            float gecenSure = 0f;

            while (gecenSure < noktaArasiGecisSuresi)
            {
                if (sinematikBitti) yield break; 
                
                gecenSure += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, gecenSure / noktaArasiGecisSuresi);
                
                cinematicCamera.transform.position = Vector3.Lerp(baslangic.position, hedef.position, t);
                
                if (odakNoktasi != null)
                {
                    cinematicCamera.transform.LookAt(odakNoktasi);
                }
                else
                {
                    cinematicCamera.transform.rotation = Quaternion.Slerp(baslangic.rotation, hedef.rotation, t);
                }
                
                yield return null;
            }

            suAnkiNokta = sonrakiNokta;
        }
    }

    IEnumerator OyunaGecisRutini()
    {
        if (infoPanel != null)
        {
            RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
            
            if (panelRect != null)
            {
                Vector2 baslangicPozisyonu = panelRect.anchoredPosition;
                Vector2 hedefPozisyon = new Vector2(baslangicPozisyonu.x, baslangicPozisyonu.y - panelRect.rect.height - 50f);

                float gecenSure = 0f;

                while (gecenSure < panelKaymaSuresi)
                {
                    gecenSure += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, gecenSure / panelKaymaSuresi);
                    panelRect.anchoredPosition = Vector2.Lerp(baslangicPozisyonu, hedefPozisyon, t);
                    yield return null;
                }
            }
            infoPanel.SetActive(false);
        }

        if (droneObjesi != null) droneObjesi.SetActive(true);
        if (cinematicCamera != null) cinematicCamera.SetActive(false);
        if (mainCamera != null) mainCamera.SetActive(true);

        foreach (GameObject uiElemani in oyunIciUIElemanlari)
        {
            if (uiElemani != null) uiElemani.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}