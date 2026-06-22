using UnityEngine;
using System.IO;
using System.Collections.Generic;

// İsimler projendeki diğer scriptlerle çakışmasın diye özel olarak adlandırıldı
[System.Serializable]
public class LevelMadalyaKaydi 
{
    public string levelAdi;
    public int bestMedal; // 0: Yok, 1: Bronz, 2: Gümüş, 3: Altın
}

[System.Serializable]
public class OyunGenelKaydi 
{
    public List<LevelMadalyaKaydi> leveller = new List<LevelMadalyaKaydi>();
}

public static class SaveManager
{
    private static string saveYolu = Application.persistentDataPath + "/oyunKaydi.json";

    public static OyunGenelKaydi KaydiYukle()
    {
        if (File.Exists(saveYolu))
        {
            string json = File.ReadAllText(saveYolu);
            return JsonUtility.FromJson<OyunGenelKaydi>(json);
        }
        else
        {
            // Dosya yoksa yeni ve boş bir kayıt oluştur
            return new OyunGenelKaydi();
        }
    }

    public static void MadalyaKaydet(string levelAdi, int kazanilanMadalya)
    {
        OyunGenelKaydi data = KaydiYukle();
        
        // Bu level daha önce kaydedilmiş mi diye kontrol et
        LevelMadalyaKaydi mevcutLevel = data.leveller.Find(l => l.levelAdi == levelAdi);

        if (mevcutLevel != null)
        {
            // Level zaten var, alınan madalya öncekinden büyükse güncelle
            if (kazanilanMadalya > mevcutLevel.bestMedal)
            {
                mevcutLevel.bestMedal = kazanilanMadalya;
            }
        }
        else
        {
            // Level ilk defa oynanıyor, listeye ekle
            LevelMadalyaKaydi yeniLevel = new LevelMadalyaKaydi();
            yeniLevel.levelAdi = levelAdi;
            yeniLevel.bestMedal = kazanilanMadalya;
            data.leveller.Add(yeniLevel);
        }

        // Güncel veriyi JSON olarak kaydet
        string guncelJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveYolu, guncelJson);
        
        Debug.Log("Oyun Kaydedildi! Yol: " + saveYolu);
    }
}