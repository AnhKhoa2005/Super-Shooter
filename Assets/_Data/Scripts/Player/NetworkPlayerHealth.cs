using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class NetworkPlayerHealth : NetworkLoadComponents
{
    [SerializeField] private Slider healthBar;
    public GameObject HealthBarObject => healthBar.gameObject;
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth => maxHealth;
    [Networked, OnChangedRender(nameof(UpdateHealthBar))] private int health { get; set; } = 100;

    public void SetHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
    }

    public void UpdateHealthBar()
    {
        healthBar.value = health;
    }

    public bool TakeDamage(int damage)
    {
        if (Object == null || Object != Object.HasStateAuthority) return false;

        int newHealth = health - damage;
        health = Mathf.Clamp(newHealth, 0, maxHealth);

        if (health <= 0)
        {
            return true;
        }
        return false;
    }
    protected override void LoadComponent()
    {
        if (healthBar == null)
            healthBar = transform.Find("Canvas/HealthBar").GetComponent<Slider>();
    }

    protected override void LoadComponentRuntime()
    {

    }
}
