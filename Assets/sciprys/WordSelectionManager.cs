using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullanıyorsanız bu gereklidir
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DG.Tweening; // DOTween kütüphanesini kullanmak için
using UnityEngine.SceneManagement;

public class WordSelectionManager : MonoBehaviour
{
    [Header("Game Settings")]
    public Color selectedColor = new Color(1f, 0.8f, 0f); // Seçilen hücre rengi (Sarı)
    public Color completedWordColor = new Color(0f, 1f, 0f); // Tamamlanan kelime rengi (Yeşil)
    public int wordScore = 100; // Kelime başına verilecek temel skor
    public int timeBonusDivider = 50; // Zaman bonusu hesaplaması için bölen
    public float countdownTime = 180f; // Oyunun geri sayım süresi (3 dakika)

    public int kategoriDegistirmeHakki = 3; // Her chapter başında 3 hak verilecek
    public TextMeshProUGUI kategoriDegistirButtonText;

    // EKLENDİ: Hint Settings başlığı altında harf ipucu hakları
    [Header("Hint Settings")] 
    public int letterHintHakki = 10; // Her seviye başında 5 hak verilecek



    public TextMeshProUGUI letterHintButtonText; // Harf al butonunun hakkını gösterecek TextMeshProUGUI

    [Header("UI References")]
    public TextMeshProUGUI selectedLettersTMP; // Seçilen harfleri gösteren metin alanı
    public TextMeshProUGUI timerTMP; // Geçen süreyi gösteren metin alanı
    public TextMeshProUGUI scoreTMP; // Skoru gösteren metin alanı
    public TextMeshProUGUI foundWordsTMP; // Bulunan kelimeleri gösteren metin alanı
    public TextMeshProUGUI countdownTMP; // Geri sayım sayacını gösteren metin alanı
    public Button undoButton; // Geri al butonu
    public Button clearButton; // Temizle butonu
    public GameObject gameOverPanel; // Oyun bitti paneli

    [SerializeField] public TextMeshProUGUI resultTextTMP;

     private AudioManager audioManager; // AudioManager referansı

    [Header("Word Hint Buttons")] 
    [SerializeField] public Button hintButton1; // 1. ipucu butonu
    [SerializeField] public Button hintButton2; // 2. ipucu butonu
    [SerializeField] public Button hintButton3; // 3. ipucu butonu
    [SerializeField] public Button hintButton4; // 4. ipucu butonu

    [SerializeField] public Button letterHintButton; // Harf göster butonu
    private Dictionary<Button, int> buttonToWordIndex = new Dictionary<Button, int>();
    private Dictionary<int, HashSet<int>> revealedLettersByWord = new Dictionary<int, HashSet<int>>();
    // Sadece ipucu harf dizilimini tutmak için
    private string currentHintString = "";

    // Dilersen başka bir UI objesinde sadece harf ipucunu göstermek için
    public TextMeshProUGUI hintOnlyText; 

    
    [SerializeField] public Button kategoriDegistirButton;
    [SerializeField] public Button GecButonu;
    [SerializeField] public TextMeshProUGUI geriSayimText;


    public TextMeshProUGUI displayedWordText; // İpucu kelimesinin/tanımının gösterileceği metin alanı
    public TextMeshProUGUI fileNameTMP; // Seçilen dosya adını gösteren metin alanı

    // EKLENDİ: GridRenderer referansı
    [Header("Component References")] 
    [SerializeField] private GridRenderer gridRenderer; // GridRenderer referansı

    // Oyun durumu değişkenleri
    private StrictNeighborWordSearch wordSearch; // Kelime arama mantığını yöneten sınıf
    private KelimeSecici kelimeSecici; // Kelime ve tanım çiftlerini tutan sınıf
    private List<Vector2Int> selectedCells = new List<Vector2Int>(); // Seçili hücrelerin pozisyonları
    private List<Button> selectedButtons = new List<Button>(); // Seçili butonların referansları
    private Stack<List<Button>> undoStack = new Stack<List<Button>>(); // Geri alma işlevi için yığın
    public List<string> foundWords = new List<string>(); // Bulunan kelimelerin listesi
    
    private float gameTime = 0f; // Oyunun başladığından beri geçen süre
    private float SeviyeGameTime = 0f;

    private float remainingTime; // Geri sayımda kalan süre
    public bool isGameActive = true; // Oyunun aktif olup olmadığını belirtir
    private int score = 0; // Oyuncunun skoru
    public int totalWords; // Toplam gizli kelime sayısı

    private Button lastClickedHintButton; // Son tıklanan ipucu butonunu takip eder

    // Yeni eklendi: Butonların başlangıç pozisyonlarını saklamak için
    private Dictionary<Button, Vector3> initialButtonPositions = new Dictionary<Button, Vector3>();
    // Yeni eklendi: Butonların kayacağı mesafeler ve animasyon süresi
    [SerializeField] private float selectedButtonOffset = -1f; // Tıklanan butonun sola kayma miktarı
    [SerializeField] private float otherButtonsOffset = 0f;    // Diğer butonların sağa kayma miktarı
    [SerializeField] private float buttonMoveDuration = 0.3f;    // Animasyon süresi
    

    [SerializeField] public TextMeshProUGUI seviyeBaslikText;
    [SerializeField] public Image transitionPanel; // Arkadaki panel

    [Header("Debug/Cheat")]
    public Button completeLevelDebugButton; // Inspector'dan atayacağınız yeni debug butonu
    

    public Transform ortakGrid; // HorizontalLayoutGroup veya VerticalLayoutGroup (ana panel)
    public GameObject harfKutusuPrefab; // Harf kutusu prefabı (içinde TMP olmalı)
    

void Start()
    {
        Application.targetFrameRate = 80;
        Debug.Log("WordSelectionManager: Başlatılıyor...");

        wordSearch = FindObjectOfType<StrictNeighborWordSearch>();
        kelimeSecici = FindObjectOfType<KelimeSecici>();
        audioManager = FindObjectOfType<AudioManager>();
        
        LevelManager.LoadProgress();
        Debug.Log("Yüklenen bölüm: " + LevelManager.currentChapter);
        
        if (gridRenderer == null)
        {
            gridRenderer = FindObjectOfType<GridRenderer>();
            if (gridRenderer == null)
            {
                //Debug.LogError("WordSelectionManager: GridRenderer sahnede bulunamadı! Lütfen atayın veya var olduğundan emin olun.");
                isGameActive = false;
                return;
            }
        }

        if (wordSearch == null)
        {
            //Debug.LogError("WordSelectionManager: StrictNeighborWordSearch bulunamadı! Lütfen sahnenizde bu bileşenin olduğundan emin olun.");
            isGameActive = false;
            return;
        }

        if (kelimeSecici == null)
        {
            //Debug.LogError("WordSelectionManager: KelimeSecici bulunamadı! Lütfen sahnenizde bu bileşenin olduğundan emin olun.");
            isGameActive = false;
            return;
        }

        if (kelimeSecici.secilenKelimeVeTanimlar.Count == 0)
        {
            //Debug.LogError("WordSelectionManager: KelimeSecici'den hiç kelime ve tanım alınamadı. Oyun başlatılamıyor.");
            isGameActive = false;
            return;
        }
        
        totalWords = kelimeSecici.secilenKelimeVeTanimlar.Count; // Otomatik say
        Debug.Log($"Toplam kelime sayısı: {totalWords}");
        

        undoButton.onClick.AddListener(UndoLastSelection);
        clearButton.onClick.AddListener(ClearSelection); 

        // İpucu butonlarının başlangıç pozisyonlarını kaydet
        if (hintButton1 != null) initialButtonPositions[hintButton1] = hintButton1.transform.localPosition;
        if (hintButton2 != null) initialButtonPositions[hintButton2] = hintButton2.transform.localPosition;
        if (hintButton3 != null) initialButtonPositions[hintButton3] = hintButton3.transform.localPosition;
        if (hintButton4 != null) initialButtonPositions[hintButton4] = hintButton4.transform.localPosition;

        if (kategoriDegistirButton != null)
        {
            kategoriDegistirButton.onClick.AddListener(KategoriDegistir);
        }

        // KAYITLI VERİLERİ YÜKLEME VE UI'YI GÜNCELLEME BAŞLANGICI
        kategoriDegistirmeHakki = LevelSaveManager.LoadKategoriDegistirmeHakki(); // Kayıtlı kategori değiştirme hakkını yükle
        kategoriDegistirButtonText.text = $"DEĞİŞTİR ({kategoriDegistirmeHakki})"; // UI'yı güncelle

        letterHintHakki = LevelSaveManager.LoadJokerHakki(); // Kayıtlı joker hakkını (harf ipucu) yükle
        if (letterHintButtonText != null)
        {
            letterHintButtonText.text = $"HARF AL ({letterHintHakki})"; // UI'yı güncelle
        }
        GecButonu.interactable = false; // Başta pasif
        foundWords = LevelSaveManager.LoadFoundWords(); // Kayıtlı bulunan kelimeleri yükle
        // Yüklenen kelimelerle UI'yı güncelleyin (UpdateFoundWordsDisplay metodu güncellenmeli veya çağrılmalı)
        // Eğer UpdateFoundWordsDisplay() metodu hali hazırda 'foundWords' listesini kullanıyorsa,
        // sadece LoadFoundWords() sonrası çağırmanız yeterli olacaktır.
        UpdateFoundWordsDisplay(); 
        // KAYITLI VERİLERİ YÜKLEME VE UI'YI GÜNCELLEME SONU

        gameOverPanel.SetActive(false);
        UpdateUI();
        // UpdateFoundWordsDisplay(); // Yukarıya taşındı
        scoreTMP.text = $"<b>SKOR:</b>      0"; // Başlangıç skoru gösterimi (Bu satırın üzerine LoadScore() eklenebilir)
        scoreTMP.text = $"<b>SEVİYE:</b>    {LevelManager.currentLevel}";



        // Butonları sıfırla ama hintButton1 hariç
        Button[] allHintButtons = { hintButton1, hintButton2, hintButton3, hintButton4 };
        foreach (Button btn in allHintButtons)
        {
            if (btn != null && btn != hintButton1)
            {
                btn.image.DOColor(Color.white, 0.2f);
                if (initialButtonPositions.ContainsKey(btn))
                {
                    btn.transform.DOLocalMoveY(initialButtonPositions[btn].y, buttonMoveDuration)
                        .SetEase(Ease.OutBack);
                }
            }
        }


        // Sonra yeni listener'ları ekle
        if (totalWords > 0 && hintButton1 != null)
        {
            hintButton1.onClick.AddListener(() => DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[0].tanim, hintButton1));
        }
        if (totalWords > 0 && hintButton2 != null)
        {
            hintButton2.onClick.AddListener(() => DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[1].tanim, hintButton2));
        }
        if (totalWords > 0 && hintButton3 != null)
        {
            hintButton3.onClick.AddListener(() => DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[2].tanim, hintButton3));
        }
        if (totalWords > 0 && hintButton4 != null)
        {
            hintButton4.onClick.AddListener(() => DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[3].tanim, hintButton4));
        }


        // ButtonToWordIndex haritalamasını yap
        if (hintButton1 != null) buttonToWordIndex[hintButton1] = 0;
        if (hintButton2 != null) buttonToWordIndex[hintButton2] = 1;
        if (hintButton3 != null) buttonToWordIndex[hintButton3] = 2;
        if (hintButton4 != null) buttonToWordIndex[hintButton4] = 3;

        if (displayedWordText != null)
        {
            displayedWordText.text = "";
        }
        // Otomatik olarak ilk kelimenin ipucunu göster
        if (kelimeSecici != null && kelimeSecici.secilenKelimeVeTanimlar.Count > 0 && hintButton1 != null)
        {
            DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[0].tanim, hintButton1);
        }

        // Harf göster butonu dinleyicisi: Öncekileri kaldırıp yeni bir tane ekle
        if (letterHintButton != null)
        {
            letterHintButton.onClick.RemoveAllListeners(); // Önemli: Dinleyicileri temizle
            letterHintButton.onClick.AddListener(() => ShowRandomLetterHint());
            //letterHintButton.interactable = (letterHintHakki > 0); // Başlangıçta etkileşim durumunu ayarla
        }

        // YENİ EKLENDİ: Dosya adını TMPro'ya yazdırma
        if (fileNameTMP != null)
        {
            if (kelimeSecici.selectedCategoryFileName != null)
            {
                fileNameTMP.text = $"<size=24><b>Kategori:</b> {kelimeSecici.selectedCategoryFileName.ToUpper()}</size>";
                Debug.Log(fileNameTMP.text);
            }
            else
            {
                fileNameTMP.text = $"<size=24><b>Kategori:</b> Bilinmiyor</size>";
            }
        }
    }

    void Update()
    {
        UpdateScoreUI();

        if (!isGameActive)
            return;

        gameTime += Time.deltaTime;
        SeviyeGameTime += Time.deltaTime;

        // Butonun durumunu değiştir (optimize şekilde)
        bool hedefDurum = gameTime >= 180f;
        if (GecButonu.interactable != hedefDurum)
            GecButonu.interactable = hedefDurum;

        geriSayimText.gameObject.SetActive(!GecButonu.interactable);
        // Geri sayımı göster
        float kalanSure = Mathf.Max(180f - gameTime, 0f);
        int dakika = Mathf.FloorToInt(kalanSure / 60f);
        int saniye = Mathf.FloorToInt(kalanSure % 60f);
        geriSayimText.text = string.Format("{0:00}:{1:00}", dakika, saniye);

        UpdateTimerDisplay();
    }


    // Geçen süre UI'ını günceller
    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        timerTMP.text = $"<b>SEVİYE</b>     {LevelManager.currentChapter}" ;
    }

    // Harf hücresine tıklandığında çağrılır
    public void OnCellClicked(int x, int y)
    {
        if (!isGameActive) return;

        Button clickedButton = GetButtonAtPosition(x, y);
        if (clickedButton == null || !clickedButton.interactable) return;
        
        if (audioManager != null)
        {
            audioManager.PlayCellClickSound();
        }

        Vector2Int clickedPos = new Vector2Int(x, y);

        if (selectedCells.Contains(clickedPos)) 
        {
            if (selectedCells.Count > 0 && selectedCells[selectedCells.Count - 1] == clickedPos)
            {
                UndoLastSelection();
            }
            UpdateUI();
            return;
        }
        
        undoStack.Push(new List<Button>(selectedButtons));

        if (selectedCells.Count == 0 || IsNeighbor(x, y))
        {
            SelectCell(x, y, clickedButton);
            CheckWordCompletion();
        }
        else
        {
            if (undoStack.Count > 0)
            {
                undoStack.Pop(); 
            }
        }
        UpdateUI();
    }
    
    // Bir hücreyi seçme işlemi
    void SelectCell(int x, int y, Button button)
    {
        selectedCells.Add(new Vector2Int(x, y));
        selectedButtons.Add(button);
        button.image.color = selectedColor;
        
        button.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        Debug.Log($"WordSelectionManager: Hücre seçildi: ({x},{y}). Seçilen kelime: {GetSelectedWord()}");
    }

    // UI'daki seçili harfler ve buton durumlarını günceller
    public void UpdateUI()
    {
        StringBuilder sb = new StringBuilder("<b>KELİME: ");
        foreach (Vector2Int pos in selectedCells)
        {
            sb.Append($"<color=#FFFF00>{wordSearch.GetGrid()[pos.x, pos.y]}</color> "); 
        }
        selectedLettersTMP.text = sb.ToString();

        undoButton.interactable = selectedCells.Count > 0;
        clearButton.interactable = true;
    }

    // Son seçimi geri alma işlemi
    void UndoLastSelection()
    {
        if (selectedCells.Count == 0) return;
        
        if (audioManager != null)
        {
            audioManager.PlayUndoSound();
        }

        Button lastSelectedButton = selectedButtons[selectedButtons.Count - 1];
        if (lastSelectedButton != null)
        {
            lastSelectedButton.image.color = Color.white;
        }

        selectedCells.RemoveAt(selectedCells.Count - 1);
        selectedButtons.RemoveAt(selectedButtons.Count - 1);

        if (undoStack.Count > 0)
        {
            undoStack.Pop();
        }
        
        Debug.Log($"WordSelectionManager: Son seçim geri alındı. Mevcut kelime: {GetSelectedWord()}");
        UpdateUI();
    }

    void ClearSelection()
    {
        Debug.Log("Seçim ve seviye ilerlemesi sıfırlanıyor...");
        if (audioManager != null)
        {
            audioManager.PlayClearButtonSound();
        }
        foreach (Vector2Int cell in selectedCells)
        {
            if (gridRenderer != null)
            {
                gridRenderer.ResetCellColor(cell.x, cell.y);
            }
        }
        selectedCells.Clear();
        selectedButtons.Clear();
        

        foundWords.Clear();
        ResetAllHintButtonsInteractable(); // Tüm butonları aktif et
        UpdateFoundWordsDisplay();   

        score = 0;
        UpdateScoreUI();

        if (gridRenderer != null)
        {
            gridRenderer.GenerateGrid();
        }
        else
        {
            //Debug.LogError("WordSelectionManager: GridRenderer referansı null! Izgara sıfırlanamadı.");
        }

        selectedButtons.Clear();
        undoStack.Clear();


        // Tüm ipucu butonlarının rengini ve pozisyonunu sıfırla
        Button[] allHintButtons = { hintButton1, hintButton2, hintButton3, hintButton4 };
        // 1. Butonları sıfırla ama ilk kelime gösterilecekse hintButton1 hariç
        foreach (Button btn in allHintButtons)
        {
            if (btn != null) // <- SIFIRLAMADAN ÖNCE FİLTRELE
            {
                btn.image.DOColor(Color.white, 0.2f);
                if (initialButtonPositions.ContainsKey(btn))
                {
                    btn.transform.DOLocalMoveY(
                        initialButtonPositions[btn].y,
                        buttonMoveDuration
                    ).SetEase(Ease.OutBack);
                }
            }
        }
            if (kelimeSecici != null && hintButton1 != null)
    {
        DOVirtual.DelayedCall(0.5f, () =>
        {
            if (kelimeSecici.secilenKelimeVeTanimlar.Count > 0)
            {
                DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[0].tanim, hintButton1);
            }
        });
    }     
        UpdateUI();
        Debug.Log("Seçim ve seviye ilerlemesi sıfırlandı.");
    }

    void UpdateScoreUI()
    {
        scoreTMP.text = $"<b>BÖLÜM</b>     {LevelManager.currentLevel}/5";
    }

    string GetSelectedWord()
    {
        char[,] grid = wordSearch.GetGrid();
        StringBuilder sb = new StringBuilder();
        
        foreach (Vector2Int pos in selectedCells)
        {
            sb.Append(grid[pos.x, pos.y]);
        }
        return sb.ToString();
    }

void CheckWordCompletion()
{
    string currentWord = GetSelectedWord();
    
    foreach (var kelimeVeTanim in kelimeSecici.secilenKelimeVeTanimlar)
    {
        if (currentWord.Equals(kelimeVeTanim.kelime.ToUpper()) && !foundWords.Contains(kelimeVeTanim.kelime)) 
        {
            if (audioManager != null)
            {
                audioManager.PlayWordFoundSound();
            }
            foundWords.Add(kelimeVeTanim.kelime);
            
            // Kelimenin hangi ipucu butonuna ait olduğunu bul ve görsel efekt uygula
            int kelimeIndex = kelimeSecici.secilenKelimeVeTanimlar.IndexOf(kelimeVeTanim);
            Button ilgiliHintButton = null;

            if (kelimeIndex == 0) ilgiliHintButton = hintButton1;
            else if (kelimeIndex == 1) ilgiliHintButton = hintButton2;
            else if (kelimeIndex == 2) ilgiliHintButton = hintButton3;
            else if (kelimeIndex == 3) ilgiliHintButton = hintButton4;

            if (ilgiliHintButton != null)
            {
                // Butonu yeşil yap ve animasyon ekle
                ilgiliHintButton.image.DOColor(completedWordColor, 0.5f);
                ilgiliHintButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
                ilgiliHintButton.interactable = false; // Buton artık tıklanamaz
            }

            MarkWordAsCompleted();
            UpdateFoundWordsDisplay();
            CheckGameCompletion();
            Debug.Log($"WordSelectionManager: Kelime bulundu! '{kelimeVeTanim.kelime}'. Skor: {score}");
            Debug.Log($"{foundWords.Count} kelime buldunuz {totalWords} kelimeden.\n\n");

            // >>> BURAYA EKLEDİĞİM KISIM <<<
            // Sonraki boş ipucu butonunu bul ve aktif hale getir
            for (int i = 0; i < kelimeSecici.secilenKelimeVeTanimlar.Count; i++)
            {
                string kelime = kelimeSecici.secilenKelimeVeTanimlar[i].kelime;

                if (!foundWords.Contains(kelime))
                {
                    Button sonrakiButon = null;

                    if (i == 0) sonrakiButon = hintButton1;
                    else if (i == 1) sonrakiButon = hintButton2;
                    else if (i == 2) sonrakiButon = hintButton3;
                    else if (i == 3) sonrakiButon = hintButton4;

                    if (sonrakiButon != null)
                    {
                        DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[i].tanim, sonrakiButon);
                    }

                    break; // İlk boş kelime bulunduğunda çık
                }
            }

            return;
        }
    }
}


    void UpdateFoundWordsDisplay()
    {  
        StringBuilder sb = new StringBuilder($"<b>BULUNDU: {foundWords.Count}/{totalWords}</b>\n");
        foreach (string word in foundWords)
        {
            sb.Append($"- <color=#FFD700>{word}</color>\n");
        }
        foundWordsTMP.text = sb.ToString();
        UpdateScoreUI();
        scoreTMP.transform.DOKill();
        scoreTMP.transform.localScale = Vector3.one;
        scoreTMP.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
    }

    void MarkWordAsCompleted()
    {
        foreach (Button btn in selectedButtons)
        {
            if (btn != null)
            {
                btn.image.color = completedWordColor;
                btn.interactable = false;
                btn.transform.DOShakeRotation(0.5f, 30f);
            }
        }
        selectedCells.Clear();
        selectedButtons.Clear();
        undoStack.Clear();
    }

    void CheckGameCompletion()
    {
        if (foundWords.Count == totalWords)
        {
            if (audioManager != null)
            {
                audioManager.PlayLevelUpSound();
            }
            gameOverPanel.SetActive(true);    
            GameOver(true);
        }
    }

    void GameOver(bool allWordsFound)
    {
        isGameActive = false;

        if(LevelManager.currentLevel == 5)
        {

            LevelSaveManager.SaveJokerHakki(10);
            LevelSaveManager.SaveKategoriDegistirmeHakki(3);
            int finalScore1 = score + Mathf.FloorToInt(1000 / SeviyeGameTime);
            ResetHints();
            string resultText1 = allWordsFound 
                ? $"<size=30><b>TEBRİKLER!</b></size>\n\n {LevelManager.currentChapter}. Seviyeyi tamamladınız!\n\n\n"
                : "";

            resultTextTMP.text = $"{resultText1}<b>Toplam Süre:</b> {Mathf.FloorToInt(SeviyeGameTime / 60f):00}:{Mathf.FloorToInt(SeviyeGameTime % 60f):00}\n";
            SeviyeGameTime = 0f;

            gameOverPanel.SetActive(true);
            gameOverPanel.transform.localScale = Vector3.zero;
            gameOverPanel.transform.DOScale(1.6f, 0.4f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => {
                    gameOverPanel.transform.DOScale(1.5f, 0.2f);
                });
        }
        else
        {
            LevelSaveManager.SaveJokerHakki(letterHintHakki);
            LevelSaveManager.SaveKategoriDegistirmeHakki(kategoriDegistirmeHakki);

            int finalScore = score + Mathf.FloorToInt(1000 / gameTime);
            string resultText = allWordsFound 
                ? $"<size=30><b>TEBRİKLER!</b></size>\n\n\n {totalWords} kelimeyi buldunuz!\n\n\n"
                : "";

            resultTextTMP.text = $"{resultText}<b>Toplam Süre:</b> {Mathf.FloorToInt(gameTime / 60f):00}:{Mathf.FloorToInt(gameTime % 60f):00}\n";
            ResetHints();
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.localScale = Vector3.zero;
            gameOverPanel.transform.DOScale(1.6f, 0.4f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => {
                    gameOverPanel.transform.DOScale(1.5f, 0.2f);
                });
        }
    }

    bool IsNeighbor(int x, int y)
    {
        if (selectedCells.Count == 0) return false;
        
        Vector2Int lastPos = selectedCells[selectedCells.Count - 1];
        return Mathf.Abs(x - lastPos.x) <= 1 && Mathf.Abs(y - lastPos.y) <= 1; 
    }

    Button GetButtonAtPosition(int x, int y)
    {
        if (gridRenderer != null)
        {
            int index = y * wordSearch.GetGridWidth() + x; 
            if (index >= 0 && index < gridRenderer.transform.childCount)
            {
                Transform cell = gridRenderer.transform.GetChild(index); 
                return cell.GetComponent<Button>(); 
            }
        }
        Debug.LogWarning($"WordSelectionManager: ({x},{y}) konumundaki buton GridRenderer'da bulunamadı veya GridRenderer null.");
        return null;
    }
private void DisplayHintText(string hintText, Button clickedHintButton)
{
    if (audioManager != null)
    {
        audioManager.PlayCategoryChangeSound();
    }

    if (lastClickedHintButton != null && lastClickedHintButton != clickedHintButton)
    {
        lastClickedHintButton.image.DOKill();
        lastClickedHintButton.image.DOColor(Color.white, 0.2f);

        if (initialButtonPositions.ContainsKey(lastClickedHintButton))
        {
            lastClickedHintButton.transform.DOKill();
            lastClickedHintButton.transform.DOLocalMoveY(
                initialButtonPositions[lastClickedHintButton].y,
                buttonMoveDuration
            ).SetEase(Ease.OutBack);
        }
    }

    if (clickedHintButton != null)
    {
        clickedHintButton.image.DOKill();
        clickedHintButton.image.DOColor(Color.green, 0.2f);

        clickedHintButton.transform.DOKill();
        clickedHintButton.transform.localScale = Vector3.one; // Ölçeği sıfırla
        clickedHintButton.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f);

        if (initialButtonPositions.ContainsKey(clickedHintButton))
        {
            clickedHintButton.transform.DOKill();
            clickedHintButton.transform.DOLocalMoveY(
                initialButtonPositions[clickedHintButton].y + selectedButtonOffset,
                buttonMoveDuration
            ).SetEase(Ease.OutBack);
        }

        Button[] allHintButtons = { hintButton1, hintButton2, hintButton3, hintButton4 };
        foreach (Button btn in allHintButtons)
        {
            if (btn != null && btn != clickedHintButton)
            {
                btn.image.DOKill();
                btn.image.DOColor(Color.white, 0.2f);

                if (initialButtonPositions.ContainsKey(btn))
                {
                    btn.transform.DOKill();
                    btn.transform.DOLocalMoveY(
                        initialButtonPositions[btn].y + otherButtonsOffset,
                        buttonMoveDuration
                    ).SetEase(Ease.OutBack);
                }
            }
        }

        lastClickedHintButton = clickedHintButton;
    }

    if (displayedWordText != null)
    {
        displayedWordText.transform.DOKill();
        string currentDisplay = $"İpucu: {hintText}";

        if (buttonToWordIndex.ContainsKey(clickedHintButton))
        {
            int kelimeIndex = buttonToWordIndex[clickedHintButton];
            if (revealedLettersByWord.ContainsKey(kelimeIndex) && revealedLettersByWord[kelimeIndex].Count > 0)
            {
                string kelime = kelimeSecici.secilenKelimeVeTanimlar[kelimeIndex].kelime;
                StringBuilder ipucuKelime = new StringBuilder();
                var revealedIndices = revealedLettersByWord[kelimeIndex];

                for (int i = 0; i < kelime.Length; i++)
                {
                    if (revealedIndices.Contains(i))
                        ipucuKelime.Append(kelime[i] + " ");
                    else
                        ipucuKelime.Append("_ ");
                }

                currentDisplay += "\n\n<b>İPUCU HARF:</b> " + ipucuKelime.ToString().TrimEnd();
            }
        }

        displayedWordText.text = currentDisplay;
        displayedWordText.transform.DOKill();
        displayedWordText.transform.localScale = Vector3.one; // Ölçeği sıfırla
        displayedWordText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);

        Debug.Log($"WordSelectionManager: İpucu gösterildi: '{hintText}'");
    }
}


    // YENİ EKLENEN METOD: Bulunan kelimeler listesini sıfırlamak için
    public void ResetFoundWords()
    {
        foundWords.Clear();
        totalWords = kelimeSecici.secilenKelimeVeTanimlar.Count; // Her resetlemede güncelle
        UpdateUI();
    }

public void ResetForNewLevel()
{
    // 1. Temizlik
    foundWords.Clear();
    selectedCells.Clear();
    selectedButtons.Clear();
    undoStack.Clear();
    revealedLettersByWord.Clear();

    score = 0;
    UpdateScoreUI();

    if (displayedWordText != null) displayedWordText.text = "";
    if (hintOnlyText != null) hintOnlyText.text = "";

    // 2. Yeni bileşenleri bul (önce!)
    kelimeSecici = FindObjectOfType<KelimeSecici>();
    wordSearch = FindObjectOfType<StrictNeighborWordSearch>();

    // 3. Hint listener’ları SIFIRLAMADAN önce yeni veriyi yüklemiş olmalıyız
    hintButton1?.onClick.RemoveAllListeners();
    hintButton2?.onClick.RemoveAllListeners();
    hintButton3?.onClick.RemoveAllListeners();
    hintButton4?.onClick.RemoveAllListeners();

    int totalWords = kelimeSecici?.secilenKelimeVeTanimlar?.Count ?? 0;

    if (totalWords > 0 && hintButton1 != null)
    {
        hintButton1.onClick.AddListener(() =>
            DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[0].tanim, hintButton1));
    }
    if (totalWords > 0 && hintButton2 != null)
    {
        hintButton2.onClick.AddListener(() =>
            DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[1].tanim, hintButton2));
    }
    if (totalWords > 0 && hintButton3 != null)
    {
        hintButton3.onClick.AddListener(() =>
            DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[2].tanim, hintButton3));
    }
    if (totalWords > 0 && hintButton4 != null)
    {
        hintButton4.onClick.AddListener(() =>
            DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[3].tanim, hintButton4));
    }

    UpdateUI();
    UpdateFoundWordsDisplay();
    ResetAllHintButtonsInteractable(); // Tüm butonları aktif et
        

        // Tüm ipucu butonlarının rengini ve pozisyonunu sıfırla
        Button[] allHintButtons = { hintButton1, hintButton2, hintButton3, hintButton4 };
// 1. Butonları sıfırla ama ilk kelime gösterilecekse hintButton1 hariç
foreach (Button btn in allHintButtons)
{
    if (btn != null) // <- SIFIRLAMADAN ÖNCE FİLTRELE
    {
        btn.image.DOColor(Color.white, 0.2f);
        if (initialButtonPositions.ContainsKey(btn))
        {
            btn.transform.DOLocalMoveY(
                initialButtonPositions[btn].y,
                buttonMoveDuration
            ).SetEase(Ease.OutBack);
        }
    }
}

    if (kelimeSecici != null && hintButton1 != null)
    {
        DOVirtual.DelayedCall(0.5f, () =>
        {
            if (kelimeSecici.secilenKelimeVeTanimlar.Count > 0)
            {
                DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[0].tanim, hintButton1);
            }
        });
    }

        
        // Harf ipucu hakkını sıfırla ve UI'ı güncelle


        // Harf ipucu butonu tekrar etkileşimli hale gelsin
        if (letterHintButton != null)
        {
            letterHintButton.interactable = true;
        }


                // ÖNEMLİ DEĞİŞİKLİK: Kategori değiştirme hakkını sadece 5'in katı olan seviyelerde sıfırla
        if (LevelManager.currentLevel  == 1) // Eğer mevcut seviye 5'in katı ise
        {
            kategoriDegistirmeHakki = 3; 
            letterHintHakki = 10; // Her yeni seviyede başlangıç hakkı
        }

        if (kategoriDegistirButtonText != null)
        {
            kategoriDegistirButtonText.text = $"DEĞİŞTİR ({kategoriDegistirmeHakki})";
        }
        if (letterHintButtonText != null)
        {
            letterHintButtonText.text = $"HARF AL ({letterHintHakki})";
        }

        // 4. Oyun Durumu
        isGameActive = true;
        gameTime = 0f;

        
        // 5. Grid'i Yenile
        if(gridRenderer != null) gridRenderer.GenerateGrid();
        
        Debug.Log("Yeni seviye için tam reset yapıldı");
    }

    public void UpdateCategoryText()
    { 
       string kategoriAdi = kelimeSecici.selectedCategoryFileName.ToUpper();
        fileNameTMP.text = $"<size=24><b>Kategori:</b> {kategoriAdi}</size>";
        Debug.Log($"Kategori TextMeshPro'ya yazıldı: {kategoriAdi}");
        Debug.Log($"WordSelectionManager: Son seçim geri alındı. Mevcut kelime: {GetSelectedWord()}");
    }

public void KategoriDegistir()
{
    if (kategoriDegistirmeHakki > 0)
    {
        displayedWordText.transform.DOKill(); // Garanti olsun diye tekrar temizle
        if (kategoriDegistirButton != null)
            kategoriDegistirButton.interactable = false; // 0. Adım

        Debug.Log("Kategori değiştiriliyor...");
        kategoriDegistirmeHakki--;


        ResetAllHintButtonsInteractable(); // Tüm butonları aktif et
        ClearSelection();
        ResetHints();
        lastClickedHintButton = null;
        revealedLettersByWord.Clear();
        currentHintString = "";
        if (hintOnlyText != null) hintOnlyText.text = "";

        if (kategoriDegistirButtonText != null)
        {
            kategoriDegistirButtonText.text = $"DEĞİŞTİR ({kategoriDegistirmeHakki})";
        }

        if (kelimeSecici == null) kelimeSecici = FindObjectOfType<KelimeSecici>();
        if (kelimeSecici == null) return;

        kelimeSecici.LoadNewWordsForLevel();
        totalWords = kelimeSecici.secilenKelimeVeTanimlar.Count;
        ResetFoundWords();
        UpdateUI();

        if (fileNameTMP != null)
        {
            fileNameTMP.text = $"<size=24><b>Kategori:</b> {kelimeSecici.selectedCategoryFileName.ToUpper()}</size>";
        }

        if (wordSearch != null)
        {
            wordSearch.hiddenWords = kelimeSecici.secilenKelimeVeTanimlar.Select(k => k.kelime).ToList();
            wordSearch.InitializeGrid();
        }

        if (gridRenderer != null)
        {
            gridRenderer.GenerateGrid();
        }

        if (kelimeSecici != null && hintButton1 != null)
        {
            DOVirtual.DelayedCall(0.4f, () =>
            {
                if (kelimeSecici.secilenKelimeVeTanimlar.Count > 0)
                {
                    //DisplayHintText(kelimeSecici.secilenKelimeVeTanimlar[0].tanim, hintButton1);
                    //displayedWordText.transform.DOKill(); // Garanti olsun diye tekrar temizle
                }
            });
        }

        if (LevelManager.currentLevel == 2 || LevelManager.currentLevel == 3)
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                letterHintHakki++;
                ShowRandomLetterHint();
            });
        }

        // Butonu 0.6 saniye sonra tekrar aktif et
        DOVirtual.DelayedCall(0.6f, () =>
        {
            if (kategoriDegistirButton != null)
                kategoriDegistirButton.interactable = true;
        });

        Debug.Log("Kategori başarıyla değiştirildi.");
    }
}

        


    public void MenuyeDon()
    {
        SceneManager.LoadScene("AnaMenü");
    }

    public void ShowRandomLetterHint()
    {
        if (letterHintHakki > 0){

            
            Debug.Log("ShowRandomLetterHint çağrıldı! Hakkınız: " + letterHintHakki); // Hata ayıklama için

            if (lastClickedHintButton == null)
            {
                Debug.LogWarning("Herhangi bir ipucu butonuna basılmadı, harf ipucu gösterilemiyor.");
                return;
            }

            if (!buttonToWordIndex.ContainsKey(lastClickedHintButton))
            {
                Debug.LogWarning("Buton kelime indeksi ile eşleşmedi.");
                return;
            }

            int kelimeIndex = buttonToWordIndex[lastClickedHintButton];
            if (kelimeIndex >= kelimeSecici.secilenKelimeVeTanimlar.Count)
            {
                Debug.LogWarning("Kelime indeksi sınırların dışında.");
                return;
            }

            string kelime = kelimeSecici.secilenKelimeVeTanimlar[kelimeIndex].kelime;
            if (string.IsNullOrEmpty(kelime))
            {
                Debug.LogWarning("Kelime boş.");
                return;
            }

            // Eğer o kelime için daha önce açılan harf indeksleri yoksa oluştur
            if (!revealedLettersByWord.ContainsKey(kelimeIndex))
            {
                revealedLettersByWord[kelimeIndex] = new HashSet<int>();
            }

            var revealedIndices = revealedLettersByWord[kelimeIndex];

            // Eğer tüm harfler açılmışsa ipucu verme
            if (revealedIndices.Count >= kelime.Length)
            {
                Debug.Log("Tüm harfler zaten gösterildi.");
                return;
            }

            // Açılmamış harfler arasından rastgele bir indeks seç
            List<int> remainingIndices = Enumerable.Range(0, kelime.Length)
                                                .Where(i => !revealedIndices.Contains(i))
                                                .ToList();

            // Eğer hiç açılmamış harf kalmadıysa (yukarıdaki kontrol yakalamalı ama garanti olsun)
            if (remainingIndices.Count == 0)
            {
                Debug.Log("Gösterilecek başka harf kalmadı.");
                return;
            }

            int randomIndex = remainingIndices[Random.Range(0, remainingIndices.Count)];

            // Seçilen harfi açılanlara ekle
            revealedIndices.Add(randomIndex);

            // Harf ipucu başarılı bir şekilde verildiği için hakkı azalt
            letterHintHakki--;
            // UI metnini güncelle
            if (letterHintButtonText != null)
            {
                letterHintButtonText.text = $"HARF AL ({letterHintHakki})";
            }

            // İpucu kelimesini oluşturuyoruz
            StringBuilder ipucuKelime = new StringBuilder();
            for (int i = 0; i < kelime.Length; i++)
            {
                if (revealedIndices.Contains(i))
                    ipucuKelime.Append(kelime[i] + " ");
                else
                    ipucuKelime.Append("_ ");
            }

            string hintTextFormatted = ipucuKelime.ToString().TrimEnd();
            currentHintString = hintTextFormatted; // Artık bu değişkende sadece ipucu var

            if (hintOnlyText != null)
            {
                hintOnlyText.text = "<b>İPUCU HARF:</b> " + hintTextFormatted;
            }
            //LevelSaveManager.SaveJokerHakki(letterHintHakki); // BURAYI EKLEYİN


            if (displayedWordText != null)
            {   
                        if (audioManager != null)
            {
                audioManager.PlayLetterHintSound();
            }
                displayedWordText.text = kelimeSecici.secilenKelimeVeTanimlar[kelimeIndex].tanim + "\n\n<b>İPUCU HARF:</b> " + ipucuKelime.ToString().TrimEnd();
            }

            Debug.Log($"Kelime: {kelime} - Gösterilen harfler: {ipucuKelime}");
        }
    }
    

    void ResetHints()
    {
        revealedLettersByWord.Clear();
        if (displayedWordText != null)
        {
            displayedWordText.text = "";
            hintOnlyText.text = "";
        }

        // Kategori değiştirildiğinde harf hakkı sıfırlanmamalı, sadece yeni seviyede sıfırlanmalı
        // Bu yüzden burada letterHintHakki'yi sıfırlamıyoruz.
    }

public void SeviyeBaslangiciAnimasyonu(int seviyeNo)
{
    transitionPanel.gameObject.SetActive(true);
    transitionPanel.color = new Color(1, 1, 1, 0);
    transitionPanel.DOFade(1f, 0.4f);

    foreach (Transform child in ortakGrid)
    {
        Destroy(child.gameObject);
    }

    var vLayout = ortakGrid.GetComponent<VerticalLayoutGroup>();
    if (vLayout != null)
    {
        vLayout.spacing = -800f;
        vLayout.childAlignment = TextAnchor.MiddleCenter;
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;
    }

    string[] gridYazilari = {
        "SEVİYE" + seviyeNo.ToString().PadLeft(2, ' '),
        "BAŞLIYOR"
    };

    float harfDelay = 0f;
    float harfAnimSuresi = 0.3f;

    foreach (string satir in gridYazilari)
    {
        GameObject satirGO = new GameObject("Satir", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        satirGO.transform.SetParent(ortakGrid, false);

        // Satır RectTransform boyutu
        RectTransform rt = satirGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600, 80); // Örneğin genişlik 600, yükseklik 80

        var hLayout = satirGO.GetComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10f;
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = false;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = false;

        foreach (char harf in satir)
        {
            GameObject kutu = Instantiate(harfKutusuPrefab, satirGO.transform);
            var text = kutu.GetComponentInChildren<TextMeshProUGUI>();
            text.text = harf.ToString();
            text.color = Color.black; // Yazı rengini siyah yap

            CanvasGroup cg = kutu.GetComponent<CanvasGroup>();
            if (cg == null) cg = kutu.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            kutu.transform.localScale = Vector3.one;

            cg.DOFade(1f, harfAnimSuresi).SetDelay(harfDelay);
            kutu.transform.DOScale(1f, harfAnimSuresi).SetEase(Ease.OutBack).SetDelay(harfDelay);

            harfDelay += 0.05f;
        }
    }

    float beklemeSuresi = harfDelay + 1.5f;

    DOVirtual.DelayedCall(beklemeSuresi, () =>
    {
        float kapanisDelay = 0f;

        foreach (Transform satir in ortakGrid)
        {
            foreach (Transform kutu in satir)
            {
                CanvasGroup cg = kutu.GetComponent<CanvasGroup>();
                if (cg == null) cg = kutu.gameObject.AddComponent<CanvasGroup>();

                cg.DOFade(0f, 0.4f).SetDelay(kapanisDelay);
                kutu.transform.DOScale(0.5f, 0.4f).SetEase(Ease.InBack).SetDelay(kapanisDelay);

                kapanisDelay += 0.03f;
            }
        }

        transitionPanel.DOFade(0f, 0.5f).SetDelay(kapanisDelay + 0.2f).OnComplete(() =>
        {
            foreach (Transform child in ortakGrid)
            {
                Destroy(child.gameObject);
            }
            transitionPanel.gameObject.SetActive(false);
        });
    });
}


public void CompleteLevelDebug()
{

                if (audioManager != null)
            {
                audioManager.PlayLevelUpSound();
            }
    // gameOverPanel'in null olup olmadığını kontrol edin
    if (gameOverPanel != null)
    {
        gameOverPanel.SetActive(true); // Oyun bitti panelini görünür yap
    }
    else
    {
        //Debug.LogError("gameOverPanel referansı null! Lütfen Inspector'da atayın.");
    }

    GameOver(true); // Oyun sonu mantığını tetikle, 'true' seviyenin başarılı bittiğini gösterir

    // Eğer daha önce dinamik silme butonları oluşturulduysa ve bunları gizlemek istiyorsanız
    // HideAllFoundWordDeleteButtons(); // Bu satırı yorumdan kaldırın veya silebilirsiniz, isteğe bağlı.

    Debug.Log("DEBUG: Seviye direkt olarak tamamlandı (GameOver(true) çağrıldı).");
}
private void ResetAllHintButtonsInteractable()
{
    Button[] allHintButtons = { hintButton1, hintButton2, hintButton3, hintButton4 };
    foreach (Button btn in allHintButtons)
    {
        if (btn != null)
        {
            btn.interactable = true; // Tüm butonları tıklanabilir yap
            btn.image.DOColor(Color.white, 0.1f); // İsteğe bağlı: rengi de sıfırla
        }
    }
}
    
}