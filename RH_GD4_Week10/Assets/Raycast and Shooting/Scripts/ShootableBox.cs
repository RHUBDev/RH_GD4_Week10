using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody))]

public class ShootableBox : MonoBehaviour {

	//The box's current health point total
	public int currentHealth = 3;
	private float xRange = 300f;
	private float zRange = 250f;
	private float yHeight = 300f;
	private int maxHealth = 3;
	[SerializeField] private Rigidbody rig;
	private float maxTorque = 20f;
	[SerializeField] private TMP_Text pointsText;
	private bool hitCollision = false;
	private int points = 0;

	private void Start()
    {
		transform.position = new Vector3(transform.position.x, 100f, transform.position.z);
		rig.AddTorque(Random.Range(-maxTorque, maxTorque), Random.Range(-maxTorque, maxTorque), Random.Range(-maxTorque, maxTorque));

	}

	public void Damage(int damageAmount)
	{
		//subtract damage amount when Damage function is called
		currentHealth -= damageAmount;

		//Check if health has fallen below zero
		if (currentHealth <= 0) 
		{
			//if health has fallen below zero, deactivate it 
			//Destroy(gameObject);
			transform.position = new Vector3(Random.Range(-xRange, xRange), yHeight, Random.Range(-zRange, zRange));
			currentHealth = maxHealth;
			rig.AddTorque(Random.Range(-maxTorque, maxTorque), Random.Range(-maxTorque, maxTorque), Random.Range(-maxTorque, maxTorque));
            if (!hitCollision)
            {
				points += 3;
            }
            else
            {
				points += 1;
			}
			pointsText.text = "Points: " + points;
			hitCollision = false;
		}
	}

    private void OnCollisionEnter(Collision collision)
    {
		hitCollision = true;
	}
}
