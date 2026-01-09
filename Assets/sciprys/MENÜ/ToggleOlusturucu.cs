using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ToggleOlusturucu : MonoBehaviour
{
    public GameObject togglePrefab; // TMP kullanan Toggle prefabı
    public Transform toggleParent;  // ScrollView içindeki Content alanı

    private List<Toggle> olusanTogglelar = new List<Toggle>();

void Start()
{
    // Tür klasöründen .txt dosyalarını al
    TextAsset[] turDosyalari = Resources.LoadAll<TextAsset>("Türler");

    // PlayerPrefs'tan seçilen türleri oku
    string secilenTurlerStr = PlayerPrefs.GetString("SecilenTurler", "");
    List<string> secilenTurler = secilenTurlerStr.Split(',')
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrEmpty(s))
        .ToList();

    // Eğer hiç kayıt yoksa (oyun ilk kez açılmış demektir) → hepsini tikli kabul et
    bool varsayilanTikli = secilenTurler.Count == 0;

    foreach (TextAsset dosya in turDosyalari)
    {
        GameObject yeniToggle = Instantiate(togglePrefab, toggleParent);
        yeniToggle.name = dosya.name;

        TMP_Text tmpText = yeniToggle.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = dosya.name;
        }
        else
        {
            Debug.LogWarning("TMP_Text bileşeni bulunamadı!");
        }

        Toggle toggleComp = yeniToggle.GetComponent<Toggle>();
        toggleComp.isOn = varsayilanTikli || secilenTurler.Contains(dosya.name); // ← burası önemli

        olusanTogglelar.Add(toggleComp);
    }
}


    public List<string> GetAktifTurler()
    {
        List<string> aktifTurler = new List<string>();
        foreach (Toggle t in olusanTogglelar)
        {
            if (t.isOn)
                aktifTurler.Add(t.name); // Toggle'un adı, dosya adı ile aynı
        }
        return aktifTurler;
    }

    public void KaydetSecilenTurler()
    {
        List<string> secilenler = GetAktifTurler();
        string secilenStr = string.Join(",", secilenler);
        PlayerPrefs.SetString("SecilenTurler", secilenStr);
        PlayerPrefs.Save();
        Debug.Log("Seçilen türler kaydedildi: " + secilenStr);
    }
}
