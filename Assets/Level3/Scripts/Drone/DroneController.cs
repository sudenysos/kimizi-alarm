using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using TMPro;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("Eğitim (Tutorial) Ayarları")]
    public bool tutorialModu = false;
    public bool maddeKullanimiAcik = true;

    [Header("Hareket Ayarları")]
    public float hareketHizi = 7f;  
    public float donusHizi = 80f;   
    
    [Header("Fizik Hissiyatı (İvme)")]
    public float ivme = 5f;         

    [Header("Motor Sesi Ayarları")]
    public AudioSource motorSesi;
    public float minPitch = 0.8f; 
    public float maxPitch = 1.6f; 
    public float pitchDegisimHizi = 5f;

    [Header("Can ve UI Sistemi")]
    public int kalanDroneHakki = 3;
    public TextMeshProUGUI canYazisi; 
    
    [Header("Sinyal UI Elementleri")]
    public Image sinyalArayuzu;   
    public Sprite sinyalGucu3;    
    public Sprite sinyalGucu2;    
    public Sprite sinyalGucu1;    
    public Sprite sinyalKoptu;    
    
    private List<SinyalBolgesi> icindeBulunduguBolgeler = new List<SinyalBolgesi>();
    private bool isRespawning = false; 
    private bool baslangicKorumasi = true; 
    
    // YENİ: Motorun kaza anındaki durumunu takip eden bayrak
    private bool motorArizali = false; 

    [Header("Görev Başarısız UI Elemanları")]
    public CanvasGroup gorevBasarisizEkran; 
    public float gecisSuresi = 1.5f; 
    public GameObject kapanacakOyunIciUI; 
    
    private Vector3 baslangicPozisyonu; 
    private Quaternion baslangicRotasyonu;

    private Rigidbody rb;
    private float hedefIleri, mevcutIleri;
    private float hedefKayma, mevcutKayma;
    private float hedefDonus, mevcutDonus;
    private float hedefYukselme, mevcutYukselme;

    [HideInInspector] public bool kontrolEdilebilirMi = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        baslangicPozisyonu = transform.position; 
        baslangicRotasyonu = transform.rotation; 
        
        UIGuncelle(); 
        SinyalGuncelle();
        Invoke("KorumayiKaldir", 0.2f); 
        
        if (gorevBasarisizEkran != null)
        {
            gorevBasarisizEkran.alpha = 0f;
            gorevBasarisizEkran.interactable = false;
            gorevBasarisizEkran.blocksRaycasts = false;
            gorevBasarisizEkran.gameObject.SetActive(false);
        }
    }

    void KorumayiKaldir()
    {
        baslangicKorumasi = false;
        SinyalGuncelle(); 
    }

    void Update()
    {
        if (!kontrolEdilebilirMi)
        {
            // YENİ: Eğer motor arızalıysa Update'in sesi düzeltmesine izin verme (Efekt çalışsın)
            if (!motorArizali && motorSesi != null) 
            {
                motorSesi.pitch = Mathf.Lerp(motorSesi.pitch, minPitch, Time.deltaTime * pitchDegisimHizi);
            }
            return;
        }

        hedefIleri = 0f;
        if (Input.GetKey(KeyCode.W)) hedefIleri = 1f;
        else if (Input.GetKey(KeyCode.S)) hedefIleri = -1f;

        hedefKayma = 0f;
        if (Input.GetKey(KeyCode.D)) hedefKayma = 1f;
        else if (Input.GetKey(KeyCode.A)) hedefKayma = -1f;

        hedefDonus = 0f;
        if (Input.GetKey(KeyCode.E)) hedefDonus = 1f;
        else if (Input.GetKey(KeyCode.Q)) hedefDonus = -1f;

        hedefYukselme = 0f;
        if (Input.GetKey(KeyCode.Space)) hedefYukselme = 1f;
        else if (Input.GetKey(KeyCode.LeftShift)) hedefYukselme = -1f;

        mevcutIleri = Mathf.Lerp(mevcutIleri, hedefIleri, Time.deltaTime * ivme);
        mevcutKayma = Mathf.Lerp(mevcutKayma, hedefKayma, Time.deltaTime * ivme);
        mevcutDonus = Mathf.Lerp(mevcutDonus, hedefDonus, Time.deltaTime * ivme);
        mevcutYukselme = Mathf.Lerp(mevcutYukselme, hedefYukselme, Time.deltaTime * ivme);

        float mevcutHizOrani = rb.linearVelocity.magnitude / hareketHizi; 
        float hedefPitch = Mathf.Lerp(minPitch, maxPitch, mevcutHizOrani);
        if(motorSesi != null) motorSesi.pitch = Mathf.Lerp(motorSesi.pitch, hedefPitch, Time.deltaTime * pitchDegisimHizi);
    }

    void FixedUpdate()
    {
        if (!kontrolEdilebilirMi) return;

        Vector3 yeniHiz = (transform.forward * mevcutIleri * hareketHizi) + 
                          (transform.right * mevcutKayma * hareketHizi) + 
                          (Vector3.up * mevcutYukselme * hareketHizi);
        
        rb.linearVelocity = yeniHiz;
        rb.angularVelocity = new Vector3(0f, mevcutDonus * donusHizi * Mathf.Deg2Rad, 0f);
    }
    
    void OnTriggerEnter(Collider other)
    {
        SinyalBolgesi bolge = other.GetComponent<SinyalBolgesi>();
        if (bolge != null && !icindeBulunduguBolgeler.Contains(bolge))
        {
            icindeBulunduguBolgeler.Add(bolge);
            SinyalGuncelle();
        }
    }

    void OnTriggerExit(Collider other)
    {
        SinyalBolgesi bolge = other.GetComponent<SinyalBolgesi>();
        if (bolge != null && icindeBulunduguBolgeler.Contains(bolge))
        {
            icindeBulunduguBolgeler.Remove(bolge);
            SinyalGuncelle();
        }
    }

    void SinyalGuncelle()
    {
        if (isRespawning) return; 

        int maxSinyal = 0;
        foreach (var bolge in icindeBulunduguBolgeler)
        {
            if (bolge != null && bolge.sinyalSeviyesi > maxSinyal) maxSinyal = bolge.sinyalSeviyesi;
        }

        if (sinyalArayuzu != null)
        {
            if (maxSinyal >= 3) sinyalArayuzu.sprite = sinyalGucu3;
            else if (maxSinyal == 2) sinyalArayuzu.sprite = sinyalGucu2;
            else if (maxSinyal == 1) sinyalArayuzu.sprite = sinyalGucu1;
            else
            {
                if(sinyalKoptu != null) sinyalArayuzu.sprite = sinyalKoptu;
                else sinyalArayuzu.sprite = sinyalGucu1; 
            }
        }

        if (maxSinyal == 0 && kontrolEdilebilirMi && !baslangicKorumasi)
        {
            StartCoroutine(KazaEşikSüresiRutini());
        }
    }

    void OnCollisionEnter(Collision carptigiObje)
    {
        if (kontrolEdilebilirMi)
        {
            StartCoroutine(KazaEşikSüresiRutini());
        }
    }

    // --- YENİ: DÜŞERKEN MOTORUN CAN ÇEKİŞME EFEKTİ ---
    IEnumerator MotorArizaEfekti()
    {
        motorArizali = true;
        float baslangicSesSeviyesi = motorSesi != null ? motorSesi.volume : 1f;

        while (motorArizali && motorSesi != null)
        {
            // Motor boğuluyor ve pervaneler sürtüyormuş gibi perdesini (pitch) ve ses düzeyini rastgele kıs
            motorSesi.pitch = Random.Range(0.2f, 1.0f);
            motorSesi.volume = Random.Range(0.2f, baslangicSesSeviyesi);
            
            // Saniyenin 20'de biri hızında bu titreşimi tekrarla (Glitch efekti)
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        }

        // Kaza bitince sesi normal seviyelerine döndür
        if (motorSesi != null)
        {
            motorSesi.pitch = minPitch;
            motorSesi.volume = baslangicSesSeviyesi;
        }
    }

    IEnumerator KazaEşikSüresiRutini()
    {
        kontrolEdilebilirMi = false; 
        kalanDroneHakki--;
        UIGuncelle();

        // Kaza anında motor arıza efektini başlat
        StartCoroutine(MotorArizaEfekti());

        if (tutorialModu)
        {
            rb.angularVelocity = new Vector3(Random.Range(-3f, 3f), 15f, Random.Range(-3f, 3f));
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.2f, -6f, rb.linearVelocity.z * 0.2f);

            float egitimSure = 0f;
            while (egitimSure < 1.0f)
            {
                egitimSure += Time.deltaTime;
                if (rb.linearVelocity.y > 0f)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, -4f, rb.linearVelocity.z);
                }
                yield return null;
            }

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (kalanDroneHakki > 0)
            {
                motorArizali = false; // Efekti durdur
                TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
                if (tm != null) tm.EgitimdeKazaYapildi(); 
            }
            else
            {
                motorArizali = false;
                if (motorSesi != null) motorSesi.Stop(); // Oyun bitti, motor tamamen sussun
                rb.isKinematic = true;
                if (gorevBasarisizEkran != null) StartCoroutine(EkraniYavascaAc());
            }
            yield break; 
        }

        rb.angularVelocity = new Vector3(Random.Range(-3f, 3f), 15f, Random.Range(-3f, 3f));
        rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.2f, -6f, rb.linearVelocity.z * 0.2f);
        
        yield return new WaitForSeconds(1.0f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(0.5f);

        if (kalanDroneHakki > 0)
        {
            motorArizali = false; // Efekti durdur
            isRespawning = true; 
            Respawn();
            
            yield return new WaitForFixedUpdate(); 
            yield return new WaitForFixedUpdate();
            
            isRespawning = false; 
            kontrolEdilebilirMi = true; 
            SinyalGuncelle(); 
        }
        else
        {
            motorArizali = false;
            if (motorSesi != null) motorSesi.Stop(); // Oyun bitti, motor tamamen sussun
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            
            if (gorevBasarisizEkran != null) StartCoroutine(EkraniYavascaAc());
        }
    }

    public void EgitimIcinMerkezeIsinla()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true; 
        rb.detectCollisions = false;

        baslangicPozisyonu = new Vector3(-0.2f, 3f, 4f);
        baslangicRotasyonu = Quaternion.Euler(0f, 180f, 0f);

        RigidbodyInterpolation eskiInterp = rb.interpolation;
        rb.interpolation = RigidbodyInterpolation.None;
        
        transform.position = baslangicPozisyonu;
        transform.rotation = baslangicRotasyonu;
        rb.position = baslangicPozisyonu;
        rb.rotation = baslangicRotasyonu;

        Physics.SyncTransforms();

        mevcutIleri = 0f; hedefIleri = 0f;
        mevcutKayma = 0f; hedefKayma = 0f;
        mevcutDonus = 0f; hedefDonus = 0f;
        mevcutYukselme = 0f; hedefYukselme = 0f;

        rb.interpolation = eskiInterp;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            col.enabled = true;
        }

        if (sinyalArayuzu != null && sinyalGucu3 != null)
        {
            sinyalArayuzu.sprite = sinyalGucu3;
        }
    }

    public void EgitimiBaslatFizigiAc()
    {
        RigidbodyInterpolation eskiInterp = rb.interpolation;
        rb.interpolation = RigidbodyInterpolation.None;

        rb.isKinematic = false;
        rb.detectCollisions = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        transform.position = baslangicPozisyonu;
        transform.rotation = baslangicRotasyonu;
        rb.position = baslangicPozisyonu;
        rb.rotation = baslangicRotasyonu;
        rb.ResetInertiaTensor(); 

        Physics.SyncTransforms();

        rb.interpolation = eskiInterp;

        // Susturulmuş motoru canlandır
        if (motorSesi != null && !motorSesi.isPlaying) motorSesi.Play(); 
    }

    public void RespawnDistanTetikle() 
    {
        Respawn();
    }

    void Respawn()
    {
        RigidbodyInterpolation eskiInterp = rb.interpolation;
        rb.interpolation = RigidbodyInterpolation.None;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.detectCollisions = false;

        transform.position = baslangicPozisyonu;
        transform.rotation = baslangicRotasyonu;
        rb.position = baslangicPozisyonu;
        rb.rotation = baslangicRotasyonu;

        Physics.SyncTransforms();

        mevcutIleri = 0f; hedefIleri = 0f;
        mevcutKayma = 0f; hedefKayma = 0f;
        mevcutDonus = 0f; hedefDonus = 0f;
        mevcutYukselme = 0f; hedefYukselme = 0f;

        rb.isKinematic = false;
        rb.detectCollisions = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.interpolation = eskiInterp;

        // Kaza sonrası susturulmuşsa tekrar başlat
        if (motorSesi != null && !motorSesi.isPlaying) motorSesi.Play();
    }

    IEnumerator EkraniYavascaAc()
    {
        if (kapanacakOyunIciUI != null) kapanacakOyunIciUI.SetActive(false);

        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
        if (tm != null)
        {
            if (tm.controlsPanel != null) tm.controlsPanel.SetActive(false);
            if (tm.maddeControlsPanel != null) tm.maddeControlsPanel.SetActive(false);
            if (tm.infoPanel != null) tm.infoPanel.SetActive(false);
        }

        gorevBasarisizEkran.gameObject.SetActive(true);
        float gecenSure = 0f;
        while (gecenSure < gecisSuresi)
        {
            gecenSure += Time.deltaTime;
            gorevBasarisizEkran.alpha = Mathf.Lerp(0f, 1f, gecenSure / gecisSuresi);
            yield return null; 
        }
        gorevBasarisizEkran.alpha = 1f;
        gorevBasarisizEkran.interactable = true;
        gorevBasarisizEkran.blocksRaycasts = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void UIGuncelle()
    {
        if (canYazisi != null) canYazisi.text = kalanDroneHakki.ToString();
    }

    public void YenidenDene()
    {
        Scene aktifSahne = SceneManager.GetActiveScene();
        SceneManager.LoadScene(aktifSahne.name); 
    }

    public void MenuyeDon()
    {
        Time.timeScale = 1f; 
        PlayerPrefs.SetInt("BolumSecimiAc", 1); 
        SceneManager.LoadScene("Scene0"); 
    }
}