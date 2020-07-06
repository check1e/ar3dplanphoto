﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Math3DUtils
{
    //Find the line of intersection between two planes.
    //The inputs are two game objects which represent the planes.
    //The outputs are a point on the line and a vector which indicates it's direction.
    public static bool planePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, GameObject plane1, GameObject plane2) {

        linePoint = Vector3.zero;
        lineVec = Vector3.zero;

        //Get the normals of the planes.
        Vector3 plane1Normal = plane1.transform.up;
        Vector3 plane2Normal = plane2.transform.up;

        //We can get the direction of the line of intersection of the two planes by calculating the
        //cross product of the normals of the two planes. Note that this is just a direction and the line
        //is not fixed in space yet.
        lineVec = Vector3.Cross(plane1Normal, plane2Normal);

        //Next is to calculate a point on the line to fix it's position. This is done by finding a vector from
        //the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
        //errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
        //the cross product of the normal of plane2 and the lineDirection.      
        Vector3 ldir = Vector3.Cross(plane2Normal, lineVec);

        float numerator = Vector3.Dot(plane1Normal, ldir);

        //Prevent divide by zero by ignoring parallel planes. //1 == 90°
        if (Mathf.Abs(numerator) > 0.1f) {

            Vector3 plane1ToPlane2 = plane1.transform.position - plane2.transform.position;
            float t = Vector3.Dot(plane1Normal, plane1ToPlane2) / numerator;
            linePoint = plane2.transform.position + t * ldir;
            return true;
        } else {
            return false;
        }
    }

    public static bool planesIntersectAtSinglePoint(Plane p0, Plane p1, Plane p2, out Vector3 intersectionPoint) {

        float det = Vector3.Dot(Vector3.Cross(p0.normal, p1.normal), p2.normal);
        if (Mathf.Abs(det) < 0.5f) {
            intersectionPoint = Vector3.zero;
            return false;
        }

        intersectionPoint =
            (-(p0.distance * Vector3.Cross(p1.normal, p2.normal)) -
            (p1.distance * Vector3.Cross(p2.normal, p0.normal)) -
            (p2.distance * Vector3.Cross(p0.normal, p1.normal))) / det;

        return true;
    }

    public static bool planesIntersectAtSinglePoint(GameObject p0, GameObject p1, GameObject p2, out Vector3 intersectionPoint) {
        return planesIntersectAtSinglePoint(GameObject2Plane(p0), GameObject2Plane(p1), GameObject2Plane(p2), out intersectionPoint);
    }

    public static Plane GameObject2Plane(GameObject go) {
        var filter = go.GetComponent<MeshFilter>();

        if (filter && filter.mesh.normals.Length > 0) {
            Vector3 normal = filter.transform.TransformDirection(filter.mesh.normals[0]);
            return new Plane(normal, go.transform.position);
        }
        throw new Exception("no normal found");
    }

    /**
     * Useless code, is this is done automatically by transform.forward, but here to understand the calculation
     * angles = transform.rotation.eulerAngles
     */
    public static Vector3 forward(Vector3 angles) {
        Vector3 forward = Vector3.zero;
        forward.x = Mathf.Sin(angles.y * Mathf.Deg2Rad) * Mathf.Cos(angles.x * Mathf.Deg2Rad);
        forward.y = Mathf.Sin(-angles.x * Mathf.Deg2Rad);
        forward.z = Mathf.Cos(angles.y * Mathf.Deg2Rad) * Mathf.Cos(angles.x * Mathf.Deg2Rad);
        return forward;
    }

    public static void MeshDivide(GameObject go) {
        Mesh m = go.GetComponent<MeshFilter>().mesh;
        List<Vector3> vertices = new List<Vector3>(m.vertices);
        List<int> triangles = new List<int>();

        Debug.Log("Vertices:" + m.vertices.Length + " Triangles: "+m.triangles.Length);

        for(int t = 0; t< m.triangles.Length/3; t++) {
            int anum = m.triangles[t * 3 + 0];
            Vector3 a = m.vertices[anum];
            int bnum = m.triangles[t * 3 + 1];
            Vector3 b = m.vertices[m.triangles[t * 3 + 1]];
            int cnum = m.triangles[t * 3 + 2];
            Vector3 c = m.vertices[m.triangles[t * 3 + 2]];

            float ab = Vector3.Distance(a, b);
            float bc = Vector3.Distance(b, c) ;
            float ca = Vector3.Distance(c, a);

            float max = Mathf.Max(new float[] { ab, bc, ca });

            int dnum = vertices.Count;

            /*triangles.Add(anum);
            triangles.Add(bnum);
            triangles.Add(cnum);*/

            if (ab == max) {
                Vector3 d = (a + b) / 2;
                vertices.Add(d);

                triangles.Add(anum);
                triangles.Add(dnum);
                triangles.Add(cnum);

                triangles.Add(dnum);
                triangles.Add(bnum);
                triangles.Add(cnum);
            } else if(bc == max) {
                Vector3 d = (b + c) / 2;
                vertices.Add(d);

                triangles.Add(bnum);
                triangles.Add(dnum);
                triangles.Add(anum);

                triangles.Add(dnum);
                triangles.Add(cnum);
                triangles.Add(anum);
            } else if (ca == max) {
                Vector3 d = (c + a) / 2;
                vertices.Add(d);

                triangles.Add(cnum);
                triangles.Add(dnum);
                triangles.Add(bnum);

                triangles.Add(dnum);
                triangles.Add(anum);
                triangles.Add(bnum);
            }
        }

        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();
        m.uv = null;
        m.normals = null;

        m.RecalculateNormals();

        Debug.Log(1);

        //Vector3.Distance(vertices[0], vertices[1])
    }
}
