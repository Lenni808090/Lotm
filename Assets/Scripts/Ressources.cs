using System;
using UnityEngine;

public class Ressources : MonoBehaviour
{
    [SerializeField] public int Health = 100;
    [SerializeField] private int rtHealth;
    [SerializeField] public int Spirituality = 100;
    [SerializeField] private int rtSpirituality;


    void Start()
    {
        rtHealth = Health;
        rtSpirituality = Spirituality;
    }
    public void takeDamage(int damage)
    {
        rtHealth = Mathf.Max(rtHealth - damage, 0);
    }

    public int getHealth()
    {
        return rtHealth;
    }

    public void setHealth(int amount)
    {
        rtHealth = amount;
    }

    public void setSpirituality(int amount)
    {
        rtSpirituality = amount;
    }

    public void useSpirituality(int amount)
    {
        rtSpirituality = Mathf.Max(rtSpirituality - amount, 0);
    }

    public int getSpirituality()
    {
        return rtSpirituality;
    }

    public bool hasDied()
    {
        if (rtHealth <= 0)
        {
            return true;
        }
        return false;
    }
}
