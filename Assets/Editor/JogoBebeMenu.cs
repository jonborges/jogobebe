#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class JogoBebeMenu
{
    [InitializeOnLoadMethod]
    static void ScheduleModelImportOnEditorLoad()
    {
        EditorApplication.delayCall += ImportModelsIfReady;
    }
    static void ImportModelsIfReady()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/MAP/scene.gltf") != null &&
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/WOMAN/scene.gltf") != null &&
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/REBECA/female_character/scene.gltf") != null)
            ImportGltfModels();
    }

    [MenuItem("Jogo Bebe/Importar modelos glTF")]
    public static void ImportGltfModels()
    {
        const string outputFolder = "Assets/Resources/Models/Imported";
        Directory.CreateDirectory(outputFolder);

        bool importedAny = false;
        importedAny |= SaveImportedModel("Assets/Models/MAP/scene.gltf", outputFolder + "/MAP.prefab");
        importedAny |= SaveImportedModel("Assets/Models/WOMAN/scene.gltf", outputFolder + "/WOMAN.prefab");
        importedAny |= SaveImportedModel("Assets/Models/REBECA/female_character/scene.gltf", outputFolder + "/REBECA.prefab");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (importedAny)
            Debug.Log("Modelos MAP, WOMAN e REBECA preparados em Resources/Models/Imported.");
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
        if (sourcePath.Contains("/MAP/"))
        {
            // O glTF do mapa traz um giro de -90 graus no objeto-raiz; o terreno
            // e os carros do jogo usam o plano XZ, entao esse giro deixava a cidade de lado.
            modelInstance.transform.localRotation = Quaternion.identity;
        }

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
            if (path == "Assets/Models/MAP/scene.gltf" || path == "Assets/Models/WOMAN/scene.gltf" ||
                path == "Assets/Models/REBECA/female_character/scene.gltf")
            {
                EditorApplication.delayCall += JogoBebeMenu.ImportGltfModels;
                return;
            }
        }
    }
}
#endif
