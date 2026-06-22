using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButtonAnims : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Ayarları")]
    public Vector3 normalScale = Vector3.one;
    public Vector3 hoverScale = new Vector3(1.15f, 1.15f, 1.15f);
    public float animationDuration = 0.15f;

    private Coroutine scaleCoroutine;
    private bool isHiding = false; // Kilit mekanizması

    void OnEnable()
    {
        transform.localScale = normalScale;
        isHiding = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHiding) return; // Gizleniyorsa büyümeye çalışma
        TriggerScale(hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isHiding) return; // Gizleniyorsa eski haline dönmeye çalışma
        TriggerScale(normalScale);
    }

    public void Appear()
    {
        isHiding = false; // Kilidi aç
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        TriggerScale(normalScale);
    }

    public void Disappear()
    {
        if (!gameObject.activeInHierarchy || isHiding) return;
        isHiding = true; // Kilidi kapat ki fare etkileşime giremesin
        TriggerScale(Vector3.zero, deactivateAtEnd: true);
    }

    private void TriggerScale(Vector3 targetScale, bool deactivateAtEnd = false)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale, deactivateAtEnd));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale, bool deactivateAtEnd)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / animationDuration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;

        if (deactivateAtEnd)
        {
            gameObject.SetActive(false);
        }
    }
}