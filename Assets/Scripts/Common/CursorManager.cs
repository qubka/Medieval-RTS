using UnityEngine;

public static class CursorManager
{
    private static int cursor = -1;
    
    public static void SetCursor(Texture2D texture)
    {
        var id = texture.GetInstanceID();
        if (cursor == id) 
            return;
		
        cursor = id;
        Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
    }
}