using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCardUI : MonoBehaviour
{
    [Header("Metin Alanları")]
    public TextMeshProUGUI txtLevelNumber;
    public TextMeshProUGUI txtLevelName;
    public TextMeshProUGUI txtDescription;

    [Header("Görsel Alanlar")]
    public Image imgMedal; 
    public Button btnPlay;
    public TextMeshProUGUI txtBtnPlay;
    
    [Header("Animasyon ve Hover")]
    public GameObject objHover; // Kilitli butonlarda fare tepkisini (hover) kapatmak için
    public Animator btnAnimator; // Kilitli butonlarda Animator'ı susturmak için

    [Header("UI Görselleri (MagicaVoxel)")]
    public Sprite altinSprite;
    public Sprite gumusSprite;
    public Sprite bronzSprite;
    public Sprite bosSprite;   
    public Sprite kilitSprite; 

    [Header("Buton Görselleri")]
    public Sprite btnActiveSprite;  
    public Sprite btnPassiveSprite; 

    private LevelData currentLevelData; 

    public void SetupCard(LevelData data)
    {
        currentLevelData = data; 

        // Temel metin atamaları
        txtLevelNumber.text = data.levelNumber;
        txtLevelName.text = data.levelName;
        txtDescription.text = data.description;

        imgMedal.gameObject.SetActive(true);
        imgMedal.color = Color.white; 

        // Unity'nin yarı şeffaf pasif rengini ezmek için butonu teknik olarak aktif tutuyoruz
        btnPlay.interactable = true; 

        if (data.isUnlocked)
        {
            // --- SEVİYE AÇIK DURUMU ---
            btnPlay.image.sprite = btnActiveSprite;
            txtBtnPlay.color = new Color(0.85f, 0.15f, 0.15f, 1f); // Orijinal Kırmızı Metin
            
            // Hover objesini ve Animator'ı AÇ
            if(objHover != null) objHover.SetActive(true); 
            if(btnAnimator != null) btnAnimator.enabled = true; // Animasyonlar çalışsın
            
            int gercekMadalyaDegeri = 0;
            OyunGenelKaydi kayitlar = SaveManager.KaydiYukle();
            
            // --- X-RAY LOGU VE ARAMA BAŞLANGIÇ ---
            if (kayitlar != null && kayitlar.leveller != null)
            {
                Debug.Log($"<color=orange>--- KART OLUŞTURULUYOR: Aradığımız Sahne Adı: '{data.sceneName}' ---</color>");
                
                foreach (var kayit in kayitlar.leveller)
                {
                    Debug.Log($"Save Dosyasının İçindeki Kayıt -> Sahne: '{kayit.levelAdi}', Madalya: {kayit.bestMedal}");
                }

                LevelMadalyaKaydi levelKaydi = kayitlar.leveller.Find(l => l.levelAdi == data.sceneName);
                if (levelKaydi != null)
                {
                    Debug.Log($"<color=green>EŞLEŞME BAŞARILI!</color> '{data.sceneName}' için bulunan madalya: {levelKaydi.bestMedal}");
                    gercekMadalyaDegeri = levelKaydi.bestMedal; 
                }
                else
                {
                    Debug.Log($"<color=red>EŞLEŞME BAŞARISIZ!</color> '{data.sceneName}' adında bir kayıt Save dosyasında yok!");
                }
            }
            else
            {
                Debug.Log("<color=red>HATA!</color> SaveManager.KaydiYukle() BOŞ (NULL) döndü. Kayıt dosyası okunamıyor.");
            }
            // --- X-RAY LOGU BİTİŞ ---

            // Kazanılan madalyaya göre ilgili PNG'yi ata
            switch (gercekMadalyaDegeri)
            {
                case 3: imgMedal.sprite = altinSprite; break;
                case 2: imgMedal.sprite = gumusSprite; break;
                case 1: imgMedal.sprite = bronzSprite; break;
                default: imgMedal.sprite = bosSprite; break; 
            }
        }
        else
        {
            // --- SEVİYE KİLİTLİ DURUMU ---
            btnPlay.image.sprite = btnPassiveSprite; 
            
            // Belirlediğimiz kapalı, tok gri (#828282) metin rengi
            txtBtnPlay.color = new Color(0.51f, 0.51f, 0.51f, 1f); 
            
            // Kilitli butonda hover objesini ve Animator'ı KAPAT
            if(objHover != null) objHover.SetActive(false); 
            if(btnAnimator != null) btnAnimator.enabled = false; // Animator susturuldu, kodun rengi artık ezilemez!

            // Kilit PNG'sini ata
            imgMedal.sprite = kilitSprite;
        }
    }

    // --- TIKLAMA KONTROLCÜSÜ (Bekçi) ---
    // --- TIKLAMA KONTROLCÜSÜ (Bekçi) ---
    public void OnPlayButtonClicked()
    {
        if (currentLevelData == null) return;

        // SEVİYE KİLİTLİYSE TIKLAMAYI YUT VE İŞLEM YAPMA
        if (!currentLevelData.isUnlocked)
        {
            Debug.Log("Bu bölüm kilitli! Buton aktif görünse de işlev engellendi.");
            return; 
        }

        // SEVİYE AÇIKSA OYUNU BAŞLAT
        Debug.Log("Bölüm yükleniyor: " + currentLevelData.sceneName);
        
        // ÇÖZÜM: Sahnedeki MenuManager'ı bul ve yükleme işini ona devret
        MenuManager menuYonetici = Object.FindFirstObjectByType<MenuManager>();
        if (menuYonetici != null)
        {
            menuYonetici.BolumYukle(currentLevelData.sceneName);
        }
        else
        {
            Debug.LogError("Sahnedeki MenuManager bulunamadı!");
        }
    }
}

[System.Serializable]
public class LevelData
{
    public int levelIndex;
    public string sceneName; 
    public string levelNumber;
    public string levelName;
    public string description;
    public int medal; 
    public bool isUnlocked;
}

[System.Serializable]
public class LevelList
{
    public LevelData[] levels;
}