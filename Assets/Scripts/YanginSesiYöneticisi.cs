using UnityEngine;

public class YanginSesiYoneticisi : MonoBehaviour
{
    public enum YanginTipi { Normal, Elektrik, Kimyasal }
    
    [Header("Yangın Tipi Ayarı")]
    public YanginTipi yanginTipi = YanginTipi.Normal;

    [Header("Başlangıç Durumu")]
    public bool baslangictaYaniyorMu = false; 
    
    [Header("Görsel Takip Sistemi (MagicaVoxel vb. İçin)")]
    [Tooltip("Bu yuvaya atadığınız obje (örn: 'default') kodla kapanırsa, ses otomatik susar.")]
    public GameObject takipEdilenGorsel;

    [Header("Hoparlörler (Audio Sources)")]
    public AudioSource anaHoparlor;
    public AudioSource detayHoparlor;

    [Header("Detay Ses Dosyaları (Kasetler)")]
    public AudioClip ahsapCitirtisi;
    public AudioClip elektrikCizirtisi;
    public AudioClip kimyasalFokurdama;

    private float anaMaxSes;
    private float detayMaxSes;
    private bool yaniyorMu = false;

    void Awake() 
    {
        if (anaHoparlor != null) anaMaxSes = anaHoparlor.volume;
        if (detayHoparlor != null) detayMaxSes = detayHoparlor.volume;

        if (detayHoparlor != null)
        {
            switch (yanginTipi)
            {
                case YanginTipi.Normal: detayHoparlor.clip = ahsapCitirtisi; break;
                case YanginTipi.Elektrik: detayHoparlor.clip = elektrikCizirtisi; break;
                case YanginTipi.Kimyasal: detayHoparlor.clip = kimyasalFokurdama; break;
            }
            detayHoparlor.loop = true;
        }
    }

    void Start()
    {
        if (!yaniyorMu) 
        {
            if (baslangictaYaniyorMu) SesiBaslat();
            else SesiDurdur(); 
        }
    }

    // YENİ EKLENEN KONTROL: Her karede görselin durumunu kontrol et
    void Update()
    {
        // Eğer bir obje takip ediliyorsa, sistem şu an yanıyorsa ama obje kapandıysa:
        if (takipEdilenGorsel != null && yaniyorMu)
        {
            if (!takipEdilenGorsel.activeInHierarchy)
            {
                SesiDurdur(); // Görsel kapandığı an sesi bıçak gibi kes!
            }
        }
    }

    public void SesiBaslat()
    {
        if (yaniyorMu) return;
        yaniyorMu = true;

        if (anaHoparlor != null && !anaHoparlor.isPlaying) anaHoparlor.Play();
        if (detayHoparlor != null && !detayHoparlor.isPlaying) detayHoparlor.Play();
        
        if (anaHoparlor != null) anaHoparlor.volume = anaMaxSes;
        if (detayHoparlor != null) detayHoparlor.volume = detayMaxSes;
    }

    public void SesiDurdur()
    {
        yaniyorMu = false;
        if (anaHoparlor != null) anaHoparlor.Stop();
        if (detayHoparlor != null) detayHoparlor.Stop();
    }

    public void SesSeviyesiniGuncelle(float mevcutCan, float maxCan)
    {
        if (!yaniyorMu) return; 

        float canOrani = Mathf.Clamp01(mevcutCan / maxCan); 
        
        if (anaHoparlor != null) anaHoparlor.volume = anaMaxSes * canOrani;
        if (detayHoparlor != null) detayHoparlor.volume = detayMaxSes * canOrani;
        
        if (mevcutCan <= 0)
        {
            SesiDurdur();
        }
    }
}