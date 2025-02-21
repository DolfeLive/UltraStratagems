
namespace UltraStratagems;
public class AssetStuff : MonoBehaviour
{

    public static void AssetBundleBs()
    {
        Stream AssetBundleStream = GetEmbeddedAsset("stratagems");

        if (AssetBundleStream == null)
        {
            Debug.LogError("AssetBundleStream is null!");
            return;
        }

        instance.assets = AssetBundle.LoadFromStream(AssetBundleStream);

        if (instance.assets == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            return;
        }

        foreach (var assetName in instance.assets.GetAllAssetNames())
        {
            print($"Loaded asset: {assetName}");
            instance.loadedAssets[assetName] = instance.assets.LoadAsset(assetName);
        }
    }
    
    public static Stream GetEmbeddedAsset(string assetName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        print($"Files: {string.Join(", ", assembly.GetManifestResourceNames())}");
        var resourceName = $"UltraStratagems.Resources.{assetName}";

        Stream stream = assembly.GetManifestResourceStream(resourceName);
        return stream;
    }

    public static Texture2D StreamToTex(Stream stream)
    {
        Texture2D tex = new Texture2D(2, 2);

        Color[] colors = new Color[4];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.red;
        }
        tex.SetPixels(colors);
        tex.Apply();

        byte[] imageData = new byte[stream.Length];
        stream.Read(imageData, 0, (int)stream.Length);
        tex.LoadImage(imageData);

        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();

        return tex;

        //byte[] data = null!;
        //using (StreamReader reader = new StreamReader(stream))
        //{
        //    using (var memstream = new MemoryStream())
        //    {
        //        var buffer = new byte[512];
        //          var bytesRead = default(int);
        //        while ((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
        //            memstream.Write(buffer, 0, bytesRead);
        //        data = memstream.ToArray();
        //    }
        //}

        //Texture2D tex = new Texture2D(2, 2);
        
        //tex.LoadRawTextureData(data);
        //return tex;
    }


    public static T LoadAsset<T>(string name) where T : Object
    {
        print($"Loading asset: 'assets/__stratagems/{name}'");
        T asset = instance.loadedAssets[$"assets/__stratagems/{name}"] as T;
        return asset;
    }
    
}
