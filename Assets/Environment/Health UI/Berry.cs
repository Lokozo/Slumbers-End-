using UnityEngine;

public class Berry : MonoBehaviour, IInteractable
{
    public float hungerRestore = 20f;
    public float energyRestore = 10f;

    public void Interact(GameObject interactor)
    {
        PlayerStats stats = interactor.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.ModifyHunger(hungerRestore);
            stats.ModifyEnergy(energyRestore);
            Debug.Log("Berry consumed! Hunger and energy restored.");
        }

        Destroy(gameObject);
    }
}
