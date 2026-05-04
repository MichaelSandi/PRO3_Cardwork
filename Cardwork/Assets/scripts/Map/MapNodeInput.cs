using UnityEngine;

public class MapNodeInput : MonoBehaviour
{
    private MapController controller;
    private MapNodeView view;

    public void Bind(MapController controller, MapNodeView view)
    {
        this.controller = controller;
        this.view = view;
    }

    public void OnClicked()
    {
        //Debug.Log("Clicked in MapNodeInput Triggered");
        //Debug.Log($"Controller: {controller}, View.NodeId: {view.NodeId}");
        if (controller == null || view == null) return;
        //Debug.Log("Clicked in MapNodeInput Triggered after null check");
        controller.OnNodeClicked(view.NodeId);
    }

    public void SetHovered(bool hovered)
    {
        // Optional: Wenn du im View eine HoverOutline hast, hier togglen
        // view.SetHovered(hovered);
    }
}