using UnityEngine;

/// <summary>
/// Moves a map camera forward based on the current map depth (distance from start).
/// Assumes RunManager.Instance.mapState.currentNodeId is set to the node the player last entered/cleared.
/// </summary>
public class MapCameraProgress : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mapCamera;
    [SerializeField] private MapLayoutDefinition layout;

    [Header("Movement")]
    [Tooltip("Direction in world space the camera should move per depth step (will be normalized).")]
    [SerializeField] private Vector3 moveDirection = new Vector3(0f, 0f, 1f);

    [Tooltip("World units per map depth step.")]
    [SerializeField] private float unitsPerStep = 10f;

    [Header("Smoothing")]
    [SerializeField] private bool smooth = true;
    [SerializeField] private float smoothSpeed = 8f;

    private Vector3 startPos;

    private void Awake()
    {
        if (mapCamera == null) mapCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        startPos = mapCamera != null ? mapCamera.transform.position : transform.position;
        moveDirection = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector3.forward;

        // initial snap
        UpdateCameraPosition(immediate: true);
    }

    private void LateUpdate()
    {
        // Only move while in map mode (optional)
        if (GameFlowController.Instance != null && !GameFlowController.Instance.IsInMapMode)
            return;

        UpdateCameraPosition(immediate: !smooth);
    }

    public void SnapToProgress()
    {
        UpdateCameraPosition(immediate: true);
    }

    private void UpdateCameraPosition(bool immediate)
    {
        int depth = GetCurrentDepth();
        Vector3 targetPos = startPos + moveDirection * unitsPerStep * depth;

        if (mapCamera == null)
        {
            if (immediate) transform.position = targetPos;
            else transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
            return;
        }

        Transform t = mapCamera.transform;
        if (immediate) t.position = targetPos;
        else t.position = Vector3.Lerp(t.position, targetPos, Time.deltaTime * smoothSpeed);
    }

    private int GetCurrentDepth()
    {
        var run = RunManager.Instance;
        if (run == null || run.mapState == null || layout == null)
            return 0;

        // If you store current node id on run:
        string currentId = run.currentNodeId;
        if (string.IsNullOrEmpty(currentId))
            return 0;

        // Compute depth as BFS distance from start (MVP; you can cache later)
        string startId = layout.GetStartNodeId();
        if (string.IsNullOrEmpty(startId) || startId == currentId)
            return 0;

        // BFS
        var q = new System.Collections.Generic.Queue<string>();
        var dist = new System.Collections.Generic.Dictionary<string, int>();

        dist[startId] = 0;
        q.Enqueue(startId);

        while (q.Count > 0)
        {
            string cur = q.Dequeue();
            int d = dist[cur];

            var outs = layout.GetOutgoing(cur);
            for (int i = 0; i < outs.Count; i++)
            {
                string nxt = outs[i];
                if (dist.ContainsKey(nxt)) continue;

                dist[nxt] = d + 1;
                q.Enqueue(nxt);
            }
        }

        return dist.TryGetValue(currentId, out int res) ? res : 0;
    }
}
