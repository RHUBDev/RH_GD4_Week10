using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class BadGuy : MonoBehaviour
{
	public Rigidbody[] rigs;
	public int currentHealth = 3;
	private int maxHealth = 3;
	private PlayerController playerController;
	[SerializeField] Animator anim;
	[SerializeField] private float moveSpeed = 1f;
	[SerializeField] CharacterController characterController;
	public AudioClip LandingAudioClip;
	public AudioClip[] FootstepAudioClips;
	[Range(0, 1)] public float FootstepAudioVolume = 0.5f;
	private bool isDead = false;
	[SerializeField] private NavMeshAgent navagent;
	private Vector3 lastSeenPosition;
	[SerializeField] private Transform eyePos;
	private Camera playerCamera;
	private float rotateSpeed = 180f;
	private float rotateAngle = 361f;
	private bool setStartRotation = false;
	private float xRange = 300f;
	private float zRange = 250f;
	private Vector3 lastPosition;
	private float lastPositionTime;
	private bool canSeePlayer = false;
	[SerializeField] private LayerMask layerMask;
	private float lastLazerTime;
	[SerializeField] private LineRenderer lazerR;
	[SerializeField] private LineRenderer lazerL;
	[SerializeField] Transform headBone;
	private Vector3 lookPoint;
	private float turnDir = 0;
	private float lazerInterval = 3;
	private float resurrectTime = 8;
	[SerializeField] CapsuleCollider coll;
	private string levelName;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		levelName = SceneManager.GetActiveScene().name;
		rigs = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rig in rigs)
		{
			rig.isKinematic = true;
		}
		//if (levelName == "MyScene")
		//{
			playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
		//}
		lastSeenPosition = transform.position;
		playerCamera = Camera.main;
		lastPosition = transform.position;
		lastLazerTime = Time.time + lazerInterval;
	}

	public void Damage(int damageAmount)
	{
		if (!isDead)
		{
			//subtract damage amount when Damage function is called
			currentHealth -= damageAmount;

			lastSeenPosition = playerCamera.transform.root.position;
			lastSeenPosition.y = 0;
			navagent.destination = lastSeenPosition;

			//Check if health has fallen below zero
			if (currentHealth <= 0)
			{
				anim.enabled = false;
				isDead = true;
				navagent.enabled = false;
				foreach (Rigidbody rig in rigs)
				{
					rig.isKinematic = false;
				}
				playerController.AddScore(1);
				coll.enabled = false;
				StartCoroutine(Resurrect());
			}
		}
	}

	IEnumerator Resurrect()
    {
		yield return new WaitForSeconds(resurrectTime);
		if (Time.time - lastLazerTime > lazerInterval)
		{
			lastLazerTime = (Time.time - lazerInterval) + 1;
		}
		coll.enabled = true;
		currentHealth = maxHealth;
		isDead = false;
		foreach (Rigidbody rig in rigs)
		{
			rig.isKinematic = true;
		}
		anim.enabled = true;
		navagent.enabled = true;
	}

	private void Update()
	{
		if (!isDead)
		{

			anim.SetFloat("Speed", 1);

			if (Vector3.Dot(transform.forward, playerController.transform.position - transform.position) > 0)
			{
				//if player is in front of enemy
				RaycastHit hit;
				if (Physics.Raycast(eyePos.transform.position, (playerCamera.transform.position - Vector3.up * 0.4f) - eyePos.transform.position, out hit, 2000f, layerMask))
				{
					//and there is clear line of sight
					if (hit.transform.CompareTag("Player"))
					{
						if (!canSeePlayer)
						{
							//if we now see player again, and a lazer is due, set lastLazerTime to 2 seconds ago, giving the player 1 second until next lazer
							if (Time.time - lastLazerTime > lazerInterval)
							{
								lastLazerTime = (Time.time - lazerInterval) + 1;
							}
						}
						canSeePlayer = true;
						lastSeenPosition = playerCamera.transform.root.position;
						lastSeenPosition.y = 0;
						navagent.destination = lastSeenPosition;

						setStartRotation = false;
						rotateAngle = 0;
					}
					else
					{
						if (canSeePlayer)
						{
							//if could see player, but now can't, get the last seen direction
							GetTurnDirection();
						}
						canSeePlayer = false;
						lookPoint = transform.forward * 100f;
					}
				}
				else
				{
					if (canSeePlayer)
					{
						//if could see player, but now can't, get the last seen direction
						GetTurnDirection();
					}
					canSeePlayer = false;
					lookPoint = transform.forward * 100f;
				}
			}
			else
			{
				if (canSeePlayer)
				{
					//if could see player, but now can't, get the last seen direction
					GetTurnDirection();
				}
				canSeePlayer = false;
				lookPoint = transform.forward * 100f;
			}

			Vector3 distVect = lastSeenPosition - transform.position;
			distVect.y = 0;
			//if the horizontal 2d distance between the enemy and the player is over 5m, keep following the last seen position
			if ((distVect).magnitude > 5)
			{
				Follow();
			}
			else
			{
				//if less than 5m away, and can still see the player, stay here
				if (canSeePlayer)
				{
					Idle();
				}
				else
				{
					//else, set rotateAngle to 0, then start searching by rotating on the spot
					if (!setStartRotation)
					{
						setStartRotation = true;
						rotateAngle = 0f;
					}
					Search();
				}
			}

			if (canSeePlayer)
			{
				//if can see player, look at him
				lookPoint = playerCamera.transform.position;
				Lazer();
			}
			else
			{
				//otherwise, look at a spot on the floor 100m away
				lookPoint = transform.forward * 100f;
			}
		}
	}

	//Used the OnFootstep and OnLand functions from the Sample player controller, cos otherwise the Sample animator had errors
	private void OnFootstep(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			if (FootstepAudioClips.Length > 0)
			{
				var index = Random.Range(0, FootstepAudioClips.Length);
				AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position, FootstepAudioVolume);
			}
		}
	}

	private void OnLand(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			AudioSource.PlayClipAtPoint(LandingAudioClip, transform.position, FootstepAudioVolume);
		}
	}

	void Follow()
	{
		//if we can't see player anymore, check if we are moving too little, if we are, we are probably stuck against a building at the end of our destination, so get a new random destination
		if (Time.time > (lastPositionTime + 1) && !canSeePlayer)
		{
			if ((lastPosition - transform.position).magnitude > 0.3f)
			{
				lastPositionTime = Time.time;
				lastPosition = transform.position;
			}
			else
			{
				GetRandomDestination();
			}
		}
	}

	void Idle()
	{
		//set destination to here, and stay idle
		navagent.destination = transform.position;
	}

	void Search()
	{
		//at last seen player point, but can't see player, turn around in the direction we last saw him, if still can't see him after a 360 turn, get new random destination
        if (turnDir < 0)
        {
			turnDir = -1;
        }
		else if(turnDir > 0)
		{
			turnDir = 1;
		}
		rotateAngle += turnDir * Time.deltaTime * rotateSpeed;
		transform.Rotate(Vector3.up * turnDir * Time.deltaTime * rotateSpeed);
		if (rotateAngle > 360 || rotateAngle < -360)
		{
			setStartRotation = false;
			rotateAngle = 0;
			GetRandomDestination();
		}
	}

	void GetRandomDestination()
	{
		//get new random destination
		lastSeenPosition = new Vector3(Random.Range(-xRange, xRange), 0, Random.Range(-zRange, zRange));
		lastSeenPosition.y = 0;
		navagent.destination = lastSeenPosition;
	}

	void Lazer()
    {
		//Shoot lazers at player!
		lazerR.SetPosition(0, lazerR.transform.position);
		lazerL.SetPosition(0, lazerL.transform.position);

		if (Time.time > lastLazerTime + lazerInterval)
        {
			//if due for the next lasers, shoot at the player's chest area, but make the lasers go twice as far
			lastLazerTime = Time.time;
			Vector3 lazerpos = playerCamera.transform.position - Vector3.up * 0.5f;
			Vector3 lazerposR = (lazerpos - lazerR.transform.position) *2 ;
			Vector3 lazerposL = (lazerpos - lazerL.transform.position) * 2;
			lazerR.SetPosition(1, lazerR.transform.position + lazerposR);
			lazerL.SetPosition(1, lazerL.transform.position + lazerposL);
			StartCoroutine(DoLazer());
		}
    }

	IEnumerator DoLazer()
    {
		//enable, then disable, lasers
		lazerR.enabled = true;
		lazerL.enabled = true;
		playerController.TakeDamage();
		yield return new WaitForSeconds(1);
		lazerR.enabled = false;
		lazerL.enabled = false;
	}

    private void LateUpdate()
	{
		if (!isDead)
		{
			//if not dead look at player if we can see him, or a point 100m forwards if not
			headBone.LookAt(lookPoint);
		}
	}

	void GetTurnDirection()
    {
		//get the angle between last seen position of the player, and our forward vector
		turnDir = Vector3.SignedAngle(transform.forward, lastSeenPosition - eyePos.transform.position, Vector3.up);
    }
}
