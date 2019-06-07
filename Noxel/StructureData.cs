using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[System.Serializable]
public class StructureData
{
    const float scale = .5f;
    const float minScale = scale * .90f;
    const float halfScale = scale * .5f;
    const float multiple = 1 / scale;
    const int listBuffers = 32;

    public List<SerializableVector3> points;

    public List<int> sideA;
    public List<int> sideB;
    public List<float> sharedPointA;
    public List<float> sharedPointB;
    public List<int> sideMaterial;

    public List<int> wallA;
    public List<int> wallB;
    public List<int> wallC;
    public List<int> wallD;
    public List<int> wallMaterial;

    //Pre render lists
    List<int> pointsFlaggedForDelete;
    //List<int> sidesFlaggedForDelete;
    //List<int> wallsFlaggedForDelete;

    //INITIALIZATION
    public StructureData()
    {
        ResetData();
    }

    public StructureData(StructureData existing)
    {
        ResetData();
        points = existing.points;

        sideA = existing.sideA;
        sideB = existing.sideB;
        sharedPointA = existing.sharedPointA;
        sharedPointB = existing.sharedPointB;
        sideMaterial = existing.sideMaterial;

        wallA = existing.wallA;
        wallB = existing.wallB;
        wallC = existing.wallC;
        wallD = existing.wallD;
        wallMaterial = existing.wallMaterial;
    }

    public void ResetData()
    {
        points = new List<SerializableVector3>();

        sideA = new List<int>();
        sideB = new List<int>();
        sharedPointA = new List<float>();
        sharedPointB = new List<float>();
        sideMaterial = new List<int>();

        wallA = new List<int>();
        wallB = new List<int>();
        wallC = new List<int>();
        wallD = new List<int>();
        wallMaterial = new List<int>();

        pointsFlaggedForDelete = new List<int>();
        //sidesFlaggedForDelete = new List<int>();
        //wallsFlaggedForDelete = new List<int>();

    }

    //Pre-Render
    public void DoPreRenderOperations()
    {
        TryDeleteFlaggedItems();
    }

    void TryDeleteFlaggedItems()
    {
        for (int i = 0; i < pointsFlaggedForDelete.Count; i++)
        {
            CleanDeletePoint(pointsFlaggedForDelete[i]);
        }
        pointsFlaggedForDelete = new List<int>();
    }

    //Post-Render

    public void DoPostRenderOperations()
    {
        BufferLists();
    }

    void BufferLists()
    {
        if (points.Capacity <= points.Count + listBuffers)
        {
            points.Capacity += points.Capacity / 2;

        }
        if (sideA.Capacity <= sideA.Count + listBuffers)
        {
            sideA.Capacity += sideA.Capacity / 2;
            sideB.Capacity = sideA.Capacity;
            sharedPointA.Capacity = sideA.Capacity;
            sharedPointB.Capacity = sideA.Capacity;
            sideMaterial.Capacity = sideA.Capacity;
        }
        if (wallA.Capacity <= wallA.Count + listBuffers)
        {
            wallA.Capacity += wallA.Capacity / 2;
            wallB.Capacity = wallA.Capacity;
            wallC.Capacity = wallA.Capacity;
            wallD.Capacity = wallA.Capacity;
            wallMaterial.Capacity = wallA.Capacity;
            //frameOnFrame.Capacity = frames.Capacity;
        }
    }


    //POINTS

    //create a point at position, merge it with nearby positions if possible, if placed on side, split it
    public int CreatePoint(Vector3 position, int iSide = -1, bool snap = true)
    {
        int existingPoint = -1;
        //if the point is closer than .95 of scale to another point, just return the existing point
        for (int i = 0; i < points.Count; i++)
        {
            if (Vector3.Distance(position, points[i]) < halfScale)
            {
                //Debug.Log("MERGED NEW WITH: " + i + " Dist: " + Vector3.Distance(position, points[i]));
                existingPoint = i;
            }
        }

        List<int> iSides = new List<int>();
        //if (iSide != -1) iSides.Add(iSide);
        //if the point is within .95 of scale to a line, put it on the line
        for (int i = 0; i < sideA.Count; i++)
        {
            if (PointInRangeOfSegment(position, points[sideA[i]], points[sideB[i]], 90))
            {
                if (Vector3.Cross(((Vector3)points[sideB[i]] - points[sideA[i]]), (position - points[sideA[i]])).magnitude < halfScale)
                {
                    iSides.Add(i);
                    float dist = SideDistanceFromPoint(ref position, iSides[iSides.Count - 1], false);
                    //If it ends up on an end of the line, return that instead
                    if (dist == -1) existingPoint = sideA[iSides[iSides.Count - 1]];
                    else if (dist == -2) existingPoint = sideB[iSides[iSides.Count - 1]];
                }
            }
        }

        //Debug.Log("Sides on Point: " + iSides.Count + " Existing: " + existingPoint);
        if (existingPoint == -1)
        {
            points.Add(position);
            existingPoint = points.Count - 1;
        }

        List<int> newSideA = new List<int>();
        List<int> newSideB = new List<int>();
        List<int> newSideMaterial = new List<int>();

        for (int i = 0; i < iSides.Count; i++)
        {
            //Debug.Log("Split: " + i + "/" + iSides.Count);
            if (existingPoint != sideA[iSides[i]] && existingPoint != sideB[iSides[i]])
            {
                //Debug.Log("Splitting");

                newSideA.Add(sideA[iSides[i]]);
                newSideB.Add(existingPoint);
                newSideMaterial.Add(sideMaterial[iSides[i]]);

                newSideA.Add(sideB[iSides[i]]);
                newSideB.Add(existingPoint);
                newSideMaterial.Add(sideMaterial[iSides[i]]);

                CleanDeleteSide(iSides[i]);
                DecrementSideList(ref iSides, iSides[i]);
            }
        }

        for (int i = 0; i < newSideA.Count; i++)
        {
            CreateSide(newSideA[i], newSideB[i], newSideMaterial[i]);
        }
        return existingPoint;
    }

    //Splits a side at a given point
    bool SplitSideOnPoint(int iPoint, int iSide)
    {


        return true;
    }

    //safely remove point at index iPoint
    public bool CleanDeletePoint(int iPoint)
    {
        if (iPoint >= points.Count || iPoint < 0)
        {
            //Debug.Log("Cannot delete point: Index out of bounds");
            return false;
        }

        //Count and list all sides attached to the point
        List<int> attachedSides = new List<int>();
        PointAttachedSides(ref attachedSides, iPoint);
        //Debug.Log("Attached sides: " + attachedSides.Count);

        //Count all walls attached to the point
        int wallCount = PointAttachedWalls(iPoint);

        //If this point is used in polys, do not remove
        if (wallCount > 0) return false;
        //If this point is not used by ANY sides, remove it
        if (attachedSides.Count == 0)
        {
            RemovePoint(iPoint);
            return true;
        }
        else
        {
            for (int i = 0; i < attachedSides.Count; i++)
            {
                if (sideA[attachedSides[i]] == iPoint) sharedPointA[attachedSides[i]] = 1;
                else if (sideB[attachedSides[i]] == iPoint) sharedPointB[attachedSides[i]] = 1;
            }

            for (int i = 0; i < attachedSides.Count; i++)
            {
                for (int i2 = i + 1; i2 < attachedSides.Count; i2++)
                {
                    float change = 1;
                    if (sideA[attachedSides[i]] == iPoint && sideA[attachedSides[i2]] == iPoint)
                        change = Vector3.Angle(((Vector3)points[sideB[attachedSides[i]]] - points[iPoint]), ((Vector3)points[sideB[attachedSides[i2]]] - points[iPoint])) / 90;
                    else if (sideA[attachedSides[i]] == iPoint && sideB[attachedSides[i2]] == iPoint)
                        change = Vector3.Angle(((Vector3)points[sideB[attachedSides[i]]] - points[iPoint]), ((Vector3)points[sideA[attachedSides[i2]]] - points[iPoint])) / 90;
                    else if (sideB[attachedSides[i]] == iPoint && sideA[attachedSides[i2]] == iPoint)
                        change = Vector3.Angle(((Vector3)points[sideA[attachedSides[i]]] - points[iPoint]), ((Vector3)points[sideB[attachedSides[i2]]] - points[iPoint])) / 90;
                    else if (sideB[attachedSides[i]] == iPoint && sideB[attachedSides[i2]] == iPoint)
                        change = Vector3.Angle(((Vector3)points[sideA[attachedSides[i]]] - points[iPoint]), ((Vector3)points[sideA[attachedSides[i2]]] - points[iPoint])) / 90;

                    change = 2f - change;

                    if (sideA[attachedSides[i]] == iPoint && sharedPointA[attachedSides[i]] > change)
                        sharedPointA[attachedSides[i]] = change;
                    else if (sideB[attachedSides[i]] == iPoint && sharedPointB[attachedSides[i]] > change)
                        sharedPointB[attachedSides[i]] = change;

                    if (sideA[attachedSides[i2]] == iPoint && sharedPointA[attachedSides[i2]] > change)
                        sharedPointA[attachedSides[i2]] = change;
                    else if (sideB[attachedSides[i2]] == iPoint && sharedPointB[attachedSides[i2]] > change)
                        sharedPointB[attachedSides[i2]] = change;
                }
            }
        }
        //If this point is used by TWO sides, and they share the same direction, combine and remove the middle
        if (attachedSides.Count == 2 && sideMaterial[attachedSides[0]] == sideMaterial[attachedSides[1]] && SidesShareDirection(attachedSides[0], attachedSides[1], 5))
        {
            //if they share SideA, swap A1 for B2
            if (sideA[attachedSides[0]] == sideA[attachedSides[1]])
                sideA[attachedSides[0]] = sideB[attachedSides[1]];
            //if they share SideB, swap B1 for A2
            else if (sideB[attachedSides[0]] == sideB[attachedSides[1]])
                sideB[attachedSides[0]] = sideA[attachedSides[1]];
            //if SideA1 shares sideB1, swap A1 for A2
            else if (sideA[attachedSides[0]] == sideB[attachedSides[1]])
                sideA[attachedSides[0]] = sideA[attachedSides[1]];
            //if SideB1 shares sideA1, swap B1 for B2
            else if (sideB[attachedSides[0]] == sideA[attachedSides[1]])
                sideB[attachedSides[0]] = sideB[attachedSides[1]];

            //Now that we've combined them into the first side, remove the second and the middle point
            CleanDeleteSide(attachedSides[1]);
            RemovePoint(iPoint);
            return true;
        }


        return false;
    }

    //Forcefully remove point at index iPoint
    public void RemovePoint(int iPoint)
    {
        if (iPoint >= points.Count || iPoint < 0)
        {
            //Debug.Log("Cannot remove point: Index out of bounds");
            return;
        }
        //Debug.Log("Deleting point");
        points.RemoveAt(iPoint);

        //Decrement indexes in sides above iPoint, remove sides containing iPoint
        for (int i = 0; i < sideA.Count; i++)
        {
            if (sideA[i] == iPoint || sideB[i] == iPoint)
            {
                RemoveSide(i);
            }
            if (sideA[i] > iPoint)
            {
                sideA[i]--;
            }
            if (sideB[i] > iPoint)
            {
                sideB[i]--;
            }
        }
        //Decrement indexes in sides above iPoint, remove walls containing iPoint
        for (int i = 0; i < wallA.Count; i++)
        {
            if (wallA[i] == iPoint || wallB[i] == iPoint || wallC[i] == iPoint || wallD[i] == iPoint)
            {
                RemoveWall(i);
            }
            if (wallA[i] > iPoint)
            {
                wallA[i]--;
            }
            if (wallB[i] > iPoint)
            {
                wallB[i]--;
            }
            if (wallC[i] > iPoint)
            {
                wallC[i]--;
            }
            if (wallD[i] > iPoint)
            {
                wallD[i]--;
            }
        }
        //Decrement points flagged for delete above iPoint
        for (int i = 0; i < pointsFlaggedForDelete.Count; i++)
        {
            if (pointsFlaggedForDelete[i] > iPoint)
            {
                pointsFlaggedForDelete[i]--;
            }
        }
    }

    public void MovePoint(int iPoint, Vector3 newPosition)
    {
        if (iPoint < 0 || iPoint >= points.Count) return;
        points[iPoint] = newPosition;
    }

    void PointAttachedSides(ref List<int> attachedSides, int iPoint)
    {
        //Count and list all sides that are attached to this point
        for (int i = 0; i < sideA.Count; i++)
        {
            if (sideA[i] == iPoint)
            {
                attachedSides.Add(i);
            }
            else if (sideB[i] == iPoint)
            {
                attachedSides.Add(i);
            }
            //Debug.Log("Point "+ iPoint + " Attached sides " + attachedSides.Count + " on side " + i + " " + sideA[i] + " " + sideB[i]);
        }
    }

    int PointAttachedWalls(int iPoint)
    {
        int wallCount = 0;
        //Count all walls that are attached to this point
        for (int i = 0; i < wallA.Count; i++)
        {
            if (wallA[i] == iPoint || wallB[i] == iPoint || wallC[i] == iPoint || wallD[i] == iPoint)
            {
                wallCount++;
            }
        }
        return wallCount;
    }

    //SIDES

    //Create a side from two point indexes
    public int CreateSide(int iPointA, int iPointB, int material)
    {
        //make sure the new side is larger than the minimum scale
        if (Vector3.Distance(points[iPointA], points[iPointB]) < minScale) return -1;
        //Make sure it isnt a dupe
        for (int i = 0; i < sideA.Count; i++)
        {
            if ((sideA[i] == iPointA && sideB[i] == iPointB) || (sideA[i] == iPointB && sideB[i] == iPointA) || (PointInRangeOfSegment(points[iPointA], points[sideA[i]], points[sideB[i]]) && PointInRangeOfSegment(points[iPointB], points[sideA[i]], points[sideB[i]]))) return i;
        }
        //Create the side between two points
        sideA.Add(iPointA);
        sideB.Add(iPointB);
        sideMaterial.Add(material);
        sharedPointA.Add(1);
        sharedPointB.Add(1);
        pointsFlaggedForDelete.Add(iPointA);
        pointsFlaggedForDelete.Add(iPointB);
        //flag for post-render splitting
        int newSide = sideA.Count - 1;
        SplitSideIntersections(newSide);

        //return new side's index
        return newSide;
    }

    //Create a side from a position and a point index
    public int CreateSide(int iPointA, Vector3 posPointB, int material)
    {
        //make sure the new side is larger than the minimum scale
        if (Vector3.Distance(points[iPointA], posPointB) < minScale) return -1;

        //Make sure this new point isnt already on a similar side
        int newPoint = -1;
        for (int i = 0; i < sideA.Count; i++)
        {
            bool ends;
            // New B is on old line
            if (PointInRangeOfSegment(posPointB, points[sideA[i]], points[sideB[i]], out ends) || ends)
            {
                //Debug.Log("Modifying from B");
                bool aEnds, bEnds;
                bool AoN = PointInRangeOfSegment(points[sideA[i]], points[iPointA], posPointB, out aEnds);
                bool BoN = PointInRangeOfSegment(points[sideB[i]], points[iPointA], posPointB, out bEnds);

                //Old A is on new line
                if (AoN || (aEnds && posPointB == points[sideB[i]]))
                {
                    newPoint = sideA[i];
                }
                //Old B is on new line
                else if (BoN || (bEnds && posPointB == points[sideA[i]]))
                {
                    newPoint = sideB[i];
                }
            }
        }

        //Create a new point and flag it for safe deleting in case it ends up useless
        if (newPoint == -1)
            newPoint = CreatePoint(posPointB);
        //Using the new point and existing point, create a side
        return CreateSide(iPointA, newPoint, material);
    }

    //Create a side from two Positions
    public int CreateSide(Vector3 posPointA, Vector3 posPointB, int material)
    {
        int newPoint = -1;
        //make sure the new side is larger than the minimum scale
        if (Vector3.Distance(posPointA, posPointB) < minScale) return -1;
        //Make sure this new point isnt already on a similar side
        for (int i = 0; i < sideA.Count; i++)
        {
            bool ends;
            // New A is on old line
            if (PointInRangeOfSegment(posPointA, points[sideA[i]], points[sideB[i]], out ends) || ends)
            {
                bool aEnds, bEnds;
                bool AoN = PointInRangeOfSegment(points[sideA[i]], posPointA, posPointB, out aEnds);
                bool BoN = PointInRangeOfSegment(points[sideB[i]], posPointA, posPointB, out bEnds);
                //Old A is on new line
                if (AoN || (aEnds && posPointA == points[sideB[i]]))
                {
                    newPoint = sideA[i];
                }
                //Old B is on new line
                else if (BoN || (bEnds && posPointA == points[sideA[i]]))
                {
                    newPoint = sideB[i];
                }
            }

        }

        if (newPoint == -1)
            newPoint = CreatePoint(posPointA);

        //Using the new point and the position, create a side
        return CreateSide(newPoint, posPointB, material);
    }

    //Delete side safely without destroying any required data for anything else
    public void CleanDeleteSide(int iSide)
    {
        //bounds checking
        if (iSide >= sideA.Count || iSide < 0)
        {
            return;
        }
        //safe removal of points
        pointsFlaggedForDelete.Add(sideA[iSide]);
        pointsFlaggedForDelete.Add(sideB[iSide]);
        //passed all checks, remove the side
        RemoveSide(iSide);
    }

    //Remove the side and everything referencing it - UNSAFE
    void RemoveSide(int iSide)
    {
        //bounds checking
        if (iSide >= sideA.Count || iSide < 0)
        {
            return;
        }
        //remove the data
        sideA.RemoveAt(iSide);
        sideB.RemoveAt(iSide);
        sideMaterial.RemoveAt(iSide);
        sharedPointA.RemoveAt(iSide);
        sharedPointB.RemoveAt(iSide);

    }

    void DecrementSideList(ref List<int> iSides, int iSide)
    {
        for (int i = 0; i < iSides.Count; i++)
        {
            if (iSides[i] == iSide)
            {
                iSides[i] = -1;
            }
            else if (iSides[i] > iSide)
            {
                iSides[i]--;
            }
        }
    }

    //Split sides that intersect at a given point
    bool SplitSideIntersections(int iSide)
    {
        //Debug.Log("Splitting intersections");
        Vector3 intersection;
        for (int i = 0; i < sideA.Count; i++)
        {
            if (sideA[iSide] != sideA[i] && sideA[iSide] != sideB[i] && sideB[iSide] != sideA[i] && sideB[iSide] != sideB[i]
                && SideIntersection(out intersection, iSide, i))
            {
                CreatePoint(intersection, -1, false);
                return true;
            }
        }
        return false;
    }

    //WALLS
    //Create wall from 4 positions
    public int CreateWall(int pointA, int pointB, int pointC, int pointD, int material)
    {
        wallA.Add(pointA);
        wallB.Add(pointB);
        wallC.Add(pointC);
        wallD.Add(pointD);
        wallMaterial.Add(material);
        //if it is a triangle, set wallD to -1
        HandleTriangle(wallA.Count - 1);
        return wallA.Count - 1;
    }

    public int CreateWall(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, int material)
    {
        Vector3 middleA, middleB;
        ClosestPointsOnTwoLines(out middleA, out middleB, pointA, Direction(pointA, pointC), pointB, Direction(pointB, pointD));
        Debug.Log("Middle dist: " + Vector3.Distance(middleA, middleB));
        if (SharePlane(pointA, pointB, pointC, pointD, 6))
        {
            if (PointInRangeOfSegment(middleA, pointA, pointC) || PointInRangeOfSegment(middleB, pointB, pointD))
            {
                return CreateWall(
                        CreatePoint(pointA),//
                        CreatePoint(pointB),
                        CreatePoint(pointD),
                        CreatePoint(pointC),
                        material);//
            }
            else
            {
                return CreateWall(
                        CreatePoint(pointA),
                        CreatePoint(pointB),
                        CreatePoint(pointC),
                        CreatePoint(pointD),
                        material);
            }
        }
        return -1;
    }

    public void CleanRemoveWall(int iWall)
    {
        if (iWall >= wallA.Count || iWall < 0)
        {
            return;
        }
        //Currently no data depends on wall, no checks needed.
        RemoveWall(iWall);
    }

    void RemoveWall(int iWall)
    {
        if (iWall >= wallA.Count || iWall < 0)
        {
            return;
        }
        pointsFlaggedForDelete.Add(wallA[iWall]);
        pointsFlaggedForDelete.Add(wallB[iWall]);
        pointsFlaggedForDelete.Add(wallC[iWall]);
        pointsFlaggedForDelete.Add(wallD[iWall]);
        Debug.Log("removing " + iWall + " on " + wallA.Count);
        wallA.RemoveAt(iWall);
        wallB.RemoveAt(iWall);
        wallC.RemoveAt(iWall);
        wallD.RemoveAt(iWall);
        wallMaterial.RemoveAt(iWall);
    }

    //INFORMATION
    bool HandleTriangle(int iWall)
    {
        if (wallA[iWall] == wallD[iWall] || wallB[iWall] == wallD[iWall])
        {
            wallD[iWall] = -1;
            return true;
        }
        else if (wallA[iWall] == wallC[iWall] || wallB[iWall] == wallC[iWall])
        {
            wallC[iWall] = wallD[iWall];
            wallD[iWall] = -1;
            return true;
        }
        return false;
    }

    public bool WallIsTriangle(int iWall)
    {
        if (wallD[iWall] == -1)
            return true;
        return false;
    }

    //returns true if a point belongs to a wall
    bool PointOnWall(int iPoint, int iWall)
    {
        if (sideA[wallA[iWall]] == iPoint ||
            sideB[wallA[iWall]] == iPoint ||
            sideA[wallB[iWall]] == iPoint ||
            sideB[wallB[iWall]] == iPoint)
        {
            return true;
        }
        else return false;
    }

    //Returns rounded distance
    public float SideDistanceFromPoint(ref Vector3 findPoint, int iSide, bool snap = true)
    {
        Vector3 pointA = points[sideA[iSide]];
        Vector3 pointB = points[sideB[iSide]];
        Ray line;
        float rayDistance;
        float sideADistance = Vector3.Distance(findPoint, pointA);
        float sideBDistance = Vector3.Distance(findPoint, pointB);
        //Closer to A
        if (sideADistance < sideBDistance)
        {
            line = new Ray(pointA, (pointB - pointA).normalized);
            rayDistance = Vector3.Project(findPoint - line.origin, line.direction).magnitude;
            if (snap) findPoint = line.GetPoint(RoundScale(rayDistance));
            else findPoint = line.GetPoint(rayDistance);


            if (rayDistance < minScale || Vector3.Angle(findPoint - line.origin, line.direction) > 90)
            {
                //Debug.Log("Near A");
                if (snap)
                    findPoint = pointA;
                return -1f;
            }
        }
        //Closer to B
        else
        {
            line = new Ray(pointB, (pointA - pointB).normalized);
            rayDistance = Vector3.Project(findPoint - line.origin, line.direction).magnitude;
            if (snap) findPoint = line.GetPoint(RoundScale(rayDistance));
            else findPoint = line.GetPoint(rayDistance);

            if (rayDistance < minScale || Vector3.Angle(findPoint - line.origin, line.direction) > 90)
            {
                //Debug.Log("Near B");
                if (snap)
                    findPoint = pointB;
                return -2f;
            }
        }
        return rayDistance;
    }

    //Returns true if sides share the same direction
    bool SidesShareDirection(int iFirstSide, int iNextSide, float accuracy = 0.3f)
    {
        float angle = Vector3.Angle(SideDirection(iFirstSide), SideDirection(iNextSide));
        if (angle < accuracy || angle > 180 - accuracy)
            return true;
        else
            return false;
    }

    float AngleBetweenSides(int iFirstSide, int iNextSide)
    {
        float angle = Vector3.Angle(SideDirection(iFirstSide), SideDirection(iNextSide));
        if (angle < 90) return angle;
        return 90 - angle;
    }

    //Returns direction from sideA to sideB at iSide
    Vector3 SideDirection(int iSide)
    {
        return Direction(points[sideB[iSide]], points[sideA[iSide]]);
    }

    //Returns a direction from a point to another
    Vector3 Direction(Vector3 from, Vector3 to)
    {
        return (to - from).normalized;
    }

    //Determines if 2 sides share a single plane
    bool SidesSharePlane(int iFirstSide, int iNextSide)
    {
        return SharePlane(points[sideA[iFirstSide]], points[sideB[iFirstSide]], points[sideA[iNextSide]], points[sideB[iNextSide]]);
    }

    //determines if 4 points share a single plane
    bool SharePlane(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float threshold = .1f)
    {
        if (a == b || a == c || a == d || b == c || b == d || c == d) return true;

        float angle = Vector3.Angle(GetNormal(a, b, c), GetNormal(a, b, d));
        if (angle < threshold || angle > 180 - threshold) return true;
        return false;
    }

    //returns the normal vector of 3 points
    public static Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        return Vector3.Cross(side1, side2).normalized;
    }

    public float RoundScale(float number)
    {
        return Mathf.Round(number * multiple) / multiple;
    }

    //using the indices of two sides, determine if they are close enough to intersect, return the intersection point
    bool SideIntersection(out Vector3 intersection, int iSideA, int iSideB)
    {
        intersection = Vector3.zero;
        Vector3 pointA, pointB;
        if (iSideA >= sideA.Count || iSideB >= sideA.Count) return false;

        if (ClosestPointsOnTwoLines(out pointA, out pointB,
            points[sideA[iSideA]], ((Vector3)points[sideB[iSideA]] - points[sideA[iSideA]]).normalized,
            points[sideA[iSideB]], ((Vector3)points[sideB[iSideB]] - points[sideA[iSideB]]).normalized))
        {
            //Debug.Log(Vector3.Distance(pointA, pointB));
            if (Vector3.Distance(pointA, pointB) < halfScale)
            {
                //Debug.Log("in range");
                bool aEnds, bEnds;
                bool aOnSide = PointInRangeOfSegment(pointA, points[sideA[iSideA]], points[sideB[iSideA]], out aEnds);
                bool bOnSide = PointInRangeOfSegment(pointB, points[sideA[iSideB]], points[sideB[iSideB]], out bEnds);
                intersection = (pointA + pointB) / 2;
                if ((aOnSide || aEnds) && (bOnSide || bEnds))
                {
                    return true;
                }
            }
        }
        return false;
    }

    static bool PointInRangeOfSegment(Vector3 point, Vector3 segmentA, Vector3 segmentB, out bool ends)
    {
        if (point == segmentA || point == segmentB) ends = true;
        else ends = false;

        return PointInRangeOfSegment(point, segmentA, segmentB);
    }

    //Gets the maximum length of connected sides with no intersections pointing in this direction
    public bool MaximizeSide(int iSide, Vector3 towardsOpposite, out Vector3 pointA, out Vector3 pointB)
    {
        int iSideA = iSide;
        int iSideB = iSide;
        int iPointA = sideA[iSideA];
        int iPointB = sideB[iSideB];

        pointA = points[iPointA];
        pointB = points[iPointB];

        Vector3 normalAxis = GetNormal(pointA, pointB, towardsOpposite);
        int signMod = 1;
        if (Vector3.SignedAngle(Direction(pointA, towardsOpposite), Direction(pointA, pointB), normalAxis) < 0)
        {
            //normalAxis = -normalAxis;
            //signMod = -1;
        }
        Debug.Log("Maximizing side " + iSide);

        while (true)
        {

            Debug.Log("Extending A");
            int newIPointA = -1;
            int newISideA = -1;
            for (int i = 0; i < sideA.Count; i++)
            {
                Vector3 newNormal;
                float angle = signMod;
                if (i != iSideA)
                {
                    if (sideA[i] == iPointA)
                    {
                        newNormal = GetNormal(points[sideB[i]], pointB, towardsOpposite);
                        angle *= Vector3.SignedAngle(Direction(pointA, pointB), Direction(pointA, points[sideB[i]]), normalAxis);
                        Debug.Log(i + " " + normalAxis + " vs " + newNormal + " " + angle);

                        if ((angle == 180 || angle == -180) && (newNormal == normalAxis || newNormal == normalAxis))
                        {
                            newIPointA = sideB[i];
                            newISideA = i;
                        }
                        else if (angle > 0)
                        {
                            newIPointA = -2;
                            break;
                        }
                    }
                    else if (sideB[i] == iPointA)
                    {
                        newNormal = GetNormal(points[sideA[i]], pointB, towardsOpposite);
                        angle *= Vector3.SignedAngle(Direction(pointA, pointB), Direction(pointA, points[sideA[i]]), normalAxis);
                        Debug.Log(i + " " + normalAxis + " vs " + newNormal + " " + angle);

                        if ((angle == 180 || angle == -180) && (newNormal == normalAxis || newNormal == normalAxis))
                        {
                            newIPointA = sideA[i];
                            newISideA = i;
                        }
                        else if (angle > 0)
                        {
                            newIPointA = -2;
                            break;
                        }
                    }
                }
            }
            Debug.Log("newIPointA " + newIPointA);
            if (newIPointA >= 0)
            {
                iSideA = newISideA;
                iPointA = newIPointA;
                pointA = points[iPointA];
            }
            else break;

        }

        while (true)
        {
            Debug.Log("Extending B");
            int newIPointB = -1;
            int newISideB = -1;
            for (int i = 0; i < sideA.Count; i++)
            {
                Vector3 newNormal;
                float angle = signMod;

                if (iSideB != i && sideA[i] == iPointB)
                {
                    newNormal = GetNormal(pointA, points[sideB[i]], towardsOpposite);
                    angle *= Vector3.SignedAngle(Direction(pointB, pointA), Direction(pointB, points[sideB[i]]), normalAxis);
                    Debug.Log(i + " " + normalAxis + " vs " + newNormal + " " + angle);

                    if ((angle == 180 || angle == -180) && (newNormal == normalAxis || newNormal == normalAxis))
                    {
                        newIPointB = sideB[i];
                        newISideB = i;
                    }
                    else if (angle < 0)
                    {
                        newIPointB = -2;
                        break;
                    }
                }
                else if (iSideB != i && sideB[i] == iPointB)
                {
                    newNormal = GetNormal(pointA, points[sideA[i]], towardsOpposite);
                    angle *= Vector3.SignedAngle(Direction(pointB, pointA), Direction(pointB, points[sideA[i]]), normalAxis);
                    Debug.Log(i + " " + normalAxis + " vs " + newNormal + " " + angle);

                    if ((angle == 180 || angle == -180) && (newNormal == normalAxis || newNormal == normalAxis))
                    {
                        newIPointB = sideA[i];
                        newISideB = i;
                    }
                    else if (angle < 0)
                    {
                        newIPointB = -2;
                        break;
                    }
                }
            }
            Debug.Log("newIPointB " + newIPointB);
            if (newIPointB >= 0)
            {
                iSideB = newISideB;
                iPointB = newIPointB;
                pointB = points[iPointB];
            }
            else break;
        }

        return true;
    }

    //Makes sure that a point is within the boundaries of a segment, but doent guarantee that it is on the segment
    static bool PointInRangeOfSegment(Vector3 point, Vector3 segmentA, Vector3 segmentB, int margin = 1)
    {
        float angle = Vector3.Angle((segmentA - point), (segmentB - point));
        //if(angle > 170 && debug)Debug.Log(angle);
        if (angle > 180 - margin) return true;

        return false;
    }

    //Finds the point on each line which is closest to the other
    static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines not parallel
        if (d != 0.0f)
        {
            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;
            return true;
        }
        else
        {
            return false;
        }
    }

    public float GetScale()
    {
        return scale;
    }
    public float GetMinScale()
    {
        return minScale;
    }
}




