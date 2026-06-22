using UnityEngine;

public class ExtinguisherSystem : MonoBehaviour
{
    public enum SondurucuModu { Su, Kopuk, Karbondioksit }

    [Header("Sistem Durumu")]
    public SondurucuModu aktifMod = SondurucuModu.Su;
    public bool atisYapiliyor = false;

    [Header("Değişim (Transition) Ayarları")]
    public float gecisSuresi = 1.5f; 
    private float gecisZamanlayici = 0f;
    private bool gecisYapiliyor = false;
    private SondurucuModu hedefMod;

    [Header("Görsel Efektler (Particle Systems)")]
    public ParticleSystem suEfekti;
    public ParticleSystem kopukEfekti;
    public ParticleSystem co2Efekti;

    [Header("Madde Ses Ayarları")]
    public AudioSource nozzleSesKaynagi; // Sesin çıkacağı hoparlör
    public AudioClip suSesi;             // Su fışkırma sesi
    public AudioClip kopukSesi;          // Köpük tıslama sesi
    public AudioClip co2Sesi;            // Gaz basınç sesi

    private DroneController droneController;

    void Start()
    {
        droneController = GetComponentInParent<DroneController>();
    }

    void Update()
    {
        if (droneController != null && !droneController.maddeKullanimiAcik)
        {
            AtisiDurdur(); 
            return; 
        }

        if (gecisYapiliyor)
        {
            gecisZamanlayici -= Time.deltaTime;
            
            AtisiDurdur(); 

            if (gecisZamanlayici <= 0f)
            {
                aktifMod = hedefMod;
                gecisYapiliyor = false;
                Debug.Log("Mod Değişimi Başarılı! Yeni Aktif Mod: " + aktifMod.ToString());
            }
            return; 
        }

        ModDegisimiKontrol();
        AtisKontrol();
    }

    void ModDegisimiKontrol()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && aktifMod != SondurucuModu.Su)
        {
            GecisiBaslat(SondurucuModu.Su);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && aktifMod != SondurucuModu.Kopuk)
        {
            GecisiBaslat(SondurucuModu.Kopuk);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && aktifMod != SondurucuModu.Karbondioksit)
        {
            GecisiBaslat(SondurucuModu.Karbondioksit);
        }
    }

    void GecisiBaslat(SondurucuModu yeniMod)
    {
        hedefMod = yeniMod;
        gecisYapiliyor = true;
        gecisZamanlayici = gecisSuresi; 
        
        Debug.Log(yeniMod.ToString() + " moduna geçiliyor... Sistem " + gecisSuresi + " saniye kilitlendi.");
        
        AtisiDurdur();
    }

    void AtisKontrol()
    {
        if (Input.GetMouseButton(0))
        {
            AtisiBaslat();
        }
        else
        {
            AtisiDurdur();
        }
    }

    void AtisiBaslat()
    {
        if (atisYapiliyor) return; 
        atisYapiliyor = true;

        if (aktifMod == SondurucuModu.Su)
        {
            if (suEfekti != null) suEfekti.Play();
            if (nozzleSesKaynagi != null && suSesi != null) 
            { 
                nozzleSesKaynagi.clip = suSesi; 
                nozzleSesKaynagi.Play(); 
            }
        }
        else if (aktifMod == SondurucuModu.Kopuk)
        {
            if (kopukEfekti != null) kopukEfekti.Play();
            if (nozzleSesKaynagi != null && kopukSesi != null) 
            { 
                nozzleSesKaynagi.clip = kopukSesi; 
                nozzleSesKaynagi.Play(); 
            }
        }
        else if (aktifMod == SondurucuModu.Karbondioksit)
        {
            if (co2Efekti != null) co2Efekti.Play();
            if (nozzleSesKaynagi != null && co2Sesi != null) 
            { 
                nozzleSesKaynagi.clip = co2Sesi; 
                nozzleSesKaynagi.Play(); 
            }
        }
    }

    void AtisiDurdur()
    {
        if (!atisYapiliyor) return;
        atisYapiliyor = false;

        // Görsel efektleri durdur
        if (suEfekti != null) suEfekti.Stop();
        if (kopukEfekti != null) kopukEfekti.Stop();
        if (co2Efekti != null) co2Efekti.Stop();

        // Sesi durdur
        if (nozzleSesKaynagi != null) nozzleSesKaynagi.Stop();
    }
}