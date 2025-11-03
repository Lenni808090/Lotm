using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private GameObject owner;
    [SerializeField] private string itemName;


    public GameObject getOwner()
    {
        return owner;
    }

}
