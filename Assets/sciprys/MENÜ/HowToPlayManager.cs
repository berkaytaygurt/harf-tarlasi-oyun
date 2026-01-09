using UnityEngine;
using UnityEngine.UI;
using TMPro; // Eğer TextMeshPro kullanıyorsan

public class HowToPlayManager : MonoBehaviour
{
    public GameObject[] steps; // Step0, Step1, Step2 ... sırasıyla atanacak
    public GameObject panel;   // HowToPlayPanel (kapatma için)
    private int currentIndex = 0;

    void Start()
    {
        ShowStep(0);
    }

    public void ShowStep(int index)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].SetActive(i == index);
        }

        currentIndex = index;
    }

    public void NextStep()
    {
        int nextIndex = currentIndex + 1;

        if (nextIndex < steps.Length)
        {
            ShowStep(nextIndex);
        }
        else
        {
            // Eğer son adımdan sonra tekrar basılırsa paneli kapat
            panel.SetActive(false);
        }
    }

    public void ResetAndOpen()
    {
        panel.SetActive(true);
        ShowStep(0);
    }

    public void Kapat()
    {
        panel.SetActive(false);
    }
}
