using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardAdditionComponent : MonoBehaviour
{
    public RewardAdditionComponent _Parent;
    public bool _IsParent = false;
    public Image[] _Image;
    public TextMeshProUGUI _TextMeshPro;

    public GameObject[] _Children;

    // Update is called once per frame
    void Update()
    {
        if (!_IsParent)
        {
            foreach (Image i in _Image)
                i.color = new Color(i.color.r, i.color.g, i.color.b, _Parent._Image[0].color.a);

            if (_TextMeshPro != null)
                _TextMeshPro.color = new Color(_TextMeshPro.color.r, _TextMeshPro.color.g, _TextMeshPro.color.b, _Parent._Image[0].color.a);
        }
    }
}
