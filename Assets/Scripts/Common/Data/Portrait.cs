using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Portrait : MonoBehaviour
{
    public Texture2D pack;
    public Transform figure;
    public Transform background;
    
    [ReadOnly] public readonly List<Image> characterParts = new List<Image>();

    private string characterPartType;
    private string imageType;
    private int firstIndex;
    private int currentIndex;
    private int lastIndex;
    private Sprite[] images;
    private const string devisionSymbol = "-";
    
    private readonly List<Sprite> sprites = new List<Sprite>();

    public void ReadImageFile()
    {
        images = Resources.LoadAll<Sprite>("Characters/Generator/" + pack.name);
        
        characterParts.Clear();
        characterParts.Add(background.GetComponent<Image>());
        for (var i = 0; i < figure.childCount; i++) {
            characterParts.Add(figure.GetChild(i).GetComponent<Image>()); // Add the rest parts of the Character
        }
    }
    
    public void NextImage(Image image)
    {
        if (image.sprite != null) {
            // Get type of current Chatacter Part
            characterPartType = image.sprite.name;
            characterPartType = characterPartType.Substring(0, characterPartType.IndexOf(devisionSymbol, StringComparison.Ordinal) + 1);

            // Get index of First Image Of this Type of Character Part
            for (var i = 0; i < images.Length; i++) {
                //Get type of current Image
                imageType = images[i].name;
                imageType = imageType.Substring(0, imageType.IndexOf(devisionSymbol, StringComparison.Ordinal) + 1);
                // Compare CharacterType and Image Type
                if (characterPartType == imageType) {
                    firstIndex = i;
                    break;
                }
            }
            
            for (var i = 0; i < images.Length; i++) {
                // Get index of the Image , which is sitting on the CharacterPart
                if (image.sprite == images[i]) {
                    currentIndex = i;
                    break;
                }
            }

            // Switch to Next Image
            if (currentIndex < images.Length - 1) {
                imageType = images[currentIndex + 1].name;
                imageType = imageType.Substring(0, imageType.IndexOf(devisionSymbol, StringComparison.Ordinal) + 1);
                image.sprite = characterPartType == imageType ? images[currentIndex + 1] : images[firstIndex];
            } else {
                image.sprite = images[firstIndex];
            }
        } else {
            Debug.Log("Please, attach initial Image to the " + image.name + " of the " + gameObject.name);
        }
    }
    
    public void PreviousImage(Image image)
    {
        if (image.sprite != null) {
            // Get type of current Chatacter Part
            characterPartType = image.sprite.name;
            characterPartType = characterPartType.Substring(0, characterPartType.IndexOf(devisionSymbol, StringComparison.Ordinal) + 1);

            // Get index of Last Image Of this Type of Character Part
            for (var i = 0; i < images.Length; i++) {
                // Get type of current Image
                imageType = images[i].name;
                imageType = imageType.Substring(0, imageType.IndexOf(devisionSymbol, StringComparison.Ordinal) + 1);
                // Compare CharacterType and Image Type
                if (characterPartType == imageType) {
                    lastIndex = i;
                }
            }
            
            for (var i = 0; i < images.Length; i++) {
                // Get index of the Image , which is sitting on the CharacterPart
                if (image.sprite == images[i]) {
                    currentIndex = i;
                    break;
                }
            }

            // Switch to Previous Image
            if (currentIndex > 0) {
                imageType = images[currentIndex - 1].name;
                imageType = imageType.Substring(0, imageType.IndexOf(devisionSymbol, StringComparison.Ordinal) + 1);
                image.sprite = characterPartType == imageType ? images[currentIndex - 1] : images[lastIndex];
            } else {
                image.sprite = images[lastIndex];
            }
        } else {
            Debug.Log("Please, attach initial Image to the " + image.name + " of the " + gameObject.name);
        }
    }
    
    public void RandomImage()
    {
        foreach (var image in characterParts) {
            if (image.sprite != null) {
                sprites.Clear();
                
                // Get type of current Chatacter Part
                characterPartType = image.sprite.name;
                characterPartType = characterPartType.Substring(0, characterPartType.IndexOf(devisionSymbol, StringComparison.Ordinal) + 1);

                //Create List of suitable Images
                foreach (var sprite in images) {
                    // Get type of current Image
                    imageType = sprite.name;
                    imageType = imageType.Substring(0, imageType.IndexOf(devisionSymbol, StringComparison.Ordinal) + 1);
                    // Pick suitable images
                    if (characterPartType == imageType) {
                        sprites.Add(sprite);
                    }
                }

                // Pick Random Image
                image.sprite = sprites[Random.Range(0, sprites.Count)];
            } else {
                Debug.Log("Please, attach initial Image to the " + image.name + " of the " + gameObject.name);
            }
        }
    }
}