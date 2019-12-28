using System;
using UnityEngine;

public enum BoxType {

    NONE = -1,
    GREEN = 0,
    BLUE,
    YELLOW
}

public class GameConstants {
    public const string MESH_DATA_FILE_NAME = "path_data.asset";
    public const string MESH_DATA_FULL_PATH = "Assets/Data/" + MESH_DATA_FILE_NAME;
}

public class MeshPoint {
    public Vector3 position;
    public Quaternion rotation;

    public Vector3 LocalToWorld (Vector3 _localSpace) {
        return this.position + rotation * _localSpace;
    }

    public Vector3 LocalToWorldNormal (Vector3 _localSpace) {
        return rotation * _localSpace;
    }
}

public static class GameMath {
    public const float TAU = 6.283185f;

    public static Vector2 GetPositionWithRadius (Vector2 _center, float _radius, float _angleInRad) {
        float xPos = Mathf.Cos (_angleInRad) * _radius;
        float yPos = Mathf.Sin (_angleInRad) * _radius;
        return _center + new Vector2 (xPos, yPos);
    }
}

public class Ticker {
    private float frequencyTime;
    public float currentPassedTime;
    public bool isRunning;
    public Action actionToRun;

    public Ticker () {
        Reset ();
    }

    public virtual void Reset () {
        isRunning = false;
        currentPassedTime = 0;
    }

    public void UpdateTick (float _dt) {
        currentPassedTime += _dt;
    }

    public void QueAction (Action _action) {
        actionToRun = _action;
    }

    public void Execute (bool _willPersist = false) {
        if (actionToRun != null) {
            actionToRun ();
            actionToRun = _willPersist ? actionToRun : null;
            Reset ();
            if (_willPersist) Start ();
        }
    }

    public void SetFrequency (float _fr) {
        frequencyTime = _fr;
    }

    public float GetFrequency () {
        return frequencyTime;
    }

    public bool HasEnoughTimePassed () {
        return currentPassedTime > GetFrequency ();
    }

    public void Start () {
        isRunning = true;
    }
}