using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static AudioClip moveSound, castleSound, checkSound, captureSound;
    static AudioSource audioSrc;

    // Start is called before the first frame update
    void Start()
    {
        moveSound = Resources.Load("move1", typeof(AudioClip)) as AudioClip;
        castleSound = Resources.Load("castle1", typeof(AudioClip)) as AudioClip;
        checkSound = Resources.Load("check1", typeof(AudioClip)) as AudioClip;
        captureSound = Resources.Load("capture1", typeof(AudioClip)) as AudioClip;
        audioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void PlaySound(string clip){
        switch (clip)
        {
            case "move1":
                audioSrc.PlayOneShot(moveSound);
                break;
            case "check1":
                audioSrc.PlayOneShot(checkSound);
                break;
            case "capture1":
                audioSrc.PlayOneShot(captureSound);
                break;
            case "castle1":
                audioSrc.PlayOneShot(castleSound);
                break;
            default:
                break;
        }
    }
}
