using PathCreation;

// This static class acts as a global path getter, abstracting away release getter optimizations.
// TODO: explain better / optimize further!
public class XZ_Path
{
    public XZ_Path(PathCreator pcreator, bool warn_on_null = true)
    {
        if (!pcreator && warn_on_null)
            Logger.LogW("Given pathcreator was null!".TM(this));
        pathcreator = WorldSystem.GetAPathCreator();
    }
    public XZ_Path(WorldSystem world)
    {
        if (!world && Logger.LogW("Given WorldSystem was null!".TM(this))) return;
        pathcreator = world.pathcreator;
    }

    public PathCreator pathcreator;

#if UNITY_EDITOR
    public VertexPath path { get { return pathcreator.path; } }
#else
    public VertexPath path;
#endif
}