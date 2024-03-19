using UnityEngine;
using BepInEx;
using System;
using System.Collections;
using HarmonyLib;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UWE;
using LongLockerNames.Patches;
using static UnityEngine.UI.Selectable;

namespace LongLockerNames
{
    // QMods by qwiso https://github.com/Qwiso/QModManager
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency("com.snmodding.nautilus", BepInDependency.DependencyFlags.HardDependency)]
    public class QPatch : BaseUnityPlugin
    {
        public const String PLUGIN_GUID = "SN.LongLockerNames";
        public const String PLUGIN_NAME = "LongLockerNames.SN";
        public const String PLUGIN_VERSION = "1.0.0";
        public void Start()
        {
            // Pass the pickerHeight argument when calling LoadSmallLockerPrefabAsync

            StartCoroutine(uGUI_SignInput_Awake_Patch.LoadSmallLockerPrefabAndCreateButton());
            Mod.Patch("BepInEx/plugins/LongLockerNames");
        }
    }

       
}
