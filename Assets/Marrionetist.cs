using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Timeline;
using UnityEngine;

public class Marrionetist : MonoBehaviour
{
    private Color gizmoColor = new Color(0f, 1f, 0f, 0.2f);
    private GameObject player;
    public List<GameObject> marrionetsList;
    private Movement movement;

    [SerializeField] Inventar inventar;
    [SerializeField] private GameObject equipedItem;
    [SerializeField] private GameObject drowsingRodPrefab;
    private GameObject drowsingRod;


    [SerializeField] private float airBulletTotalRange = 200;
    [SerializeField] private int airBulletDamage = 20;


    [SerializeField] private float substitutionTime = 5;
    [SerializeField] private float substitutionDistance = 30;




    [SerializeField] private LayerMask fireJumpZoneMask;
    [SerializeField] private float fireJumpDistance = 30f;
    [SerializeField] private Ressources playerRessources;
    private Dictionary<GameObject, Color> highlightedObjcs;
    private HashSet<GameObject> insideFireJumpMask;





    [SerializeField] private float controleTickRate = 1f;
    [SerializeField] private Coroutine spiritualityCoroutine;
    [SerializeField] private float controlRange = 150f;
    [SerializeField] private float initialControlRange = 15f;
    [SerializeField] private GameObject currentMarrionet;
    void Start()
    {
        movement = GetComponent<Movement>();
        player = this.gameObject;
        currentMarrionet = player;
        highlightedObjcs = new Dictionary<GameObject, Color>();
        insideFireJumpMask = new HashSet<GameObject>();
    }

    void Update()
    {

        if (currentMarrionet == player)
        {
            equipedItem = inventar.currentItem();
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            RaycastHit rayHit;

            if (Input.GetMouseButtonDown(1))
            {
                if (Physics.Raycast(ray, out rayHit) && Vector3.Distance(player.transform.position, rayHit.transform.position) <= initialControlRange)
                {
                    takeControl(rayHit.collider.gameObject);
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(ray, out rayHit) && Vector3.Distance(player.transform.position, rayHit.transform.position) <= airBulletTotalRange)
                {
                    Ressources hitRessources = rayHit.transform.gameObject.GetComponent<Ressources>();
                    if (hitRessources != null)
                    {
                        hitRessources.takeDamage(airBulletDamage);
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                if (Physics.Raycast(ray, out rayHit) && Vector3.Distance(player.transform.position, rayHit.transform.position) <= fireJumpDistance)
                {
                    if (rayHit.transform.tag == "Tree")
                    {
                        fireJump(rayHit.transform.gameObject);
                    }
                }
            }
            else if (Input.GetKey(KeyCode.Tab))
            {
                insideFireJumpMask.Clear();
                Collider[] hits = Physics.OverlapSphere(player.transform.position, fireJumpDistance, fireJumpZoneMask);
                foreach (Collider hit in hits)
                {
                    if (hit.tag == "Tree")
                    {
                        Renderer rend = hit.gameObject.GetComponent<Renderer>();
                        Color orgColor = rend.material.color;
                        insideFireJumpMask.Add(hit.gameObject);
                        if (!highlightedObjcs.ContainsKey(hit.gameObject))
                        {
                            highlightedObjcs.Add(hit.gameObject, orgColor);
                            rend.material.color = Color.red;
                        }
                    }
                }
                List<GameObject> toBeRemoved = new List<GameObject>();
                foreach (var obj in highlightedObjcs)
                {
                    if (!insideFireJumpMask.Contains(obj.Key))
                    {
                        obj.Key.gameObject.GetComponent<Renderer>().material.color = obj.Value;
                        toBeRemoved.Add(obj.Key);
                    }
                }

                foreach (GameObject obj in toBeRemoved)
                {
                    highlightedObjcs.Remove(obj);
                }
            }
            else if (Input.GetKeyUp(KeyCode.Tab))
            {
                foreach (var entry in highlightedObjcs)
                {
                    GameObject obj = entry.Key;
                    Color orgColor = entry.Value;
                    Renderer rend = obj.GetComponent<Renderer>();
                    rend.material.color = orgColor;
                }
                highlightedObjcs.Clear();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(substitutionTimer());
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                if (drowsingRod == null)
                {
                    if (equipedItem != null)
                    {
                        Item item = equipedItem.GetComponent<Item>();
                        if (item != null)
                        {
                            GameObject prevOwner = item.getOwner();
                            if (prevOwner != null)
                            {
                                Debug.Log("Searching for " + prevOwner.transform.name);
                                searchPreOwner(prevOwner);
                            }
                            else
                            {
                                Debug.Log("no previous Owner");
                            }
                        }
                    }
                }
            }
        }
        else
        {
            equipedItem = null;

            if (Input.GetKeyDown(KeyCode.B))
            {
                switchToPlayer();
            }

            if (Vector3.Distance(currentMarrionet.transform.position, player.transform.position) > controlRange)
            {
                Destroy(currentMarrionet);
                switchToPlayer();
            }


        }

        for (int i = 0; i < marrionetsList.Count; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                switchMarrionett(i);
            }
        }

    }

    public void takeControl(GameObject marrionet)
    {
        StartCoroutine(DamageEverySecond(marrionet));
    }

    IEnumerator DamageEverySecond(GameObject marrionet)
    {
        if (marrionet == null) yield break;
        if (marrionetsList.Contains(marrionet)) yield break;
        Ressources enemyRessources = marrionet.GetComponent<Ressources>();
        if (enemyRessources == null) yield break;
        if (enemyRessources.hasDied()) yield break;

        while (true)
        {
            if (Vector3.Distance(marrionet.transform.position, player.transform.position) < initialControlRange)
            {
                enemyRessources.takeDamage(10);
                if (enemyRessources.getHealth() <= 0)
                {
                    marrionetsList.Add(marrionet);
                    Debug.Log("Marrionet added: " + marrionet.name);
                    enemyRessources.setHealth(enemyRessources.Health);
                    yield break;
                }
            }
            else
            {
                enemyRessources.setHealth(enemyRessources.Health);
                Debug.Log("Controlled being out of range");
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator substitutionTimer()
    {
        float timer = 0f;

        Color orgColor = player.GetComponent<Renderer>().material.color;
        player.GetComponent<Renderer>().material.color = Color.yellow;

        while (timer <= substitutionTime)
        {
            if (playerRessources.hasDied())
            {
                paperSubstitution();
                player.GetComponent<Renderer>().material.color = orgColor;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        player.GetComponent<Renderer>().material.color = orgColor;

    }


    public void paperSubstitution()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 spawnPosition = player.transform.position + new Vector3(
            Mathf.Cos(angle) * substitutionDistance,
            0f,
            Mathf.Sin(angle) * substitutionDistance
        );

        playerRessources.setHealth(playerRessources.Health);
        player.transform.position = spawnPosition;
    }


    public void stopControlingMarrionett()
    {
        if (spiritualityCoroutine != null)
        {
            StopCoroutine(spiritualityCoroutine);
            spiritualityCoroutine = null;
        }
    }

    public void startControlingMarionett()
    {
        if (spiritualityCoroutine == null)
        {
            spiritualityCoroutine = StartCoroutine(coroutineMarrionett());
        }
    }
    IEnumerator coroutineMarrionett()
    {
        while (player != currentMarrionet)
        {
            playerRessources.useSpirituality(2);
            yield return new WaitForSeconds(controleTickRate);
        }
    }

    public void switchMarrionett(int index)
    {
        if (index < 0 || index >= marrionetsList.Count) return;

        GameObject marrionet = marrionetsList[index];

        if (marrionet == currentMarrionet) return;
        currentMarrionet = marrionet;
        startControlingMarionett();
        movement.setRb(marrionet.GetComponent<Rigidbody>());

        Transform cameraHolder = marrionet.transform.Find("Camera Holder");
        if (cameraHolder == null)
        {
            Debug.LogError("camera holder not found");
        }
        movement.setCameraHolder(cameraHolder);
    }

    public void switchToPlayer()
    {

        currentMarrionet = player;
        movement.setRb(player.GetComponent<Rigidbody>());
        stopControlingMarrionett();
        Transform cameraHolder = player.transform.Find("Camera Holder");
        if (cameraHolder == null)
        {
            Debug.LogError("camera holder not found");
        }
        movement.setCameraHolder(cameraHolder);
    }


    public void fireJump(GameObject tree)
    {
        playerRessources.useSpirituality(2);
        player.GetComponent<Rigidbody>().MovePosition(tree.transform.position);
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        if (highlightedObjcs.ContainsKey(tree)) highlightedObjcs.Remove(tree);
        Destroy(tree);
    }

    public void searchPreOwner(GameObject prevOwner)
    {
        StartCoroutine(DrowsingRod(prevOwner));
    }

    IEnumerator DrowsingRod(GameObject searchedObj)
    {
        drowsingRod = Instantiate(
            drowsingRodPrefab,
            player.transform.position + Vector3.up * 2f,
            Quaternion.identity
        );

        drowsingRod.transform.SetParent(player.transform);

        while (Vector3.Distance(player.transform.position, searchedObj.transform.position) > 5)
        {
            Vector3 direction = searchedObj.transform.position - player.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                drowsingRod.transform.rotation = lookRotation;
            }

            yield return null;
        }

        Destroy(drowsingRod);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, fireJumpDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fireJumpDistance);
    }
}
