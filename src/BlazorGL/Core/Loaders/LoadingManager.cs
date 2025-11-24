namespace BlazorGL.Core.Loaders;

/// <summary>
/// Manages and tracks the loading progress of multiple assets
/// Provides callbacks for progress, completion, and error events
/// </summary>
public class LoadingManager
{
    private int _itemsTotal = 0;
    private int _itemsLoaded = 0;

    /// <summary>
    /// Called when an item starts loading
    /// Parameters: url, itemsLoaded, itemsTotal
    /// </summary>
    public Action<string, int, int>? OnStart { get; set; }

    /// <summary>
    /// Called when an item finishes loading
    /// Parameters: url, itemsLoaded, itemsTotal
    /// </summary>
    public Action<string, int, int>? OnLoad { get; set; }

    /// <summary>
    /// Called during loading progress
    /// Parameters: url, itemsLoaded, itemsTotal
    /// </summary>
    public Action<string, int, int>? OnProgress { get; set; }

    /// <summary>
    /// Called when an item fails to load
    /// Parameters: url
    /// </summary>
    public Action<string>? OnError { get; set; }

    /// <summary>
    /// Gets the number of items currently loading
    /// </summary>
    public int ItemsLoaded => _itemsLoaded;

    /// <summary>
    /// Gets the total number of items to load
    /// </summary>
    public int ItemsTotal => _itemsTotal;

    /// <summary>
    /// Gets whether all items have finished loading
    /// </summary>
    public bool IsLoading => _itemsLoaded < _itemsTotal;

    public LoadingManager(
        Action<string, int, int>? onLoad = null,
        Action<string, int, int>? onProgress = null,
        Action<string>? onError = null)
    {
        OnLoad = onLoad;
        OnProgress = onProgress;
        OnError = onError;
    }

    /// <summary>
    /// Notifies the manager that an item has started loading
    /// </summary>
    public void ItemStart(string url)
    {
        _itemsTotal++;
        OnStart?.Invoke(url, _itemsLoaded, _itemsTotal);
    }

    /// <summary>
    /// Notifies the manager that an item finished loading successfully
    /// </summary>
    public void ItemEnd(string url)
    {
        _itemsLoaded++;
        OnProgress?.Invoke(url, _itemsLoaded, _itemsTotal);

        if (_itemsLoaded == _itemsTotal)
        {
            OnLoad?.Invoke(url, _itemsLoaded, _itemsTotal);
        }
    }

    /// <summary>
    /// Notifies the manager that an item failed to load
    /// </summary>
    public void ItemError(string url)
    {
        OnError?.Invoke(url);
    }

    /// <summary>
    /// Resolves a URL (can be overridden for custom path handling)
    /// </summary>
    public virtual string ResolveURL(string url)
    {
        return url;
    }

    /// <summary>
    /// Sets URL modifier callback for custom URL transformation
    /// </summary>
    public Func<string, string>? URLModifier { get; set; }

    /// <summary>
    /// Adds a handler for modifying URLs before loading
    /// </summary>
    public void SetURLModifier(Func<string, string> callback)
    {
        URLModifier = callback;
    }
}
