using UnityEngine;
using System.Collections;

public enum PlayerState
{
	Idle,
	Moving,
	Jumping
}

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
	//----------------------------------------------------------
	#region Fields

	[SerializeField]
	private float jumpHeight = 1.5f;
	[SerializeField]
	private float timeToJumpApex = 0.4f;
	[SerializeField]
	private float wallJumpWait = 0.2f;
	[SerializeField]
	private float wallJumpForce = 2.0f;
	[SerializeField]
	private bool invertGravity = false;
	[SerializeField]
	private bool invertHorizontalAxis = false;

	private Controller2D controller = null;
	private Vector3 velocity = Vector3.zero;
	private PlayerState currentState;
	private PlayerState previousState;
	private float accelerationTimeAirborne = 0.2f;
	private float accelerationTimeGrounded = 0.1f;
	private float moveSpeed = 6.0f;
	private float gravity = 0.0f;
	private float jumpVelocity = 0.0f;
	private float velocityXSmoothing = 0.0f;
	private float lastJumpTime = 0.0f;
	private bool wallJumping = false;
	private bool doubleJumping = false;
	private bool jumping = false;

	#endregion


	//----------------------------------------------------------
	#region Properties

	public PlayerState CurrentState
	{
		get
		{
			return this.currentState;
		}

		private set
		{
			if(this.currentState == value)
			{
				return;
			}

			Debug.Log("Current State: " + value.ToString());
			this.previousState = this.currentState;
			this.currentState = value;
			this.OnPlayerStateChange(this.currentState, this.previousState);
		}
	}

	#endregion


	//----------------------------------------------------------
	#region Start method

	private void Start()
	{
		this.controller = GetComponent<Controller2D>();
		this.gravity = -(2 * this.jumpHeight) / Mathf.Pow(this.timeToJumpApex, 2.0f);
		this.jumpVelocity = Mathf.Abs(this.gravity) * this.timeToJumpApex;
		this.gravity = (this.invertGravity == true) ? -this.gravity : this.gravity;
		this.jumpVelocity = (this.invertGravity == true) ? -this.jumpVelocity : this.jumpVelocity;
		this.currentState = PlayerState.Idle;
		this.previousState = this.currentState;
	}

	#endregion


	//----------------------------------------------------------
	#region Update method

	private void Update()
	{
		if (this.controller.collisions.above == true || this.controller.collisions.below == true)
		{
			this.jumping = false;
			this.doubleJumping = false;
			this.wallJumping = false;
			this.velocity.y = 0.0f;
			this.lastJumpTime = 0.0f;

			if(this.velocity == Vector3.zero)
			{
				this.CurrentState = PlayerState.Idle;
			}
			else
			{
				this.CurrentState = PlayerState.Moving;
			}
		}

		if (Input.GetButtonDown("Jump") == true && this.jumping == true && this.doubleJumping == false)
		{
			this.doubleJumping = true;
			this.velocity.y = this.jumpVelocity;
		}

		if (this.invertGravity == false)
		{
			if (Input.GetButtonDown("Jump") == true && this.jumping == false && this.controller.collisions.below == true)
			{
				this.lastJumpTime = Time.time;
				this.jumping = true;
				this.CurrentState = PlayerState.Jumping;
				this.velocity.y = this.jumpVelocity;
			}
		} 
		else
		{
			if (Input.GetButtonDown("Jump") == true && this.jumping == false && this.controller.collisions.above == true)
			{
				this.lastJumpTime = Time.time;
				this.jumping = true;
				this.CurrentState = PlayerState.Jumping;
				this.velocity.y = this.jumpVelocity;
			}
		}

		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		float targetVelocityX = ((this.invertHorizontalAxis == true) ? -input.x : input.x) * this.moveSpeed;

		if (this.invertHorizontalAxis == false)
		{
			if (Input.GetButtonDown("Jump") == true && this.jumping == true && this.wallJumping == false && ((this.lastJumpTime + this.wallJumpWait) < Time.time) && this.controller.collisions.left == true && input.x < -0.6f)
			{
				this.wallJumping = true;

				velocity.x = this.wallJumpForce * this.moveSpeed;
				this.velocity.y = this.jumpVelocity;
			}
		
			if (Input.GetButtonDown("Jump") == true && this.jumping == true && this.wallJumping == false && ((this.lastJumpTime + this.wallJumpWait) < Time.time) && this.controller.collisions.right == true && input.x > 0.6f)
			{
				this.wallJumping = true;
				velocity.x = -(this.wallJumpForce * this.moveSpeed);
				this.velocity.y = this.jumpVelocity;
			}
		} 
		else
		{
			if (Input.GetButtonDown("Jump") == true && this.jumping == true && this.wallJumping == false && ((this.lastJumpTime + this.wallJumpWait) < Time.time) && this.controller.collisions.left == true && input.x > 0.6f)
			{
				this.wallJumping = true;
				velocity.x = this.wallJumpForce * this.moveSpeed;
				this.velocity.y = this.jumpVelocity;
			}
			
			if (Input.GetButtonDown("Jump") == true && this.jumping == true && this.wallJumping == false && ((this.lastJumpTime + this.wallJumpWait) < Time.time) && this.controller.collisions.right == true && input.x < -0.6f)
			{
				this.wallJumping = true;
				velocity.x = -(this.wallJumpForce * this.moveSpeed);
				this.velocity.y = this.jumpVelocity;
			}
		}

		if (this.invertGravity == false)
		{
			this.velocity.x = Mathf.SmoothDamp(this.velocity.x, targetVelocityX, ref this.velocityXSmoothing, (this.controller.collisions.below == true) ? this.accelerationTimeGrounded : this.accelerationTimeAirborne);
		} 
		else
		{
			this.velocity.x = Mathf.SmoothDamp(this.velocity.x, targetVelocityX, ref this.velocityXSmoothing, (this.controller.collisions.above == true) ? this.accelerationTimeGrounded : this.accelerationTimeAirborne);
		}

		this.velocity.y += this.gravity * Time.deltaTime;
		this.controller.Move(this.velocity * Time.deltaTime);
	}

	#endregion


	//----------------------------------------------------------
	#region SetGravity method

	private void SetGravity(bool inverted)
	{
		this.invertGravity = inverted;
		this.gravity = -(2 * this.jumpHeight) / Mathf.Pow(this.timeToJumpApex, 2.0f);
		this.jumpVelocity = Mathf.Abs(this.gravity) * this.timeToJumpApex;
		this.gravity = (this.invertGravity == true) ? -this.gravity : this.gravity;
		this.jumpVelocity = (this.invertGravity == true) ? -this.jumpVelocity : this.jumpVelocity;
	}

	#endregion


	//----------------------------------------------------------
	#region OnPlayerStateChange method

	private void OnPlayerStateChange(PlayerState current, PlayerState previous)
	{
	}

	#endregion
}
