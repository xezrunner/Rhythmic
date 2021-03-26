using PathCreation;
using UnityEngine;

[ExecuteInEditMode]
public abstract class PathSceneTool : MonoBehaviour
{
    public bool autoUpdate = true;
    public event System.Action onDestroyed;
    public PathCreator pathCreator;
    public VertexPath path { get { return pathCreator.path; } }

    public void TriggerUpdate(bool force = false) => PathUpdated(force);

    protected virtual void OnDestroy() => onDestroyed?.Invoke();
    protected abstract void PathUpdated(bool force = false);
}