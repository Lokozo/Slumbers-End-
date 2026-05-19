using System.Collections.Generic;
using UnityEngine;

public class CraftingStationManager : MonoBehaviour
{
    public static CraftingStationManager Instance;

    private HashSet<CraftingStationType> unlockedStations =
        new HashSet<CraftingStationType>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UnlockStation(CraftingStationType station)
    {
        if (station == CraftingStationType.None)
            return;

        unlockedStations.Add(station);

        Debug.Log("Unlocked station: " + station);
    }

    public bool HasStation(CraftingStationType station)
    {
        if (station == CraftingStationType.None)
            return true;

        return unlockedStations.Contains(station);
    }
}