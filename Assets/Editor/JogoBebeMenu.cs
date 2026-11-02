#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class JogoBebeMenu
{
    const string OutputFolder = "Assets/Resources/Models/Imported";

    // Cada modelo pode estar em Assets/Models/... (jeito antigo) ou direto em Assets/Resources/Models/...
    static readonly string[] MapPaths =
    {
        "Assets/Models/MAP/scene.gltf",
        "Assets/Resources/Models/MAP/scene.gltf"
    };
    static readonly string[] WomanPaths =
    {
        "Assets/Models/WOMAN/scene.gltf",
        "Assets/Resources/Models/WOMAN/scene.gltf"
    };
    static readonly string[] RebecaPaths =
    {
        "Assets/Models/REBECA/female_character/scene.gltf",
        "Assets/Resources/Models/REBECA/female_character/scene.gltf",
        "Assets/Resources/Models/REBECA/scene.gltf"
    };

    [InitializeOnLoadMethod]
    static void ScheduleModelImportOnEditorLoad()
    {
        EditorApplication.delayCall += () => ImportAll(false);
    }

    [MenuItem("Jogo Bebe/Importar modelos glTF")]
    public static void ImportGltfModels()
    {
        ImportAll(true);
    }

    /// <summary>force = recria os prefabs mesmo que ja existam; sem force so cria os que faltam.</summary>
    static void ImportAll(bool force)
    {
        Directory.CreateDirectory(OutputFolder);

        bool changed = false;
        var done = new List<string>();
        changed |= Import("MAP", MapPaths, force, done);
        changed |= Import("WOMAN", WomanPaths, force, done);
        changed |= Import("REBECA", RebecaPaths, force, done);

        if (changed)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Modelos preparados em Resources/Models/Imported: " + string.Join(", ", done.ToArray()));
        }
        else if (force)
        {
            Debug.LogWarning("Nenhum modelo glTF encontrado/importado. Confira se o Unity glTFast terminou de importar " +
                             "e se os arquivos estao em Assets/Models/ ou Assets/Resources/Models/.");
        }
    }

    static bool Import(string key, string[] candidates, bool force, List<string> done)
    {
        string source = null;
        foreach (var c in candidates)
            if (AssetDatabase.LoadAssetAtPath<GameObject>(c) != null) { source = c; break; }
        if (source == null) return false; // modelo ausente (ex.: REBECA) nao impede os outros

        string prefabPath = OutputFolder + "/" + key + ".prefab";
        if (!force && File.Exists(prefabPath)) return false;

        bool ok = SaveImportedModel(source, prefabPath);
        if (ok) done.Add(key);
        return ok;
    }

    static bool SaveImportedModel(string sourcePath, string prefabPath)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
        if (model == null)
        {
            Debug.LogError("Nao foi possivel importar " + sourcePath + ". Confira se Unity glTFast terminou a importacao.");
            return false;
        }

        var prefabRoot = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
        var modelInstance = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (modelInstance == null)
        {
            Object.DestroyImmediate(prefabRoot);
            Debug.LogError("Nao foi possivel criar uma instancia de " + sourcePath + ".");
            return false;
        }

        modelInstance.transform.SetParent(prefabRoot.transform, false);
        // O glTF do mapa tem dois nos-raiz com giros de -90 e +90 graus em X que se anulam:
        // o resultado ja e a cidade no plano XZ, com Y para cima. Nao precisa girar nada.
        modelInstance.transform.localRotation = Quaternion.identity;

        var prefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        Object.DestroyImmediate(prefabRoot);
        return prefab != null;
    }

    [MenuItem("Jogo Bebe/Criar cena Rua")]
    static void CreateStreetScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("GameBootstrap");
        go.AddComponent<JogoBebe.GameBootstrap>();

        Directory.CreateDirectory("Assets/Scenes");
        const string path = "Assets/Scenes/Rua.unity";
        EditorSceneManager.SaveScene(scene, path);

        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!list.Exists(s => s.path == path)) list.Insert(0, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();

        Debug.Log("Cena Rua criada em " + path + " e adicionada ao Build Settings. Aperte Play.");
    }
}

public class JogoBebeGltfPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var path in importedAssets)
        {
            bool isModel = path.EndsWith("/scene.gltf") &&
                           (path.Contains("/MAP/") || path.Contains("/WOMAN/") || path.Contains("/REBECA/"));
            if (!isModel) continue;

            EditorApplication.delayCall += () => JogoBebeMenu.ImportGltfModels();
            return;
        }
    }
}
#endif
