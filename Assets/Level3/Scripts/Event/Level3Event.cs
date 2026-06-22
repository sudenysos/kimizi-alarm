using UnityEngine;

public class PanoYangin : MonoBehaviour
{
    [Header("Yangın Durumu")]
    public float yanginCani = 100f;
    private float maksimumCan;
    private float harlanmaLimiti; 
    
    [Header("Denge Ayarları")]
    public float co2SondurmeGucu = 0.6f;   // CO2'nin söndürme hızı
    public float suHarlanmaGucu = 1.5f;    // Suyun ateşi harlama gücü
    public float kopukHarlanmaGucu = 0.8f; // Köpüğün ateşi harlama gücü
    
    [Header("Model Değişimi (Swap)")]
    public GameObject saglamPano; // Before objesi
    public GameObject yanmisPano; // After objesi
    
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
    }

    void OnParticleCollision(GameObject carpanMadde)
    {
        if (yanginCani <= 0) return; 

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
            emission.rateOverTimeMultiplier = baslangicYogunluklari[i] * (yanginCani / maksimumCan);
        }

        if (yanginCani <= 0)
        {
            Sondur();
        }
    }

    void Sondur()
    {
        if (yanmisPano != null)
        {
            yanmisPano.SetActive(true);
        }

        if (saglamPano != null)
        {
            saglamPano.SetActive(false);
        }
        
        Debug.Log("Pano yangını söndürüldü! Çatıdaki üniteler artık kalıcı olarak söndürülebilir.");

        if (Level3Manager.Instance != null)
        {
            Level3Manager.Instance.SetPanoExtinguished();
        }
        else
        {
            Debug.LogWarning("Sahne içinde Level3Manager bulunamadı, bu yüzden çatı ünitelerine haber verilemedi.");
        }
    }
}