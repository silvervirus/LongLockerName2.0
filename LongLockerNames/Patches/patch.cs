
using HarmonyLib;
using Nautilus.Extensions;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UWE;
using static System.Net.Mime.MediaTypeNames;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;

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
            // Set input field properties
            __instance.inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            __instance.inputField.characterLimit = Mod.config.SmallLockerTextLimit;

            // Adjust input field size
            RectTransform rt = __instance.inputField.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, TextFieldHeight);

            // Remove ContentSizeFitter component from text component
            GameObject.Destroy(__instance.inputField.textComponent.GetComponent<ContentSizeFitter>());

            // Adjust text component size
            rt = __instance.inputField.textComponent.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, TextFieldHeight);

            // Set text alignment
            __instance.inputField.textComponent.alignment = TextAlignmentOptions.Center;

            // Add color picker system if enabled
            if (Mod.config.ColorPickerOnLockers)
            {
                Button currentButton = __instance.transform.GetChild(1).GetComponent<Button>();
                int height = Mod.config.ExtraColorsOnLockers ? 1200 : 210;
                AddColorPickerSystem(__instance, currentButton, "Locker",  -20, 0, height);
            }
        }


        private static Button AddColorPickerSystem(uGUI_SignInput __instance, Button currentButton, string label, int xOffset, int yOffset, int pickerHeight)
        {
            if (currentButton != null)
            {
                if (buttonPrefab == null)
                {
                    CreateButtonPrefab(smallLockerPrefab);
                }

                var picker = AddColorPicker(__instance, label, xOffset, yOffset, pickerHeight);

                var go = currentButton.gameObject;
                GameObject.DestroyImmediate(currentButton);
                currentButton = go.AddComponent<Button>();
                currentButton.transition = buttonPrefab.transition;
                currentButton.targetGraphic = go.GetComponentInChildren<Image>();
                currentButton.colors = buttonPrefab.colors;

                currentButton.onClick.RemoveAllListeners();
                currentButton.onClick.AddListener(() => {
                    picker.SetActive(!picker.activeSelf);
                });

                __instance.colorizedElements = __instance.colorizedElements.Concat(new[] { picker.GetComponentInChildren<TextMeshProUGUI>() }).ToArray();

                return currentButton;
            }

            return null;
        }



        public static IEnumerator LoadSmallLockerPrefabAndCreateButton()
        {
            // Start loading the prefab asynchronously
            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(TechType.SmallLocker);

            // Wait for the prefab task to complete:
            yield return task;

            // Check if the task was successful
            if (task.GetResult())
            {
                // Get the loaded small locker prefab
                GameObject prefab = task.GetResult();
                smallLockerPrefab = prefab;

                // Call the method to create the button prefab
                OnSmallLockerPrefabLoadedAndCreateButton();
            }
            else
            {
                Debug.LogError("Failed to load Small locker prefab.");
            }
        }





        private static void OnSmallLockerPrefabLoadedAndCreateButton()
        {
            // Call the method to create the button prefab
            CreateButtonPrefab(smallLockerPrefab);
        }

        private static void CreateButtonPrefab(GameObject smallLockerPrefab)
        {
            // Access small locker prefab to get the original button
            var signInput = smallLockerPrefab.GetComponentInChildren<uGUI_SignInput>();
            var original = signInput.transform.GetChild(1).gameObject.GetComponent<Button>();

            // Create the button prefab
            buttonPrefab = GameObject.Instantiate(original);
            var go = buttonPrefab.gameObject;
            GameObject.DestroyImmediate(buttonPrefab);
            buttonPrefab = go.AddComponent<Button>();

            // Set button properties
            buttonPrefab.transition = original.transition;
            buttonPrefab.targetGraphic = go.GetComponentInChildren<Image>();
            buttonPrefab.colors = original.colors;

            go.SetActive(true);
        }
        private static GameObject AddColorPicker(uGUI_SignInput __instance, string labelText, int xOffset, int yOffset, int pickerHeight)
        {
            GameObject picker = new GameObject("ColorPicker", typeof(RectTransform));
            picker.SetActive(false);

            RectTransform pickerRT = picker.transform as RectTransform;
            RectTransformExtensions.SetParams(pickerRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), __instance.transform);
            pickerRT.sizeDelta = new Vector2(480f, (float)pickerHeight); // Adjust the width to match the background size
            pickerRT.anchoredPosition += new Vector2((float)xOffset, (float)yOffset);

            // Load circular sprite
            Sprite circularSprite = null;
            Atlas.Sprite atlasSprite = RamuneLib.Utils.ImageUtils.GetSprite("Circle");
            if (atlasSprite != null)
            {
                Texture2D texture = atlasSprite.texture;
                if (texture != null)
                {
                    circularSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                }
            }

            // Add black background
            GameObject background = new GameObject("Background", typeof(RectTransform));
            RectTransform bgRT = background.transform as RectTransform;
            bgRT.SetParent(pickerRT, false);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = new Vector2(60.00f, -142.86f); // Adjust the background size
            bgRT.anchoredPosition = Vector2.zero;

            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = Color.black;

            GameObject labelTextObject = new GameObject("LabelText", typeof(RectTransform));
            TextMeshProUGUI label = labelTextObject.AddComponent<TextMeshProUGUI>();
            label.text = labelText;
            label.font = __instance.inputField.textComponent.font;
            label.fontSize = (int)(__instance.inputField.textComponent.fontSize + 9f);
            label.fontStyle = __instance.inputField.textComponent.fontStyle;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Top;
            RectTransform labelRT = labelTextObject.transform as RectTransform;
            labelRT.SetParent(pickerRT, false);
            labelRT.anchorMin = new Vector2(0f, 1f);
            labelRT.anchorMax = new Vector2(1f, 1f);
            labelRT.pivot = new Vector2(0.5f, 1f);
            labelRT.sizeDelta = new Vector2(0f, 40f);

            GameObject pickerPart = new GameObject("PickerPart", typeof(RectTransform));
            RectTransform pickerPartRT = pickerPart.transform as RectTransform;
            pickerPartRT.SetParent(pickerRT, false);
            pickerPartRT.anchorMin = new Vector2(0f, 1f);
            pickerPartRT.anchorMax = new Vector2(1f, 1f);
            pickerPartRT.pivot = new Vector2(0.5f, 1f);
            pickerPartRT.anchoredPosition = new Vector2(0f, -80f);

            GridLayoutGroup gridLayoutGroup = pickerPart.AddComponent<GridLayoutGroup>();
            gridLayoutGroup.padding = new RectOffset(20, 20, 20, 20);
            gridLayoutGroup.spacing = new Vector2(10f, 10f);

            // Calculate cell size dynamically based on available space and number of color pickers
            int maxColumns = 6; // Maximum number of columns (circles) per row
            int maxRows = Mathf.CeilToInt((float)__instance.colors.Length / (float)maxColumns); // Maximum number of rows based on the number of colors
            float cellWidth = (480f - gridLayoutGroup.padding.horizontal - (maxColumns - 1) * gridLayoutGroup.spacing.x) / maxColumns;
            float cellHeight = (pickerHeight - gridLayoutGroup.padding.vertical - (maxRows - 1) * gridLayoutGroup.spacing.y) / maxRows;
            gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);

            for (int i = 0; i < __instance.colors.Length; i++)
            {
                Color color = __instance.colors[i];

                GameObject colorButton = new GameObject("ColorPickerButton", typeof(RectTransform));
                colorButton.SetActive(true);
                colorButton.transform.SetParent(pickerPartRT, false);

                Image buttonImage = colorButton.AddComponent<Image>();
                buttonImage.sprite = circularSprite; // Set circular sprite
                buttonImage.color = color;

                colorButton.AddComponent<Button>();

                int colorIndexCapture = i;
                colorButton.GetComponent<Button>().onClick.AddListener(() => {
                    __instance.colorIndex = colorIndexCapture;
                });
            }

            return picker;
        }













        private static Sprite GetCircularSprite()
        {
            Texture2D texture = new Texture2D(64, 64); // Adjust texture size as needed
            Color[] colors = new Color[texture.width * texture.height];
            float radius = texture.width / 2f;
            Vector2 center = new Vector2(radius, radius);
            Color fillColor = Color.clear; // Change to desired fill color
            Color outlineColor = Color.white; // Change to desired outline color
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance < radius)
                    {
                        colors[y * texture.width + x] = fillColor;
                    }
                    else
                    {
                        colors[y * texture.width + x] = outlineColor;
                    }
                }
            }
            texture.SetPixels(colors);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }



        private static void PatchSign(uGUI_SignInput __instance)
        {
            __instance.inputField.characterLimit = Mod.config.SignTextLimit;

            if (Mod.config.ColorPickerOnSigns)
            {
                var currentButton = __instance.transform.GetChild(0).GetChild(8).GetComponent<Button>();
                var height = Mod.config.ExtraColorsOnSigns ? 1200 : 210;
                AddColorPickerSystem(__instance, currentButton, "SIGN", -300,-300, height);
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
                newColors = newColors.Concat(new[] { rgb(47, 79, 79) }).ToArray();
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