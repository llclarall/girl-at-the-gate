using UnityEngine;
using System.Collections;

/// <summary>
/// This script is responsible for controlling the audio playback based on the movement of a specified target.
/// </summary>

[RequireComponent(typeof(AudioSource))]
public class MotionAudioController : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Vector3 lastPosition;

    [SerializeField, Tooltip("Threshold for movement detection. Adjust as needed.")]
    private float movementThreshold = 0.0001f;
    
    [SerializeField, Tooltip("Duration of the fade-out effect in seconds")]
    private float fadeOutDuration = 0.5f;

    private bool wasMoving = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        animator = GetComponentInParent<Animator>();

        if (animator != null)
        {
            targetTransform = animator.transform;
        }
        else
        {
            Debug.LogError("No Animator component found in parent or its children.");
        }

        if (targetTransform != null)
        {
            lastPosition = targetTransform.position;
        }
    }

    void Update()
    {
        if (targetTransform == null)
        {
            return;
        }

        float movement = Vector3.Distance(targetTransform.position, lastPosition);
        bool isMoving = movement > movementThreshold;

        if (isMoving && !wasMoving)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            
            audioSource.volume = 1f;
            
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else if (!isMoving && wasMoving)
        {
            if (audioSource.isPlaying)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeOut());
            }
        }

        wasMoving = isMoving;

        lastPosition = targetTransform.position;
    }

    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float currentTime = 0;

        while (currentTime < fadeOutDuration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeOutDuration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
        fadeCoroutine = null;
    }
}