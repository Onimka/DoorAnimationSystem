using System.Collections;
using UnityEngine;

public class DoorAnimateSystem : MonoBehaviour
{
    // The door on the stage should be closed,
    // at the start it will open automatically at the angle you need,
    // just mark "Open" as true

    [SerializeField] private bool Open;

    [Header("Setting up animations")]
    [SerializeField] private AnimationSettings animationSettings;

    [Header("Sound Settings")]
    [SerializeField]  private AudioSettings audioSettings;

    [Header("For locked doors ")]
    [SerializeField] private bool RequiresKey = false;

    [SerializeField] private float ShakeDuration = 0.5f;
    [SerializeField] private float ShakeIntensity = 0.15f;

    [Header("Advanced Settings")]
    [SerializeField] private AxisRotation axisRotation = AxisRotation.rotY;

    private bool isOpening = false;
    private bool isClosing = false;
    private float timer = 0f;
    private Quaternion initialRotation;

    private void Start()
    {
        audioSettings.audioSource = GetComponent<AudioSource>();
        initialRotation = transform.rotation;
        if (Open)
        {
            switch (axisRotation)
            {
                case AxisRotation.rotX:
                    transform.Rotate(animationSettings.openAngle, 0, 0); 
                    break;
                case AxisRotation.rotY:
                    transform.Rotate(0, animationSettings.openAngle, 0);
                    break;
                case AxisRotation.rotZ:
                    transform.Rotate(0, 0, animationSettings.openAngle);
                    break;
            }
        }
    }
    private bool HasKey()
    {
        // There should be a logic for checking the presence of a key here
        // You can use a global variable to store information about the availability of a key
        return true;
    }
    public void Interaction()
    {
        audioSettings.SoundStop();

        timer = 0f;
        isClosing = false;
        isOpening = false;


        if (!RequiresKey)
        {
            Open = !Open;
            if (Open) OpenDoor();
            else CloseDoor();
        }
        else
        {
            if (!HasKey())
            {
                StartCoroutine(ShakeDoor()); // If there is no key, we call coroutin to shake the door
                return;
            }
            else
            {
                audioSettings.SoundPlay(audioSettings.requiresKey_Open, audioSettings.Volume_requiresKey_Open);
                RequiresKey = false;
            }
        }
    }
    private void OpenDoor()
    {
            isClosing = false;
            isOpening = true;
        StartCoroutine(OpenDoorCor());
        audioSettings.SoundPlay(audioSettings.openSound, audioSettings.Volume_openSound);
    }
    private void CloseDoor()
    {
        isOpening = false;
        isClosing = true;
        StartCoroutine(CloseDoorCor());
        audioSettings.SoundPlay(audioSettings.closeSound, audioSettings.Volume_closeSound);
        
    }

    IEnumerator OpenDoorCor()
    {
        while (isOpening)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / animationSettings.openDuration);
            float curveValue = animationSettings.openCurve.Evaluate(t);

            switch (axisRotation)
            {
                case AxisRotation.rotX:
                    transform.rotation = Quaternion.Lerp(initialRotation, initialRotation * Quaternion.Euler(animationSettings.openAngle, 0, 0), curveValue);
                    break;
                case AxisRotation.rotY:
                    transform.rotation = Quaternion.Lerp(initialRotation, initialRotation * Quaternion.Euler(0, animationSettings.openAngle, 0), curveValue);
                    break;
                case AxisRotation.rotZ:
                    transform.rotation = Quaternion.Lerp(initialRotation, initialRotation * Quaternion.Euler(0, 0, animationSettings.openAngle), curveValue);
                    break;
            }

            if (t >= 1f)
            {
                isOpening = false;
                timer = 0f;
                audioSettings.SoundStop();
            }
            yield return null;
        }
    }

    IEnumerator CloseDoorCor()
    {
        while (isClosing)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / animationSettings.closeDuration);
            float curveValue = animationSettings.closeCurve.Evaluate(t);
            switch (axisRotation)
            {
                case AxisRotation.rotX:
                    transform.rotation = Quaternion.Lerp(initialRotation * Quaternion.Euler(animationSettings.openAngle, 0, 0), initialRotation, curveValue);
                    break;
                case AxisRotation.rotY:
                    transform.rotation = Quaternion.Lerp(initialRotation * Quaternion.Euler(0, animationSettings.openAngle, 0), initialRotation, curveValue);
                    break;
                case AxisRotation.rotZ:
                    transform.rotation = Quaternion.Lerp(initialRotation * Quaternion.Euler(0, 0, animationSettings.openAngle), initialRotation, curveValue);
                    break;
            }

            if (t >= 1f)
            {
                isClosing = false;
                timer = 0f;
                audioSettings.SoundStop();
                audioSettings.SoundPlay(audioSettings.closeEndSound, audioSettings.Volume_closeEndSound);
            }
            yield return null;
        }
    }

    private IEnumerator ShakeDoor()
    {
        Quaternion initialRotation = transform.rotation;

        float timer = 0f;
        audioSettings.SoundPlay(audioSettings.requiresKey_DontOpen, audioSettings.Volume_requiresKey_DontOpen);
        
        while (timer < ShakeDuration)
        {
            timer += Time.deltaTime;
            float shakeX = Random.Range(-ShakeIntensity, ShakeIntensity);
            float shakeY = Random.Range(-ShakeIntensity, ShakeIntensity);
            transform.rotation = initialRotation * Quaternion.Euler(shakeX, shakeY, 0f);
            yield return null;
        }

        transform.rotation = initialRotation; 
        audioSettings.SoundStop();
    }   
}

public enum AxisRotation
{
    rotX = 0,
    rotY = 1,
    rotZ = 2,
}

[System.Serializable]
public class AudioSettings
{
    public AudioSource audioSource;
    [Header("Sounds for doors")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip closeEndSound;
    public AudioClip requiresKey_DontOpen;
    public AudioClip requiresKey_Open;

    [Header("Adjusting the volume for sounds")]
    [Range(0f, 20f)] public float Volume_openSound = 1;
    [Range(0f, 20f)] public float Volume_closeSound = 1;
    [Range(0f, 20f)] public float Volume_closeEndSound = 1;
    [Range(0f, 20f)] public float Volume_requiresKey_DontOpen = 1;
    [Range(0f, 20f)] public float Volume_requiresKey_Open = 1;

    public void SoundPlay(AudioClip clipSound, float volumeSound)
    {
        if (audioSource && clipSound) audioSource.PlayOneShot(clipSound, volumeSound);
    }
    public void SoundStop()
    {
        if (audioSource) audioSource.Stop();
    }
}

[System.Serializable]
public class AnimationSettings
{
    public AnimationCurve openCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve closeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float openDuration = 1f;
    public float closeDuration = 1f;
    public float openAngle = 90f;
}