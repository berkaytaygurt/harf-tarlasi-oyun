using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening; // DOTween kütüphanesini kullanmak için
public class LevelManager : MonoBehaviour
{
    public static int currentChapter = 1;
    public static int currentLevel = 1; 
    public GameObject panel;             // Panel referansı (İlerle butonu bu panelin içinde olmalı)
    public Button levelUpButton;         // Buton referansı

    private StrictNeighborWordSearch wordSearch;
    private GridRenderer gridRenderer; 
    private KelimeSecici kelimeSecici; // YENİ: KelimeSecici referansı

    public GecPaneli gecPaneli;  // Inspector'dan atanacak
    

void Start()
{
    currentLevel = LevelSaveManager.LoadLevel();
    currentChapter = LevelSaveManager.LoadChapter();
    wordSearch = FindObjectOfType<StrictNeighborWordSearch>();
    gridRenderer = FindObjectOfType<GridRenderer>(); 
    kelimeSecici = FindObjectOfType<KelimeSecici>(); 

    if (wordSearch == null)
    {
        //Debug.LogError("LevelManager: StrictNeighborWordSearch bulunamadı!");
        return;
    }
    if (gridRenderer == null)
    {
        //Debug.LogError("LevelManager: GridRenderer bulunamadı!");
        return;
    }
    if (kelimeSecici == null)
    {
        //Debug.LogError("LevelManager: KelimeSecici bulunamadı!");
        return;
    }

    gridRenderer.WordSearch = wordSearch;

    if (panel != null)
        panel.SetActive(false);

    if (levelUpButton != null)
    {
        levelUpButton.onClick.RemoveAllListeners(); 
        levelUpButton.onClick.AddListener(SeviyeArttir);
    }

    // 🔽 EKLE: Tüm hint butonlarını kapat
    WordSelectionManager selectionManager = FindObjectOfType<WordSelectionManager>();
    if (selectionManager != null)
    {
        if (selectionManager.hintButton1 != null) selectionManager.hintButton1.gameObject.SetActive(false);
        if (selectionManager.hintButton2 != null) selectionManager.hintButton2.gameObject.SetActive(false);
        if (selectionManager.hintButton3 != null) selectionManager.hintButton3.gameObject.SetActive(false);
        if (selectionManager.hintButton4 != null) selectionManager.hintButton4.gameObject.SetActive(false);
    }

    // 🔽 Ardından seviyeye göre ayarları yap
    SetGridDimensionsForCurrentLevel();

    wordSearch.InitializeGrid(); 
    gridRenderer.GenerateGrid(); 
}

public void SeviyeArttir()
{
    WordSelectionManager selectionManager = FindObjectOfType<WordSelectionManager>();
    currentLevel++;
    
        // Her 5 seviyede chapter 1 art, seviye 1'e dön
    if (currentLevel > 5)
    {
        currentLevel = 1;
        selectionManager.kategoriDegistirmeHakki = 3;
        selectionManager.kategoriDegistirButtonText.text = $"DEĞİŞTİR ({selectionManager.kategoriDegistirmeHakki})";
        currentChapter++;
        
        LevelSaveManager.SaveChapter(currentChapter); // <- burası eklendi
        Debug.Log($"Yeni bölüme geçildi! Chapter: {currentChapter}");
        
    }

Debug.Log($"LoadLevel döndürüyor: {LevelSaveManager.LoadLevel()}");


    Debug.Log($"Yeni seviye kaydediliyor: {currentLevel}");
    LevelSaveManager.SaveLevel(currentLevel);



    FindObjectOfType<WordSelectionManager>().ResetForNewLevel();
    if (panel != null) panel.SetActive(false);

    // 2. WordSelectionManager'daki listeleri temizle
    
    if (selectionManager != null)
    {
        selectionManager.ResetFoundWords();
        //selectionManager.selectedCells.Clear();
        //selectionManager.selectedButtons.Clear();
        //selectionManager.undoStack.Clear();
        selectionManager.UpdateUI();
    }


    // 1. Grid boyutunu güncelle (ÖNCE bu yapılmalı)
    SetGridDimensionsForCurrentLevel();
    Debug.Log($"Seviye {currentLevel} için grid boyutu ayarlandı: {wordSearch.gridWidth}x{wordSearch.gridHeight}");


    // 3. Yeni kelimeleri yükle
    if (kelimeSecici == null) kelimeSecici = FindObjectOfType<KelimeSecici>();
    if (kelimeSecici != null)
    {
        kelimeSecici.LoadNewWordsForLevel();
        // WordSelectionManager'daki totalWords'i güncelle
        selectionManager.totalWords = kelimeSecici.secilenKelimeVeTanimlar.Count;
        Debug.Log($"Yeni kelimeler yüklendi. Toplam kelime: {selectionManager.totalWords}");
    }
    selectionManager.fileNameTMP.text = $"<size=24><b>Kategori:</b> {kelimeSecici.selectedCategoryFileName.ToUpper()}</size>";
    Debug.Log($"Kategori güncellendi: {kelimeSecici.selectedCategoryFileName}");        
    

    // 4. Grid'i YENİDEN OLUŞTUR (ÖNEMLİ!)
    if (wordSearch != null)
    {
        wordSearch.InitializeGrid(); // Grid verilerini yenile
    }

    if (gridRenderer != null)
    {
        gridRenderer.GenerateGrid(); // Fiziksel gridi yeniden oluştur
        // Butonları resetle

    }

    Debug.Log($"Seviye {currentLevel} başarıyla başladı. Grid boyutu: {wordSearch.gridWidth}x{wordSearch.gridHeight}");
}

    private void SetGridDimensionsForCurrentLevel()
    {

         WordSelectionManager selectionManager = FindObjectOfType<WordSelectionManager>();
         //selectionManager.UpdateFoundWordsDisplay();
        if (currentLevel == 1)
        {   
            selectionManager.SeviyeBaslangiciAnimasyonu(currentChapter);
            selectionManager.foundWordsTMP.text = ($"<b>BULUNDU: {selectionManager.foundWords.Count}/{3}</b>\n");
            wordSearch.gridWidth = 6;
            wordSearch.gridHeight = 6;
            Debug.Log($"Seviye {currentLevel}: Grid boyutu 6x6 olarak ayarlandı.");
            selectionManager.kategoriDegistirButton.gameObject.SetActive(true);
            selectionManager.hintOnlyText.gameObject.SetActive(false);

            if (selectionManager.hintButton1 != null) selectionManager.hintButton1.gameObject.SetActive(true);
            if (selectionManager.hintButton2 != null) selectionManager.hintButton2.gameObject.SetActive(true);
            if (selectionManager.hintButton3 != null) selectionManager.hintButton3.gameObject.SetActive(true);
            if (selectionManager.hintButton4 != null) selectionManager.hintButton4.gameObject.SetActive(false); // ❌ ARTIK GİZLİ
        }
        else if (currentLevel == 2 ||currentLevel == 3)
        {       
                selectionManager.foundWordsTMP.text = ($"<b>BULUNDU: {selectionManager.foundWords.Count}/{1}</b>\n");
                selectionManager.kategoriDegistirButton.gameObject.SetActive(true);
                if (selectionManager.hintButton1 != null) selectionManager.hintButton1.gameObject.SetActive(true);
                if (selectionManager.hintButton2 != null) selectionManager.hintButton2.gameObject.SetActive(false);
                if (selectionManager.hintButton3 != null) selectionManager.hintButton3.gameObject.SetActive(false);
                if (selectionManager.hintButton4 != null) selectionManager.hintButton4.gameObject.SetActive(false);

                
                if (selectionManager.displayedWordText != null) 
                selectionManager.displayedWordText.gameObject.SetActive(false);
                selectionManager.hintOnlyText.gameObject.SetActive(true);

            if(currentLevel == 2){
                wordSearch.gridWidth = 7;
                wordSearch.gridHeight = 2;
            }
            else{
                wordSearch.gridWidth = 4;
                wordSearch.gridHeight = 4;                
            }

            // 0.4 saniye sonra harf hakkını artır ve rastgele harf göster
            DOVirtual.DelayedCall(0.5f, () =>
            {
                selectionManager.letterHintHakki++;
                selectionManager.ShowRandomLetterHint();
            });
            
        }
        else if (currentLevel == 4)
        {
                selectionManager.foundWordsTMP.text = ($"<b>BULUNDU: {selectionManager.foundWords.Count}/{4}</b>\n");
                selectionManager.kategoriDegistirButton.gameObject.SetActive(true);
                if (selectionManager.hintButton1 != null) selectionManager.hintButton1.gameObject.SetActive(true);
                if (selectionManager.hintButton2 != null) selectionManager.hintButton2.gameObject.SetActive(true);
                if (selectionManager.hintButton3 != null) selectionManager.hintButton3.gameObject.SetActive(true);
                if (selectionManager.hintButton4 != null) selectionManager.hintButton4.gameObject.SetActive(true);

                selectionManager.hintOnlyText.gameObject.SetActive(false);
                if (selectionManager.displayedWordText != null) selectionManager.displayedWordText.gameObject.SetActive(true);
            wordSearch.gridWidth = 7;
            wordSearch.gridHeight = 8;
            Debug.Log($"Seviye {currentLevel}: Grid boyutu 7x8 olarak ayarlandı.");
        }
        else // currentLevel >= 3
        {
                selectionManager.foundWordsTMP.text = ($"<b>BULUNDU: {selectionManager.foundWords.Count}/{4}</b>\n");
                selectionManager.kategoriDegistirButton.gameObject.SetActive(true);
                if (selectionManager.hintButton1 != null) selectionManager.hintButton1.gameObject.SetActive(true);
                if (selectionManager.hintButton2 != null) selectionManager.hintButton2.gameObject.SetActive(true);
                if (selectionManager.hintButton3 != null) selectionManager.hintButton3.gameObject.SetActive(true);
                if (selectionManager.hintButton4 != null) selectionManager.hintButton4.gameObject.SetActive(true);

                selectionManager.hintOnlyText.gameObject.SetActive(false);
                if (selectionManager.displayedWordText != null) selectionManager.displayedWordText.gameObject.SetActive(true);

            wordSearch.gridWidth = 8;
            wordSearch.gridHeight = 9;
            Debug.Log($"Seviye {currentLevel}: Grid boyutu 8x10 olarak ayarlandı.");
        }
    }

    public void ShowLevelUpButton()
    {
        if (panel != null) 
        {
            panel.SetActive(true);
            Debug.Log("LevelManager: 'İlerle' paneli aktif edildi (Buton ile birlikte).");
        }
    }
            public static void LoadProgress()
    {
        currentLevel = LevelSaveManager.LoadLevel();
        currentChapter = LevelSaveManager.LoadChapter();
    }

}