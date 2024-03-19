
using HarmonyLib;
using Nautilus.Extensions;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections;
using UWE;

namespace LongLockerNames.Patches
{ 
        [HarmonyPatch(typeof(uGUI_SignInput))]
        [HarmonyPatch("OnDeselect")]
        public static class uGUI_SignInput_OnDeselect_Patch
        {
            private static void Postfix(uGUI_SignInput __instance)
            {
                foreach (Transform child in __instance.transform)
                {
                    if (child.gameObject.name == "ColorPicker")
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_SignInput))]
        [HarmonyPatch("Awake")]
        public static class uGUI_SignInput_Awake_Patch
        {
            private static Button buttonPrefab;
            private const float TextFieldHeight = 600;

            private static void Postfix(uGUI_SignInput __instance)
            {
                if (IsOnSmallLocker(__instance))
                {
                    AddColors(__instance);
                    PatchSmallLocker(__instance);
                }
                else if (IsOnSign(__instance))
                {
                    AddColors(__instance);
                    PatchSign(__instance);
                }
            }

            public static IEnumerator LoadSmallLockerPrefabAsync()
            {
                // Load the small locker prefab from resources asynchronously
                ResourceRequest request = Resources.LoadAsync<GameObject>("Submarine/Build/SmallLocker");

                // Wait until the loading is complete
                yield return request;

                // Check if the loading operation was successful
                if (request.asset != null)
                {
                    // Get the loaded small locker prefab
                    smallLockerPrefab = request.asset as GameObject;

                    // Call the method to continue the logic
                    OnSmallLockerPrefabLoaded();
                }
                else
                {
                    Debug.LogError("Small locker prefab not found.");
                }
            }
            private static bool IsOnSmallLocker(uGUI_SignInput __instance)
            {
                var root = __instance.gameObject.GetComponentInParent<Constructable>();
                return root.gameObject.name.Contains("SmallLocker");
            }

            private static bool IsOnSign(uGUI_SignInput __instance)
            {
                var root = __instance.gameObject.GetComponentInParent<Constructable>();
                return root.gameObject.name.Contains("Sign");
            }

            private static void PatchSmallLocker(uGUI_SignInput __instance)
            {
                __instance.inputField.lineType = TMPro.TMP_InputField.LineType.MultiLineNewline;
                __instance.inputField.characterLimit = Mod.config.SmallLockerTextLimit;

                var rt = __instance.inputField.transform as RectTransform;
                RectTransformExtensions.SetSize(rt, rt.rect.width, TextFieldHeight);

                GameObject.Destroy(__instance.inputField.textComponent.GetComponent<ContentSizeFitter>());
                rt = __instance.inputField.textComponent.transform as RectTransform;
                RectTransformExtensions.SetSize(rt, rt.rect.width, TextFieldHeight);

                __instance.inputField.textComponent.alignment = TextAlignmentOptions.Center;

                if (Mod.config.ColorPickerOnLockers)
                {
                    var currentButton = __instance.transform.GetChild(1).GetComponent<Button>();
                    var height = Mod.config.ExtraColorsOnLockers ? 1200 : 210;
                    AddColorPickerSystem(__instance, ref currentButton, "LOCKER", -20, height);
                }
            }

            private static void AddColorPickerSystem(uGUI_SignInput __instance, ref Button currentButton, string label, int xOffset, int pickerHeight)
            {
                if (currentButton != null)
                {
                    if (buttonPrefab == null)
                    {
                        CreateButtonPrefab();
                    }

                    var picker = AddColorPicker(__instance, label, xOffset, pickerHeight);

                    var go = currentButton.gameObject;
                    GameObject.DestroyImmediate(currentButton);
                    currentButton = go.AddComponent<Button>();
                    currentButton.transition = buttonPrefab.transition;
                    currentButton.targetGraphic = go.GetComponentInChildren<Image>();
                    currentButton.colors = buttonPrefab.colors;

                    currentButton.onClick.RemoveAllListeners();
                    var instance = __instance;
                    currentButton.onClick.AddListener(() =>
                    {
                        picker.SetActive(!picker.activeSelf);
                    });

                    __instance.colorizedElements = __instance.colorizedElements.Concat(new[] { picker.GetComponentInChildren<TextMeshProUGUI>() }).ToArray();
                }
            }

            private static void CreateButtonPrefab()
            {
                // Start the coroutine to load the small locker prefab asynchronously


            }

            // Call this method when the small locker prefab is loaded and ready to use
            private static void OnSmallLockerPrefabLoaded()
            {
                // Check if the small locker prefab is loaded successfully
                if (smallLockerPrefab != null)
                {
                    // Get the uGUI_SignInput component from the small locker
                    var signInput = smallLockerPrefab.GetComponentInChildren<uGUI_SignInput>();

                    // Check if the uGUI_SignInput component is found
                    if (signInput != null)
                    {
                        // Proceed with setting up the button prefab
                        var original = signInput.transform.GetChild(1)?.gameObject.GetComponent<Button>();

                        // Check if the original button is found
                        if (original != null)
                        {
                            // Instantiate the button prefab
                            buttonPrefab = GameObject.Instantiate(original);

                            // Get the button gameObject
                            var go = buttonPrefab.gameObject;

                            // Destroy the original button gameObject to avoid memory leaks
                            GameObject.DestroyImmediate(original.gameObject);

                            // Add the Button component to the button prefab gameObject
                            buttonPrefab = go.AddComponent<Button>();

                            // Copy properties from the original button to the button prefab
                            buttonPrefab.transition = original.transition;
                            buttonPrefab.targetGraphic = go.GetComponentInChildren<Image>();
                            buttonPrefab.colors = original.colors;

                            // Activate the button prefab gameObject
                            go.SetActive(true);
                        }
                        else
                        {
                            Debug.LogError("Original button is null.");
                        }
                    }
                    else
                    {
                        Debug.LogError("uGUI_SignInput component not found.");
                    }
                }
                else
                {
                    Debug.LogError("Small locker prefab is null.");
                }
            }

            private static GameObject AddColorPicker(uGUI_SignInput __instance, string label, int xOffset, int pickerHeight)
            {
                var picker = new GameObject("ColorPicker", typeof(RectTransform));
                picker.SetActive(false);

                var rt = picker.transform as RectTransform;
                RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), __instance.transform);
                RectTransformExtensions.SetSize(rt, 430, pickerHeight);
                rt.anchoredPosition += new Vector2(xOffset, 0);

                var image = picker.gameObject.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 1f);

                var layout = picker.gameObject.AddComponent<GridLayoutGroup>();
                layout.cellSize = new Vector2(60, 60);
                layout.padding = new RectOffset(20, 20, 130, 20);
                layout.spacing = new Vector2(-5, -5);

                var textGO = new GameObject("ExampleText", typeof(RectTransform));
                var text = textGO.AddComponent<TextMeshProUGUI>();
                text.text = label;
                text.font = __instance.inputField.textComponent.font as TMP_FontAsset;
                text.fontSize = __instance.inputField.textComponent.fontSize + 12;
                text.fontStyle = __instance.inputField.textComponent.fontStyle;
                text.color = __instance.inputField.textComponent.color;
                text.alignment = TextAlignmentOptions.Center;

                var l = text.gameObject.AddComponent<LayoutElement>();
                l.ignoreLayout = true;

                RectTransformExtensions.SetParams(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), picker.transform);
                RectTransformExtensions.SetSize(text.rectTransform, 430, 130);
                text.rectTransform.anchoredPosition = new Vector2(0, 0);

                for (int i = 0; i < __instance.colors.Length; ++i)
                {
                    var color = __instance.colors[i];

                    var colorButton = new GameObject("ColorPickerButton", typeof(RectTransform));
                    colorButton.gameObject.SetActive(true);

                    rt = colorButton.transform as RectTransform;
                    RectTransformExtensions.SetParams(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), picker.transform);

                    image = colorButton.AddComponent<Image>();
                    image.color = color;
                    image.sprite = buttonPrefab.GetComponentInChildren<Image>().sprite;

                    var le = colorButton.AddComponent<LayoutElement>();
                    le.minWidth = le.minHeight = 60;

                    var button = colorButton.AddComponent<Button>();
                    button.transition = buttonPrefab.transition;
                    button.colors = buttonPrefab.colors;

                    var instance = __instance;
                    var colorIndex = i;
                    button.onClick.AddListener(() =>
                    {
                        var old = instance.colorIndex;
                        instance.colorIndex = colorIndex;
                    });
                }

                return picker;
            }

            private static void PatchSign(uGUI_SignInput __instance)
            {
                __instance.inputField.characterLimit = Mod.config.SignTextLimit;

                if (Mod.config.ColorPickerOnSigns)
                {
                    var currentButton = __instance.transform.GetChild(0).GetComponent<Button>();
                    var height = Mod.config.ExtraColorsOnSigns ? 1200 : 210;
                    AddColorPickerSystem(__instance, ref currentButton, "SIGN", -20, height);
                }
            }



            private static void AddColors(uGUI_SignInput __instance)
            {
                var originalColorCount = __instance.colors.Length;

                if (originalColorCount == 7 && !Mod.config.ExtraColorsOnLockers)
                {
                    return;
                }
                if (originalColorCount == 8 && !Mod.config.ExtraColorsOnSigns)
                {
                    return;
                }

                var newColors = new Color[] {
                rgb(205, 92, 92),
                rgb(240, 128, 128),
                rgb(250, 128, 114),
                rgb(233, 150, 122),
                rgb(255, 160, 122),
                rgb(220, 20, 60),
                rgb(255, 0, 0),
                rgb(178, 34, 34),
                rgb(139, 0, 0),
                rgb(255, 192, 203),
                rgb(255, 182, 193),
                rgb(255, 105, 180),
                rgb(255, 20, 147),
                rgb(199, 21, 133),
                rgb(219, 112, 147),
                rgb(255, 160, 122),
                rgb(255, 127, 80),
                rgb(255, 99, 71),
                rgb(255, 69, 0),
                rgb(255, 140, 0),
                rgb(255, 165, 0),
                rgb(255, 215, 0),
                rgb(255, 255, 0),
                rgb(255, 255, 224),
                rgb(255, 250, 205),
                rgb(250, 250, 210),
                rgb(255, 239, 213),
                rgb(255, 228, 181),
                rgb(255, 218, 185),
                rgb(238, 232, 170),
                rgb(240, 230, 140),
                rgb(189, 183, 107),
                rgb(230, 230, 250),
                rgb(216, 191, 216),
                rgb(221, 160, 221),
                rgb(238, 130, 238),
                rgb(218, 112, 214),
                rgb(255, 0, 255),
                rgb(255, 0, 255),
                rgb(186, 85, 211),
                rgb(147, 112, 219),
                rgb(102, 51, 153),
                rgb(138, 43, 226),
                rgb(148, 0, 211),
                rgb(153, 50, 204),
                rgb(139, 0, 139),
                rgb(128, 0, 128),
                rgb(75, 0, 130),
                rgb(106, 90, 205),
                rgb(72, 61, 139),
                rgb(123, 104, 238),
                rgb(173, 255, 47),
                rgb(127, 255, 0),
                rgb(124, 252, 0),
                rgb(0, 255, 0),
                rgb(50, 205, 50),
                rgb(152, 251, 152),
                rgb(144, 238, 144),
                rgb(0, 250, 154),
                rgb(0, 255, 127),
                rgb(60, 179, 113),
                rgb(46, 139, 87),
                rgb(34, 139, 34),
                rgb(0, 128, 0),
                rgb(0, 100, 0),
                rgb(154, 205, 50),
                rgb(107, 142, 35),
                rgb(128, 128, 0),
                rgb(85, 107, 47),
                rgb(102, 205, 170),
                rgb(143, 188, 139),
                rgb(32, 178, 170),
                rgb(0, 139, 139),
                rgb(0, 128, 128),
                rgb(0, 255, 255),
                rgb(0, 255, 255),
                rgb(224, 255, 255),
                rgb(175, 238, 238),
                rgb(127, 255, 212),
                rgb(64, 224, 208),
                rgb(72, 209, 204),
                rgb(0, 206, 209),
                rgb(95, 158, 160),
                rgb(70, 130, 180),
                rgb(176, 196, 222),
                rgb(176, 224, 230),
                rgb(173, 216, 230),
                rgb(135, 206, 235),
                rgb(135, 206, 250),
                rgb(0, 191, 255),
                rgb(30, 144, 255),
                rgb(100, 149, 237),
                rgb(123, 104, 238),
                rgb(65, 105, 225),
                rgb(0, 0, 255),
                rgb(0, 0, 205),
                rgb(0, 0, 139),
                rgb(0, 0, 128),
                rgb(25, 25, 112),
                rgb(255, 248, 220),
                rgb(255, 235, 205),
                rgb(255, 228, 196),
                rgb(255, 222, 173),
                rgb(245, 222, 179),
                rgb(222, 184, 135),
                rgb(210, 180, 140),
                rgb(188, 143, 143),
                rgb(244, 164, 96),
                rgb(218, 165, 32),
                rgb(184, 134, 11),
                rgb(205, 133, 63),
                rgb(210, 105, 30),
                rgb(139, 69, 19),
                rgb(160, 82, 45),
                rgb(165, 42, 42),
                rgb(128, 0, 0),
                rgb(255, 255, 255),
                rgb(255, 250, 250),
                rgb(240, 255, 240),
                rgb(192, 192, 192),
                rgb(169, 169, 169),
                rgb(128, 128, 128),
                rgb(105, 105, 105),
                rgb(119, 136, 153),
                rgb(112, 128, 144)
            };

                if (originalColorCount == 7)
                {
                    newColors = newColors.Append(rgb(47, 79, 79)).ToArray();
                }

                __instance.colors = __instance.colors.Concat(newColors).ToArray();
            }
            private static GameObject smallLockerPrefab;
            private static Color rgb(int r, int g, int b)
            {
                return new Color(r / 255f, g / 255f, b / 255f);
            }

        }
    }
