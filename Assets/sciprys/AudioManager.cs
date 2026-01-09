using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Ses Klipleri")]
    public AudioClip cellClickSound;
    public AudioClip wordFoundSound;
    public AudioClip levelUpSound;
    public AudioClip clearButtonSound;
    public AudioClip undoButtonSound;
    public AudioClip letterHintSound;   // YENİ EKLENDİ: Harf al sesi
    public AudioClip categoryChangeSound; // YENİ EKLENDİ: Kategori değiştir sesi

    private AudioSource audioSource;

    public bool sesAcik = true;


    [Header("Arkaplan Müziği")]
    public AudioClip backgroundMusicClip;
    private AudioSource musicSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            Debug.Log("AudioManager: Yeni AudioSource eklendi.");
        }

        // PlayerPrefs'tan ses durumu oku, yoksa açık kabul et (1)
        sesAcik = PlayerPrefs.GetInt("SesAcik", 1) == 1;

        // AudioListener ses seviyesini ayarla
        AudioListener.volume = sesAcik ? 1f : 0f;


                // Müzik için ayrı bir kaynak
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.3f; // ses seviyesi

        // Ayara göre müzik başlat
        bool muzikAcik = PlayerPrefs.GetInt("MuzikAcik", 1) == 1;
        if (muzikAcik && backgroundMusicClip != null)
        {
            musicSource.Play();
        }
    }

    public void PlayCellClickSound()
    {
        if (!sesAcik) return;

        if (cellClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cellClickSound);
            Debug.Log("AudioManager: Hücre tıklama sesi çalındı.");
        }
        else if (cellClickSound == null)
        {
            Debug.LogWarning("AudioManager: CellClickSound atanmamış!");
        }
    }

    public void PlayUndoSound()
    {
        if (!sesAcik) return;

        if (undoButtonSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(undoButtonSound);
            Debug.Log("AudioManager: Undo butonu sesi çalındı.");
        }
        else if (undoButtonSound == null)
        {
            Debug.LogWarning("AudioManager: UndoButtonSound atanmamış!");
        }
    }

    public void PlayWordFoundSound()
    {
        if (!sesAcik) return;

        if (wordFoundSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(wordFoundSound);
            Debug.Log("AudioManager: Kelime bulunma sesi çalındı.");
        }
        else if (wordFoundSound == null)
        {
            Debug.LogWarning("AudioManager: WordFoundSound atanmamış!");
        }
    }

    public void PlayLevelUpSound()
    {
        if (!sesAcik) return;

        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
            Debug.Log("AudioManager: Seviye atlama sesi çalındı.");
        }
        else
        {
            Debug.LogWarning("AudioManager: LevelUpSound atanmamış!");
        }
    }

    public void PlayClearButtonSound()
    {
        if (!sesAcik) return;

        if (clearButtonSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clearButtonSound);
            Debug.Log("AudioManager: Temizle butonu sesi çalındı.");
        }
        else if (clearButtonSound == null)
        {
            Debug.LogWarning("AudioManager: ClearButtonSound atanmamış!");
        }
    }

    // YENİ EKLENEN METOT: Harf Al sesi çalma
    public void PlayLetterHintSound()
    {
        if (!sesAcik) return;

        if (letterHintSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(letterHintSound);
            Debug.Log("AudioManager: Harf ipucu sesi çalındı.");
        }
        else if (letterHintSound == null)
        {
            Debug.LogWarning("AudioManager: LetterHintSound atanmamış!");
        }
    }

    // YENİ EKLENEN METOT: Kategori Değiştir sesi çalma
    public void PlayCategoryChangeSound()
    {
        if (!sesAcik) return;

        if (categoryChangeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(categoryChangeSound);
            Debug.Log("AudioManager: Kategori değiştirme sesi çalındı.");
        }
        else if (categoryChangeSound == null)
        {
            Debug.LogWarning("AudioManager: CategoryChangeSound atanmamış!");
        }
    }
        public void SetMuzikDurumu(bool acik)
    {
        PlayerPrefs.SetInt("MuzikAcik", acik ? 1 : 0);
        PlayerPrefs.Save();

        if (acik)
        {
            if (!musicSource.isPlaying && backgroundMusicClip != null)
                musicSource.Play();
        }
        else
        {
            musicSource.Stop();
        }
    }
}