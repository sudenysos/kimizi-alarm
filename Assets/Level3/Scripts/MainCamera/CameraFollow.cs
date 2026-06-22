using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Takip Ayarları")]
    public Transform hedef; // Drone
    private DroneController droneScript;

    [Header("Kamera Pozisyonu (Offset)")]
    public Vector3 mesafe = new Vector3(0f, 1.5f, -3f);

    [Header("Çarpışma (Duvar) Ayarları")]
    public LayerMask engelKatmanlari; // Kamerayı engelleyecek nesneler (Duvarlar vb.)
    public float duvarBoslugu = 0.2f; // Kameranın duvarın tam içine girmemesi için bırakılacak minik pay

    // Kaza anında kamerayı dondurmak için gereken değişkenler
    private Vector3 kazaAnindakiOfset;
    private Quaternion kazaAnindakiRotasyon;
    private bool kazaOfsetiAlindi = false;

    // Kamera aktifleştiği an çalışır
    void OnEnable()
    {
        if (hedef != null)
        {
            Vector3 idealPozisyon = hedef.position + (hedef.forward * mesafe.z) + (hedef.up * mesafe.y) + (hedef.right * mesafe.x);
            transform.position = idealPozisyon;
            transform.rotation = Quaternion.LookRotation(hedef.position - transform.position);
        }
    }

    void Start()
    {
        if (hedef != null)
        {
            droneScript = hedef.GetComponent<DroneController>();
        }
    }

    void LateUpdate()
    {
        if (hedef == null) return;

        // --- 1. DURUM: KAZA VE DÜŞÜŞ ANI (Veya Tutorial Beklemesi) ---
        if (droneScript != null && !droneScript.kontrolEdilebilirMi)
        {
            if (!kazaOfsetiAlindi)
            {
                kazaAnindakiOfset = transform.position - hedef.position;
                kazaAnindakiRotasyon = transform.rotation;
                kazaOfsetiAlindi = true;
            }

            Vector3 kazaIdealPozisyon = hedef.position + kazaAnindakiOfset;

            RaycastHit kazaHit;
            if (Physics.Linecast(hedef.position, kazaIdealPozisyon, out kazaHit, engelKatmanlari))
            {
                transform.position = kazaHit.point + (hedef.position - kazaHit.point).normalized * duvarBoslugu;
            }
            else
            {
                transform.position = kazaIdealPozisyon;
            }

            transform.rotation = kazaAnindakiRotasyon;
            
            return; 
        }

        // --- 2. DURUM: NORMAL UÇUŞ ---
        kazaOfsetiAlindi = false;

        Vector3 idealPozisyon = hedef.position + (hedef.forward * mesafe.z) + (hedef.up * mesafe.y) + (hedef.right * mesafe.x);

        RaycastHit hit;
        if (Physics.Linecast(hedef.position, idealPozisyon, out hit, engelKatmanlari))
        {
            transform.position = hit.point + (hedef.position - hit.point).normalized * duvarBoslugu;
        }
        else
        {
            transform.position = idealPozisyon;
        }

        transform.rotation = Quaternion.LookRotation(hedef.position - transform.position);
    }

    // YENİ VE GÜVENLİ METHOD: Diğer sahneleri bozmadan kameranın kaza hafızasını dışarıdan sıfırlar
    public void ResetKameraKildi()
    {
        if (hedef == null) return;

        // Dronun güncellenmiş (ışınlanmış) konumuna göre temiz açıyı hesapla
        Vector3 idealPozisyon = hedef.position + (hedef.forward * mesafe.z) + (hedef.up * mesafe.y) + (hedef.right * mesafe.x);
        transform.position = idealPozisyon;
        transform.rotation = Quaternion.LookRotation(hedef.position - transform.position);

        // Kaza kilit verilerini bu temiz ve düz açıya göre güncelle ki Şef konuşurken yamulmasın
        kazaAnindakiOfset = transform.position - hedef.position;
        kazaAnindakiRotasyon = transform.rotation;
        kazaOfsetiAlindi = true;
    }
}