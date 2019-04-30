using UnityEngine;
using System.Collections;

public class XHeatDistortEffect : MonoBehaviour
{

    [HideInInspector]
    static public int ret = 0;

    [HideInInspector]
    static public Camera cam = null;

    private Material mat;
    bool finished = false;

    private bool isEnd = false;

    void Start()
    {
        int layerNum = LayerMask.NameToLayer("Distort");
        gameObject.layer = layerNum;
    }

    void OnEnable()
    {
        ret++;
        finished = false;
        isEnd = false;
    }

    void OnDisable()
    {
        finished = true;
        if (!isEnd)
        {
            isEnd = true;
            ret--;
        }
    }

    void Update()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null && !ps.isPlaying && finished == false)
        {
            if (!ps.IsAlive())
            {
                finished = true;
                if (!isEnd)
                {
                    isEnd = true;
                    ret--;
                }
            }
        }
    }
}
