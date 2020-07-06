﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ToastPlugin;
using TreeEditor;
using UnityEngine;
using UnityEngine.UI;

public class DrawProjector : MonoBehaviour
{
    public GameObject plane; //where to display the photo
    public GameObject spherePrefab;

    public GameObject cube;

    bool debug = true;
    private int w;
    private int h;
    int vfov = 60;
    float far = 1f; //how far should we place the projector for the projected plane

    public string fn = null;

    // Draw frame and put it 1m far from camera
    void Start() {
        this.name = "Projector " + fn;

        //load texture
        if (!String.IsNullOrEmpty(fn)) { //otherwise display already loaded texture projectcube.jpg
            string path = UnityEngine.Application.persistentDataPath + "/" + fn;
            
            if (!System.IO.File.Exists(path)) {
                Debug.LogError("file not found: " + path);
            }
            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(fileData); //load also width/height
            plane.GetComponent<Renderer>().material.mainTexture = tex;

            //project texture
            this.GetComponent<Projector>().material = new Material(Shader.Find("Projector/Multiply"));
            Material thisProjectorMat = this.GetComponent<Projector>().material;
            thisProjectorMat.SetTexture("_ShadowTex", tex);
            thisProjectorMat.SetTexture("_FalloffTex", tex);
            this.GetComponent<Projector>().material = thisProjectorMat;


            w = tex.width;
            h = tex.height;
        } else {
            w = 4618;
            h = 3464;
        }
        
        Debug.Log("w:" + w + " h:" + h);
        this.GetComponent<Camera>().aspect = this.GetComponent<Projector>().aspectRatio = (float)w / h;

        float hfov = getHorizontalFov();
        Debug.Log("hfov:" + hfov);

        var halfw = getHalfWidth();
        var halfh = getHalfHeight();

        Vector3 centerplane = this.transform.position + this.transform.forward * far; //an alternative is to use go.transform.Translate
        plane.transform.position = centerplane;
        plane.transform.localScale = new Vector3(halfw * 2 * 0.1f, 0.1f, halfh * 2 * 0.1f);

        /*
        Vector3 righthalf = this.transform.right * halfw;
        Vector3 uphalf = this.transform.up * halfh;
        Vector3 ne = centerplane + righthalf + uphalf;
        Vector3 se = centerplane + righthalf - uphalf;
        Vector3 sw = centerplane - righthalf - uphalf;
        Vector3 nw = centerplane - righthalf + uphalf;
        */
        /*
        Vector3 ne = ProjectOnPlaneCenter(new Vector2(1, 1));
        Vector3 se = ProjectOnPlaneCenter(new Vector2(1, -1));
        Vector3 sw = ProjectOnPlaneCenter(new Vector2(-1, -1));
        Vector3 nw = ProjectOnPlaneCenter(new Vector2(-1, 1));
        */

        Vector3 ne = ProjectOnPlaneViewport(new Vector2(1, 1));
        Vector3 se = ProjectOnPlaneViewport(new Vector2(1, 0));
        Vector3 sw = ProjectOnPlaneViewport(new Vector2(0, 0));
        Vector3 nw = ProjectOnPlaneViewport(new Vector2(0, 1));

        Debug.DrawLine(this.transform.position, centerplane, Color.blue, 99f);
        Debug.DrawLine(this.transform.position, ne, Color.white, 99f);
        Debug.DrawLine(this.transform.position, se, Color.white, 99f);
        Debug.DrawLine(this.transform.position,sw, Color.white, 99f);
        Debug.DrawLine(this.transform.position, nw, Color.white, 99f);       

        GameObject go = Instantiate(spherePrefab, ne, Quaternion.identity);
        go.transform.parent = this.transform;
        go = Instantiate(spherePrefab, se, Quaternion.identity);
        go.transform.parent = this.transform;
        go = Instantiate(spherePrefab, sw, Quaternion.identity);
        go.transform.parent = this.transform;
        go = Instantiate(spherePrefab, nw, Quaternion.identity);
        go.transform.parent = this.transform;

        //https://forum.unity.com/threads/solved-image-projection-shader.254196/
    }

    Vector3 ProjectOnPlaneCenter(Vector2 uv) { //center is (0,0)
        Vector3 centerplane = plane.transform.position; //calculated in Start()
        var halfw = getHalfWidth();
        var halfh = getHalfHeight();
        Vector3 right = this.transform.right * uv.x * halfw;
        Vector3 up = this.transform.up * uv.y * halfh;
        return centerplane + right + up;
    }

    Vector3 ProjectOnPlaneViewport(Vector3 uv) { //center is (0.5, 0.5)
        Vector3 centerplane = plane.transform.position; //calculated in Start()
        var halfw = getHalfWidth();
        var halfh = getHalfHeight();
        Vector3 right = this.transform.right * (uv.x - 0.5f) * 2 * halfw;
        Vector3 up = this.transform.up * (uv.y - 0.5f) * 2 * halfh;
        return centerplane + right + up;
    }

    GameObject InstSphere(Vector3 vector3, Color color) {
        GameObject o = Instantiate(spherePrefab, vector3, Quaternion.identity);
        o.GetComponent<Renderer>().material.color = color;
        return o;
    }

    float getHorizontalFov() {
        var aspect = this.w * 1.0f / this.h;
        var vFovRad = this.vfov * Mathf.Deg2Rad;
        var radHFOV = 2 * Mathf.Atan(Mathf.Tan(vFovRad / 2) * aspect);
        var hFOV = Mathf.Rad2Deg * radHFOV;
        return hFOV;
    }

    float getHalfWidth() {
        // https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
        var hFovDeg = this.getHorizontalFov();
        return 1.0f * this.far * Mathf.Tan(Mathf.Deg2Rad * (hFovDeg / 2));
    }

    float getWidth() {
        return this.getHalfWidth() * 2;
    }

    float getHalfHeight() {
        return this.far * Mathf.Tan(Mathf.Deg2Rad * (this.vfov / 2));
    }

    float getHeight() {
        return this.getHalfHeight() * 2;
    }

    public Vector3 projectSimple(Vector3 v, float z) {
        float y2 = (z * v.y) / v.z;
        float x2 = (z * v.x) / v.z;
        return new Vector3(x2, y2, z);
    }

    //alternative to WorldToViewportPoint if camera rotation is (0,0,0)
    public Vector2 WorldToViewportPointSimple(Vector3 worldV) {
        Vector3 projected = projectSimple(worldV, 1.0f);

        if(debug) {
            GameObject pgo = InstSphere(projected, Color.cyan);
            pgo.transform.rotation = this.transform.rotation;
        }

        float u = projected.x / getWidth() + 0.5f;
        float v = projected.y / getHeight() + 0.5f;

        return new Vector2(u, v);
    }

    public Vector2 WorldToViewportPoint(Vector3 position) {
        Vector2 uv =  this.GetComponent<Camera>().WorldToViewportPoint(position);
        if(debug) {
            //Vector3 projected = this.transform.forward + uv * new Vector2(getWidth(), getHeight);
            //GameObject pgo = InstSphere(projected, Color.cyan);
            //pgo.transform.rotation = this.transform.rotation;

            Vector3 projected = ProjectOnPlaneViewport(uv);
            GameObject pgo = InstSphere(projected, Color.cyan);
        }
        return uv;

    }

    public void GenerateGO1Face(GameObject go, int numface = 3) {
        Mesh m = go.GetComponent<MeshFilter>().mesh;
        //main face : 4 6 7 5

        Debug.Log("vertices: "+ m.vertices.Length+ " triangles: "+ m.triangles.Length);

        // 6 faces, 36 triangles, 24 vertices
        int arr = m.triangles[0];

        string wavefrontV = "";
        string wavefrontVT = "";
        string wavefrontF = "";

        String n = Path.GetFileNameWithoutExtension(this.fn);

        //for (int i = 0 ; i<m.triangles.Length; i++) Debug.Log("triangle: " + i + " -> "+ m.triangles[i]);

        for (int i = 0; i < 6; i++) {
            int numv = m.triangles[numface * 6 + i];
            Vector3 lVertex = m.vertices[numv]; //1 behind

            Vector3 wVertex = go.transform.TransformPoint(lVertex); //idem cube.transform.localToWorldMatrix.MultiplyPoint3x4(localVertex);
            if (debug) {
                GameObject o = InstSphere(wVertex, Color.red);
            }
            wavefrontV += "v "+wVertex.x+" "+wVertex.y+" "+wVertex.z+" 1.0\n";

            Vector3 uv = this.WorldToViewportPoint(wVertex);
            wavefrontVT += "vt "  + uv.x + " " + uv.y + "\n";
        }
        wavefrontF += "f 1/1 2/2 3/3\n";
        wavefrontF += "f 4/4 5/5 6/6\n";

        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + n + ".obj");
        writer.Write("# BlaBla\n\nmtllib ./" + n + ".mtl\n\n" + wavefrontV + "\n" + wavefrontVT + "\n" + wavefrontF);
        writer.Close();
    }

    public void GenerateGO(GameObject go) {
        Mesh m = go.GetComponent<MeshFilter>().mesh;

        // 6 faces, 36 triangles, 24 vertices

        string wavefrontV = "";
        string wavefrontVT = "";
        string wavefrontF = "";

        String n = Path.GetFileNameWithoutExtension(this.fn);

        for (int i = 0; i < m.triangles.Length; i++) {
            int numv = m.triangles[i];
            Vector3 lVertex = m.vertices[numv];

            Vector3 wVertex = go.transform.TransformPoint(lVertex);
            if (debug) {
                GameObject o = InstSphere(wVertex, Color.red);
            }
            wavefrontV += "v " + wVertex.x + " " + wVertex.y + " " + wVertex.z + " 1.0\n";

            Vector2 uv = this.WorldToViewportPoint(wVertex);
            wavefrontVT += "vt " + uv.x + " " + uv.y + "\n";

            if (i % 3 == 0) {
                wavefrontF += "f " + (i + 1) + "/" + (i + 1) + " " + (i + 2) + "/" + (i + 2) + " " + (i + 3) + "/" + (i + 3) + "\n";
            }
        }

        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + n + ".obj");
        writer.Write("# BlaBla\n\nmtllib ./" + n + ".mtl\n\n" + wavefrontV + "\n" + wavefrontVT + "\n" + wavefrontF);
        writer.Close();
    }
    

    public void GenerateGO1FaceUsingTriangleFn(GameObject go, int numface = 2) {
        string wavefrontV = "";
        string wavefrontVT = "";
        string wavefrontF = "";

        String n = Path.GetFileNameWithoutExtension(this.fn);

        for (int i = 0; i<2; i++) {
            GenerateTriangle(go, numface*2+i, ref wavefrontV, ref wavefrontVT, ref wavefrontF);
        }

        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + n + ".obj");
        writer.Write("# BlaBla\n\nmtllib ./" + n + ".mtl\n\n" + wavefrontV + "\n" + wavefrontVT + "\n" + wavefrontF);
        writer.Close();
    }
    

    public void GenerateGOUsingTriangleFn(GameObject go) {
        string wavefrontV = "";
        string wavefrontVT = "";
        string wavefrontF = "";

        Mesh m = go.GetComponent<MeshFilter>().mesh;
        String n = Path.GetFileNameWithoutExtension(this.fn);

        for (int numt = 0; numt < m.triangles.Length/3; numt ++) {
            GenerateTriangle(go, numt, ref wavefrontV, ref wavefrontVT, ref wavefrontF);
        }

        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + n + ".obj");
        writer.Write("# BlaBla\n\nmtllib ./" + n + ".mtl\n\n" + wavefrontV + "\n" + wavefrontVT + "\n" + wavefrontF);
        writer.Close();
    }


    // Generate without optimisation (duplicated vertices)
    void GenerateTriangle(GameObject go, int numtriangle, ref string v, ref string vt, ref string f)
    {
        Mesh m = go.GetComponent<MeshFilter>().mesh;
        f += "f ";

        for (int i = 0; i < 3; i++) {
            int numt = numtriangle * 3 + i;
            int numv = m.triangles[numt];
            Vector3 lVertex = m.vertices[numv];

            Vector3 wVertex = go.transform.TransformPoint(lVertex); //idem cube.transform.localToWorldMatrix.MultiplyPoint3x4(localVertex);
            if(debug) {
                GameObject o = InstSphere(wVertex, Color.red);
            }
                
            v += "v " + wVertex.x + " " + wVertex.y + " " + wVertex.z + " 1.0\n";

            Vector2 uv = this.WorldToViewportPoint(wVertex);
            vt += "vt " + uv.x + " " + uv.y + "\n";

            int lines = v.Split('\n').Length - 1;
            f += lines + "/" + lines + " ";
        }
        f += "\n";
    }
}
