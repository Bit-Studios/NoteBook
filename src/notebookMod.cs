using UnityEngine;
using KSP.Game;
using SpaceWarp.API.Mods;
using Screen = UnityEngine.Screen;
using KSP.UI.Binding;
using KSP.Sim.impl;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.OAB;
using KSP.Modules;
using Shapes;
using SpaceWarp.API.Assets;
using SpaceWarp;
using BepInEx;
using SpaceWarp.API.UI.Appbar;
using KSP.Messages;
using Steamworks;
using Unity.Mathematics;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.State;
using KSP;
using System.IO.Ports;
using UnityEngine.UI;
using System.Reflection;
using static VehiclePhysics.VPAudio;

namespace notebook;
[BepInPlugin("com.shadowdev.notebook", "notebook", "0.0.1")]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class notebookMod : BaseSpaceWarpPlugin
{
    private const char V = '/';
    private int windowWidth = 700;
    private int windowHeight = 700;
    private Rect windowRect;
    private static GUIStyle boxStyle;
    private bool showUI = false;
    private static string SetWindowWidthStr = "300";
    public static bool IsDev = false;
    private static bool ShowConfig = false;
    public static string NotebookMode = "list";
    private static string LocationFile = Assembly.GetExecutingAssembly().Location;
    private static string LocationDirectory = Path.GetDirectoryName(LocationFile);
    public static string ActiveFile = $"{LocationFile}/null.note";
    public static int AddAtIndex = 0;
    public static string NewText = "";
    public static bool InputEnabled = true;
    public override void OnInitialized()
    {
        Appbar.RegisterAppButton(
           "NoteBook",
            "BTN-NB",
            AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"), ToggleButton);
    }
    void Awake()
    {
        windowRect = new Rect((Screen.width * 0.85f) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), 0, 0);
    }
    void Update()
    {
        if (!IsDev)
        {
            
        }
        if (Directory.Exists($"{LocationDirectory}/notes")) { }
        else
        {
            Directory.CreateDirectory($"{LocationDirectory}/notes");
        }
        if (Directory.Exists($"{LocationDirectory}/notes/{GameManager.Instance.Game.SessionGuidString}")) { }
        else
        {
            Directory.CreateDirectory($"{LocationDirectory}/notes/{GameManager.Instance.Game.SessionGuidString}");
        }
    }
    void ToggleButton(bool toggle)
    {
        showUI = toggle;
        GameObject.Find("BTN-NB")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(toggle);
        
    }
    private void ListNotes()
    {
        boxStyle = GUI.skin.GetStyle("Box");
        string[] Notes = Directory.GetFiles($"{LocationDirectory}/notes/{GameManager.Instance.Game.SessionGuidString}", "*.note");
        foreach (string Note in Notes)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{new FileInfo(Note).Name}", GUILayout.Width(windowWidth / 1.5f));
            if (GUILayout.Button("Open"))
            {
                NotebookMode = "show";
                ActiveFile = Note;
                AddAtIndex = 0;
            }
            GUILayout.EndHorizontal();
            
        }
        GUILayout.BeginHorizontal();
        NewText = GUILayout.TextArea(NewText, GUILayout.Width(windowWidth / 1.1f));
        if (GUILayout.Button("Add"))
        {
            File.WriteAllText($"{LocationDirectory}/notes/{GameManager.Instance.Game.SessionGuidString}/{NewText}.note", "New Note");
            NewText = "";

        }

        GUILayout.EndHorizontal();
    }
    private void DisplayNote()
    {
        boxStyle = GUI.skin.GetStyle("Box");
        string[] noteDataS = File.ReadAllLines(ActiveFile);
        List<string> noteData = new List<string>();
        noteData = noteDataS.ToList<string>();
        int cind = 0;
        foreach (string Note in noteDataS)
        {
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Note}", GUILayout.Width(windowWidth / 1.1f));
            if (GUILayout.Button("X"))
            {
                noteData.Remove(Note);
                File.WriteAllLines(ActiveFile, noteData.ToArray());
            }
            if (GUILayout.Button("+"))
            {
                AddAtIndex = cind;
            }
            GUILayout.EndHorizontal();
            
            if (cind == AddAtIndex)
            {
                GUILayout.BeginHorizontal();
                NewText = GUILayout.TextArea(NewText, GUILayout.Width(windowWidth / 1.1f));
                if (GUILayout.Button("Add"))
                {
                    noteData.Insert(cind, NewText);
                    NewText = "";
                    File.WriteAllLines(ActiveFile, noteData.ToArray());
                }
                
                GUILayout.EndHorizontal();
            }
            cind++;
        }
        if(AddAtIndex == -1)
        {
            GUILayout.BeginHorizontal();
            NewText = GUILayout.TextArea(NewText, GUILayout.Width(windowWidth / 1.1f));
            if (GUILayout.Button("Add"))
            {
                noteData.Insert(cind, NewText);
                NewText = "";
                File.WriteAllLines(ActiveFile, noteData.ToArray());
            }

            GUILayout.EndHorizontal();
        }
        else
        {
            if (GUILayout.Button("+"))
            {
                AddAtIndex = -1;
            }
        }
        
    }
    void OnGUI()
    {
        GUI.skin = SpaceWarp.API.UI.Skins.ConsoleSkin;
        if (showUI)
        {
            windowRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                windowRect,
                FillWindow,
                "Notebook",
                GUILayout.Height(0),
                GUILayout.Width(windowWidth)) ;
        }
    }
    private void FillWindow(int windowID)
    {
        boxStyle = GUI.skin.GetStyle("Box");
        GUILayout.BeginVertical();
        if (GUI.Button(new Rect(windowWidth - 23, 6, 18, 18), "<b>x</b>", new GUIStyle(GUI.skin.button) { fontSize = 10, }))
        {
            NewText = "";
            NotebookMode = "list";
            showUI = false;
        }
        if (InputEnabled)
        {
            if (GUI.Button(new Rect(windowWidth - 73, 6, 30, 18), "<color=green><b>INP</b></color>", new GUIStyle(GUI.skin.button) { fontSize = 10, }))
            {
                GameManager.Instance.Game.Input.Disable();
                InputEnabled = false;
            }
        }
        else
        {
            if (GUI.Button(new Rect(windowWidth - 73, 6, 30, 18), "<color=red><b>INP</b></color>", new GUIStyle(GUI.skin.button) { fontSize = 10, }))
            {
                GameManager.Instance.Game.Input.Enable();
                InputEnabled = true;
            }
        }
        
        if (GUI.Button(new Rect(23, 6, 18, 18), "<b><</b>", new GUIStyle(GUI.skin.button) { fontSize = 10, }))
        {
            NewText = "";
            NotebookMode = "list";
        }
        switch (NotebookMode)
        {
            case "list":
                ListNotes();
                break;
            case "show":
                DisplayNote();
                break;
        }
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, windowWidth, 700));
    }
}