using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public void CompleteLevel()
    {
        LevelManager.currentLevel++;

        if (LevelManager.currentLevel > 5)
        {
            LevelManager.currentLevel = 1;
            LevelManager.currentChapter++;
            LevelSaveManager.SaveChapter(LevelManager.currentChapter);
        }

        LevelSaveManager.SaveLevel(LevelManager.currentLevel);

        // Eğer sahne geçişi gerekiyorsa burası kalabilir, yoksa kaldır
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
