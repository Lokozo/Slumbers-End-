using UnityEngine;
using System.Collections;

public class TumorCheck : MonoBehaviour
{
    public bool tumorActivated = false;

    [Header("Effects")]
    public GameObject fireEffects;

    [Header("Audio")]
    public AudioSource tumorAudio;

    [Header("Required Item")]
    public Item lighterItem;

    [Header("Optional Cinematic")]
    public CutsceneTrigger tumorCutscene;

    [Header("Ladder")]
    public Ladder basementLadder;

    private void Start()
    {
        // Fire OFF at start
        if (fireEffects != null)
            fireEffects.SetActive(false);

        // Sound OFF at start
        if (tumorAudio != null)
        {
            tumorAudio.Stop();
            tumorAudio.playOnAwake = false;
        }

        // Disable ladder at start
        if (basementLadder != null)
            basementLadder.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

 
        if (tumorActivated)
            return;

        // Check if player has lighter
        if (PlayerInventory.Instance.HasResource(lighterItem))
        {
            
            tumorActivated = true;

          
            GetComponent<Collider>().enabled = false;

          
            if (tumorCutscene != null)
            {
                StartCoroutine(StartTumorEvent());
            }
            else
            {
                StartCoroutine(StartFireEvent());
            }
        }
        else
        {
            Debug.Log("Need Lighter");
        }
    }

    private IEnumerator StartTumorEvent()
    {
     
        CutsceneManager.Instance.PlayCutscene(
            tumorCutscene.cutsceneParent);

     
        yield return null;


        while (!CutsceneManager.IsCutscenePlaying)
        {
            yield return null;
        }

        while (CutsceneManager.IsCutscenePlaying)
        {
            yield return null;
        }

       
        yield return StartCoroutine(StartFireEvent());

        if (basementLadder != null)
            basementLadder.enabled = true;

        Debug.Log("Tumor Burned");
    }

    private IEnumerator StartFireEvent()
    {
        yield return new WaitForSeconds(0.5f);

       
        if (fireEffects != null)
            fireEffects.SetActive(true);

        // Play fire sound
        if (tumorAudio != null)
            tumorAudio.Play();
    }
}