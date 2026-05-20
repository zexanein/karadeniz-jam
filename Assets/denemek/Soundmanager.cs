using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Rendering;

public class Soundmanager : MonoBehaviour
{
    [SerializeField] AudioClip TrunPaperAudio;

    [SerializeField] private AudioClip[] _walksounds, WriteSounds, PutOnScale, PutonFloor, ScaleMove, ManTalk;

    AudioSource source;
    public static bool ScaleEffectSound, WriteNoteBookSound,TurnPaperSound, WalkCostumerSound,PutOnScaleSound, PutonFloorSound, ManTalkSound;


    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        float pitch = 1;
        if (ScaleEffectSound)
        {
            pitch = 1;
            ScaleEffectSound = false;
            int rast = Random.Range(0, ScaleMove.Length);
            source.PlayOneShot(ScaleMove[rast]);
        }
        if (WriteNoteBookSound)
        {
            pitch = 1;
            WriteNoteBookSound = false;
            int rast = Random.Range(0, WriteSounds.Length);
            source.PlayOneShot(WriteSounds[rast]);
        }
        if (TurnPaperSound)
        {
            pitch = 1;
            TurnPaperSound = false;
            source.PlayOneShot(TrunPaperAudio);
        }
        if (WalkCostumerSound)
        {
            pitch = 1;
            WalkCostumerSound = false;
            int rast = Random.Range(0, 2);
            source.PlayOneShot(_walksounds[rast]);
        }
        if (PutOnScaleSound)
        {
            pitch = 1;
            PutOnScaleSound = false;
            int rast = Random.Range(0, PutOnScale.Length);
            source.PlayOneShot(PutOnScale[rast]);
        }
        if (PutonFloorSound)
        {
            pitch = 1;
            PutonFloorSound = false;
            int rast = Random.Range(0,PutonFloor.Length);
            source.PlayOneShot(PutonFloor[rast]);
        }
        if (ManTalkSound)
        {
            ManTalkSound = false;
            pitch = Random.Range(1, 1.5f);
            int rast = Random.Range(0, ManTalk.Length);
            source.PlayOneShot(ManTalk[rast]);
        }
        source.pitch = pitch;
    }

}
