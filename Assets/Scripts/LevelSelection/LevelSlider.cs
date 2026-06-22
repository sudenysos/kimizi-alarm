using UnityEngine;
using UnityEngine.UI;

public class LevelSlider : MonoBehaviour
{
    [Header("Sayfa Panelleri")]
    [Tooltip("Sahnede duran sayfa panellerini sırasıyla (Page_A, Page_B, Page_C) buraya sürükleyin.")]
    public RectTransform[] pages; // Animasyon için GameObject yerine RectTransform kullanıyoruz

    [Header("Kaydırma Ayarları")]
    public float slideSpeed = 10f; // Kayma animasyonunun hızı (Yüksek değer = daha hızlı)
    public float pageSpacing = 1920f; // Sayfalar arası piksel mesafesi (Ekran genişliği)

    [Header("Gezinme Butonları")]
    public Button btnPrevious;
    public Button btnNext;

    private int currentPageIndex = 0;

    void Start()
    {
        // Oyun başladığında sayfaların hepsini açık tut ve başlangıç pozisyonlarına jilet gibi yerleştir
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].gameObject.SetActive(true); // Artık sayfalar hiç kapanmayacak
                
                // İlk anda animasyonsuz olarak direkt yerlerine oturt (A=0, B=1920, C=3840)
                float targetX = (i - currentPageIndex) * pageSpacing;
                pages[i].anchoredPosition = new Vector2(targetX, pages[i].anchoredPosition.y);
            }
        }
        UpdateButtons();
    }

    void Update()
    {
        // Her karede (frame) sayfaları yumuşak bir şekilde (Lerp) hedeflerine doğru kaydır
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                float targetX = (i - currentPageIndex) * pageSpacing;
                Vector2 targetPosition = new Vector2(targetX, pages[i].anchoredPosition.y);
                
                pages[i].anchoredPosition = Vector2.Lerp(pages[i].anchoredPosition, targetPosition, Time.deltaTime * slideSpeed);
            }
        }
    }

    public void NextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            UpdateButtons();
        }
    }

    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        // Butonları tamamen gizleyip/gösteren eski sisteme geri döndük
        if (btnPrevious != null) 
        {
            btnPrevious.gameObject.SetActive(currentPageIndex > 0);
        }

        if (btnNext != null) 
        {
            btnNext.gameObject.SetActive(currentPageIndex < pages.Length - 1);
        }
    }
}