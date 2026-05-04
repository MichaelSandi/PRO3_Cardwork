using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverClip;

    private bool hovered;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hovered) return;
        hovered = true;

        audioSource.PlayOneShot(hoverClip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }

}