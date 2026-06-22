using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    void Awake()
    {
        // Oyunu her zaman 1920x1080 pencere modunda başlat
        // Böylece UI elementlerin (Anchor'ların sayesinde) her zaman doğru yerde kalır
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
        
        Debug.Log("Çözünürlük 1920x1080 olarak sabitlendi!");
    }
}