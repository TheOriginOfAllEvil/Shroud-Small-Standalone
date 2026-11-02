using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.IO;
using SS_ScriptHandler;

namespace ShroudSmallPatcher
{
    public class MatLib
    {
        //Declaring the materials to use
        public static Material convexHull;
        public static Material foam;
        public static Material objectInteraction;
        public static Material water4;
        public static Material particleSplash;
        public static Material mask;

        //Declaring the main file paths
        public const string boatMain = "BOAT medi medium (50)";
        public const string boatSub = "medi medium new";
        public const string structConst = "structure_container";

        public static void RegisterMaterials()
        {
            convexHull = GameObject.Find(boatMain).transform.Find(boatSub).transform.Find(structConst).transform.Find("mask").GetComponent<MeshRenderer>().material;
            foam = GameObject.Find(boatMain).transform.Find("WaterFoam (1)").GetComponent<MeshRenderer>().material;
            objectInteraction = GameObject.Find(boatMain).transform.Find("WaterObjectInteractionSphereBack").GetComponent<MeshRenderer>().material;
            water4 = GameObject.Find(boatMain).transform.Find(boatSub).transform.Find("damage_water").GetComponent<MeshRenderer>().material;
            particleSplash = GameObject.Find(boatMain).transform.Find("overflow particles (2)").GetComponent<Renderer>().material;
            mask = GameObject.Find(boatMain).transform.Find(boatSub).transform.Find(structConst).transform.Find("mask_splash").GetComponent<MeshRenderer>().material;

            if (convexHull != null && foam != null && objectInteraction != null && water4 != null)
            {
                Debug.LogWarning("Sh'ba: Materials Registered");
            }
        }
    }

    internal class Patches
    {


        [HarmonyPatch(typeof(FloatingOriginManager))]
        public static class FloatingOriginManagerPatches
        {
            public static GameObject riverSloop;
            public static GameObject embarkBoat;
            public static bool boatInstalled;

            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            public static void StartPatch(FloatingOriginManager __instance)
            {
                if (__instance.name == "_shifting world")
                {
                    ShipSetup();
                    if (riverSloop == null)
                    {
                        Debug.LogError("Sh'ba is null");
                    }

                    else
                    {
                        GameObject sloop = Object.Instantiate(riverSloop, __instance.transform);
                        embarkBoat = GameObject.Find("WALK Shroud Small");
                        var walkCol = GameObject.Find("walk cols").transform;
                        embarkBoat.transform.SetParent(walkCol);
                        embarkBoat.transform.localPosition = new Vector3(-175, 0, -175);

                        doorSetup(sloop);

                        MatLib.RegisterMaterials();
                        sloop.transform.Find("WaterFoam").GetComponent<MeshRenderer>().material = MatLib.foam;
                        sloop.transform.Find("WaterObjectInteractionSphereBack").GetComponent<MeshRenderer>().material = MatLib.objectInteraction;
                        sloop.transform.Find("WaterObjectInteractionSphereFront").GetComponent<MeshRenderer>().material = MatLib.objectInteraction;
                        sloop.transform.Find("shroud small").transform.Find("Sh'ba Mask").GetComponent<MeshRenderer>().material = MatLib.convexHull;
                        sloop.transform.Find("shroud small").transform.Find("damage_water").GetComponent<MeshRenderer>().material = MatLib.water4;
                        sloop.transform.Find("overflow particles left").GetComponent<Renderer>().material = MatLib.particleSplash;
                        sloop.transform.Find("overflow particles right").GetComponent<Renderer>().material = MatLib.particleSplash;
                        //sloop.transform.Find("shroud small").transform.Find("Sloop_Splash_Mask").GetComponent<MeshRenderer>().material = MatLib.mask;

                        Debug.LogWarning("Boat loaded");
                    }
                }
                else
                {
                    GameState.currentBoat = null;
                    GameState.lastBoat = null;
                }
            }
            private static void ShipSetup()
            {   //load the assets from the bundle, assigns the script to the assets

                string assetPath = Paths.PluginPath + "\\Shattered Seas Small\\shroud small";
                if (!File.Exists(assetPath))
                {
                    Debug.LogError("Boat not installed correctly or removed");
                    boatInstalled = false;
                }
                else
                {
                    var bundle = AssetBundle.LoadFromFile(assetPath);
                    string boatPath = "Assets/Shattered Seas/BOAT Shroud Small (160).prefab";
                    riverSloop = bundle.LoadAsset(boatPath) as GameObject;
                    boatInstalled = true;
                }

            }

            private static void doorSetup(GameObject boat)
            {
                foreach (GPButtonTrapdoor component in boat.GetComponentsInChildren<GPButtonTrapdoor>())
                {
                    component.gameObject.AddComponent<DoorColToggler>();
                }
            }
        }
    }
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class BoatPatcher : BaseUnityPlugin
    {
        public const string pluginGuid = "com.TheOriginOfAllEvil.riverSloop";
        internal const string pluginName = "Shattered Seas Shroud Small";
        public const string pluginVersion = "1.3.0";
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {pluginGuid} is loaded!");

            //Initialising Harmony Instance
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), pluginGuid);
        }
    }
}

