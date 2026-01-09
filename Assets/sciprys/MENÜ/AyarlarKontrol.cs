using UnityEngine;
using UnityEngine.UI;

public class AyarlarKontrol : MonoBehaviour
{
    public GameObject ayarlarPanel;
    public Toggle sesToggle;
    public Toggle muzikToggle; // UI'da atayacaksın
    public Button geriButton;

    public GameObject howToPlayPanel; // Nasıl Oynanır paneli
    public Button howToPlayKapatButton; // Panel içindeki "Kapat" butonu

    public GameObject SorunBildirPanel; // Sorun Bildir paneli
    public Button sorunBildirKapatButton; // Sorun Bildir paneli içindeki "Kapat" butonu

    public AudioManager audioManager;

    private const string SesAcikKey = "SesAcik";

    void Start()
    {
        bool sesAcik = PlayerPrefs.GetInt(SesAcikKey, 1) == 1;
        sesToggle.isOn = sesAcik;

        if (audioManager != null)
            audioManager.sesAcik = sesAcik;

        sesToggle.onValueChanged.AddListener(SesAcKapa);
        geriButton.onClick.AddListener(AyarlarKapat);

        if (ayarlarPanel != null)
            ayarlarPanel.SetActive(false);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        if (howToPlayKapatButton != null)
            howToPlayKapatButton.onClick.AddListener(HowToPlayKapat);

        bool muzikAcik = PlayerPrefs.GetInt("MuzikAcik", 1) == 1;
        muzikToggle.isOn = muzikAcik;

        muzikToggle.onValueChanged.AddListener(audioManager.SetMuzikDurumu);

        if (SorunBildirPanel != null)
            SorunBildirPanel.SetActive(false);

        if (sorunBildirKapatButton != null)
            sorunBildirKapatButton.onClick.AddListener(SorunBildirKapat);
    }

    public void AyarlarAc()
    {
        if (ayarlarPanel != null)
            ayarlarPanel.SetActive(true);
    }

    public void AyarlarKapat()
    {
        if (ayarlarPanel != null)
            ayarlarPanel.SetActive(false);
    }

    private void SesAcKapa(bool acik)
    {
        if (audioManager != null)
            audioManager.sesAcik = acik;

        PlayerPrefs.SetInt(SesAcikKey, acik ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void HowToPlayAc()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    public void HowToPlayKapat()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    // ⭐ SORUN BİLDİR PANEL İŞLEVLERİ
    public void SorunBildirAc()
    {
        if (SorunBildirPanel != null)
            SorunBildirPanel.SetActive(true);
    }

    public void SorunBildirKapat()
    {
        if (SorunBildirPanel != null)
            SorunBildirPanel.SetActive(false);
    }
}
