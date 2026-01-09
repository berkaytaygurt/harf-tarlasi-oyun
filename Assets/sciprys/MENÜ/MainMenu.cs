using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // TMP kullanacaksan bu şart
using System.Collections.Generic;
using System.Linq;
public class MainMenu : MonoBehaviour
{
    public ToggleOlusturucu toggleOlusturucu; // Toggle'ları tutan script
    
    public Button loadButton;
    public Button silButton;
    public GameObject ayarlarPaneli; // Inspector'dan bu GameObject'e paneli sürükle
    public GameObject FarkliKelimelerPaneli;


    public TextMeshProUGUI bolumBilgiText;

    [System.Serializable]
    public class KelimeVeTanim
    {
        public string kelime;
        public string tanim;
    }


    public TextMeshProUGUI bilinmeyenKelimeText;
    public TextMeshProUGUI bilinmeyenTanimText; // Tanım için yeni TMP alanı
    [HideInInspector] public List<KelimeVeTanim> secilenKelimeVeTanimlar = new List<KelimeVeTanim>();
    [HideInInspector] public string selectedCategoryFileName;

    private StrictNeighborWordSearch strictNeighborWordSearchRef;


void Start()
{
    // Kayıt varsa Load butonunu aktif et
    loadButton.interactable = PlayerPrefs.HasKey("KayitliSeviye");

    int level = PlayerPrefs.GetInt("KayitliSeviye", 1);
    int chapter = PlayerPrefs.GetInt("KayitliBolum", 1);

    if (bolumBilgiText != null)
    {
        bolumBilgiText.text = "Oyna";
    }
}


    // Yeni oyun başlatır, varsa kayıt silinir
public void PlayGame()
{
    if (PlayerPrefs.HasKey("KayitliSeviye"))
    {
        PlayerPrefs.DeleteKey("KayitliSeviye");
        PlayerPrefs.DeleteKey("KayitliBolum");
    }

    LevelManager.currentLevel = 1;
    LevelManager.currentChapter = 1;

    // Seçilen türleri kaydet
    if (toggleOlusturucu != null)
    {
        toggleOlusturucu.KaydetSecilenTurler();
    }

    SceneManager.LoadScene("SampleScene");
}


    // Kayıtlı oyunu yükler ve devam eder
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("KayitliSeviye"))
        {
            LevelManager.currentLevel = LevelSaveManager.LoadLevel();
            LevelManager.currentChapter = LevelSaveManager.LoadChapter();
            Debug.Log($"Kayıt yüklendi: Level {LevelManager.currentLevel}, Chapter {LevelManager.currentChapter}");

            SceneManager.LoadScene("SampleScene"); // Oyun sahnesi adı buraya
        }
        else
        {
            Debug.LogWarning("Yüklenecek kayıt bulunamadı!");
            // İstersen buraya uyarı veya yeni oyun başlatma kodu ekle
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void PlayOrLoadGame()
{
    if (PlayerPrefs.HasKey("KayitliSeviye"))
    {
        // Kayıt varsa, yükle
        LevelManager.currentLevel = LevelSaveManager.LoadLevel();
        LevelManager.currentChapter = LevelSaveManager.LoadChapter();
        Debug.Log($"Kayıt bulundu: Level {LevelManager.currentLevel}, Chapter {LevelManager.currentChapter}");
    }
    else
    {
        // Kayıt yoksa yeni oyun başlat
        LevelManager.currentLevel = 1;
        LevelManager.currentChapter = 1;
        Debug.Log("Kayıt bulunamadı, yeni oyun başlatılıyor.");
    }

    SceneManager.LoadScene("SampleScene"); // Oyun sahnenin adı
}

public void ResetGameData()
{
    PlayerPrefs.DeleteAll(); // Tüm kayıtları siler
    PlayerPrefs.Save();
    Debug.Log("Tüm oyun verileri sıfırlandı.");

    // İsteğe bağlı: Kullanıcıyı başa atmak için sahneyi yeniden yükleyebilirsin:
   
}

// Ayarlar panelini açar
public void OpenSettings()
{
    if (ayarlarPaneli != null)
    {
        ayarlarPaneli.SetActive(true);
    }
}

// Ayarlar panelini kapatır
public void CloseSettings()
{
    if (ayarlarPaneli != null)
    {
        ayarlarPaneli.SetActive(false);
    }
}

    // Ayarlar panelini açar
    public void OpenKelimeler()
    {
        if (FarkliKelimelerPaneli != null)
        {
            FarkliKelimelerPaneli.SetActive(true);
        }
    }

    // Ayarlar panelini kapatır
    public void CloseKelimeler()
    {
        if (FarkliKelimelerPaneli != null)
        {
            FarkliKelimelerPaneli.SetActive(false);
        }
    }

    public void BilinmeyenKelimeler()
    {
        List<KelimeVeTanim> tumKelimeler = new List<KelimeVeTanim>();
        TextAsset fiilDosya = Resources.Load<TextAsset>("Fiil/Kelimeler");

        if (fiilDosya == null)
        {
            Debug.LogError("KelimeSecici: 'Fiil/Kelimeler.txt' dosyası bulunamadı!");
            return;
        }

        selectedCategoryFileName = "Kelimeler";
        string[] satirlar = fiilDosya.text.Split('\n');

        foreach (string satir in satirlar)
        {
            string trimmed = satir.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            string[] parcalar = trimmed.Split('=');
            if (parcalar.Length >= 2)
            {
                tumKelimeler.Add(new KelimeVeTanim
                {
                    kelime = parcalar[0].Trim().ToUpper(),
                    tanim = parcalar[1].Trim()
                });
            }
        }

        secilenKelimeVeTanimlar = tumKelimeler.OrderBy(x => Random.value).Take(1).ToList();

        if (secilenKelimeVeTanimlar.Count > 0)
        {
            var secilen = secilenKelimeVeTanimlar[0];
            bilinmeyenKelimeText.text = secilen.kelime;
            bilinmeyenTanimText.text = secilen.tanim;
        }
        else
        {
            bilinmeyenKelimeText.text = "Kelime yok";
            bilinmeyenTanimText.text = "Tanım yok";
        }
    }
}
