using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class KelimeSecici : MonoBehaviour
{
    private ToggleOlusturucu toggleOlusturucu;

    [System.Serializable]
    public class KelimeVeTanim
    {
        public string kelime;
        public string tanim;
    }

    [System.Serializable]
    public class KategoriListWrapper
    {
        public List<string> kategoriler = new List<string>();
    }

    [HideInInspector] public List<KelimeVeTanim> secilenKelimeVeTanimlar = new List<KelimeVeTanim>();
    [HideInInspector] public string selectedCategoryFileName;

    private StrictNeighborWordSearch strictNeighborWordSearchRef;

    private List<string> gosterilmisKategoriler = new List<string>();

void Awake()
{
    if (FindObjectsOfType<KelimeSecici>().Length > 1)
    {
        Debug.LogWarning("KelimeSecici: Sahnede birden fazla KelimeSecici bulundu. Bu obje yok ediliyor.");
        Destroy(gameObject);
        return;
    }

    DontDestroyOnLoad(gameObject);

    strictNeighborWordSearchRef = FindObjectOfType<StrictNeighborWordSearch>();
    if (strictNeighborWordSearchRef == null)
    {
        Debug.LogError("KelimeSecici: StrictNeighborWordSearch bulunamadı!");
    }

    Debug.Log("KelimeSecici: Başlatılıyor...");

    LoadShownCategories();

    if (LevelManager.currentChapter % 4 == 0 && LevelManager.currentChapter != 0)
    {
        gosterilmisKategoriler.Clear();
        SaveShownCategories();
        Debug.Log("Bölüm 4'e bölünebiliyor. Gösterilmiş kategoriler sıfırlandı.");
    }

    string loadedCategory = LevelSaveManager.LoadSelectedCategoryFileName();
    List<KelimeVeTanim> loadedWords = LevelSaveManager.LoadSelectedWords();
    int savedLevel = LevelSaveManager.LoadSavedLevel(); // Burada yüklenmişse, kaydedilen seviye

    // Toggle'dan güncel aktif türleri al
    string secilenTurlerStr = PlayerPrefs.GetString("SecilenTurler", "");
    List<string> aktifTurler = secilenTurlerStr
        .Split(',')
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrEmpty(s))
        .ToList();

    // Kayıtlı kategori artık aktif türlerde değilse veya seviye uyuşmuyorsa, temizle
    bool kategoriUygun = !string.IsNullOrEmpty(loadedCategory) && aktifTurler.Contains(loadedCategory);
    bool seviyeAyni = (savedLevel == LevelManager.currentLevel);

    if (!kategoriUygun || !seviyeAyni)
    {
        Debug.Log("Kayıtlı kategori uygun değil veya seviye uyuşmuyor. Yeni kelimeler yükleniyor.");
        loadedCategory = null;
        loadedWords.Clear();
        LevelSaveManager.SaveSelectedCategoryFileName("");
        LevelSaveManager.SaveSelectedWords(new List<KelimeVeTanim>());
    }

    if (!string.IsNullOrEmpty(loadedCategory) && loadedWords.Any())
    {
        selectedCategoryFileName = loadedCategory;
        secilenKelimeVeTanimlar = loadedWords;
        if (strictNeighborWordSearchRef != null)
        {
            strictNeighborWordSearchRef.hiddenWords = secilenKelimeVeTanimlar.Select(k => k.kelime).ToList();
        }
    }
    else
    {
        LoadNewWordsForLevel();
        LevelSaveManager.SaveSelectedCategoryFileName(selectedCategoryFileName);
        LevelSaveManager.SaveSelectedWords(secilenKelimeVeTanimlar);
        LevelSaveManager.SaveCurrentLevel(LevelManager.currentLevel); // Yeni seviyeyi de kaydet
    }
}


    public void LoadNewWordsForLevel()
    {
        List<KelimeVeTanim> tumKelimeler = new List<KelimeVeTanim>();

        if (LevelManager.currentLevel == 5)
        {
            TextAsset fiilDosya = Resources.Load<TextAsset>("Fiil/fiiller");
            if (fiilDosya == null)
            {
                Debug.LogError("KelimeSecici: 'Fiil/fiiller.txt' dosyası bulunamadı!");
                return;
            }

            selectedCategoryFileName = "fiiller";
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

            secilenKelimeVeTanimlar = tumKelimeler.OrderBy(x => Random.value).Take(4).ToList();
            if (strictNeighborWordSearchRef != null)
                strictNeighborWordSearchRef.hiddenWords = secilenKelimeVeTanimlar.Select(k => k.kelime).ToList();

            LevelSaveManager.SaveSelectedCategoryFileName(selectedCategoryFileName);
            LevelSaveManager.SaveSelectedWords(secilenKelimeVeTanimlar);
            Debug.Log("Seviye 5: fiiller.txt dosyasından kelimeler yüklendi.");
            return;
        }

        TextAsset[] dosyalar = Resources.LoadAll<TextAsset>("Türler");
        if (dosyalar == null || dosyalar.Length == 0)
        {
            Debug.LogError("KelimeSecici: 'Resources/Türler' klasöründe TXT dosyası bulunamadı!");
            return;
        }

        string secilenTurlerStr = PlayerPrefs.GetString("SecilenTurler", "");
        List<string> aktifTurler = secilenTurlerStr.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

        if (aktifTurler.Count > 0)
            dosyalar = dosyalar.Where(d => aktifTurler.Contains(d.name)).ToArray();

        TextAsset[] uygunKategoriler = dosyalar
            .Where(d => !gosterilmisKategoriler.Contains(d.name))
            .ToArray();

        if (uygunKategoriler.Length == 0)
        {
            Debug.Log("Tüm kategoriler daha önce gösterilmiş. Liste sıfırlanıyor...");
            gosterilmisKategoriler.Clear();
            SaveShownCategories();
            uygunKategoriler = dosyalar;
        }

        TextAsset hedefDosya = uygunKategoriler[Random.Range(0, uygunKategoriler.Length)];
        selectedCategoryFileName = hedefDosya.name;
        gosterilmisKategoriler.Add(selectedCategoryFileName);
        SaveShownCategories();
        Debug.Log("Seçilen kategori: " + selectedCategoryFileName);

        string[] satirlarNormal = hedefDosya.text.Split('\n');
        foreach (string satir in satirlarNormal)
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

int mevcutKelimeSayisi = tumKelimeler.Count;
int kelimeSayisi;

if (LevelManager.currentLevel == 1)
{
    kelimeSayisi = 3;
}
else if (LevelManager.currentLevel == 2 || LevelManager.currentLevel == 3)
{
    kelimeSayisi = 1;
}
else if (LevelManager.currentLevel == 4 || LevelManager.currentLevel == 5)
{
    kelimeSayisi = 4;
}
else
{
    kelimeSayisi = 4;
}

// Liste boyutu yetersizse uyar ve kalanları al
if (mevcutKelimeSayisi < kelimeSayisi)
{
    Debug.LogWarning($"Yeterli kelime yok! İstenen: {kelimeSayisi}, mevcut: {mevcutKelimeSayisi}. Mevcut tüm kelimeler seçiliyor.");
    kelimeSayisi = mevcutKelimeSayisi;
}

        secilenKelimeVeTanimlar = tumKelimeler.OrderBy(x => Random.value).Take(kelimeSayisi).ToList();

        if (strictNeighborWordSearchRef != null)
        {
            strictNeighborWordSearchRef.hiddenWords = secilenKelimeVeTanimlar.Select(k => k.kelime).ToList();
            Debug.Log("Seçilen kelimeler: " + string.Join(", ", strictNeighborWordSearchRef.hiddenWords));
        }
    }

    private void LoadShownCategories()
    {
        string json = PlayerPrefs.GetString("GosterilmisKategoriler", "{\"kategoriler\":[]}");
        gosterilmisKategoriler = JsonUtility.FromJson<KategoriListWrapper>(json).kategoriler;
    }

    private void SaveShownCategories()
    {
        KategoriListWrapper wrapper = new KategoriListWrapper { kategoriler = gosterilmisKategoriler };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("GosterilmisKategoriler", json);
        PlayerPrefs.Save();
    }


}
