using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (MeshFilter))]
public class Block : MonoBehaviour {
    public MaterialComboAsset m_ComboAsset;
    private MeshRenderer m_MeshRend;

    private void Awake () {
        m_MeshRend = GetComponent<MeshRenderer> ();
    }

    public void SetType (BoxType _boxType) {
        m_MeshRend.material = m_ComboAsset.GetMatByType (_boxType);
    }

    public void SetRandomType () {
        int rt = Random.Range (0, m_ComboAsset.m_ComboList.Length);
        SetType ((BoxType) rt);
    }

    private void OnTriggerEnter (Collider other) {
        // Debug.Log ("other " + other.transform.GetComponent<MeshRenderer> ().material.name);
        // Debug.Log ("my mat " + m_MeshRend.material.name);

        if (string.Equals (other.transform.GetComponent<MeshRenderer> ().material.name, m_MeshRend.material.name)) {
            GameEventManager.OnRightCollide?.Invoke ();
        } else {
            GameEventManager.OnWrongCollide?.Invoke ();
        }
    }
}