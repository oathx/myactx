using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XBoxMotifyAttackWarning : MonoBehaviour
{
    public float durtion = 5f;
    private Material ps = null;
    public float elapse = 0f;
    public bool isstart = false;

    void Start()
    {
        ps = GetComponent<MeshRenderer>().material;
        Material temp_ps = new Material(ps);
        GetComponent<MeshRenderer>().material = temp_ps;
        ps = GetComponent<MeshRenderer>().material;
        elapse = 0f;
        isstart = false;
    }

    void Update()
    {
        if (ps != null)
        {
            float rate = elapse / durtion;
            elapse += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, rate);

            Color temp_color = new Color(1, 1, 1, alpha);
            ps.SetColor("_TintColor", temp_color);
        }
    }

    void OnEnable()
    {
        ps = GetComponent<MeshRenderer>().material;
        Material temp_ps = new Material(ps);
        GetComponent<MeshRenderer>().material = temp_ps;
        ps = GetComponent<MeshRenderer>().material;
        elapse = 0f;
        isstart = false;
        ps.SetColor("_TintColor", new Color(1, 1, 1, 0));
    }

    void OnDestroy()
    {
        isstart = false;
    }

    void OnDisable()
    {
        isstart = false;
    }
}
