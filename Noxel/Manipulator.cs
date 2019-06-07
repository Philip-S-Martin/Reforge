using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class Manipulator : MonoBehaviour
{
    RaycastHit rayHit;
    Ray rayBase;
    private float rayTime;
    public Camera camera;
    private Quaternion lastRotation;
    private Vector3 lastPos;
    GUIStyle style;
    public int saveNum;
    int materialID;

    public GameObject structurePrefab;
    public GameObject blueprintPrefab;
    public GameObject pointerPrefab;
    GameObject structure;
    GameObject blueprint;
    Structure buildStructure;
    Blueprint userBlueprint;

    public GameObject characterController;
    bool menuOpen;

    void Start()
    {
        style = new GUIStyle();
        saveNum = 0;
        materialID = 0;

        structure = Instantiate(structurePrefab) as GameObject;
        blueprint = Instantiate(blueprintPrefab) as GameObject;

        buildStructure = structure.GetComponent(typeof(Structure)) as Structure;
        userBlueprint = blueprint.GetComponent(typeof(Blueprint)) as Blueprint;

        userBlueprint.SetPointer(pointerPrefab);
        lastRotation = camera.transform.rotation;
        lastPos = camera.transform.position;
        menuOpen = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            menuOpen = !menuOpen;
            if(menuOpen)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            if (camera.transform.rotation!=lastRotation || camera.transform.position!=lastPos)
            {
                rayBase = camera.ScreenPointToRay(Input.mousePosition);
                lastRotation = camera.transform.rotation;
                lastPos = camera.transform.position;
                //
                if (Physics.Raycast(rayBase, out rayHit, 100))
                {
                    
                }
            }
            for (int i = 1; i < 10; ++i)
            {
                if (Input.GetKeyDown("" + i))
                {
                    userBlueprint.ChangeMaterial(i - 1);
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                userBlueprint.ChangeDirection();
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                userBlueprint.ChangeDirection();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                userBlueprint.ChangeMode();
            }

            //here
            userBlueprint.BlueprintSelect(rayHit, structure, Input.GetAxis("Mouse ScrollWheel"));
            if (Input.GetMouseButtonDown(0))
            {
                if (userBlueprint.GetLength() != 0)
                {
                    userBlueprint.ResetLength();
                }
                else if (rayHit.transform.gameObject == structure)
                    buildStructure.RemovePart(rayHit);
            }

            if (Input.GetMouseButtonDown(1))
            {
                userBlueprint.FirstSelection();
            }
            if (Input.GetMouseButtonUp(1))
            {
                userBlueprint.SecondSelection();
            }
        }
    }

    void HandleMenu()
    {
        GUI.color = Color.white;
        GUI.backgroundColor = Color.black;
        GUI.contentColor = Color.white;

        for (int i = 0; i < 10; ++i)
        {
            if (Input.GetKeyDown("" + i))
            {
                saveNum = i;
            }
        }
        if (GUI.Button(new Rect(20, 20, 40, 40), saveNum + " -"))
        {
            saveNum--;
            if (saveNum < 0) saveNum = 9;
        }
        if (GUI.Button(new Rect(80, 20, 40, 40), saveNum + " +"))
        {
            saveNum++;
            if (saveNum > 9) saveNum = 0;
        }
        if (GUI.Button(new Rect(20, 70, 100, 40), "Save " + saveNum))
        {
            StructureData newSaveGame = buildStructure.GetData();
            string saveGameName = "" + saveNum;
            SaveLoad.Save(newSaveGame, saveGameName);
        }

        if (SaveLoad.SaveExists("" + saveNum) && GUI.Button(new Rect(20, 120, 100, 40), "Load " + saveNum))
        {
            StructureData loadedGame = SaveLoad.Load("" + saveNum);
            if (loadedGame != null)
            {
                buildStructure.SetData(loadedGame);
            }
        }
    }

    void OnGUI()
    {
        if (menuOpen)
        {
            HandleMenu();
            GUI.Box(new Rect(Screen.width - 210, 10, 200, 180), 
                "\nControls:\n\n" +
                "WASD - Movement\n" +
                "Left Click - Delete/Zero Height\n" +
                "Right Click - Menu\n" +
                "Scroll Wheel - Change Height\n" +
                "Hold Shift - Build Horizontal\n" +
                "E - Change Build Mode\n" +
                "Left Shift - Sprint\n" +
                "L - Lock Cursor\n" +
                "Esc - Unlock Cursor");
        }
        else{
            GUI.Box(new Rect(-5,-5,150,20), "Press Tab for Menu");
        }
        GUI.Box(new Rect(Input.mousePosition.x - 5, Screen.height - Input.mousePosition.y - 5, 10, 10), "");
        GUI.Box(new Rect(Input.mousePosition.x - 5, Screen.height - Input.mousePosition.y - 5, 10, 10), "");
    }
}

