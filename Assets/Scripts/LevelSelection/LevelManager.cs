using UnityEngine;
using System.IO;

public class LevelManager : MonoBehaviour
{
    [Header("Sahnede Kurulu Kartlar")]
    [Tooltip("Sahnede duran 4 kartı sırasıyla (Bölüm 0, 1, 2, 3) buraya sürükleyin.")]
    public LevelCardUI[] levelCards;

    void Awake()
    {
        // Kartlar ilk karede hazır olsun diye Start yerine Awake içinde yüklüyoruz
        LoadLevelData();
    }

    void LoadLevelData()
    {
        // Mac ve Windows sistemlerde sorunsuz çalışacak dosya yolu oluşturma
        string filePath = Path.Combine(Application.streamingAssetsPath, "levels.json");

        if (File.Exists(filePath))
        {
            // JSON dosyasını düz metin (string) olarak oku
            string jsonText = File.ReadAllText(filePath);

            // Unity'nin kendi JsonUtility sistemiyle metni C# sınıflarına dönüştür
            LevelList levelList = JsonUtility.FromJson<LevelList>(jsonText);

            // Cihazda kayıtlı olan gerçek oyuncu ilerlemesini yükle
            OyunGenelKaydi cihazKayitlari = SaveManager.KaydiYukle();

            // JSON'dan gelen verileri sahnede kurulu olan kartlara sırasıyla dağıt
            for (int i = 0; i < levelCards.Length; i++)
            {
                if (i < levelList.levels.Length)
                {
                    LevelData data = levelList.levels[i];

                    // --- DİNAMİK KİLİT AÇMA MANTIĞI ---
                    // Eğer indeks 0 ise (Bölüm 0 - Eğitim), her zaman açık kalır.
                    // Eğer indeks 0'dan büyükse, bir önceki bölümün madalya durumunu kontrol et.
                    if (i > 0)
                    {
                        string oncekiSahneAdi = levelList.levels[i - 1].sceneName;
                        
                        // Bir önceki sahneye ait bir madalya kaydı var mı bak
                        LevelMadalyaKaydi oncekiLevelGelişimi = cihazKayitlari.leveller.Find(l => l.levelAdi == oncekiSahneAdi);

                        // Eğer bir önceki bölüm oynanmışsa ve en az 1 madalya (bestMedal > 0) alındıysa sıradaki bölümün kilidini aç
                        if (oncekiLevelGelişimi != null && oncekiLevelGelişimi.bestMedal > 0)
                        {
                            data.isUnlocked = true;
                        }
                        else
                        {
                            data.isUnlocked = false; // Kayıt yoksa kilitli kalmaya devam et
                        }
                    }

                    // Dinamik olarak güncellenmiş veriyi karta gönder
                    levelCards[i].SetupCard(data);
                }
            }
            
            Debug.Log("<Color=green><b>[LevelManager]</b> JSON verileri ve cihaz kayıtları senkronize edildi, kilitler güncellendi!</Color>");
        }
        else
        {
            Debug.LogError("[LevelManager] Hata: 'levels.json' dosyası belirtilen yolda bulunamadı: " + filePath);
        }
    }
}