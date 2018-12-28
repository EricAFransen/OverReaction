using UnityEngine;

public class MainController : MonoBehaviour
{
    public ICardController CardController { get; private set; }
    public Simulation Sim { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        try
        {
            if (GameSparksManager.instance.iAmOnline)
            {
                if (GameSparksManager.instance.iAmHost)
                {
                    Debug.Log("I am the host. Creating a HostCardController.");
                    CardController = new HostCardController();
                } else
                {
                    Debug.Log("I am not the host. Creating a ClientCardController.");
                    CardController = new ClientCardController();
                }
            } else
            {
                Debug.Log("I am offline. Creating an OfflineController.");
                CardController = new OfflineCardController();
            }
        } catch (System.Exception e)
        {
            Debug.LogWarning("Exception encountered while starting controller. Creating offline card controller.");
            CardController = new OfflineCardController();
        }

        Sim = Camera.main.GetComponent<Simulation>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
