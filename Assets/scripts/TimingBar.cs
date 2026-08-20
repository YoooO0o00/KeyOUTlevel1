using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class TimingBar : MonoBehaviour
{
    [Header("Références")]
    public RectTransform cursor;
    public RectTransform blueZone;
    public RectTransform greenZone;

    [Header("Alien - Animation")]
    public SpriteRenderer[] animationFrames;

    [Header("Alien - Physics")]
    public Rigidbody2D alienRigidbody;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject successPanel;

    [Header("Restart")]
    public GameObject restartButton;

    [Header("Vies")]
    public int maxLives = 3;
    public GameObject[] lifeImages;

    [Header("Chrono")]
    public float timeLimit = 10f;
    public TextMeshProUGUI timerText;

    [Header("Réglages")]
    public float speed = 300f;
    public float successDelay = 2f; // Delay in seconds before showing success screen

    [Header("Audio - SFX")]
    public AudioSource sfxSource;        // Source audio pour les effets
    public AudioClip perfectSFX;         // Son pour PERFECT
    public AudioClip goodSFX;            // Son pour GOOD
    public AudioClip missSFX;            // Son pour MISS
    public AudioClip gameOverSFX;        // Son pour Game Over
    public AudioClip successSFX;         // Son pour Succès
    public float sfxVolume = 0.7f;       // Volume global des SFX

    private int currentLives;
    private float currentTime;

    private RectTransform bar;

    private float direction = 1f;

    private bool playing = true;

    private int currentFrame = 0;


    // =========================================================
    // INITIALISATION
    // =========================================================

    void Awake()
    {
        bar = GetComponent<RectTransform>();

        // Initialise les vies
        currentLives = maxLives;

        // Initialise le chrono
        currentTime = timeLimit;

        // Cache Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Cache Success
        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        // Cache Restart
        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }

        // Initialisation de l'audio
        InitializeAudio();

        // Rigidbody en Kinematic
        if (alienRigidbody != null)
        {
            alienRigidbody.bodyType = RigidbodyType2D.Kinematic;
        }

        // Initialise les vies
        InitializeLives();

        // Initialise l'Alien
        InitializeAnimation();

        // Initialise l'affichage du chrono
        UpdateTimerDisplay();
    }


    // =========================================================
    // INITIALISATION AUDIO
    // =========================================================

    void InitializeAudio()
    {
        // Si aucune source audio n'est assignée, on en crée une
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }
        else
        {
            sfxSource.volume = sfxVolume;
            sfxSource.playOnAwake = false;
        }
    }


    // =========================================================
    // JOUER UN SON
    // =========================================================

    void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (!playing)
            return;

        // -----------------------------------------------------
        // CHRONO
        // -----------------------------------------------------

        UpdateTimer();


        // Si le chrono vient de provoquer un Game Over
        if (!playing)
            return;


        // -----------------------------------------------------
        // CURSEUR
        // -----------------------------------------------------

        MoveCursor();


        // -----------------------------------------------------
        // ESPACE
        // -----------------------------------------------------

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // Pas de son pour le clic espace
            CheckTiming();
        }
    }


    // =========================================================
    // CHRONO
    // =========================================================

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;

            UpdateTimerDisplay();

            // Le joueur n'a pas libéré l'Alien à temps
            GameOver();

            return;
        }

        UpdateTimerDisplay();
    }


    // =========================================================
    // AFFICHAGE DU CHRONO
    // =========================================================

    void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        timerText.text = Mathf.CeilToInt(currentTime).ToString();
    }


    // =========================================================
    // INITIALISATION DES VIES
    // =========================================================

    void InitializeLives()
    {
        if (lifeImages == null)
            return;

        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] != null)
            {
                lifeImages[i].SetActive(true);
            }
        }
    }


    // =========================================================
    // INITIALISATION DES FRAMES
    // =========================================================

    void InitializeAnimation()
    {
        if (animationFrames == null ||
            animationFrames.Length == 0)
        {
            Debug.LogWarning(
                "Aucune SpriteRenderer n'est assignée dans Animation Frames."
            );

            return;
        }

        // Désactive toutes les frames
        for (int i = 0; i < animationFrames.Length; i++)
        {
            if (animationFrames[i] != null)
            {
                animationFrames[i].gameObject.SetActive(false);
            }
        }

        // Première frame
        currentFrame = 0;

        if (animationFrames[currentFrame] != null)
        {
            animationFrames[currentFrame]
                .gameObject
                .SetActive(true);
        }
    }


    // =========================================================
    // MOUVEMENT DU CURSEUR
    // =========================================================

    void MoveCursor()
    {
        if (cursor == null || bar == null)
            return;

        Vector2 position = cursor.anchoredPosition;

        position.y += speed * direction * Time.deltaTime;

        float bottom = -bar.rect.height / 2f;
        float top = bar.rect.height / 2f;

        float cursorHalfHeight = cursor.rect.height / 2f;


        // Bord supérieur
        if (position.y >= top - cursorHalfHeight)
        {
            position.y = top - cursorHalfHeight;

            direction = -1f;
        }


        // Bord inférieur
        if (position.y <= bottom + cursorHalfHeight)
        {
            position.y = bottom + cursorHalfHeight;

            direction = 1f;
        }

        cursor.anchoredPosition = position;
    }


    // =========================================================
    // VÉRIFICATION DU TIMING
    // =========================================================

    void CheckTiming()
    {
        if (cursor == null ||
            blueZone == null ||
            greenZone == null)
        {
            return;
        }

        float cursorY = cursor.anchoredPosition.y;

        float blueY = blueZone.anchoredPosition.y;

        float greenY = greenZone.anchoredPosition.y;

        float blueHalfHeight =
            blueZone.rect.height / 2f;

        float greenHalfHeight =
            greenZone.rect.height / 2f;


        // =====================================================
        // PERFECT
        // =====================================================

        if (Mathf.Abs(cursorY - greenY) <= greenHalfHeight)
        {
            Debug.Log("PERFECT !");
            
            // Joue le son PERFECT
            PlaySFX(perfectSFX, 1.2f);

            NextAnimationFrame();

            if (playing)
            {
                MoveZonesRandomly();
            }
        }


        // =====================================================
        // GOOD
        // =====================================================

        else if (Mathf.Abs(cursorY - blueY) <= blueHalfHeight)
        {
            Debug.Log("GOOD !");
            
            // Joue le son GOOD
            PlaySFX(goodSFX, 1f);

            NextAnimationFrame();

            if (playing)
            {
                MoveZonesRandomly();
            }
        }


        // =====================================================
        // MISS
        // =====================================================

        else
        {
            // Joue le son MISS
            PlaySFX(missSFX, 0.8f);
            
            LoseLife();
        }
    }


    // =========================================================
    // PERD UNE VIE
    // =========================================================

    void LoseLife()
    {
        currentLives--;

        Debug.Log(
            "MISS ! Il reste " +
            currentLives +
            " vie(s)."
        );


        // -----------------------------------------------------
        // RETIRE UNE IMAGE DE VIE
        // -----------------------------------------------------

        int lostLifeIndex = currentLives;

        if (lifeImages != null &&
            lostLifeIndex >= 0 &&
            lostLifeIndex < lifeImages.Length)
        {
            if (lifeImages[lostLifeIndex] != null)
            {
                lifeImages[lostLifeIndex]
                    .SetActive(false);
            }
        }


        // -----------------------------------------------------
        // PLUS DE VIES
        // -----------------------------------------------------

        if (currentLives <= 0)
        {
            GameOver();
            return;
        }


        // -----------------------------------------------------
        // IL RESTE DES VIES
        // -----------------------------------------------------

        ResetCursorPosition();
    }


    // =========================================================
    // RESET DU CURSEUR
    // =========================================================

    void ResetCursorPosition()
    {
        if (cursor == null || bar == null)
            return;

        float bottom =
            -bar.rect.height / 2f;

        float cursorHalfHeight =
            cursor.rect.height / 2f;

        cursor.anchoredPosition =
            new Vector2(
                cursor.anchoredPosition.x,
                bottom + cursorHalfHeight
            );

        direction = 1f;
    }


    // =========================================================
    // GAME OVER
    // =========================================================

    void GameOver()
    {
        // Évite de déclencher Game Over plusieurs fois
        if (!playing)
            return;

        playing = false;

        // Joue le son de Game Over
        PlaySFX(gameOverSFX, 1f);

        // Affiche Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Affiche Restart
        ShowRestartButton();

        Debug.Log("GAME OVER !");
    }


    // =========================================================
    // DÉPLACEMENT DES ZONES
    // =========================================================

    void MoveZonesRandomly()
    {
        if (bar == null ||
            blueZone == null ||
            greenZone == null)
        {
            return;
        }

        float bottom =
            -bar.rect.height / 2f;

        float top =
            bar.rect.height / 2f;

        float blueHalfHeight =
            blueZone.rect.height / 2f;

        float minY =
            bottom + blueHalfHeight;

        float maxY =
            top - blueHalfHeight;

        float newBlueY =
            Random.Range(minY, maxY);


        // Zone bleue
        blueZone.anchoredPosition =
            new Vector2(
                blueZone.anchoredPosition.x,
                newBlueY
            );


        // -----------------------------------------------------
        // ZONE VERTE
        // -----------------------------------------------------

        float greenHalfHeight =
            greenZone.rect.height / 2f;

        float greenMinY =
            newBlueY
            - blueHalfHeight
            + greenHalfHeight;

        float greenMaxY =
            newBlueY
            + blueHalfHeight
            - greenHalfHeight;

        float newGreenY =
            Random.Range(
                greenMinY,
                greenMaxY
            );


        greenZone.anchoredPosition =
            new Vector2(
                greenZone.anchoredPosition.x,
                newGreenY
            );
    }


    // =========================================================
    // ANIMATION DE L'ALIEN
    // =========================================================

    void NextAnimationFrame()
    {
        if (animationFrames == null ||
            animationFrames.Length == 0)
        {
            return;
        }


        // Désactive la frame actuelle
        if (animationFrames[currentFrame] != null)
        {
            animationFrames[currentFrame]
                .gameObject
                .SetActive(false);
        }


        // Frame suivante
        currentFrame++;


        // =====================================================
        // DERNIÈRE FRAME
        // =====================================================

        if (currentFrame >= animationFrames.Length)
        {
            currentFrame =
                animationFrames.Length - 1;


            // Active la dernière frame
            if (animationFrames[currentFrame] != null)
            {
                animationFrames[currentFrame]
                    .gameObject
                    .SetActive(true);
            }


            // Alien libéré
            ActivateSuccess();

            return;
        }


        // Active la nouvelle frame
        if (animationFrames[currentFrame] != null)
        {
            animationFrames[currentFrame]
                .gameObject
                .SetActive(true);
        }
    }


    // =========================================================
    // SUCCÈS
    // =========================================================

    void ActivateSuccess()
    {
        // Arrête le chrono et le curseur
        playing = false;

        // Joue le son de succès
        PlaySFX(successSFX, 1.5f);

        // Passe le Rigidbody en Dynamic
        if (alienRigidbody != null)
        {
            alienRigidbody.bodyType =
                RigidbodyType2D.Dynamic;
        }

        // Start the coroutine to delay the success screen
        StartCoroutine(ShowSuccessWithDelay());

        Debug.Log(
            "SUCCESS ! Alien libéré à temps !"
        );
    }


    // =========================================================
    // COROUTINE POUR LE DELAI DU SUCCÈS
    // =========================================================

    IEnumerator ShowSuccessWithDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(successDelay);

        // Affiche Success
        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }

        // Affiche Restart
        ShowRestartButton();
    }


    // =========================================================
    // RESTART BUTTON
    // =========================================================

    void ShowRestartButton()
    {
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }
    }


    // =========================================================
    // RESTART
    // =========================================================

    public void RestartGame()
    {
        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }
}