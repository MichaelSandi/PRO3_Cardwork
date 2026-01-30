using UnityEngine;

public class CardInput : MonoBehaviour
{
    private HandInteractionController controller;
    private CardView view;

    private void Awake()
    {
        controller = FindFirstObjectByType<HandInteractionController>();
        view = GetComponentInParent<CardView>();
    }

    public void OnClicked()
    {
        //Debug.Log("CardInput.OnClicked fired on: " + gameObject.name);

        if (controller == null) Debug.LogError("No HandInteractionController found!");
        if (view == null) Debug.LogError("No CardView found in parents!");

        if (controller == null || view == null) return;

        if (view.BoundInstance == null) Debug.LogError("CardView has no BoundInstance!");
        else Debug.Log("BoundInstance: " + view.BoundInstance.GetName());

        controller.OnCardClicked(view.BoundInstance);
    }

}