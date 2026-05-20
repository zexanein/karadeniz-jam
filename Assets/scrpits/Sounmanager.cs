using System.Collections.Generic;
using UnityEngine;

public class Sounmanager : MonoBehaviour
{
    AudioSource source;

    [SerializeField] List<AudioClip> clips = new List<AudioClip>();
    [SerializeField] List<AudioClip> cats = new List<AudioClip>();
    [SerializeField] List<AudioClip> rats= new List<AudioClip>();
    [SerializeField] List<AudioClip> dash = new List<AudioClip>();

    private void Start()
    {
        source = GetComponent<AudioSource>();  
    }

    public void PlaySound(string path)
    {
        if (path == "cat")
        {
            int random = Random.Range(0,cats.Count);
            source.PlayOneShot(cats[random]);
        }
        if (path == "rat")
        {
            int random = Random.Range(0, rats.Count);
            source.PlayOneShot(rats[random]);
        }
        if (path == "dash")
        {
            int random = Random.Range(0, dash.Count);
            source.PlayOneShot(dash[random]);
        }

        if (path == "mrr")
            source.PlayOneShot(clips[0]);
        if (path == "pence")
            source.PlayOneShot(clips[1]);
    
    }

}
