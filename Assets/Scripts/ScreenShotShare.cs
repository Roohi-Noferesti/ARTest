using System.Collections;
using UnityEngine;

public class ScreenShotShare : MonoBehaviour
{
    public Texture2D CaptureScreenshot()
    {
        int width = Screen.width;
        int height = Screen.height;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        return tex;
    }

    public void ShareScreenshot()
    {
        StartCoroutine(ShareRoutine());
    }

    private IEnumerator ShareRoutine()
    {
        yield return new WaitForEndOfFrame();

        Texture2D ss = CaptureScreenshot();
        string filePath = System.IO.Path.Combine(Application.temporaryCachePath, "shared_img.png");
        System.IO.File.WriteAllBytes(filePath, ss.EncodeToPNG());

        new NativeShare().AddFile(filePath)
                         .SetText("Check out my progress!")
                         .Share();
    }
}
