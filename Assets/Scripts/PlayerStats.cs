using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Elements")]
    public Slider healthSlider; // sleep hier de slider in de inspector

    [Header("Lose Screen")]
    public GameObject loseScreen; // sleep hier je lose screen object in de inspector

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (loseScreen != null)
            loseScreen.SetActive(false); // verberg lose screen bij start
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Player is dead!");

        // ✅ Activeer het lose screen
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
        }

        // ✅ Pauzeer ALLES behalve UI
        Time.timeScale = 0f;

        // ✅ Muis vrijgeven
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}