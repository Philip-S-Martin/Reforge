using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class StructureRenderer
{
    const int trisPerWall = 12;
    const int trisPerSide = 32;

    StructureData data;
    public MaterialData materials;

    MeshFilter meshFilter;

    Vector3[] verts;
    int[] tris;
    Color32[] vertColors;

    public StructureRenderer(StructureData newData, MeshFilter newMeshFilter)
    {
        data = newData;
        meshFilter = newMeshFilter;
        meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        materials = new MaterialData();
    }
    //Select a position based on the mesh
    public bool SelectFromMesh(ref Vector3 selectPos, out int selectID, out int selectType, int iTri)
    {
        selectID = -1;
        selectType = -1;
        float sideHelper;

        //IS A SIDE
        //Because sides have a static number of tiangles, we know the index of the side is: triangle Index/triangles per side
        if (iTri < data.sideA.Count * trisPerSide)
        {
            selectID = iTri / trisPerSide;
            selectType = 1;
            sideHelper = data.SideDistanceFromPoint(
                ref selectPos,
                selectID);
            //Debug.Log(sideHelper);
            //These two just help me manage thigns that wind up near the end of the line!
            if (sideHelper == -1)
            {
                selectPos = data.points[data.sideA[selectID]];
            }
            else if (sideHelper == -2)
            {
                selectPos = data.points[data.sideB[selectID]];
            }
        }
        else
        {
            iTri -= data.sideA.Count * trisPerSide;

            //IS A WALL
            if (iTri < data.wallA.Count * trisPerWall)
            {
                selectID = iTri / trisPerWall;
                selectType = 2;
            }
        }
        return true;
    }

    public bool RenderNothing()
    {
        UnityEngine.Object.Destroy(meshFilter.mesh);
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        return true;
    }

    public bool SteppedRenderStructure()
    {
        RenderStructure();
        data.DoPostRenderOperations();
        //RenderStructure();

        return true;
    }

    public bool RenderStructure()
    {
        data.DoPreRenderOperations();
        int numTris = data.sideA.Count * trisPerSide + data.wallA.Count * trisPerWall;
        int numVerts = numTris * 3;
        int numVertsCreated = 0;
        PrepData(numVerts);

        SidesMesh(ref numVertsCreated);
        PolysMesh(ref numVertsCreated);

        UnityEngine.Object.Destroy(meshFilter.mesh);
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        meshFilter.mesh.vertices = verts;
        meshFilter.mesh.triangles = tris;
        meshFilter.mesh.colors32 = vertColors;

        meshFilter.mesh.RecalculateNormals();

        return true;
    }

    bool PrepData(int numVerts)
    {
        verts = new Vector3[numVerts];
        vertColors = new Color32[numVerts];

        tris = new int[numVerts];
        for (int i = 0; i < tris.Length; i++)
        {
            tris[i] = i;
        }
        return true;
    }

    bool SidesMesh(ref int numVertsCreated)
    {
        float scale = .2f;
        float reduce = .86f;
        float partial = .5f;
        float distance;
        int rInt;
        Random.InitState(42);
        Vector3 normalA, normalB;
        float scaleA, scaleB;
        Vector3 farSideNormal, topNormal, farTopNormal, sideNormal, partialTopNormal, farPartialSideNormal;
        Vector3[] middle = new Vector3[2];
        Vector3[,] positions = new Vector3[3, 6];

        Color32 color = new Color32(102, 51, 0, 255);

        int i;

        for (i = 0; i < data.sideA.Count; i++)
        {
            scale = materials.GetSideScale(data.sideMaterial[i]);

            int firstVert = numVertsCreated;
            farSideNormal = new Vector3(1, 0, 1);
            topNormal = new Vector3(0, 1, 0);
            normalA = (Vector3)data.points[data.sideA[i]] - data.points[data.sideB[i]];

            Vector3.OrthoNormalize(ref normalA, ref topNormal, ref farSideNormal);
            if (Vector3.SignedAngle(topNormal, farSideNormal, normalA) < 0) farSideNormal = -farSideNormal;

            distance = Vector3.Distance(data.points[data.sideA[i]], data.points[data.sideB[i]]);



            farSideNormal *= scale;
            topNormal *= scale;

            farTopNormal = topNormal * reduce;
            sideNormal = farSideNormal * reduce;

            partialTopNormal = topNormal * partial;
            farPartialSideNormal = farSideNormal * partial;

            normalA *= scale;
            normalB = normalA;
            scaleA = 1;
            scaleB = 1;


            normalA *= data.sharedPointA[i];
            //scaleB = .9f;

            normalB *= data.sharedPointB[i];


            rInt = Random.Range(0, 64);

            if (distance > 1 && !materials.IsUniform(data.sideMaterial[i]))
            {
                middle[0] = ((Vector3)data.points[data.sideA[i]] * 2 + data.points[data.sideB[i]]) / 3;
                middle[1] = (data.points[data.sideA[i]] + (Vector3)data.points[data.sideB[i]] * 2) / 3;
            }
            else
            {
                middle[0] = ((Vector3)data.points[data.sideA[i]] + data.points[data.sideB[i]]) / 2;
                middle[1] = middle[0];

                farTopNormal *= .9f;
                farSideNormal *= .9f;
                farPartialSideNormal *= .9f;
            }

            topNormal *= scaleA;
            partialTopNormal *= scaleA;
            sideNormal *= scaleA;
            positions[0, 0] = data.points[data.sideA[i]] + normalA + topNormal;
            positions[0, 1] = data.points[data.sideA[i]] + normalA + partialTopNormal + sideNormal;
            positions[0, 2] = data.points[data.sideA[i]] + normalA - partialTopNormal + sideNormal;
            positions[0, 3] = data.points[data.sideA[i]] + normalA - topNormal;
            positions[0, 4] = data.points[data.sideA[i]] + normalA - partialTopNormal - sideNormal;
            positions[0, 5] = data.points[data.sideA[i]] + normalA + partialTopNormal - sideNormal;

            positions[1, 0] = middle[(rInt) % 2] + farTopNormal - farPartialSideNormal;
            rInt /= 2;
            positions[1, 1] = middle[(rInt) % 2] + farTopNormal + farPartialSideNormal;
            rInt /= 2;
            positions[1, 2] = middle[(rInt) % 2] + farSideNormal;
            rInt /= 2;
            positions[1, 3] = middle[(rInt) % 2] - farTopNormal + farPartialSideNormal;
            rInt /= 2;
            positions[1, 4] = middle[(rInt) % 2] - farTopNormal - farPartialSideNormal;
            rInt /= 2;
            positions[1, 5] = middle[(rInt) % 2] - farSideNormal;

            topNormal /= scaleA;
            partialTopNormal /= scaleA;
            sideNormal /= scaleA;
            topNormal *= scaleB;
            partialTopNormal *= scaleB;
            sideNormal *= scaleB;
            positions[2, 0] = data.points[data.sideB[i]] - normalB + topNormal;
            positions[2, 1] = data.points[data.sideB[i]] - normalB + partialTopNormal + sideNormal;
            positions[2, 2] = data.points[data.sideB[i]] - normalB - partialTopNormal + sideNormal;
            positions[2, 3] = data.points[data.sideB[i]] - normalB - topNormal;
            positions[2, 4] = data.points[data.sideB[i]] - normalB - partialTopNormal - sideNormal;
            positions[2, 5] = data.points[data.sideB[i]] - normalB + partialTopNormal - sideNormal;
            int iPosition;

            if (distance > 1 && !materials.IsUniform(data.sideMaterial[i]))
                for (iPosition = 0; iPosition < 6; iPosition++)
                {
                    verts[numVertsCreated++] = positions[0, iPosition];
                    verts[numVertsCreated++] = positions[1, iPosition];
                    verts[numVertsCreated++] = positions[1, (iPosition + 1) % 6];

                    verts[numVertsCreated++] = positions[1, (iPosition + 1) % 6];
                    verts[numVertsCreated++] = positions[0, (iPosition + 1) % 6];
                    verts[numVertsCreated++] = positions[0, iPosition];

                    verts[numVertsCreated++] = positions[2, iPosition];
                    verts[numVertsCreated++] = positions[1, (iPosition + 1) % 6];
                    verts[numVertsCreated++] = positions[1, iPosition];

                    verts[numVertsCreated++] = positions[1, (iPosition + 1) % 6];
                    verts[numVertsCreated++] = positions[2, iPosition];
                    verts[numVertsCreated++] = positions[2, (iPosition + 1) % 6];
                }
            else
            {
                for (iPosition = 0; iPosition < 6; iPosition++)
                {
                    verts[numVertsCreated++] = positions[0, iPosition];
                    verts[numVertsCreated++] = positions[1, iPosition];
                    verts[numVertsCreated++] = positions[2, iPosition];

                    verts[numVertsCreated++] = positions[1, (iPosition + 1) % 6];
                    verts[numVertsCreated++] = positions[0, (iPosition + 1) % 6];
                    verts[numVertsCreated++] = positions[0, iPosition];

                    verts[numVertsCreated++] = positions[2, iPosition];
                    verts[numVertsCreated++] = positions[1, (iPosition + 1) % 6];
                    verts[numVertsCreated++] = positions[0, iPosition];

                    verts[numVertsCreated++] = positions[1, (iPosition + 1) % 6];
                    verts[numVertsCreated++] = positions[2, iPosition];
                    verts[numVertsCreated++] = positions[2, (iPosition + 1) % 6];
                }
            }

            for (int tri = 0; tri < 4; tri++)
            {
                verts[numVertsCreated++] = positions[0, 0];
                verts[numVertsCreated++] = positions[0, tri + 1];
                verts[numVertsCreated++] = positions[0, tri + 2];

                verts[numVertsCreated++] = positions[2, 0];
                verts[numVertsCreated++] = positions[2, tri + 2];
                verts[numVertsCreated++] = positions[2, tri + 1];
            }

            Color32 thisColor = materials.GetColor(data.sideMaterial[i]);
            for (int iColor = firstVert; iColor < numVertsCreated; iColor++)
            {
                vertColors[iColor] = thisColor;
            }
        }

        return true;
    }

    bool PolysMesh(ref int numVertsCreated)
    {
        float scale;
        Vector3[] normals = new Vector3[2];
        Color32 color = new Color32(255, 255, 255, 255);
        /*
        Vector3 farSideNormal = new Vector3();
        Vector3 topNormal = new Vector3();
        Vector3 farTopNormal, sideNormal, partialTopNormal, farPartialSideNormal;*/

        int i;
        //Debug.Log("Building Poly");
        for (i = 0; i < data.wallA.Count; i++)
        {
            scale = materials.GetWallScale(data.wallMaterial[i]);
            int firstVert = numVertsCreated;
            normals[0] = Vector3.Cross
                (
                    ((Vector3)data.points[data.wallB[i]] - data.points[data.wallA[i]]).normalized,
                    ((Vector3)data.points[data.wallC[i]] - data.points[data.wallA[i]]).normalized
                ).normalized * scale;
            normals[1] = Vector3.Cross
                (
                    ((Vector3)data.points[data.wallC[i]] - data.points[data.wallA[i]]).normalized,
                    ((Vector3)data.points[data.wallB[i]] - data.points[data.wallA[i]]).normalized
                ).normalized * scale;

            if (data.wallD[i] != -1)
            {
                //FACES
                verts[numVertsCreated++] = normals[0] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallC[i]];

                verts[numVertsCreated++] = normals[1] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallB[i]];

                verts[numVertsCreated++] = normals[0] + data.points[data.wallD[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallB[i]];

                verts[numVertsCreated++] = normals[1] + data.points[data.wallD[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallC[i]];

                //EDGES
                //ONE
                verts[numVertsCreated++] = normals[1] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallA[i]];

                verts[numVertsCreated++] = normals[1] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallA[i]];

                //TWO
                verts[numVertsCreated++] = normals[1] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallD[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallB[i]];

                verts[numVertsCreated++] = normals[1] + data.points[data.wallD[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallD[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallB[i]];

                //THREE
                verts[numVertsCreated++] = normals[1] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallD[i]];

                verts[numVertsCreated++] = normals[1] + data.points[data.wallD[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallD[i]];

                //FOUR
                verts[numVertsCreated++] = normals[1] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallC[i]];

                verts[numVertsCreated++] = normals[1] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallC[i]];
            }
            else
            {
                //FACES
                verts[numVertsCreated++] = normals[0] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallC[i]];

                verts[numVertsCreated++] = normals[1] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallB[i]];

                //EDGES
                //ONE
                verts[numVertsCreated++] = normals[1] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallA[i]];

                verts[numVertsCreated++] = normals[1] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallA[i]];

                //TWO
                verts[numVertsCreated++] = normals[1] + data.points[data.wallB[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallB[i]];

                verts[numVertsCreated++] = normals[1] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallB[i]];

                //THREE
                verts[numVertsCreated++] = normals[1] + data.points[data.wallC[i]];
                verts[numVertsCreated++] = normals[1] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallC[i]];

                verts[numVertsCreated++] = normals[1] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallA[i]];
                verts[numVertsCreated++] = normals[0] + data.points[data.wallC[i]];

                numVertsCreated += 12;
            }
            Color32 thisColor = materials.GetColor(data.wallMaterial[i]);
            for (int iColor = firstVert; iColor < numVertsCreated; iColor++)
            {
                vertColors[iColor] = thisColor;
            }
        }
        return true;
    }

    public StructureData Data
    {
        get
        {
            return data;
        }

        set
        {
            data = value;
        }
    }

    public MeshFilter MeshFilter
    {
        get
        {
            return meshFilter;
        }

        set
        {
            meshFilter = value;
        }
    }

    public static float DistanceFromSide(ref Vector3 findPoint, Vector3 sideA, Vector3 sideB)
    {
        Ray line;
        float rayDistance;
        float sideADistance = Vector3.Distance(findPoint, sideA);
        float sideBDistance = Vector3.Distance(findPoint, sideB);

        if (sideADistance < sideBDistance)
        {
            line = new Ray(sideA, (sideB - sideA).normalized);
            rayDistance = Vector3.Project(findPoint - line.origin, line.direction).magnitude;
            findPoint = line.GetPoint(Mathf.Round(rayDistance * 3) / 3);

            if (rayDistance < .15 || Vector3.Angle(findPoint - line.origin, line.direction) > 90)
            {
                findPoint = sideA;
                return -1f;
            }
        }
        else
        {
            line = new Ray(sideB, (sideA - sideB).normalized);
            rayDistance = Vector3.Project(findPoint - line.origin, line.direction).magnitude;
            findPoint = line.GetPoint(Mathf.Round(rayDistance * 3) / 3);

            if (rayDistance < .15 || Vector3.Angle(findPoint - line.origin, line.direction) > 90)
            {
                findPoint = sideB;
                return -2f;
            }
        }
        return rayDistance;
    }
}

