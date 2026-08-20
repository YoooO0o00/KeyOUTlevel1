using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    private AudioSource audioSource;

    void Start()
    {
        // Récupère l'AudioSource attachée au même GameObject
        audioSource = GetComponent<AudioSource>();
        
        // Joue la musique en boucle
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void LoadScene()
    {
        // Arrête la musique avant de charger
        if (audioSource != null)
            audioSource.Stop();
            
        SceneManager.LoadScene(sceneName);
    }
}