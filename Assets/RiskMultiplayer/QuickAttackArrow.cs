using UnityEngine;
using Mirror;

public class QuickAttackArrow : NetworkBehaviour
{
    private void Start()
    {
        
    }

    [Command(requiresAuthority = false)]
    public void SetArrowAuthority()
    {
        NetworkServer.Spawn(this.gameObject, connectionToClient);
        this.netIdentity.AssignClientAuthority(connectionToClient);
    }
}
