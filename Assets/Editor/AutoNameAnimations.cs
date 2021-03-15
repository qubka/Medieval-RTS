using UnityEditor;
using UnityEngine;

public class AutoNameAnimations : AssetPostprocessor 
{
    private static readonly string[] RenameMatches = { "Take 001", "mixamo.com" };

    private void OnPostprocessModel(GameObject gameObject) 
    {
        var importer = assetImporter as ModelImporter;

        //importer.clipAnimations wil be 0 if its using the default clip animations, 
        //if there has been manual edits or edits by this script the length wont be 0
        if (importer.clipAnimations.Length == 0) {

            var clipAnimations = importer.defaultClipAnimations;
            var useSuffix = importer.defaultClipAnimations.Length > 1;
            var reimportRequired = false;

            for (var i = 0; i < clipAnimations.Length; i++) {
                for (var j = 0; j < RenameMatches.Length; j++) {
                    if (clipAnimations[i].takeName == RenameMatches[j] || clipAnimations[i].name == RenameMatches[j]) {
                        var newAnimationName = gameObject.name + (useSuffix ? "_" + j.ToString() : "");
                        clipAnimations[i].takeName = clipAnimations[i].name = newAnimationName;
                        reimportRequired = true;
                    }
                }
            }

            if (reimportRequired) {
                importer.clipAnimations = clipAnimations;
                importer.SaveAndReimport();
            }
        }
    }
}