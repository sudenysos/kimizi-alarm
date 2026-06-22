using UnityEngine;

public class YagYangini : MonoBehaviour
{
    [Header("Yağ Yangını Canı")]
    [Tooltip("Sadece Köpük (2) ile canı azalır. Su (1) işe yaramaz.")]
    public float yanginCani = 100f;
    private float maksimumCan;

    private ParticleSystem[] tumEfektler;
    private float[] baslangicYogunluklari;
    private bool sondurulduMu = false;

    void Start()
    {
        maksimumCan = yanginCani;

        // Ateşin ve dumanın tüm partiküllerini bul
        tumEfektler = GetComponentsInChildren<ParticleSystem>();
        baslangicYogunluklari = new float[tumEfektler.Length];

        // Başlangıç yoğunluklarını hafızaya al
        for (int i = 0; i < tumEfektler.Length; i++)
        {
            baslangicYogunluklari[i] = tumEfektler[i].emission.rateOverTimeMultiplier;
        }
    }

    // Drone'dan gelen maddeler ateşe çarptığında çalışır
    void OnParticleCollision(GameObject carpanMadde)
    {
        if (sondurulduMu) return;

        // MADDE KONTROLÜ (Bulmaca Mekaniği)
        if (carpanMadde.name == "2") 
        {
            // Köpük (2) doğru maddedir, yangını söndürür.
            yanginCani -= 0.5f; 
        }
        else if (carpanMadde.name == "1")
        {
            // Su (1) yağ yangınına etki etmez! İleride istersen buraya ateşi harlatma cezası bile ekleyebiliriz.
            // Şimdilik sadece hasar almasını engelliyoruz.
        }

        // Canın sıfırın altına inmesini engelle
        yanginCani = Mathf.Clamp(yanginCani, 0f, maksimumCan);

        // Ateşin görselini canına göre yavaşça kıs
        for (int i = 0; i < tumEfektler.Length; i++)
        {
            var emission = tumEfektler[i].emission;
            emission.rateOverTimeMultiplier = baslangicYogunluklari[i] * (yanginCani / maksimumCan);
        }

        // Canı bittiyse söndürme işlemini başlat
        if (yanginCani <= 0)
        {
            Sondur();
        }
    }

    void Sondur()
    {
        sondurulduMu = true;

        // Üst objedeki model değiştirme scriptini (AgacDurumu'nu burada ızgara için kullanıyoruz) tetikle
        if (transform.parent != null)
        {
            AgacDurumu izgaraDurumu = transform.parent.GetComponent<AgacDurumu>();
            if (izgaraDurumu != null) 
            {
                izgaraDurumu.AgaciSondur(); 
            }
        }

        // Ateşi tamamen kapat
        gameObject.SetActive(false); 

        // SAHNE BİTİŞİ: Tek hedefimiz bu ızgara olduğu için direkt görevi tamamla!
        MissionCompleteManager bitisYoneticisi = FindAnyObjectByType<MissionCompleteManager>();
        if (bitisYoneticisi != null)
        {
            bitisYoneticisi.GoreviTamamla();
        }
        else
        {
            Debug.LogError("MissionCompleteManager bulunamadı!");
        }
    }
}