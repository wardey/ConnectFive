using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Piece : MonoBehaviour {

    const int numColors = 7;
    public enum PieceColor
    {
        Red,
        Blue,
        Cyan,
        Yellow,
        Green,
        Magenta,
        Black
    }

    public Image img;
    public PieceColor color;

    private void Start()
    {
        img = GetComponent<Image>();
        SetColor();
    }

    public void Init(PieceColor color)
    {
        this.color = color;
        SetColor();
    }

    public void HighLight()
    {

    }

    public void SetSelfTransparent()
    {
        if (img == null) return;

        Color c = img.color;
        c.a = 0;
        img.color = c;
    }

    public void SetColor()
    {
        if (img == null) return;

        switch(color)
        {
            case PieceColor.Red:
                img.color = Color.red;
                break;
            case PieceColor.Blue:
                img.color = Color.blue;
                break;
            case PieceColor.Cyan:
                img.color = Color.cyan;
                break;
            case PieceColor.Yellow:
                img.color = Color.yellow;
                break;
            case PieceColor.Green:
                img.color = Color.green;
                break;
            case PieceColor.Magenta:
                img.color = Color.magenta;
                break;
            case PieceColor.Black:
                img.color = Color.black;
                break;
            default:
                img.color = Color.white;
                break;
        }
    }
}
