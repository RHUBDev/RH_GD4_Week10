using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
	
	[SerializeField] CapsuleCollider coll;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		rigs = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rig in rigs)
		{
			rig.isKinematic = true;
		}
		playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
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
		yield return new WaitForSeconds(3);
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
			/*Vector3 vect = playerController.transform.position - characterController.transform.position;
			vect.y = 0;
			characterController.transform.rotation = Quaternion.LookRotation(vect);
			characterController.Move(vect.normalized * Time.deltaTime * moveSpeed);
			*/

			anim.SetFloat("Speed", 1);

			if (Vector3.Dot(transform.forward, playerController.transform.position - transform.position) > 0)
			{
				RaycastHit hit;
				if (Physics.Raycast(eyePos.transform.position, (playerCamera.transform.position - Vector3.up * 0.4f) - eyePos.transform.position, out hit, 2000f, layerMask))
				{
					if (hit.transform.CompareTag("Player"))
					{
						//Debug.Log("Rayhit = " + hit.point);
						Debug.Log("#1");
                        if (!canSeePlayer)
                        {
							if(Time.time - lastLazerTime > lazerInterval)
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
						Debug.Log("#1.0");
						if (canSeePlayer)
						{
							GetTurnDirection();
						}
						canSeePlayer = false;
						lookPoint = transform.forward * 100f;
					}
				}
				else
				{
					Debug.Log("#1.1");
                    if (canSeePlayer)
					{
						GetTurnDirection();
					}
					canSeePlayer = false;
					lookPoint = transform.forward * 100f;
				}
			}
			else
			{
				Debug.Log("#1.2");
				if (canSeePlayer)
				{
					GetTurnDirection();
				}
				canSeePlayer = false;
				lookPoint = transform.forward * 100f;
			}

			Vector3 distVect = lastSeenPosition - transform.position;
			distVect.y = 0;
			if ((distVect).magnitude > 5)
			{
				Debug.Log("#2");
				Follow();
			}
			else
			{
				if (canSeePlayer)
				{
					Idle();
				}
				else
				{
					if (!setStartRotation)
					{
						Debug.Log("###1 " + Time.time);
						setStartRotation = true;
						rotateAngle = 0f;
					}
					Search();
				}
			}

            if (canSeePlayer)
			{
				lookPoint = playerCamera.transform.position;
				Lazer();
            }
            else
            {
				lookPoint = transform.forward * 100f;
            }
		}
	}

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

		Debug.Log("#3");
		if (Time.time > (lastPositionTime + 1) && !canSeePlayer)
		{
			if ((lastPosition - transform.position).magnitude > 0.3f)
			{

				Debug.Log("#4");
				lastPositionTime = Time.time;
				lastPosition = transform.position;
			}
			else
			{
				Debug.Log("#4.1");
				GetRandomDestination();
			}
		}
	}

	void Idle()
	{
		navagent.destination = transform.position;
	}

	void Search()
	{
		Debug.Log("#5");
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
			Debug.Log("###2 " + Time.time);
			GetRandomDestination();
		}
	}

	void GetRandomDestination()
	{
		Debug.Log("#6");
		lastSeenPosition = new Vector3(Random.Range(-xRange, xRange), 0, Random.Range(-zRange, zRange));
		lastSeenPosition.y = 0;
		navagent.destination = lastSeenPosition;
	}

	void Lazer()
    {
		lazerR.SetPosition(0, lazerR.transform.position);
		lazerL.SetPosition(0, lazerL.transform.position);

		if (Time.time > lastLazerTime + lazerInterval)
        {
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
		lazerR.enabled = true;
		lazerL.enabled = true;
		playerController.TakeDamage();
		yield return new WaitForSeconds(1);
		lazerR.enabled = false;
		lazerL.enabled = false;
	}

    private void LateUpdate()
	{
		headBone.LookAt(lookPoint);
	}

	void GetTurnDirection()
    {
		turnDir = Vector3.SignedAngle(transform.forward, lastSeenPosition - eyePos.transform.position, Vector3.up);
    }
}
