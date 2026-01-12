using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public CardLibrary cardLibrary;

    private CardInstance card1;
    private CardInstance card2;
    private void Start()
    {
        // Beispiel: erste Karte ziehen
        CardInstance myCard = cardLibrary.GetCardInstance(0);

        Debug.Log("Gezogene Karte: " + myCard.GetName());

        // Beispiel: zufällige Karte ziehen
        CardInstance randomCard = cardLibrary.GetRandomCardInstance();
        Debug.Log("Zufällige Karte: " + randomCard.GetName());

        int orangeDamage = myCard.GetTotalValueByColor(CardColors.Orange);
        Debug.Log("Oranger Schaden dieser Karte: " + orangeDamage);

// Zugriff auf einzelne Ecken
        CardCorner topLeft = myCard.topLeft;
        Debug.Log("TopLeft Farbe: " + myCard.topLeft.cornerColor + ", Wert: " + topLeft.value);


        // Zwei Karten zum testen vom Schneiden holen
        card1 = cardLibrary.GetRandomCardInstance();
        card2 = cardLibrary.GetRandomCardInstance();
        
    }

    private void Update()
    {
        #region Cutting Test
        var kb = Keyboard.current;
        if (kb == null) return;
        
        if (kb.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Infos:\n" +
                      $"Card1 Name = {card1.GetName()}\n" +
                      $"Card1 Summe Orange = {card1.GetTotalValueByColor(CardColors.Orange)}\n" +
                      $"Card1 Summe Blau = {card1.GetTotalValueByColor(CardColors.Blue)}\n" +
                      $"Card2 Name = {card2.GetName()}\n" +
                      $"Card2 Summe Orange = {card2.GetTotalValueByColor(CardColors.Orange)}\n" +
                      $"Card2 Summe Blau = {card2.GetTotalValueByColor(CardColors.Blue)}");
        }

        if (kb.hKey.wasPressedThisFrame)
        {
            // Horizontal schneiden
            CardCutter.CutHorizontal(card1, card2);

            Debug.Log("Karten horizontal geschnitten");
        }

        if (kb.vKey.wasPressedThisFrame)
        {
            // Vertikal schneiden
            CardCutter.CutVertical(card1, card2);

            Debug.Log("Karten vertikal geschnitten");
        }
        #endregion
    }
}