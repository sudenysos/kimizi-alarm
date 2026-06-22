using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 

public class MissionCompleteManager : MonoBehaviour
{
    [Header("Oyun İçi Kapanacaklar")]
    public GameObject droneObjesi;
    public GameObject mainCamera;
    public GameObject[] oyunIciUIElemanlari;

    [Header("İstatistik ve Puanlama")]
    public DroneController droneController;      
    public TextMeshProUGUI kullanilanDroneYazisi; 
    public TextMeshProUGUI gecenSureYazisi;
    public int sabitGorevPuani = 1000;           
    
    [Header("Madalya Sistemi (YENİ)")]
    public Image madalyaGorseli;     
    public Sprite altinMadalya;      
    public Sprite gumusMadalya;      
    public Sprite bronzMadalya;      
    
    // Barajlar
    public int altinBaraji = 2900;
    public int gumusBaraji = 1850;

    private float gecenSure = 0f;
    private bool gorevBittiMi = false;

    [Header("Geçiş (Fade) Ayarları")]
    public float yanginSonrasiBekleme = 0.5f; 
    public CanvasGroup siyahEkran; 
    public float kararmaSuresi = 1f;

    [Header("Final Sinematiği")]
    public GameObject outroCamera;
    public Transform[] outroNoktalari;
    public float noktaArasiGecisSuresi = 2.5f;

    [Header("İşçi Sahneleri (YENİ)")]
    public GameObject stage0; // Sabit işçiler
    public GameObject stage1; // Yürüyen işçiler

    [Header("Bitiş UI")]
    public CanvasGroup missionCompletePanel; 
    public float panelAcilmaSuresi = 1.5f;   

    void Start()
    {
        gecenSure = 0f;
        gorevBittiMi = false;

        if (missionCompletePanel != null)
        {
            missionCompletePanel.alpha = 0f;
            missionCompletePanel.interactable = false;
            missionCompletePanel.blocksRaycasts = false;
            missionCompletePanel.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!gorevBittiMi && droneController != null && droneController.kontrolEdilebilirMi)
        {
            gecenSure += Time.deltaTime;
        }
    }

    public void GoreviTamamla()
    {
        if (gorevBittiMi) return; 
        gorevBittiMi = true;

        StartCoroutine(BitisRutini());
    }

    IEnumerator BitisRutini()
    {
        yield return new WaitForSeconds(yanginSonrasiBekleme);

        // 1. Ekranı Karart
        if (siyahEkran != null)
        {
            siyahEkran.gameObject.SetActive(true);

            Image siyahResim = siyahEkran.GetComponent<Image>();
            if (siyahResim != null)
            {
                Color c = siyahResim.color;
                c.a = 1f; 
                siyahResim.color = c;
            }

            float suan = 0f;
            while (suan < kararmaSuresi)
            {
                suan += Time.deltaTime;
                siyahEkran.alpha = Mathf.Clamp01(suan / kararmaSuresi);
                yield return null;
            }
            siyahEkran.alpha = 1f;
        }

        // --- EKRAN TAM SİYAHKEN SAHNEYİ DEĞİŞTİR ---
        if (stage0 != null) stage0.SetActive(false);
        if (stage1 != null) stage1.SetActive(true);

        if (droneObjesi != null) droneObjesi.SetActive(false);
        if (mainCamera != null) mainCamera.SetActive(false);
        
        foreach (GameObject ui in oyunIciUIElemanlari)
        {
            if (ui != null) ui.SetActive(false);
        }

        // --- GÜVENLİ KALKAN: EĞİTİM SAHNESİ İÇİN MADDE PANELİNİ KAPAT ---
        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
        if (tm != null && tm.maddeControlsPanel != null)
        {
            tm.maddeControlsPanel.SetActive(false);
        }

        if (outroCamera != null && outroNoktalari.Length > 0)
        {
            outroCamera.transform.position = outroNoktalari[0].position;
            outroCamera.transform.rotation = outroNoktalari[0].rotation;
            outroCamera.SetActive(true);
        }

        // 2. Ekranı Aydınlat
        StartCoroutine(SiyahEkraniAydinlat());

        yield return new WaitForSeconds(0.2f);

        // 3. Outro Kamerasını Hareket Ettir
        if (outroCamera != null && outroNoktalari.Length > 1)
        {
            for (int i = 1; i < outroNoktalari.Length; i++)
            {
                Transform baslangic = outroNoktalari[i - 1];
                Transform hedef = outroNoktalari[i];
                float suan = 0f;

                while (suan < noktaArasiGecisSuresi)
                {
                    suan += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, suan / noktaArasiGecisSuresi);
                    
                    outroCamera.transform.position = Vector3.Lerp(baslangic.position, hedef.position, t);
                    outroCamera.transform.rotation = Quaternion.Slerp(baslangic.rotation, hedef.rotation, t);
                    
                    yield return null;
                }
            }
        }

        yield return new WaitForSeconds(2.5f);

        // --- İSTATİSTİK VE MADALYA HESAPLAMA ---
        if (droneController != null)
        {
            int kullanilanDroneSayisi = (3 - droneController.kalanDroneHakki) + 1;
            
            if (kullanilanDroneYazisi != null)
            {
                kullanilanDroneYazisi.text = "KULLANILAN DRONE: " + kullanilanDroneSayisi;
            }

            int dakika = Mathf.FloorToInt(gecenSure / 60f);
            int saniye = Mathf.FloorToInt(gecenSure % 60f);
            if (gecenSureYazisi != null)
            {
                gecenSureYazisi.text = string.Format("GEÇEN SÜRE: {0:00}:{1:00}", dakika, saniye);
            }

            int toplamSaniye = Mathf.FloorToInt(gecenSure);
            int hamSkor = sabitGorevPuani - toplamSaniye;
            if (hamSkor < 0) hamSkor = 0; 
            
            int finalSkoru = hamSkor * droneController.kalanDroneHakki;
            int kazanilanMadalyaDegeri = 0;

            // --- YENİ EKLENEN: TUTORIAL İÇİN KOŞULSUZ ALTIN MADALYA ---
            if (tm != null) // tm'yi yukarıda tanımlamıştık, null değilse eğitimdeyiz demektir!
            {
                if (madalyaGorseli != null) madalyaGorseli.sprite = altinMadalya;
                kazanilanMadalyaDegeri = 3; 
                Debug.Log("Eğitim tamamlandı! Koşulsuz Altın Madalya verildi.");
            }
            else // Normal bölümler için standart puan hesaplaması
            {
                if (finalSkoru >= altinBaraji)
                {
                    if (madalyaGorseli != null) madalyaGorseli.sprite = altinMadalya;
                    kazanilanMadalyaDegeri = 3; 
                }
                else if (finalSkoru >= gumusBaraji)
                {
                    if (madalyaGorseli != null) madalyaGorseli.sprite = gumusMadalya;
                    kazanilanMadalyaDegeri = 2; 
                }
                else
                {
                    if (madalyaGorseli != null) madalyaGorseli.sprite = bronzMadalya;
                    kazanilanMadalyaDegeri = 1; 
                }
            }

            string aktifLevelAdi = SceneManager.GetActiveScene().name;
            SaveManager.MadalyaKaydet(aktifLevelAdi, kazanilanMadalyaDegeri);
        }

        // 4. Bitiş Panelini Aç
        if (missionCompletePanel != null)
        {
            missionCompletePanel.gameObject.SetActive(true);
            float panelGecenSure = 0f;

            while (panelGecenSure < panelAcilmaSuresi)
            {
                panelGecenSure += Time.deltaTime;
                missionCompletePanel.alpha = Mathf.Lerp(0f, 1f, panelGecenSure / panelAcilmaSuresi);
                yield return null;
            }

            missionCompletePanel.alpha = 1f;
            missionCompletePanel.interactable = true;
            missionCompletePanel.blocksRaycasts = true;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    IEnumerator SiyahEkraniAydinlat()
    {
        if (siyahEkran != null)
        {
            float suan = 0f;
            while (suan < kararmaSuresi)
            {
                suan += Time.deltaTime;
                siyahEkran.alpha = Mathf.Clamp01(1f - (suan / kararmaSuresi));
                yield return null;
            }
            siyahEkran.gameObject.SetActive(false);
        }
    }
}