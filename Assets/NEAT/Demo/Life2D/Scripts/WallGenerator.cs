using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour
{

    public GameObject _wall1;
    public GameObject _wall2;
    public GameObject _wall3;
    public GameObject _wall4;
    public GameObject _ground;

    void Start()
    {
        float size = _ground.transform.localScale.x;

        Vector3 pos1 = new Vector3(0f, size / 2 + .5f, 0f);
        Vector3 pos2 = new Vector3(0f, -size / 2 - .5f, 0f);
        Vector3 pos3 = new Vector3(size / 2 + .5f, 0f, 0f);
        Vector3 pos4 = new Vector3(-size / 2 - .5f, 0f, 0f);

        Vector3 scale1 = new Vector3(size + 2, 1f, 1f);
        Vector3 scale2 = new Vector3(1f, size + 2, 1f);

        _wall1.transform.position = pos1;
        _wall2.transform.position = pos2;
        _wall3.transform.position = pos3;
        _wall4.transform.position = pos4;

        _wall1.transform.localScale = scale1;
        _wall2.transform.localScale = scale1;
        _wall3.transform.localScale = scale2;
        _wall4.transform.localScale = scale2;

        _wall1.SetActive(true);
        _wall2.SetActive(true);
        _wall3.SetActive(true);
        _wall4.SetActive(true);
    }

    void Update()
    {

    }
}
