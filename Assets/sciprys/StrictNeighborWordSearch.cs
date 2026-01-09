using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine.SceneManagement;
public class StrictNeighborWordSearch : MonoBehaviour
{

    [SerializeField] public GameObject yenidenDeneButonu; // Inspector’dan butonu ata
    [Header("Oyun Ayarları")]
    public List<string> hiddenWords = new List<string>();

    public int gridWidth = 8;
    public int gridHeight = 12;
    public int maxPlacementAttempts = 800;

    private char[,] grid;
    private string[,] cellOwners;

    public WordSelectionManager wordSelectionManager; // Inspector'dan atanmalı

        private readonly Vector2Int[] directions = {
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
        new Vector2Int(-1, 0),  new Vector2Int(1, 0),
        new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1)
    };

    void Start()
    {

     //InitializeGrid(); 
         if (yenidenDeneButonu != null)
        yenidenDeneButonu.SetActive(false); // Başta gizli
    }

    public void kapat(){
         if (yenidenDeneButonu != null)
        yenidenDeneButonu.SetActive(false); // Başta gizli
        
        wordSelectionManager.kategoriDegistirmeHakki++;
        wordSelectionManager.kategoriDegistirButtonText.text = "DEĞİŞTİR (" + wordSelectionManager.kategoriDegistirmeHakki + ")";
    }



    public void InitializeGrid()
    {
        if (hiddenWords.Count == 0)
        {
            Debug.LogError("Yerleştirilecek kelime bulunamadı!");
            return;
        }

        grid = new char[gridWidth, gridHeight];
        cellOwners = new string[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = ' ';
                cellOwners[x, y] = null;
            }

        PlaceWordsWithStrictNeighbors();
        FillEmptySpaces();
        LogGridToConsole();
    }

    void PlaceWordsWithStrictNeighbors()
    {
        List<string> sortedHiddenWords = hiddenWords.OrderByDescending(w => w.Length).ToList();

        foreach (string word in sortedHiddenWords)
        {
            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < maxPlacementAttempts)
            {
                Vector2Int startPos = new Vector2Int(
                    Random.Range(0, gridWidth),
                    Random.Range(0, gridHeight));

                placed = TryPlaceWord(word.ToUpper(), startPos, word);
                attempts++;
            }

            if (!placed)
            {
                Debug.LogWarning($"Kelime yerleştirilemedi: {word} ({attempts} deneme)");
                
                // Butonu aktif et
                if (yenidenDeneButonu != null)
                    yenidenDeneButonu.SetActive(true);

            }
        }
    }

    bool TryPlaceWord(string word, Vector2Int startPos, string wordOwner)
    {
        if (grid[startPos.x, startPos.y] != ' ' || cellOwners[startPos.x, startPos.y] != null)
            return false;

        char[,] tempGrid = new char[gridWidth, gridHeight];
        string[,] tempCellOwners = new string[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            {
                tempGrid[x, y] = grid[x, y];
                tempCellOwners[x, y] = cellOwners[x, y];
            }

        List<Vector2Int> tempPath = new List<Vector2Int>();
        Vector2Int currentPos = startPos;
        bool possible = true;

        for (int i = 0; i < word.Length; i++)
        {
            if (i == 0)
            {
                if (tempGrid[currentPos.x, currentPos.y] != ' ' || tempCellOwners[currentPos.x, currentPos.y] != null)
                {
                    possible = false;
                    break;
                }
                tempGrid[currentPos.x, currentPos.y] = word[i];
                tempCellOwners[currentPos.x, currentPos.y] = wordOwner;
                tempPath.Add(currentPos);
            }
            else
            {
                List<Vector2Int> validNeighbors = GetValidNeighbors(currentPos, tempGrid, tempCellOwners);
                if (validNeighbors.Count == 0)
                {
                    possible = false;
                    break;
                }

                currentPos = validNeighbors[Random.Range(0, validNeighbors.Count)];
                tempGrid[currentPos.x, currentPos.y] = word[i];
                tempCellOwners[currentPos.x, currentPos.y] = wordOwner;
                tempPath.Add(currentPos);
            }
        }

        if (possible)
        {
            for (int i = 0; i < tempPath.Count; i++)
            {
                grid[tempPath[i].x, tempPath[i].y] = word[i];
                cellOwners[tempPath[i].x, tempPath[i].y] = wordOwner;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    List<Vector2Int> GetValidNeighbors(Vector2Int pos, char[,] currentGrid, string[,] currentCellOwners)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (neighbor.x >= 0 && neighbor.x < gridWidth &&
                neighbor.y >= 0 && neighbor.y < gridHeight)
            {
                if (currentGrid[neighbor.x, neighbor.y] == ' ' && currentCellOwners[neighbor.x, neighbor.y] == null)
                {
                    neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
    }

    void FillEmptySpaces()
    {
        string turkceHarfler = "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZ";
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (grid[x, y] == ' ')
                    grid[x, y] = turkceHarfler[Random.Range(0, turkceHarfler.Length)];

        Debug.Log("Boşluklar rastgele harflerle dolduruldu.");
    }

    void LogGridToConsole()
    {
        StringBuilder sb = new StringBuilder("--- IZGARA ---\n");
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
                sb.Append(grid[x, y] + " ");
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    public char[,] GetGrid() => grid;
    public int GetGridWidth() => gridWidth;
    public int GetGridHeight() => gridHeight;
}
