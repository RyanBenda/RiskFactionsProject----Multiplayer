using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class AddAIButton : NetworkBehaviour
{
    public Button _Button;
    public GameObject _PlayerPrefab;
    public ScrollRect _PlayerList;

    void Awake()
    {
        this.gameObject.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    public void AddAIOpponent()
    {
        GameObject g = Instantiate(_PlayerPrefab, _PlayerList.content);
        NetworkServer.Spawn(g);

        RpcParentUpdate(g.transform);
    }

    [ClientRpc] //Updates the parent of the set up object
    void RpcParentUpdate(Transform obj)
    {
        obj.parent = _PlayerList.content;
        obj.localScale = Vector3.one;
    }
}
