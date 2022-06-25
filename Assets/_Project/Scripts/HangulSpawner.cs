using System.Collections.Generic;
using UnityEngine;

public class HangulSpawner : MonoBehaviour {

    [Header("Hangul Characters")]
    public List<Hangul> hangulPrefabs;

    public void Spawn(string objectName) {
        Debug.Log("Spawning " + objectName);
        foreach (var item in hangulPrefabs) {
            if (objectName == item.name) {
                item.gameObject.SetActive(true);
                item.Reveal();
            }
        }
    }
}
