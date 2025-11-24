namespace BlazorGL.Core.Geometries;

/// <summary>
/// Generic buffer geometry that allows setting custom attributes
/// Used for creating custom geometries from raw data
/// </summary>
public class BufferGeometry : Geometry
{
    public BufferGeometry()
    {
    }

    /// <summary>
    /// Sets a named attribute with data
    /// </summary>
    public void SetAttribute(string name, float[] data, int itemSize)
    {
        switch (name.ToLower())
        {
            case "position":
                Vertices = data;
                break;
            case "normal":
                Normals = data;
                break;
            case "uv":
                UVs = data;
                break;
            case "color":
                Colors = data;
                break;
            case "skinindex":
                SkinIndices = data;
                break;
            case "skinweight":
                SkinWeights = data;
                break;
        }
    }

    /// <summary>
    /// Sets indices
    /// </summary>
    public void SetIndex(uint[] indices)
    {
        Indices = indices;
    }
}
