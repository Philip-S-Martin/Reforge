using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
/*
 * Goals:
 *      - These are the commands used by the UI that can recognize 
 */

public class Structure : MonoBehaviour
{


    protected StructureData thisData;
    protected StructureRenderer thisRenderer;
    protected GameObject thisObject;

    private StructureData bufferData;
    private StructureRenderer bufferRenderer;
    private GameObject bufferObject;

    protected void Start()
    {
        thisObject = this.gameObject;
        thisData = new StructureData();
        thisRenderer = new StructureRenderer(thisData, thisObject.GetComponent<MeshFilter>());

        //how do we instantiate substructures without making an infinite loop???
        //bufferObject = 
        //bufferData = new StructureData();
        //bufferRenderer = new StructureRenderer(bufferData, bufferObject.GetComponent<MeshFilter>());
    }

    /*public Structure()
    {
        thisData = new StructureData();
        thisRenderer = new StructureRenderer(thisData, thisObject.GetComponent<MeshFilter>());
    }*/

    /*public bool BuildSide(RaycastHit hitA, RaycastHit hitB, int material, GameObject target)
    {
        Structure targetStructure = target.GetComponent<Structure>();
        return targetStructure.BuildSide(hitA.point, hitB.point, material);
    }*/

    public bool BuildWall(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, int material, bool doRender)
    {
        //Debug.Log("Wall from Structure");
        thisData.CreateWall(pointA, pointB, pointC, pointD, material);

        if (doRender)
        {
            thisRenderer.SteppedRenderStructure();
            thisObject.GetComponent<MeshCollider>().sharedMesh = thisObject.GetComponent<MeshFilter>().mesh;
        }
        return true;
    }

    public bool MaximizeSide(int iSide, Vector3 towardsOpposite, out Vector3 pointA, out Vector3 pointB)
    {
        return thisData.MaximizeSide(iSide, towardsOpposite, out pointA, out pointB);
    }

    public bool RemovePart(RaycastHit hit)
    {
        int selectID, selectType;
        Vector3 point = hit.point;
        SelectFromMesh(ref point, out selectID, out selectType, hit.triangleIndex);
        Debug.Log("ID: " + selectID + " TYPE: " + selectType);
        if (selectType == 1)
        {
            thisData.CleanDeleteSide(selectID);
            thisRenderer.SteppedRenderStructure();
            thisObject.GetComponent<MeshCollider>().sharedMesh = thisObject.GetComponent<MeshFilter>().mesh;
            return true;
        }

        if (selectType == 2)
        {
            thisData.CleanRemoveWall(selectID);
            thisRenderer.SteppedRenderStructure();
            thisObject.GetComponent<MeshCollider>().sharedMesh = thisObject.GetComponent<MeshFilter>().mesh;
            return true;
        }

        return false;
    }

    //build into bufferData
    public bool BuildSide(Vector3 pointA, Vector3 pointB, int material, bool doRender = true)
    {
        thisData.CreateSide(pointA, pointB, material);
        if (doRender)
        {
            thisRenderer.SteppedRenderStructure();
            thisObject.GetComponent<MeshCollider>().sharedMesh = thisObject.GetComponent<MeshFilter>().mesh;
        }
        return true;
    }


    //needs to work with thisRenderer OR bufferRenderer!!!
    public bool SelectFromMesh(ref Vector3 selectPos, out int selectID, out int selectType, int iTri)
    {
        return thisRenderer.SelectFromMesh(ref selectPos, out selectID, out selectType, iTri);
    }

    bool MergeBuffer()
    {
        //copy bufferData into local tempData
        StructureData tempData = bufferData;
        GameObject tempObject = bufferObject;
        StructureRenderer tempRenderer = bufferRenderer;
        


        
        return true;
    } 

    public void GetSidePosition(int sideIndex, out Vector3 pointA, out Vector3 pointB)
    {
        pointA = thisData.points[thisData.sideA[sideIndex]];
        pointB = thisData.points[thisData.sideB[sideIndex]];
    }

    public StructureData GetData()
    {
        return thisData;
    }
    public void SetData(StructureData newData)
    {
        thisData = newData;
        thisRenderer.Data = thisData;
        thisRenderer.SteppedRenderStructure();
        thisObject.GetComponent<MeshCollider>().sharedMesh = thisObject.GetComponent<MeshFilter>().mesh;
    }

    protected void MergeIntoStructure(Structure mergeInto)
    {
        bool render = false;
        //Debug.Log("Merging");
        //merge as much as you can without rendering
        Debug.Log("BP wall count " + thisData.wallA.Count);
        for (int i = 0; i < thisData.wallA.Count; i++)
        {
            if (i == thisData.wallA.Count - 1) render = true;
            if (thisData.wallD[i] != -1)
                mergeInto.BuildWall(thisData.points[thisData.wallA[i]], thisData.points[thisData.wallB[i]], thisData.points[thisData.wallC[i]], thisData.points[thisData.wallD[i]], thisData.wallMaterial[i], render);
            else
                mergeInto.BuildWall(thisData.points[thisData.wallA[i]], thisData.points[thisData.wallB[i]], thisData.points[thisData.wallC[i]], thisData.points[thisData.wallB[i]], thisData.wallMaterial[i], render);

        }
        for (int i = 0; i < thisData.sideA.Count; i++)
        {
            //Debug.Log("\tSide: " + i);
            if (i == thisData.sideA.Count - 1) render = true;
            mergeInto.BuildSide(thisData.points[thisData.sideA[i]], thisData.points[thisData.sideB[i]], thisData.sideMaterial[i], render);
        }

        //merge last side and render
    }
}

