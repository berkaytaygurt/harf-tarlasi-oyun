using UnityEngine;
using TMPro;
using UnityEngine.UI; // HorizontalLayoutGroup için gerekli
public class GridYazici : MonoBehaviour
{
public Transform ortakGrid;
public Transform Oyna;
public Transform Ayarlar;
public Transform Tutorial;
public Transform Sorun;



public GameObject harfKutusuPrefab;

void Start()
{
    int bolum = PlayerPrefs.GetInt("KayitliSeviye", 1);
    int seviye = PlayerPrefs.GetInt("KayitliBolum", 1);

    // Yazıların hepsi tek diziye
    string[] gridYazilari = {
        //"KE    LU Lİ  YO    ME   ",
        //"        ",
        "SEVİYE" + seviye.ToString().PadLeft(2, ' '), // örn: SEVİYE 1
        "BÖLÜM " + bolum.ToString().PadLeft(2, ' ')  // örn: BÖLÜM 2
    };

    string[] oyna = {"OYNA"};
    string[] ayarlaryazı = {"AYARLAR"};
    string[] tut = {"KILAVUZ"};
    string[] sor = {"YARDIMDESTEK"};



    foreach (string satir in gridYazilari)
    {
        foreach (char harf in satir)
        {
            GameObject kutu = Instantiate(harfKutusuPrefab, ortakGrid);
            kutu.GetComponentInChildren<TextMeshProUGUI>().text = harf.ToString();
        }
    }
    
    foreach (string satir in oyna)
    {
        foreach (char harf in satir)
        {
            GameObject kutu = Instantiate(harfKutusuPrefab, Oyna);
            kutu.GetComponentInChildren<TextMeshProUGUI>().text = harf.ToString();
        }
    }

    foreach (string satir in ayarlaryazı)
    {
        foreach (char harf in satir)
        {
            GameObject kutu = Instantiate(harfKutusuPrefab, Ayarlar);
            kutu.GetComponentInChildren<TextMeshProUGUI>().text = harf.ToString();
        }
    }
    foreach (string satir in tut)
    {
        foreach (char harf in satir)
        {
            GameObject kutu = Instantiate(harfKutusuPrefab, Tutorial);
            kutu.GetComponentInChildren<TextMeshProUGUI>().text = harf.ToString();
        }
    }    
    foreach (string satir in sor)
    {
        foreach (char harf in satir)
        {
            GameObject kutu = Instantiate(harfKutusuPrefab, Sorun);
            kutu.GetComponentInChildren<TextMeshProUGUI>().text = harf.ToString();
        }
    }

}

}
