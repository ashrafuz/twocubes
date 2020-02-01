using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameEventManager : MonoBehaviour {
    public static Action<int> OnNewTrackGenerated;
    public static Action OnRightCollide;
    public static Action OnWrongCollide;
    public static Action OnNoMoreLivesLeft;

    private void Awake () {
        OnNewTrackGenerated = null;
        OnRightCollide = null;
        OnWrongCollide = null;
        OnNoMoreLivesLeft = null;
    }
}