using UnityEngine;
using System.Collections;

public class ACUnit : MonoBehaviour
{
    [Header("Yangın Durumu")]
    public float yanginCani = 100f;
    private float maksimumCan;
    private float harlanmaLimiti;
    
    [Header("Denge Ayarları")]
    public float co2SondurmeGucu = 0.6f;   // CO2'nin söndürme hızı
    public float suHarlanmaGucu = 1.5f;    // Suyun ateşi harlama gücü
    public float kopukHarlanmaGucu = 0.8f; // Köpüğün ateşi harlama gücü
    
    [Header("Model Referansları")]
    public GameObject saglamUnite; 
    public GameObject yanmisUnite; 
    
    [Header("Ayarlar")]
    public float tekrarCanlanmaSuresi = 3.0f;

    public bool isExtinguishedPermanently = false;
    private bool isTempExtinguished = false;

    private ParticleSystem[] tumEfektler;
    private float[] baslangicYogunluklari;
    private float sonHarlamaZamani = 0f;

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

        saglamUnite.SetActive(true);
        yanmisUnite.SetActive(false);
    }

    void OnParticleCollision(GameObject carpanMadde)
    {
        if (isExtinguishedPermanently || isTempExtinguished || yanginCani <= 0) return;

        bool yanlisMudahale = false;

        // --- DOĞRU VE YANLIŞ MÜDAHALE MANTIĞI ---
        if (carpanMadde.name == "1") // SU
        {
            yanginCani += suHarlanmaGucu; 
            yanlisMudahale = true;
        }
        else if (carpanMadde.name == "2") // KÖPÜK
        {
            yanginCani += kopukHarlanmaGucu; 
            yanlisMudahale = true;
        }
        else if (carpanMadde.name == "3") // CO2
        {
            yanginCani -= co2SondurmeGucu; 
        }

        yanginCani = Mathf.Clamp(yanginCani, 0f, harlanmaLimiti);

        // Yanlış müdahalede partiküllerin anlık patlaması
        if (yanlisMudahale && Time.time > sonHarlamaZamani + 0.5f)
        {
            foreach(ParticleSystem efekt in tumEfektler)
            {
                efekt.Emit(40); 
            }
            sonHarlamaZamani = Time.time; 
        }

        for(int i = 0; i < tumEfektler.Length; i++)
        {
            var emission = tumEfektler[i].emission;
            // Burst sistemleri (kıvılcım) etkilenmez ama alevler yavaşça küçülür
            emission.rateOverTimeMultiplier = baslangicYogunluklari[i] * (yanginCani / maksimumCan);
        }

        if (yanginCani <= 0)
        {
            AttemptExtinguish();
        }
    }

    void AttemptExtinguish()
    {
        if (Level3Manager.Instance != null && Level3Manager.Instance.IsPanoExtinguished())
        {
            isExtinguishedPermanently = true;
            KapatAtesEfektlerini();
            
            saglamUnite.SetActive(false);
            yanmisUnite.SetActive(true);

            Level3Manager.Instance.CheckLevelCompleteStatus();
        }
        else
        {
            StartCoroutine(TemporaryExtinguishRoutine());
        }
    }

    IEnumerator TemporaryExtinguishRoutine()
    {
        isTempExtinguished = true;
        KapatAtesEfektlerini();
        
        yield return new WaitForSeconds(tekrarCanlanmaSuresi);

        if (Level3Manager.Instance != null && !Level3Manager.Instance.IsPanoExtinguished())
        {
            yanginCani = maksimumCan; 
            AcAtesEfektlerini();
            isTempExtinguished = false;
        }
        else
        {
            isExtinguishedPermanently = true;
            saglamUnite.SetActive(false);
            yanmisUnite.SetActive(true);
            Level3Manager.Instance.CheckLevelCompleteStatus();
        }
    }

    void KapatAtesEfektlerini()
    {
        for(int i = 0; i < tumEfektler.Length; i++)
        {
            tumEfektler[i].Stop(); 
            tumEfektler[i].Clear(); 
        }
    }

    void AcAtesEfektlerini()
    {
        for(int i = 0; i < tumEfektler.Length; i++)
        {
            tumEfektler[i].Play(); 
        }
    }
}