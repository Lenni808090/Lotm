using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inventar : MonoBehaviour
{
    [SerializeField] private List<GameObject> inventar = new List<GameObject>();
    [SerializeField] private int inventarIndex = 0;
    private int prevInventarIndex = 0;
    [SerializeField] private TextMeshProUGUI text;
    private GameObject instItem;
    [SerializeField] private GameObject itemPos;
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (inventar.Count == 0)
            return;

        if (scroll > 0f)
        {
            inventarIndex = (inventarIndex + 1) % inventar.Count;

        }

        else if (scroll < 0f)
        {
            inventarIndex = (inventarIndex - 1 + inventar.Count) % inventar.Count;
        }
    }

    public void collectItem(GameObject item)
    {
        inventar.Add(item);
    }

    public GameObject currentItem()
    {
        if (inventar.Count > 0 && inventarIndex >= 0 && inventarIndex < inventar.Count)
        {
            GameObject newItem = inventar[inventarIndex];
            if (inventarIndex != prevInventarIndex)
            {
                text.text = newItem.name;
                Destroy(instItem);
                instItem = Instantiate(newItem, itemPos.transform.position, itemPos.transform.rotation, itemPos.transform);
                prevInventarIndex = inventarIndex;
            }
            return inventar[inventarIndex];
        }
        return null;
    }
}
