﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorTestMain : MonoBehaviour
{
    public GameObject projectorPrefab;
    public GameObject toProject1;

    protected GameObject projector;

    void Start()
    {
        Debug.Log(this.transform.position);
        projector = Instantiate(projectorPrefab, Vector3.zero, Quaternion.identity);

        DrawProjector dp = projector.GetComponent<DrawProjector>();
        dp.fn = "cube.jpg";
    }


    [ContextMenu("GenerateObj")]
    void GenerateObj() {
        DrawProjector dp = projector.GetComponent<DrawProjector>();
        //Math3DUtils.MeshDivide(this.toProject1, 3);
       // dp.GenerateGOUsingTriangleFn(this.toProject1);
        dp.GenerateGO(this.toProject1);
        //dp.GenerateGO1FaceUsingTriangleFn(this.toProject1, 2);
        //dp.GenerateGO1Face(this.toProject1, 2);
    }
}
