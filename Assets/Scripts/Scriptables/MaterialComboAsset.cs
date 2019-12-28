using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatBlockCombo {
    public BoxType m_BoxType;
    public Material m_Material;
}

[CreateAssetMenu]
public class MaterialComboAsset : ScriptableObject {

    public MatBlockCombo[] m_ComboList;

    public Material GetMatByType (BoxType _btype) {
        foreach (var item in m_ComboList) {
            if (item.m_BoxType == _btype) {
                return item.m_Material;
            }
        }

        Debug.LogError ("Couldn't find material for type " + _btype);
        return null;
    }
}