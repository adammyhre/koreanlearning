using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Hangul : MonoBehaviour {
    
    [SerializeField] GameObject[] romanization;
    [SerializeField] AudioClip audioClip;
    [SerializeField] GameObject vfx;
    AudioSource audioSource;
    
    [Header("Tween Settings")]
    [SerializeField] float tweenDuration = 3f;
    [SerializeField] float tweenStrength = 0.85f;
    [SerializeField] int tweenVibrato = 10;
    [SerializeField] float tweenRandomness = 90f;
    
    void Start() {
        audioSource = GetComponent<AudioSource>();
    }
    
    public void Reveal() {
        Instantiate(vfx, romanization[0].transform.position, Quaternion.identity);
        audioSource.PlayOneShot(audioClip);
        foreach (var character in romanization) {
            character.SetActive(true);
            character.transform.DOShakePosition(tweenDuration, tweenStrength, tweenVibrato, tweenRandomness, false,
                true);
        }
    }
}
