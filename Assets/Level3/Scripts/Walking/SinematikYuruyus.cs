using UnityEngine;

public class SinematikYuruyus : MonoBehaviour
{
    [Header("Yürüyüş Ayarları")]
    [Tooltip("Karakterin saniyede kaç birim ileri gideceğini belirler.")]
    public float yurumeHizi = 1.2f; 

    void Update()
    {
        // Karakteri her karede, baktığı kendi ileri (Z) yönünde hareket ettirir
        transform.Translate(Vector3.forward * yurumeHizi * Time.deltaTime);
    }
}