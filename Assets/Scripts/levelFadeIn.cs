using System.Collections;
using UnityEngine;

public class LevelFadeIn : MonoBehaviour
{
    [Header("Fade Ayarları")]
    public CanvasGroup fadeCanvasGroup; 
    public float fadeSuresi = 2.0f;       // Varsayılan süreyi 2 saniyeye çıkardık (Daha belirgin olsun)

    void Start()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.interactable = true;
            fadeCanvasGroup.blocksRaycasts = true;

            StartCoroutine(FadeInEfecti());
        }
    }

    IEnumerator FadeInEfecti()
    {
        // --- KRİTİK DOKUNUŞ ---
        // Unity sahneyi yüklerken oluşan o ilk karedeki zaman sıçramasını (lag) atlamak için 
        // döngü başlamadan önce tam 1 kare bekliyoruz. Zamanlama böylece kusursuz olacak.
        yield return null; 

        float gecenSure = 0f;

        while (gecenSure < fadeSuresi)
        {
            gecenSure += Time.deltaTime; 
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, gecenSure / fadeSuresi);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = false;
        
        fadeCanvasGroup.gameObject.SetActive(false);
    }
}