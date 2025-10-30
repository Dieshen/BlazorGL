namespace BlazorGL.Core;

/// <summary>
/// Container object for grouping other objects together.
/// Groups have no visual representation - they're used purely for organization
/// and applying transformations to multiple objects at once.
/// </summary>
public class Group : Object3D
{
    public Group()
    {
        Name = "Group";
    }

    public Group(string name)
    {
        Name = name;
    }
}
