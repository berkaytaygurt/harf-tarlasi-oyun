using UnityEngine;
using UnityEngine.UI;
using TMPro; // Eğer hücre prefab'ında TextMeshPro kullanıyorsanız bu gereklidir

[RequireComponent(typeof(GridLayoutGroup))]
public class GridRenderer : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    private StrictNeighborWordSearch _wordSearch; 
    public StrictNeighborWordSearch WordSearch 
    {
        get { return _wordSearch; }
        set { _wordSearch = value; }
    }

    [SerializeField] private int letterFontSize = 48;

    void Start()
    {
        if (cellPrefab == null)
        {
            Debug.LogError("GridRenderer: Cell Prefab atanmamış!");
            return;
        }

        if (GetComponent<GridLayoutGroup>() == null)
        {
            gameObject.AddComponent<GridLayoutGroup>();
        }
    }

public void GenerateGrid()
{
    if (_wordSearch == null) 
    {
        Debug.LogError("GridRenderer: StrictNeighborWordSearch referansı atanmamış! Izgara oluşturulamadı.");
        return;
    }

    char[,] grid = _wordSearch.GetGrid();
    int gridWidth = _wordSearch.GetGridWidth(); 
    int gridHeight = _wordSearch.GetGridHeight(); 
    
    GridLayoutGroup gridLayout = GetComponent<GridLayoutGroup>();
    gridLayout.cellSize = new Vector2(120, 120);
    gridLayout.spacing = new Vector2(6, 6);
    gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    gridLayout.constraintCount = gridWidth; 

    // Önceki grid'i temizle
    foreach (Transform child in transform)
    {
        Destroy(child.gameObject);
    }

    for (int y = 0; y < gridHeight; y++)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            GameObject cell = Instantiate(cellPrefab, transform);
            
            // Text bileşenlerini ayarla
            TextMeshProUGUI cellTextTMP = cell.GetComponentInChildren<TextMeshProUGUI>();
            Text cellTextLegacy = cell.GetComponentInChildren<Text>();

            if (cellTextTMP != null)
            {
                cellTextTMP.text = grid[x, y].ToString();
                cellTextTMP.fontSize = letterFontSize;
                cellTextTMP.color = Color.black;
            }
            else if (cellTextLegacy != null)
            {
                cellTextLegacy.text = grid[x, y].ToString();
                cellTextLegacy.fontSize = letterFontSize;
                cellTextLegacy.color = Color.black;
            }

            // Buton ayarları
            Button btn = cell.GetComponent<Button>();
            if (btn != null)
            {
                // Butonu aktif et ve varsayılan renkleri ayarla
                btn.interactable = true;
                btn.image.color = Color.white;
                
                // Event listener eklemeden önce temizle
                btn.onClick.RemoveAllListeners();
                
                // Pozisyonları lokal değişkenlere atayarak closure problemi önle
                int xPos = x, yPos = y;
                
                btn.onClick.AddListener(() => {
                    WordSelectionManager manager = FindObjectOfType<WordSelectionManager>();
                    if (manager != null && manager.isGameActive)
                    {
                        manager.OnCellClicked(xPos, yPos);
                    }
                    else
                    {
                        Debug.LogWarning("WordSelectionManager bulunamadı veya oyun aktif değil!");
                    }
                });
            }
            else
            {
                Debug.LogError($"Hücre ({x},{y}) buton bileşeni içermiyor!");
            }
        }
    }
    
    Debug.Log($"GridRenderer: {gridWidth}x{gridHeight} boyutunda ızgara başarıyla oluşturuldu.");
}

    public void ResetCellColor(int x, int y)
    {
        if (_wordSearch == null) return; 

        int index = y * _wordSearch.GetGridWidth() + x; 
        if (index >= 0 && index < transform.childCount)
        {
            Transform cell = transform.GetChild(index); 
            Button btn = cell.GetComponent<Button>(); 
            if (btn != null)
            {
                btn.image.color = Color.white; 
            }
        }
    }
}