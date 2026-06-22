using UnityEngine;

public class TutorialYanginHedefi : MonoBehaviour
{
    public enum YanginTipi { KatiOdun, SiviYag, ElektrikBatarya }

    [Header("Yangın Türü Ayarı")]
    public YanginTipi yanginTipi;

    [Header("Yangın Durumu")]
    public float yanginCani = 100f;
    private float maksimumCan;
    private float harlanmaLimiti; 
    
    [Header("Denge Ayarları")]
    public float sondurmeGucu = 0.6f;   
    public float harlanmaGucu = 1.2f;    
    
    [Header("Model Değişimi")]
    public GameObject saglamModel; // Before nesnesi
    public GameObject yanmisModel; // After nesnesi
    
    private ParticleSystem[] tumEfektler; 
    private float[] baslangicYogunluklari;
    private float sonHarlamaZamani = 0f;
    
    // KİLİT NOKTASI: Partikül makineli tüfeği etkisini %100 engeller!
    private bool islemTamamlandi = false; 

    void Start()
    {
        maksimumCan = yanginCani;
        harlanmaLimiti = maksimumCan * 1.5f; 
        
        tumEfektler = GetComponentsInChildren<ParticleSystem>(); 
        baslangicYogunluklari = new float[tumEfektler.Length];

        for(int i = 0; i < tumEfektler.Length; i++)
        {
            baslangicYogunluklari[i] = tumEfektler[i].emission.rateOverTimeMultiplier;
        }

        // Başlangıçta Before açık, After kapalı
        if (saglamModel != null) saglamModel.SetActive(true);
        if (yanmisModel != null) yanmisModel.SetActive(false);
    }

    void OnParticleCollision(GameObject carpanMadde)
    {
        // 1. GÜVENLİK DUVARI: Eğer bu ateş daha önce söndürüldüyse, çarpan diğer yüzlerce partikülü anında reddet!
        if (islemTamamlandi) return; 

        bool yanlisMudahale = false; 
        bool dogruMudahale = false;
        string maddeAdi = carpanMadde.name; 

        switch (yanginTipi)
        {
            case YanginTipi.KatiOdun:
                if (maddeAdi == "1") dogruMudahale = true; 
                else if (maddeAdi == "2" || maddeAdi == "3") yanlisMudahale = true;
                break;
            case YanginTipi.SiviYag:
                if (maddeAdi == "2") dogruMudahale = true; 
                else if (maddeAdi == "1" || maddeAdi == "3") yanlisMudahale = true;
                break;
            case YanginTipi.ElektrikBatarya:
                if (maddeAdi == "3") dogruMudahale = true; 
                else if (maddeAdi == "1" || maddeAdi == "2") yanlisMudahale = true;
                break;
        }

        if (dogruMudahale) yanginCani -= sondurmeGucu;
        else if (yanlisMudahale) yanginCani += harlanmaGucu;

        yanginCani = Mathf.Clamp(yanginCani, 0f, harlanmaLimiti);

        if (yanlisMudahale && Time.time > sonHarlamaZamani + 0.5f)
        {
            foreach(ParticleSystem efekt in tumEfektler) { efekt.Emit(30); }
            sonHarlamaZamani = Time.time; 
        }

        for(int i = 0; i < tumEfektler.Length; i++)
        {
            var emission = tumEfektler[i].emission;
            emission.rateOverTimeMultiplier = baslangicYogunluklari[i] * (yanginCani / maksimumCan);
        }

        // 2. GÜVENLİK DUVARI: Can sıfıra indiğinde sadece bir kez içeri gir.
        if (yanginCani <= 0f && !islemTamamlandi)
        {
            islemTamamlandi = true; // Kapıları tamamen kilitle!
            Sondur();
        }
    }

    void Sondur()
    {
        // Alevleri durdur
        foreach (ParticleSystem efekt in tumEfektler)
        {
            efekt.Stop();
        }

        // Modelleri değiştir
        if (yanmisModel != null) yanmisModel.SetActive(true);
        if (saglamModel != null) saglamModel.SetActive(false);

        // Yöneticiye mesajı gönderirken KENDİ İSMİNİ de gönder
        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
        if (tm != null)
        {
            tm.BirHedefSonduruldu(gameObject.name); 
        }
    }
}