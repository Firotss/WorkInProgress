using UnityEngine;
using UnityEditor;
using System.IO;

public static class ImportSoundsEditor
{
    const string RESOURCES_SOUNDS = "Assets/Resources/Sounds";

    [MenuItem("Tools/Import Sounds to Resources")]
    public static void ImportSounds()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string sourceDir = Path.Combine(projectRoot, "Sounds");
        if (!Directory.Exists(sourceDir))
        {
            sourceDir = Path.Combine(Application.dataPath, "Sounds");
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogWarning("Sounds folder not found. Create a 'Sounds' folder in the project root or in Assets/ and add Play_card.wav, Take_back_card.wav, Shuffle.wav, End_Turn.wav.");
                return;
            }
        }

        if (!Directory.Exists(RESOURCES_SOUNDS))
            Directory.CreateDirectory(RESOURCES_SOUNDS);

        string[] wavs = Directory.GetFiles(sourceDir, "*.wav");
        int copied = 0;
        foreach (string src in wavs)
        {
            string name = Path.GetFileName(src);
            if (name.StartsWith(".")) continue;
            string dest = Path.Combine(RESOURCES_SOUNDS, name);
            File.Copy(src, dest, true);
            copied++;
        }
        AssetDatabase.Refresh();
        Debug.Log($"Imported {copied} sound(s) to {RESOURCES_SOUNDS}. Assign them on GameManager > SoundManager if needed.");
    }
}
