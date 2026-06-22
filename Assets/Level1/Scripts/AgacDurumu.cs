using UnityEngine;

public class AgacDurumu : MonoBehaviour
{
    public bool yaniyorMu = false;
    public GameObject agacinAtesi; 

    [Header("Model Değişimi")]
    [Tooltip("Sahnede görünen yeşil ağaç (default objesinin kendisi)")]
    public GameObject saglamAgac; 
    [Tooltip("Gizlediğimiz siyah ağaç (Yanmis_Model objesi)")]
    public GameObject yanmisAgac; 

    void Start()
    {
        // Oyun başında yanmış siyah modelin kapalı olduğundan kod taraflı da emin oluyoruz
        if (yanmisAgac != null) 
        {
            yanmisAgac.SetActive(false);
        }
    }

    public void AtesiBaslat()
    {
        if (!yaniyorMu)
        {
            yaniyorMu = true;
            if (agacinAtesi != null)
            {
                agacinAtesi.SetActive(true); // İçindeki kapalı ateşi uyandır
            }
        }
    }

    public void AgaciSondur()
    {
        yaniyorMu = false;

        // Modelleri pürüzsüzce yer değiştir
        if (saglamAgac != null) saglamAgac.SetActive(false);
        if (yanmisAgac != null) yanmisAgac.SetActive(true);
    }
}