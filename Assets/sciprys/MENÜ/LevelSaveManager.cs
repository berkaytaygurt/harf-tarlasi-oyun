using UnityEngine;
using System.Collections.Generic; // List için
using System.Linq; // Eğer string.Join veya Split kullanılıyorsa (JSON ile gerek kalmayacak)

public static class LevelSaveManager
{
    private const string LEVEL_KEY = "KayitliSeviye";
    private const string CHAPTER_KEY = "KayitliBolum";
    private const string JOKER_HAKKI_KEY = "KayitliJokerHakki";
    private const string KATEGORI_DEGISTIRME_HAKKI_KEY = "KayitliKategoriDegistirmeHakki";
    private const string FOUND_WORDS_KEY = "KayitliBulunanKelimeler"; 

        // LevelSaveManager.cs içine ekleyin
    private const string SELECTED_CATEGORY_KEY = "KayitliKategoriAdi";
    private const string SELECTED_WORDS_KEY = "KayitliSecilenKelimeler";

    
    // Yardımcı sınıf: List<string> tipini PlayerPrefs'e JsonUtility ile kaydetmek için.
    // Unity'nin JsonUtility'si doğrudan List<T> kaydetmeyi desteklemez, bir sınıf sarmalayıcıya ihtiyaç duyar.
    [System.Serializable]
    private class StringListWrapper
    {
        public List<string> list;
    }

    public static void SaveLevel(int level)
    {
        PlayerPrefs.SetInt(LEVEL_KEY, level);
        PlayerPrefs.Save();
        Debug.Log($"Seviye kaydedildi: {level}");
    }

    public static int LoadLevel()
    {
        // Varsayılan değer olarak 1'i kullanıyoruz. LevelManager.currentLevel henüz atanmamış olabilir.
        return PlayerPrefs.GetInt(LEVEL_KEY, 1); 
    }

    public static void SaveChapter(int chapter)
    {
        PlayerPrefs.SetInt(CHAPTER_KEY, chapter);
        PlayerPrefs.Save();
        Debug.Log($"Bölüm kaydedildi: {chapter}");
    }

    public static int LoadChapter()
    {
        return PlayerPrefs.GetInt(CHAPTER_KEY, 1); // Varsayılan bölüm 1
    }

    public static void SaveJokerHakki(int hakki)
    {
        PlayerPrefs.SetInt(JOKER_HAKKI_KEY, hakki);
        PlayerPrefs.Save();
        Debug.Log($"Joker hakkı kaydedildi: {hakki}");
    }

    public static int LoadJokerHakki()
    {
        // Varsayılan joker hakkı 5
        return PlayerPrefs.GetInt(JOKER_HAKKI_KEY, 10); 
    }

    public static void SaveKategoriDegistirmeHakki(int hakki)
    {
        PlayerPrefs.SetInt(KATEGORI_DEGISTIRME_HAKKI_KEY, hakki);
        PlayerPrefs.Save();
        Debug.Log($"Kategori değiştirme hakkı kaydedildi: {hakki}");
    }

    public static int LoadKategoriDegistirmeHakki()
    {
        // Varsayılan kategori değiştirme hakkı 3
        return PlayerPrefs.GetInt(KATEGORI_DEGISTIRME_HAKKI_KEY, 3); 
    }

    public static void SaveFoundWords(List<string> foundWords)
    {
        // Listeyi bir sarmalayıcı objeye koyup JSON'a çevir
        StringListWrapper wrapper = new StringListWrapper { list = foundWords };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(FOUND_WORDS_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Bulunan kelimeler kaydedildi: " + json);
    }

    public static List<string> LoadFoundWords()
    {
        // Kayıtlı JSON string'ini al. Yoksa boş bir liste JSON'ı ile varsayılan değer ver.
        string json = PlayerPrefs.GetString(FOUND_WORDS_KEY, "{\"list\":[]}"); 
        StringListWrapper wrapper = JsonUtility.FromJson<StringListWrapper>(json);
        
        // Eğer wrapper veya list null ise boş bir liste döndür
        if (wrapper == null || wrapper.list == null)
        {
            return new List<string>();
        }
        Debug.Log("Bulunan kelimeler yüklendi: " + json);
        return wrapper.list;
    }
    // Unity'nin JsonUtility'si doğrudan List<T> kaydetmeyi desteklemez, bir sınıf sarmalayıcıya ihtiyaç duyar.
[System.Serializable]
public class KelimeVeTanimListWrapper // Public olmalı ki KelimeSecici içinden erişebilelim
{
    public List<KelimeSecici.KelimeVeTanim> list; // KelimeSecici'nin iç sınıfına referans veriyoruz
}

// Yeni metotlar:
public static void SaveSelectedCategoryFileName(string fileName)
{
    PlayerPrefs.SetString(SELECTED_CATEGORY_KEY, fileName);
    PlayerPrefs.Save();
    Debug.Log($"Seçilen kategori adı kaydedildi: {fileName}");
}

public static string LoadSelectedCategoryFileName()
{
    return PlayerPrefs.GetString(SELECTED_CATEGORY_KEY, ""); // Varsayılan boş string
}

public static void SaveSelectedWords(List<KelimeSecici.KelimeVeTanim> words)
{
    KelimeVeTanimListWrapper wrapper = new KelimeVeTanimListWrapper { list = words };
    string json = JsonUtility.ToJson(wrapper);
    PlayerPrefs.SetString(SELECTED_WORDS_KEY, json);
    PlayerPrefs.Save();
    Debug.Log("Seçilen kelimeler kaydedildi: " + json);
}

public static List<KelimeSecici.KelimeVeTanim> LoadSelectedWords()
{
    string json = PlayerPrefs.GetString(SELECTED_WORDS_KEY, "{\"list\":[]}");
    KelimeVeTanimListWrapper wrapper = JsonUtility.FromJson<KelimeVeTanimListWrapper>(json);
    if (wrapper == null || wrapper.list == null)
    {
        return new List<KelimeSecici.KelimeVeTanim>();
    }
    Debug.Log("Seçilen kelimeler yüklendi: " + json);
    return wrapper.list;
}

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(LEVEL_KEY);
        PlayerPrefs.DeleteKey(CHAPTER_KEY);
        PlayerPrefs.DeleteKey(JOKER_HAKKI_KEY);
        PlayerPrefs.DeleteKey(KATEGORI_DEGISTIRME_HAKKI_KEY);
        PlayerPrefs.DeleteKey(FOUND_WORDS_KEY);

       
        PlayerPrefs.DeleteKey(SELECTED_CATEGORY_KEY);
        PlayerPrefs.DeleteKey(SELECTED_WORDS_KEY);
        PlayerPrefs.Save();
        Debug.Log("Tüm oyun ilerlemesi (seviye, bölüm, joker, kategori hakkı ve bulunan kelimeler) sıfırlandı.");
    }
        public static int LoadSavedLevel()
{
    return PlayerPrefs.GetInt("SavedLevel", -1); // -1: hiç kaydedilmemiş
}

public static void SaveCurrentLevel(int level)
{
    PlayerPrefs.SetInt("SavedLevel", level);
    PlayerPrefs.Save();
}
}