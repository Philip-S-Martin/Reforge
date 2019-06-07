using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Mode
{
    POINT,
    SIDE,
    FRAME,
    VERTICAL,
    WALL
}

public class Blueprint : Structure
{
    const int maxSelections = 2;
    GameObject pointerPrefab;
    GameObject pointer;

    RaycastHit[] selection;
    Vector3[] selectPosition;
    Structure[] targets;
    int[] selectIndex;
    int[] selectType;
    int iSelection;
    int lastSelectIndex;
    int material;
    private GUIStyle currentStyle = null;
    Texture2D background;
    Texture2D[] mats;

    int[][] pointsOnSelection;
    Vector3[][] pointsOnWall;
    float length;
    float width;
    Vector3 lengthVector;
    bool isVertical = true;

    Mode selectionMode;

    // Use this for initialization
    new void Start()
    {
        base.Start();

        selection = new RaycastHit[maxSelections];
        selectPosition = new Vector3[maxSelections];
        targets = new Structure[maxSelections];
        selectIndex = new int[maxSelections];
        selectType = new int[maxSelections];
        iSelection = 0;


        length = 0;
        width = 0;

        pointsOnSelection = new int[maxSelections][];

        for (int i = 0; i < maxSelections; i++)
        {
            pointsOnSelection[i] = new int[] { -1, -1 };
        }

        pointsOnWall = new Vector3[maxSelections][];

        for (int i = 0; i < maxSelections; i++)
        {
            pointsOnWall[i] = new Vector3[] { Vector3.zero, Vector3.zero };
        }
    }

    public void SetPointer(GameObject newPointer)
    {
        GameObject pointerPrefab = newPointer;
        pointer = Instantiate(pointerPrefab) as GameObject;
    }

    void ResetModel()
    {
        thisRenderer.RenderNothing();
        thisData.ResetData();

        pointsOnSelection = new int[maxSelections][];

        for (int i = 0; i < maxSelections; i++)
        {
            pointsOnSelection[i] = new int[] { -1, -1 };
        }
    }

    public void BlueprintSelect(RaycastHit hit, GameObject target, float lengthQuantum)
    {
        //Change length if scroll wheel is activated + or -
        if (lengthQuantum > 0) length += thisData.GetScale();
        else if (lengthQuantum < 0) length -= thisData.GetScale();

        //Select position
        selectPosition[iSelection] = hit.point;
        //Take target(primary structure) and make that the target selection, this is temporary
        targets[iSelection] = target.GetComponent<Structure>() as Structure;
        //If the Ray hit the target, select from a position in the mesh
        if (hit.transform != null && hit.transform.gameObject == target)
            targets[iSelection].SelectFromMesh(ref selectPosition[iSelection], out selectIndex[iSelection], out selectType[iSelection], hit.triangleIndex);
        else
        {
            selectIndex[iSelection] = -1;
            selectType[iSelection] = -1;
        }
        //Handle dimensions
        if (iSelection == 1)
        {
            //WIDTH (Selection to selection)
            width = RoundUp(Vector3.Distance(selectPosition[0], selectPosition[1]));
            if (hit.transform != null && hit.transform.gameObject != target || selectType[iSelection] != 1)
                selectPosition[1] = selectPosition[0] + (selectPosition[1] - selectPosition[0]).normalized * width;
            if (width > 10) width = 10;

            //LENGTH VECTOR (managed by scrolling)
            if (!isVertical)
            {
                selectPosition[1].y = selectPosition[0].y;
                lengthVector = Vector3.Cross((selectPosition[1] - selectPosition[0]).normalized, Vector3.up);
            }
            else
            {
                lengthVector = Vector3.up;
            }
        }
        //Width cannot be assigned unless iSelection == 1 (second point)
        else width = 0;

        //Wall and Frame modes manage different types of data, so they have different handlers
        if (selectionMode == Mode.WALL)
            HandleWallMode();
        else
            HandleFrameMode();
    }

    public bool FirstSelection()
    {
        iSelection = 1;
        return true;
    }

    public bool SecondSelection()
    {

        BuildBlueprint();
        iSelection = 0;
        thisRenderer.RenderNothing();
        return false;
    }

    void HandleFrameMode()
    {
        if (width >= thisData.GetScale() && Mathf.Abs(length) >= thisData.GetScale())
        {
            if (selectionMode != Mode.FRAME)
            {
                ChangeToFrame();
            }
        }
        else if (width >= thisData.GetScale())
        {
            if (selectionMode != Mode.SIDE)
            {
                ChangeToSide();
            }
        }
        else if (Mathf.Abs(length) >= thisData.GetScale())
        {
            if (selectionMode != Mode.VERTICAL)
            {
                ChangeToVertical();
            }
        }
        else
        {
            if (selectionMode != Mode.POINT)
            {
                ChangeToPoint();
            }
            pointer.transform.position = selectPosition[iSelection];
        }

        thisData.MovePoint(pointsOnSelection[0][0], selectPosition[0]);
        thisData.MovePoint(pointsOnSelection[1][0], selectPosition[1]);
        thisData.MovePoint(pointsOnSelection[0][1], selectPosition[0] + lengthVector * RoundUp(length));
        thisData.MovePoint(pointsOnSelection[1][1], selectPosition[1] + lengthVector * RoundUp(length));

        thisRenderer.SteppedRenderStructure();
    }

    void HandleWallMode()
    {
        pointer.transform.position = selectPosition[iSelection];
        if (selectType[iSelection] == 1)
        {
            if (iSelection == 0)
            {
                targets[iSelection].GetSidePosition(selectIndex[0], out pointsOnWall[0][0], out pointsOnWall[0][1]);
            }
            if (iSelection == 1 && lastSelectIndex != selectIndex[iSelection])
            {
                targets[iSelection].GetSidePosition(selectIndex[0], out pointsOnWall[0][0], out pointsOnWall[0][1]);
                targets[iSelection].GetSidePosition(selectIndex[1], out pointsOnWall[1][0], out pointsOnWall[1][1]);

                if (StructureData.GetNormal(pointsOnWall[1][0], pointsOnWall[0][0], pointsOnWall[0][1]) != Vector3.zero)
                    targets[iSelection].MaximizeSide(selectIndex[0], pointsOnWall[1][0], out pointsOnWall[0][0], out pointsOnWall[0][1]);
                else
                    targets[iSelection].MaximizeSide(selectIndex[0], pointsOnWall[1][1], out pointsOnWall[0][0], out pointsOnWall[0][1]);

                if (StructureData.GetNormal(pointsOnWall[0][0], pointsOnWall[1][0], pointsOnWall[1][1]) != Vector3.zero)
                    targets[iSelection].MaximizeSide(selectIndex[1], pointsOnWall[0][0], out pointsOnWall[1][0], out pointsOnWall[1][1]);
                else
                    targets[iSelection].MaximizeSide(selectIndex[1], pointsOnWall[0][1], out pointsOnWall[1][0], out pointsOnWall[1][1]);

                if (pointsOnWall[0][1] != pointsOnWall[1][1] || pointsOnWall[0][0] != pointsOnWall[1][0])
                {
                    ResetModel();
                    BuildWall(pointsOnWall[0][0], pointsOnWall[0][1], pointsOnWall[1][0], pointsOnWall[1][1], material, true);
                    thisRenderer.SteppedRenderStructure();
                }
            }
        }
        else
        {
            pointsOnWall[iSelection][0] = Vector3.zero;
            pointsOnWall[iSelection][1] = Vector3.zero;
            ResetModel();
        }
        lastSelectIndex = selectIndex[iSelection];
    }

    public bool BuildBlueprint()
    {
        if (iSelection > 0) iSelection = 0;
        else return false;

        MergeIntoStructure(targets[0]);

        Start();

        return true;
    }



    void ChangeToVertical()
    {
        selectionMode = Mode.VERTICAL;
        pointer.transform.position = new Vector3(-9999, -9999, -9999);
        ResetModel();
        pointsOnSelection[0][0] = (thisData.CreatePoint(selectPosition[0], -1, false));
        pointsOnSelection[0][1] = (thisData.CreatePoint(selectPosition[0] + Vector3.up * RoundUp(length), -1, false));

        thisData.CreateSide(pointsOnSelection[0][0], pointsOnSelection[0][1], material);
    }

    void ChangeToSide()
    {
        selectionMode = Mode.SIDE;
        pointer.transform.position = new Vector3(-9999, -9999, -9999);
        ResetModel();
        pointsOnSelection[0][0] = (thisData.CreatePoint(selectPosition[0], -1, false));
        pointsOnSelection[1][0] = (thisData.CreatePoint(selectPosition[1], -1, false));

        thisData.CreateSide(pointsOnSelection[0][0], pointsOnSelection[1][0], material);
    }

    void ChangeToFrame()
    {
        selectionMode = Mode.FRAME;
        pointer.transform.position = new Vector3(-9999, -9999, -9999);
        ResetModel();
        pointsOnSelection[0][0] = (thisData.CreatePoint(selectPosition[0], -1, false));
        pointsOnSelection[1][0] = (thisData.CreatePoint(selectPosition[1], -1, false));
        pointsOnSelection[0][1] = (thisData.CreatePoint(selectPosition[0] + Vector3.up * RoundUp(length), -1, false));
        pointsOnSelection[1][1] = (thisData.CreatePoint(selectPosition[1] + Vector3.up * RoundUp(length), -1, false));

        thisData.CreateSide(pointsOnSelection[0][0], pointsOnSelection[1][0], material);
        thisData.CreateSide(pointsOnSelection[0][0], pointsOnSelection[0][1], material);
        thisData.CreateSide(pointsOnSelection[1][0], pointsOnSelection[1][1], material);
        thisData.CreateSide(pointsOnSelection[0][1], pointsOnSelection[1][1], material);
    }

    void ChangeToPoint()
    {
        selectionMode = Mode.POINT;
        ResetModel();
    }

    void ChangeToWall()
    {
        selectionMode = Mode.WALL;
        ResetModel();
    }

    public void ChangeDirection()
    {
        if (isVertical)
        {
            isVertical = false;
        }
        else
        {
            isVertical = true;
            lengthVector = Vector3.up;
        }
    }

    public void ChangeMode()
    {
        if (selectionMode != Mode.WALL)
            selectionMode = Mode.WALL;
        else
            ChangeToPoint();
    }

    public bool ChangeMaterial(int nMaterial)
    {
        if (material != nMaterial && nMaterial < thisRenderer.materials.GetMax())
        {
            material = nMaterial;
            return true;
        }
        else return false;
    }

    public int GetLength()
    {
        return (int)length;
    }

    public void ResetLength()
    {
        length = 0;
    }

    float RoundUp(float number)
    {
        return Mathf.Round(number * 2) / 2;
    }

    void OnGUI()
    {
        InitStyles();
        currentStyle.normal.background = background;
        GUI.Box(new Rect(Screen.width / 2 - thisRenderer.materials.GetMax() * 30 - 80, Screen.height - 45, 70, 20), "" + selectionMode, currentStyle);
        GUI.Box(new Rect(Screen.width / 2 - thisRenderer.materials.GetMax() * 30 + material * 60, Screen.height - 80, 50, 20), thisRenderer.materials.GetName(material), currentStyle);

        for (int i = 0; i < thisRenderer.materials.GetMax(); i++)
        {
            currentStyle.normal.background = mats[i];
            GUI.Box(new Rect(Screen.width / 2 - thisRenderer.materials.GetMax() * 30 + i * 60, Screen.height - 60, 50, 50), "", currentStyle);
        }
    }

    private void InitStyles()
    {
        if (currentStyle == null)
        {
            currentStyle = new GUIStyle(GUI.skin.box);
            currentStyle.normal.background = MakeTex(2, 2, new Color(0f, 1f, 0f, 0.5f));
            currentStyle.normal.textColor = new Color32(255, 255, 255, 255);

            background = MakeTex(2, 2, Color.black);
            mats = new Texture2D[thisRenderer.materials.GetMax()];
            for (int i = 0; i < thisRenderer.materials.GetMax(); i++)
            {
                mats[i] = MakeTex(2, 2, thisRenderer.materials.GetColor(i));
            }
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
