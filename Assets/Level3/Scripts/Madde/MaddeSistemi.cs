using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaddeSistemi : MonoBehaviour
{
    public enum SöndürücüMadde { Su, Köpük, CO2 }

    [Header("Aktif Madde")]
    public SöndürücüMadde mevcutMadde = SöndürücüMadde.Su;

    [Header("UI Elemanları")]
    public RectTransform[] maddeSlotlari; 

    [Header("Görseller (Sırasıyla: Su, Köpük, CO2)")]
    public Sprite[] aktifSprite;   // Renkli ve parlak olanlar
    public Sprite[] pasifSprite;   // Mat gri olanlar

    [Header("Animasyon Ayarları")]
    public float aktifBoyut = 1.3f;    
    public float normalBoyut = 1.0f;   
    public float gecisSuresi = 0.25f; 

    private Coroutine[] animasyonRutnleri = new Coroutine[3];

    // --- YENİ: SENKRONİZASYON VE KİLİT DEĞİŞKENLERİ ---
    private ExtinguisherSystem atisSistemi;
    private DroneController droneController;
    private float uiKilitSayaci = 0f;

    void Start()
    {
        // Mekanik ve kontrol sistemlerini otomatik olarak bul
        atisSistemi = FindAnyObjectByType<ExtinguisherSystem>();
        droneController = FindAnyObjectByType<DroneController>();

        for (int i = 0; i < maddeSlotlari.Length; i++)
        {
            // Görselleri ilk durumda ayarla
            GorselGuncelle(i, (i == 0)); 
            
            float baslangicKatsayisi = (i == 0) ? aktifBoyut : normalBoyut;
            maddeSlotlari[i].localScale = Vector3.one * baslangicKatsayisi;
        }
    }

    void Update()
    {
        // 1. GÜVENLİK KALKANI: Madde kullanımı kapalıysa (Eğitim Modu vb.) UI değişmesin
        if (droneController != null && !droneController.maddeKullanimiAcik) return;

        // 2. SENKRONİZASYON KALKANI: Eğer mekanik sistem geçiş yapıyorsa, UI tuşlarını kilitle!
        if (uiKilitSayaci > 0f)
        {
            uiKilitSayaci -= Time.deltaTime;
            return; // Süre bitene kadar aşağıdaki spam tuş basımlarını tamamen reddet
        }

        // Kilit yoksa tuşları dinle
        if (Input.GetKeyDown(KeyCode.Alpha1) && mevcutMadde != SöndürücüMadde.Su) MaddeDegistir(0); 
        else if (Input.GetKeyDown(KeyCode.Alpha2) && mevcutMadde != SöndürücüMadde.Köpük) MaddeDegistir(1); 
        else if (Input.GetKeyDown(KeyCode.Alpha3) && mevcutMadde != SöndürücüMadde.CO2) MaddeDegistir(2); 
    }

    void MaddeDegistir(int secilenIndeks)
    {
        mevcutMadde = (SöndürücüMadde)secilenIndeks;

        // ÇÖZÜM: UI değiştiği an kendini Atış Sistemi'nin gerçek "Geçiş Süresi" kadar kilitle!
        // Böylece UI ve Mekanik her zaman saniyesi saniyesine aynı anda açılıp kapanacak.
        if (atisSistemi != null) uiKilitSayaci = atisSistemi.gecisSuresi;
        else uiKilitSayaci = 1.5f; // Sisteme ulaşılamazsa yedek süre

        for (int i = 0; i < maddeSlotlari.Length; i++)
        {
            // 1. Görseli değiştir
            GorselGuncelle(i, (i == secilenIndeks));

            // 2. Boyutu animasyonla değiştir
            float hedefBoyut = (i == secilenIndeks) ? aktifBoyut : normalBoyut;

            if (animasyonRutnleri[i] != null) StopCoroutine(animasyonRutnleri[i]);
            if (maddeSlotlari[i] != null)
                animasyonRutnleri[i] = StartCoroutine(YumusakBoyutGecisi(maddeSlotlari[i], hedefBoyut));
        }
    }

    // Görsel değiştirme fonksiyonu
    void GorselGuncelle(int indeks, bool aktifMi)
    {
        Image img = maddeSlotlari[indeks].GetComponent<Image>();
        if (img != null)
        {
            img.sprite = aktifMi ? aktifSprite[indeks] : pasifSprite[indeks];
        }
    }

    IEnumerator YumusakBoyutGecisi(RectTransform slot, float hedef)
    {
        Vector3 baslangic = slot.localScale;
        Vector3 hedefVektor = Vector3.one * hedef;
        float gecenZaman = 0f;

        while (gecenZaman < gecisSuresi)
        {
            gecenZaman += Time.deltaTime;
            float oran = gecenZaman / gecisSuresi;
            float yumusatilmisOran = Mathf.SmoothStep(0f, 1f, oran);
            slot.localScale = Vector3.Lerp(baslangic, hedefVektor, yumusatilmisOran);
            yield return null;
        }
        slot.localScale = hedefVektor;
    }
}