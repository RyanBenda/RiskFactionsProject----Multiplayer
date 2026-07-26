using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RiskArmyColourSelector : MonoBehaviour
{
    public RiskMultiplayerSetUpSceneScript _SceneScript;

    public Transform _Content;
    public List<ArmyColourButton> _Buttons;
    public GameObject _ButtonPrefab;

    public RiskMultiplayerPlayerSetUpScript _Requester;

    // Sets up all the buttons on start this way I can add more Armies later if I want to
    private void Start()
    {
        int ind = 0;
        foreach (ArmySelection asn in _SceneScript._ArmyChoices)
        {
            GameObject b = Instantiate(_ButtonPrefab, _Content);
            ArmyColourButton acb = b.GetComponent<ArmyColourButton>();
            acb._Army = asn;
            acb._Colour = asn._Army._ArmyColour;
            acb._Button.image.color = acb._Colour;

            if (acb._Army._Chosen)
            {
                acb._Cross.gameObject.SetActive(true);
                acb._Button.interactable = false;
            }
            acb._Selector = this;
            acb._ColourIndex = ind;
            ind++;

            _Buttons.Add(acb);
        }
    }

    // Updates all the Buttons to the correct interactable state
    public void OnEnable()
    {
        int ind = 0;
        foreach (ArmySelection asn in _SceneScript._ArmyChoices)
        {
            if (asn._Chosen && _Buttons.Count > ind)
            {
                _Buttons[ind]._Cross.gameObject.SetActive(true);
                _Buttons[ind]._Button.interactable = false;
            }
            else if (_Buttons.Count > ind)
            {
                _Buttons[ind]._Cross.gameObject.SetActive(false);
                _Buttons[ind]._Button.interactable = true;
            }
            ind++;
        }
    }
}
