using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    [Header("Settings")] 
    public bool fadeOut;
    
    [Header("Music")] 
    public AudioPlay playList;
    public AudioPlay ambLoop;
    public AudioSource menuMusic;
    public AudioPlay exitMenuMusic;
    
    [Header("Sound effects")] 
    public AudioPlay entryStinger;
    public AudioPlay summonGame;
    public AudioPlay summonKnight;
    public AudioPlay score;
    public AudioPlay victoryStinger;
    public AudioPlay defeatStinger;
    public AudioPlay killKnight;
    public AudioPlay moveKnight;
    public AudioPlay wrongMove;
    public AudioPlay loosePoint;
}
