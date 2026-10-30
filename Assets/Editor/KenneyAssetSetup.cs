using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace KenneyPlatformer.Editor
{
    /// <summary>
    /// Imports the supplied Kenney pixel-art packs with point filtering, slices the
    /// XML-described environment atlases, and builds reusable Tile assets,
    /// character animation clips, Animator Controllers, and character prefabs.
    /// Run from Tools > Kenney > Configure Imported Assets.
    /// </summary>
    public static class KenneyAssetSetup
    {
        private const string Root = "Assets/Art/Kenney";
        private const string EnvironmentRoot = Root + "/PlatformerArtExtended";
        private const string CharactersRoot = Root + "/PlatformerCharacters/PNG";
        private const string GeneratedRoot = Root + "/Generated";
        private const float EnvironmentPixelsPerUnit = 70f;
        private const float CharacterPixelsPerUnit = 18f;
        private const float AnimationFrameRate = 8f;

        [MenuItem("Tools/Kenney/Configure Imported Assets", priority = 10)]
        public static void ConfigureAll()
        {
            if (!AssetDatabase.IsValidFolder(Root))
            {
                Debug.LogError("Kenney assets were not found at " + Root);
                return;
            }

            ConfigureTextureImporters();
            SliceEnvironmentAtlases();
            AssetDatabase.Refresh();

            CreateEnvironmentTiles();
            CreateCharacterAnimationsAndPrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Kenney setup complete. Pixel import settings, atlas slices, tile assets, animation clips, Animator Controllers, and character prefabs have been configured.");
        }

        private static void ConfigureTextureImporters()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Root });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.alphaIsTransparency = true;
                importer.isReadable = false;
                importer.maxTextureSize = 4096;
                importer.spritePixelsPerUnit = path.StartsWith(CharactersRoot, StringComparison.Ordinal)
                    ? CharacterPixelsPerUnit
                    : EnvironmentPixelsPerUnit;

                if (path.EndsWith("/Spritesheet/sheet.png", StringComparison.OrdinalIgnoreCase))
                    importer.spriteImportMode = SpriteImportMode.Multiple;

                importer.SaveAndReimport();
            }
        }

        private static void SliceEnvironmentAtlases()
        {
            string[] xmlGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { EnvironmentRoot });
            foreach (string guid in xmlGuids)
            {
                string xmlPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!xmlPath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) continue;

                string pngPath = Path.ChangeExtension(xmlPath, ".png").Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
                if (importer == null) continue;

                try
                {
                    XDocument document = XDocument.Load(xmlPath);
                    var sprites = new List<SpriteMetaData>();
                    foreach (XElement element in document.Descendants("SubTexture"))
                    {
                        string name = (string)element.Attribute("name");
                        float x = (float?)element.Attribute("x") ?? 0;
                        float yTop = (float?)element.Attribute("y") ?? 0;
                        float width = (float?)element.Attribute("width") ?? 0;
                        float height = (float?)element.Attribute("height") ?? 0;
                        if (string.IsNullOrEmpty(name) || width <= 0 || height <= 0) continue;

                        // Kenney XML atlas coordinates start at the top-left;
                        // Unity sprite rects start at the bottom-left.
                        int textureHeight = GetTextureHeight(pngPath);
                        var meta = new SpriteMetaData
                        {
                            name = Path.GetFileNameWithoutExtension(name),
                            rect = new Rect(x, textureHeight - yTop - height, width, height),
                            alignment = (int)SpriteAlignment.Center,
                            pivot = new Vector2(0.5f, 0.5f),
                            border = Vector4.zero
                        };
                        sprites.Add(meta);
                    }

                    if (sprites.Count == 0) continue;
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;
                    importer.spritePixelsPerUnit = EnvironmentPixelsPerUnit;
#pragma warning disable 0618
                    importer.spritesheet = sprites.ToArray();
#pragma warning restore 0618
                    importer.SaveAndReimport();
                }
                catch (Exception exception)
                {
                    Debug.LogError("Could not slice Kenney atlas " + xmlPath + ": " + exception.Message);
                }
            }
        }

        private static int GetTextureHeight(string assetPath)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture != null) return texture.height;

            // Read dimensions from the imported texture without making it readable.
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                var settings = importer.GetDefaultPlatformTextureSettings();
                settings.maxTextureSize = 4096;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (texture != null) return texture.height;
            }

            throw new InvalidOperationException("Unable to read atlas dimensions for " + assetPath);
        }

        private static void CreateEnvironmentTiles()
        {
            string outputRoot = GeneratedRoot + "/Tiles";
            EnsureFolder(GeneratedRoot);
            EnsureFolder(outputRoot);

            string[] themeDirectories = Directory.GetDirectories(EnvironmentRoot, "PNG *", SearchOption.TopDirectoryOnly);
            foreach (string absoluteThemePath in themeDirectories)
            {
                string themePath = absoluteThemePath.Replace('\\', '/');
                string themeName = Path.GetFileName(themePath).Substring(4);
                string outputTheme = outputRoot + "/" + SafeName(themeName);
                EnsureFolder(outputTheme);

                string[] pngPaths = Directory.GetFiles(themePath, "*.png", SearchOption.AllDirectories)
                    .Select(p => p.Replace('\\', '/'))
                    .Where(p => !p.Contains("/Spritesheet/") &&
                                !Path.GetFileName(p).Equals("preview.png", StringComparison.OrdinalIgnoreCase) &&
                                !Path.GetFileName(p).Equals("sample.png", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                foreach (string pngPath in pngPaths)
                {
                    string assetPath = ToAssetPath(pngPath);
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite == null)
                    {
                        sprite = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().FirstOrDefault();
                    }
                    if (sprite == null) continue;

                    string tileName = SafeName(Path.GetFileNameWithoutExtension(assetPath));
                    string tilePath = outputTheme + "/" + tileName + ".asset";
                    Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                    if (tile == null)
                    {
                        tile = ScriptableObject.CreateInstance<Tile>();
                        AssetDatabase.CreateAsset(tile, tilePath);
                    }
                    tile.name = tileName;
                    tile.sprite = sprite;
                    tile.colliderType = Tile.ColliderType.Sprite;
                    EditorUtility.SetDirty(tile);
                }

                // Also expose each sliced atlas sprite as a Tile asset.
                string[] atlasPngs = Directory.GetFiles(themePath, "sheet.png", SearchOption.AllDirectories);
                foreach (string atlasPng in atlasPngs)
                {
                    string atlasAssetPath = ToAssetPath(atlasPng);
                    foreach (Sprite sprite in AssetDatabase.LoadAllAssetsAtPath(atlasAssetPath).OfType<Sprite>())
                    {
                        string tilePath = outputTheme + "/" + SafeName(sprite.name) + ".asset";
                        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                        if (tile == null)
                        {
                            tile = ScriptableObject.CreateInstance<Tile>();
                            AssetDatabase.CreateAsset(tile, tilePath);
                        }
                        tile.name = SafeName(sprite.name);
                        tile.sprite = sprite;
                        tile.colliderType = Tile.ColliderType.Sprite;
                        EditorUtility.SetDirty(tile);
                    }
                }
            }
        }

        private static void CreateCharacterAnimationsAndPrefabs()
        {
            EnsureFolder(GeneratedRoot);
            EnsureFolder(GeneratedRoot + "/Animations");
            EnsureFolder(GeneratedRoot + "/Controllers");
            EnsureFolder(GeneratedRoot + "/Prefabs");

            if (!Directory.Exists(CharactersRoot)) return;
            string[] characterDirectories = Directory.GetDirectories(CharactersRoot, "*", SearchOption.TopDirectoryOnly);

            foreach (string absoluteCharacterPath in characterDirectories)
            {
                string characterPath = absoluteCharacterPath.Replace('\\', '/');
                string characterName = Path.GetFileName(characterPath);
                string posesPath = characterPath + "/Poses";
                if (!Directory.Exists(posesPath)) continue;

                string outputAnimationFolder = GeneratedRoot + "/Animations/" + characterName;
                EnsureFolder(outputAnimationFolder);

                var poseFiles = Directory.GetFiles(posesPath, "*.png", SearchOption.TopDirectoryOnly)
                    .Select(p => p.Replace('\\', '/'))
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var groupedFrames = new Dictionary<string, List<Sprite>>(StringComparer.OrdinalIgnoreCase);
                foreach (string poseFile in poseFiles)
                {
                    string poseAssetPath = ToAssetPath(poseFile);
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(poseAssetPath);
                    if (sprite == null) continue;

                    string stem = Path.GetFileNameWithoutExtension(poseFile);
                    string prefix = characterName.ToLowerInvariant() + "_";
                    string suffix = stem.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                        ? stem.Substring(prefix.Length)
                        : stem;

                    string groupName = StripTrailingDigits(suffix);
                    if (!groupedFrames.TryGetValue(groupName, out List<Sprite> frames))
                    {
                        frames = new List<Sprite>();
                        groupedFrames[groupName] = frames;
                    }
                    frames.Add(sprite);
                }

                foreach (var pair in groupedFrames)
                {
                    string clipPath = outputAnimationFolder + "/" + characterName + "_" + SafeName(pair.Key) + ".anim";
                    CreateOrUpdateClip(clipPath, pair.Value, pair.Key);
                }

                string controllerPath = GeneratedRoot + "/Controllers/" + characterName + ".controller";
                AnimatorController controller = CreateController(characterName, groupedFrames, controllerPath);

                Sprite idleSprite = FindFirst(groupedFrames, "idle") ?? groupedFrames.Values.FirstOrDefault()?.FirstOrDefault();
                if (idleSprite != null)
                    CreateCharacterPrefab(characterName, idleSprite, controller);
            }
        }

        private static AnimatorController CreateController(
            string characterName,
            Dictionary<string, List<Sprite>> frames,
            string controllerPath)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            else
            {
                // Rebuild generated controllers deterministically when this tool is run again.
                foreach (AnimatorControllerLayer layer in controller.layers)
                {
                    var stateMachine = layer.stateMachine;
                    foreach (ChildAnimatorState state in stateMachine.states)
                        stateMachine.RemoveState(state.state);
                }
            }

            AddParameter(controller, "MoveSpeed", AnimatorControllerParameterType.Float);
            AddParameter(controller, "Grounded", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "VerticalVelocity", AnimatorControllerParameterType.Float);
            foreach (string trigger in new[] { "Hurt", "Climb", "Swim", "Action", "Cheer", "Duck", "Skid", "Slide" })
                AddParameter(controller, trigger, AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            var states = new Dictionary<string, AnimatorState>(StringComparer.OrdinalIgnoreCase);
            foreach (string key in frames.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
            {
                string clipPath = GeneratedRoot + "/Animations/" + characterName + "/" +
                                  characterName + "_" + SafeName(key) + ".anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null) continue;
                string stateName = ToStateName(key);
                AnimatorState state = machine.AddState(stateName);
                state.motion = clip;
                states[key] = state;
            }

            AnimatorState idle = GetState(states, "idle") ?? states.Values.FirstOrDefault();
            if (idle != null) machine.defaultState = idle;
            AnimatorState walk = GetState(states, "walk");
            AnimatorState jump = GetState(states, "jump");
            AnimatorState fall = GetState(states, "fall");
            AnimatorState hurt = GetState(states, "hurt");

            if (idle != null && walk != null)
            {
                AddTransition(idle, walk, "MoveSpeed", AnimatorConditionMode.Greater, 0.1f);
                AddTransition(walk, idle, "MoveSpeed", AnimatorConditionMode.Less, 0.1f);
            }
            if (jump != null && fall != null)
                AddTransition(jump, fall, "VerticalVelocity", AnimatorConditionMode.Less, 0f);
            if (fall != null && idle != null)
                AddTransition(fall, idle, "Grounded", AnimatorConditionMode.If, 0f);
            if (fall != null && walk != null)
                AddTransition(fall, walk, "MoveSpeed", AnimatorConditionMode.Greater, 0.1f, "Grounded");
            if (jump != null)
            {
                AnimatorStateTransition toJump = machine.AddAnyStateTransition(jump);
                toJump.hasExitTime = false;
                toJump.duration = 0.05f;
                toJump.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
                toJump.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VerticalVelocity");
            }
            if (hurt != null)
            {
                AnimatorStateTransition toHurt = machine.AddAnyStateTransition(hurt);
                toHurt.hasExitTime = false;
                toHurt.duration = 0.03f;
                toHurt.AddCondition(AnimatorConditionMode.If, 0, "Hurt");
                AddExitToIdle(hurt, idle);
            }

            AddTriggeredState(machine, states, "climb", "Climb", idle);
            AddTriggeredState(machine, states, "swim", "Swim", idle);
            AddTriggeredState(machine, states, "action", "Action", idle);
            AddTriggeredState(machine, states, "cheer", "Cheer", idle);
            AddTriggeredState(machine, states, "duck", "Duck", idle);
            AddTriggeredState(machine, states, "skid", "Skid", idle);
            AddTriggeredState(machine, states, "slide", "Slide", idle);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void AddTriggeredState(AnimatorStateMachine machine,
            Dictionary<string, AnimatorState> states, string stateKey, string trigger, AnimatorState idle)
        {
            AnimatorState state = GetState(states, stateKey);
            if (state == null) return;
            AnimatorStateTransition transition = machine.AddAnyStateTransition(state);
            transition.hasExitTime = false;
            transition.duration = 0.04f;
            transition.AddCondition(AnimatorConditionMode.If, 0, trigger);
            AddExitToIdle(state, idle);
        }

        private static void AddExitToIdle(AnimatorState state, AnimatorState idle)
        {
            if (idle == null || state == null || state == idle) return;
            AnimatorStateTransition transition = state.AddTransition(idle);
            transition.hasExitTime = true;
            transition.exitTime = 0.9f;
            transition.duration = 0.05f;
        }

        private static void AddTransition(AnimatorState from, AnimatorState to,
            string parameter, AnimatorConditionMode mode, float threshold, string secondParameter = null)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.AddCondition(mode, threshold, parameter);
            if (!string.IsNullOrEmpty(secondParameter))
                transition.AddCondition(AnimatorConditionMode.If, 0, secondParameter);
        }

        private static void AddParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            if (!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, type);
        }

        private static void CreateOrUpdateClip(string clipPath, List<Sprite> sprites, string groupName)
        {
            if (sprites == null || sprites.Count == 0) return;
            sprites = sprites.Where(s => s != null).ToList();
            if (sprites.Count == 0) return;

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip { frameRate = AnimationFrameRate };
                AssetDatabase.CreateAsset(clip, clipPath);
            }
            else
            {
                AnimationUtility.SetObjectReferenceCurve(clip,
                    EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), null);
            }

            clip.frameRate = AnimationFrameRate;
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i / AnimationFrameRate,
                    value = sprites[i]
                };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = sprites.Count > 1 || groupName.Equals("idle", StringComparison.OrdinalIgnoreCase);
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }

        private static void CreateCharacterPrefab(string characterName, Sprite idleSprite, AnimatorController controller)
        {
            string prefabFolder = GeneratedRoot + "/Prefabs";
            EnsureFolder(prefabFolder);

            var root = new GameObject(characterName);
            root.tag = "Untagged";
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = idleSprite;
            renderer.sortingOrder = 5;

            Animator animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.gravityScale = 3f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CapsuleCollider2D collider = root.AddComponent<CapsuleCollider2D>();
            Vector2 spriteSize = idleSprite.bounds.size;
            collider.size = new Vector2(Mathf.Max(0.35f, spriteSize.x * 0.68f),
                                        Mathf.Max(0.5f, spriteSize.y * 0.88f));
            collider.direction = CapsuleDirection2D.Vertical;

            string prefabPath = prefabFolder + "/" + characterName + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static Sprite FindFirst(Dictionary<string, List<Sprite>> frames, string key)
        {
            return frames.TryGetValue(key, out List<Sprite> value) ? value.FirstOrDefault() : null;
        }

        private static AnimatorState GetState(Dictionary<string, AnimatorState> states, string key)
        {
            return states.TryGetValue(key, out AnimatorState state) ? state : null;
        }

        private static string StripTrailingDigits(string value)
        {
            int end = value.Length;
            while (end > 0 && char.IsDigit(value[end - 1])) end--;
            return value.Substring(0, end);
        }

        private static string ToStateName(string key)
        {
            if (string.IsNullOrEmpty(key)) return "Default";
            return char.ToUpperInvariant(key[0]) + key.Substring(1);
        }

        private static string SafeName(string value)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');
            return value.Replace(' ', '_');
        }

        private static string ToAssetPath(string absoluteOrProjectPath)
        {
            string normalized = absoluteOrProjectPath.Replace('\\', '/');
            int index = normalized.IndexOf("Assets/", StringComparison.Ordinal);
            return index >= 0 ? normalized.Substring(index) : normalized;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }

    /// <summary>Applies pixel-art defaults whenever a Kenney texture is imported.</summary>
    public sealed class KenneyPixelTexturePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Art/Kenney/", StringComparison.Ordinal)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;
            importer.spritePixelsPerUnit = assetPath.Contains("/PlatformerCharacters/")
                ? 18f : 70f;
            if (assetPath.EndsWith("/Spritesheet/sheet.png", StringComparison.OrdinalIgnoreCase))
                importer.spriteImportMode = SpriteImportMode.Multiple;
        }
    }
}
