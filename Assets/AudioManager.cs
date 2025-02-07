using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("audio Source")]
    [SerializeField] AudioSource SFXSource;

    [Header("audio Clip")]
    public AudioClip attackSound;
    public AudioClip clickSound;
    public AudioClip comboSound;
    public AudioClip enemyAttackSound;
    public AudioClip enemyDeathSound;
    public AudioClip piecesdestroySound;

    public void SFXClip(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
