using UnityEngine;
using TMPro;
public class GecPaneli : MonoBehaviour
{
    public GameObject hedefPanel;      // Inspector'dan atanacak, manuel aç-kapa için
    private LevelManager levelManager;


    void Start()
    {
        hedefPanel.SetActive(false);

        levelManager = FindObjectOfType<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("LevelManager bulunamadı!");
        }
    }


    // hedefPanel için manuel aç-kapa fonksiyonları (bozulmadı)
    public void PaneliAc()
    {
        hedefPanel.SetActive(true);
    }

    public void PaneliKapat()
    {
        hedefPanel.SetActive(false);
    }
    public void hakver(){
        WordSelectionManager selectionManager = FindObjectOfType<WordSelectionManager>();
        selectionManager.letterHintHakki += 2;
        selectionManager.letterHintButtonText.text = $"HARF AL ({selectionManager.letterHintHakki})";
    }

}
