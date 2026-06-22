using UnityEngine;

public class Level3Manager : MonoBehaviour
{
    public static Level3Manager Instance { get; private set; }

    [Header("Seviye Durumu")]
    private bool isPanoExtinguished = false;

    [Header("Soğutma Üniteleri")]
    public ACUnit[] acUnits; // Çatıdaki tüm AC ünitelerini buraya sürükle

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // AC Üniteleri panonun sönüp sönmediğini buradan öğrenir
    public bool IsPanoExtinguished()
    {
        return isPanoExtinguished;
    }

    // PanoYangin.cs içinden çağrılacak
    public void SetPanoExtinguished()
    {
        isPanoExtinguished = true;
        Debug.Log("Level3Manager: Pano devre dışı! Çatıdaki üniteler artık kalıcı olarak söndürülebilir.");
    }

    // Her AC ünitesi kalıcı olarak söndüğünde bu fonksiyonu çalıştırıp bölümün bitip bitmediğini kontrol eder
    public void CheckLevelCompleteStatus()
    {
        // Eğer pano hala yanıyorsa zaten bölüm bitemez
        if (!isPanoExtinguished) return;

        bool tumUnitelerSondu = true;

        foreach (ACUnit unit in acUnits)
        {
            if (!unit.isExtinguishedPermanently)
            {
                tumUnitelerSondu = false;
                break;
            }
        }

        // Hem pano hem de tüm AC üniteleri söndüyse, final sinematiğini başlat!
        if (tumUnitelerSondu)
        {
            Debug.Log("Tüm hedefler yok edildi! Görev tamamlanıyor...");
            MissionCompleteManager bitisYoneticisi = FindAnyObjectByType<MissionCompleteManager>();
            if (bitisYoneticisi != null)
            {
                bitisYoneticisi.GoreviTamamla();
            }
        }
    }
}