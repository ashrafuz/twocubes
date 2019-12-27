using UnityEngine;

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
}