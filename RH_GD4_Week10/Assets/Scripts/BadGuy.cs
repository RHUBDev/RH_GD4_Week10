using UnityEngine;

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

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		rigs = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rig in rigs)
		{
			rig.isKinematic = true;
		}
		playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
	}

	public void Damage(int damageAmount)
	{
		//subtract damage amount when Damage function is called
		currentHealth -= damageAmount;
		
		//Check if health has fallen below zero
		if (currentHealth <= 0)
		{
			anim.enabled = false;
			isDead = true;
			foreach (Rigidbody rig in rigs)
            {
				rig.isKinematic = false;
            }
			playerController.AddScore(1);
		}
	}

	private void Update()
	{
		if (!isDead)
		{
			Vector3 vect = playerController.transform.position - characterController.transform.position;
			vect.y = 0;
			characterController.transform.rotation = Quaternion.LookRotation(vect);
			characterController.Move(vect.normalized * Time.deltaTime * moveSpeed);
			anim.SetFloat("Speed", 1);
		}
	}

	private void OnFootstep(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			if (FootstepAudioClips.Length > 0)
			{
				var index = Random.Range(0, FootstepAudioClips.Length);
				AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(characterController.center), FootstepAudioVolume);
			}
		}
	}

	private void OnLand(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(characterController.center), FootstepAudioVolume);
		}
	}
}
