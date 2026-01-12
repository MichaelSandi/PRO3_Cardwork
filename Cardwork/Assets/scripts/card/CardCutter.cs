public static class CardCutter
{
    // Horizontaler Schnitt: swap top corners
    public static void CutHorizontal(CardInstance cardA, CardInstance cardB)
    {
        // Swap TopLeft
        (cardA.topLeft, cardB.topLeft) = (cardB.topLeft, cardA.topLeft);

        // Swap TopRight
        (cardA.topRight, cardB.topRight) = (cardB.topRight, cardA.topRight);
    }

    // Vertikaler Schnitt: swap left corners
    public static void CutVertical(CardInstance cardA, CardInstance cardB)
    {
        // Swap TopLeft
        (cardA.topLeft, cardB.topLeft) = (cardB.topLeft, cardA.topLeft);

        // Swap BottomLeft
        (cardA.bottomLeft, cardB.bottomLeft) = (cardB.bottomLeft, cardA.bottomLeft);
    }
}
