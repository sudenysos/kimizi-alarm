using UnityEngine;

public class YanginYayilmasi : MonoBehaviour
{
    [Header("Yangın Büyüme ve Yayılma")]
    public static bool oyunBasladi = true;
    public float yayilmaHizi = 0.2f;
    public float maxTetikleyiciBoyutu = 4.5f;

    [Header("Yangın Canı (Söndürme Süresi)")]
    [Tooltip("Ateşin toplam canı. Bu değer ne kadar yüksekse o kadar geç söner.")]
    public float yanginCani = 100f;
    private float maksimumCan;

    private SphereCollider tetikleyiciKure;
    private ParticleSystem[] tumEfektler;
    private float[] baslangicYogunluklari;
    private bool sondurulduMu = false;

    // YENİ: Scriptin eğitim sahnesinde diğer scriptlerle çakışmasını önleyen kilit
    private bool egitimSahnesindeMi = false;

    void Start()
    {
        // GÜVENLİK KALKANI: Eğer sahnede TutorialManager varsa, burası eğitim bölümüdür.
        // Bu script uyku moduna geçer ve işi TutorialYanginHedefi'ne bırakır.
        if (Object.FindFirstObjectByType<TutorialManager>() != null)
        {
            egitimSahnesindeMi = true;
            return; 
        }

        maksimumCan = yanginCani;
        tetikleyiciKure = GetComponent<SphereCollider>();

        // Ateşin ve dumanın tüm partiküllerini bul
        tumEfektler = GetComponentsInChildren<ParticleSystem>();
        baslangicYogunluklari = new float[tumEfektler.Length];

        // Başlangıç yoğunluklarını hafızaya al
        for (int i = 0; i < tumEfektler.Length; i++)
        {
            baslangicYogunluklari[i] = tumEfektler[i].emission.rateOverTimeMultiplier;
        }
    }

    void Update()
    {
        // Eğitim sahnesindeysek veya oyun başlamadıysa yayılma fiziğini durdur
        if (egitimSahnesindeMi || !oyunBasladi || sondurulduMu) return;

        // Görünmez yayılma küresi sürekli büyür ve diğer ağaçları arar
        if (tetikleyiciKure != null && tetikleyiciKure.radius < maxTetikleyiciBoyutu)
        {
            tetikleyiciKure.radius += (yayilmaHizi * 0.5f) * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider diger)
    {
        // Eğitim sahnesindeysek tetiklenmeyi durdur
        if (egitimSahnesindeMi || !oyunBasladi || sondurulduMu) return;

        // Küre temiz bir ağaca dokunduğunda onu da yak
        if (diger.CompareTag("Agac"))
        {
            AgacDurumu agac = diger.GetComponent<AgacDurumu>();
            if (agac != null && !agac.yaniyorMu)
            {
                agac.AtesiBaslat();
            }
        }
    }

    // Drone'dan gelen su damlaları ateşe çarptığında çalışır
    void OnParticleCollision(GameObject carpanMadde)
    {
        // ÇÖZÜM: Eğitim sahnesindeysek su çarpışmasını görmezden gel! 
        // Bırak bu çarpışmayı aynı objenin üzerindeki TutorialYanginHedefi scripti yönetsin.
        if (egitimSahnesindeMi || sondurulduMu) return;

        // Orman yangınında SU (1) can götürür
        if (carpanMadde.name == "1") 
        {
            yanginCani -= 0.5f; // Her bir su damlasının vurduğu hasar
        }

        // Canın sınırların dışına çıkmasını engelle
        yanginCani = Mathf.Clamp(yanginCani, 0f, maksimumCan);

        // ATEŞİN GÖRSELİNİ CANINA GÖRE KIS 
        for (int i = 0; i < tumEfektler.Length; i++)
        {
            var emission = tumEfektler[i].emission;
            emission.rateOverTimeMultiplier = baslangicYogunluklari[i] * (yanginCani / maksimumCan);
        }

        if (yanginCani <= 0)
        {
            Sondur();
        }
    }

    void Sondur()
    {
        if (egitimSahnesindeMi) return;

        sondurulduMu = true;

        // Üst objedeki ağaç scriptini bul ve ona "Modelleri Değiştir!" komutunu gönder
        if (transform.parent != null)
        {
            AgacDurumu agac = transform.parent.GetComponent<AgacDurumu>();
            if (agac != null) 
            {
                agac.AgaciSondur(); 
            }
        }

        // Ateşi ve dumanı tamamen kapat
        gameObject.SetActive(false); 

        // SAHNE KONTROLÜ: Sahnede yanan başka ağaç kaldı mı?
        AgacDurumu[] tumAgaclar = FindObjectsByType<AgacDurumu>(FindObjectsSortMode.None);
        bool baskaYananVarMi = false;

        foreach (AgacDurumu a in tumAgaclar)
        {
            if (a.yaniyorMu)
            {
                baskaYananVarMi = true;
                break;
            }
        }

        // Eğer yanan HİÇBİR ağaç kalmadıysa Level 3'ten kopyalanan bitiş sistemini çalıştır
        if (!baskaYananVarMi)
        {
            MissionCompleteManager bitisYoneticisi = FindAnyObjectByType<MissionCompleteManager>();
            if (bitisYoneticisi != null)
            {
                bitisYoneticisi.GoreviTamamla();
            }
            else
            {
                Debug.LogError("Sahnede MissionCompleteManager bulunamadı! Lütfen hiyerarşiyi kontrol edin.");
            }
        }
    }
}