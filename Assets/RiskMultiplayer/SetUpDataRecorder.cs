using UnityEngine;
using Mirror;

public struct ArmySetUpDetails
{
    public string _SetName;
    public Color _SetColour;
    public ArmyScriptableObject _SetArmy;
}


public class SetUpDataRecorder : NetworkBehaviour
{

    public readonly SyncList<ArmySetUpDetails> _ArmyInfo = new SyncList<ArmySetUpDetails>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
