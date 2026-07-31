using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Mirror;

public class StarsAdditionComponent : NetworkBehaviour
{
    public Image _Card;
    public TextMeshProUGUI _StarText;
    public TextMeshProUGUI _CardText;

    public void DoCardReveal(int stars) //Does the card reveal for the player recieving the card but hides it from everyone else
    {
        _StarText.color = new Color(0, 0, 0, 0);
        _CardText.color = new Color(0, 0, 0, 0);

        if (stars == 1)
            _StarText.text = "*";
        else if (stars == 2)
            _StarText.text = "**";

        Vector3 temp = this.transform.localPosition;
        this.transform.localPosition = new Vector3(this.transform.localPosition.x + 100, this.transform.localPosition.y - 100, this.transform.localPosition.z);
        this.transform.DOLocalMove(temp, 1);
        this.transform.DOScale(Vector3.one, 1f);
        _Card.DOColor(Color.white, 1f).OnComplete(() => EndCardReveal());
        if (GameCanvasComponent._GameInstance._LocalPlayer._IsTurn)
            _StarText.DOColor(Color.black, 1f);
        else
            _CardText.DOColor(Color.black, 1f);
    }

    void EndCardReveal()
    {
        this.transform.localScale = Vector3.zero;
        _Card.color = new Color(1, 1, 1, 0);
        _StarText.color = new Color(0, 0, 0, 0);
        _CardText.color = new Color(0, 0, 0, 0);
        if (isServer)
            GameCanvasComponent._GameInstance.CmdProgressTurn();
    }
}
