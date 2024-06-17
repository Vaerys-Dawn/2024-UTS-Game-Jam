/*
	Created by @DawnosaurDev at youtube.com/c/DawnosaurStudios
	Thanks so much for checking this out and I hope you find it helpful! 
	If you have any further queries, questions or feedback feel free to reach out on my twitter or leave a comment on youtube :D

	Feel free to use this in your own games, and I'd love to see anything you make!
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static PlayerDataWithDash;

public class PlayerMovement : MonoBehaviour
{
	//Scriptable object which holds all the player's movement parameters. If you don't want to use it
	//just paste in all the parameters, though you will need to manuly change all references in this script
	[SerializeField]
	private PlayerDataWithDash Data;

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    public CapsuleCollider2D Collider { get; private set; }
	#endregion

	#region STATE PARAMETERS
	//Variables control the various actions the player can perform at any time.
	//These are fields which can are public allowing for other sctipts to read them
	//but can only be privately written to.
	public bool IsFacingRight { get; private set; } = true;
	public bool IsJumping { get; private set; }
	public bool IsWallJumping { get; private set; }
	public bool IsDashing { get; private set; }
	public bool IsWallClimbing { get; private set; }

    public bool IsSliding { get; private set; }

    public bool IsInteracting { get; private set; }

    public bool IsDying { get; private set; }


    public bool IsWindup { get; private set; }

    //Timers (also all fields, could be private and a method returning a bool could be used)
    public float LastOnGroundTime { get; private set; }
	public float LastOnWallTime { get; private set; }
	public float LastOnWallRightTime { get; private set; }
	public float LastOnWallLeftTime { get; private set; }

    public float AttackCooldownTime { get; private set; }

    public float ComboTime { get; private set; }

    //Jump
    private bool _isJumpCut;
	private bool _isJumpFalling;

	//Wall Jump
	private float _wallJumpStartTime;
	private int _lastWallJumpDir;

	//Dash
	private int _dashesLeft;
	private bool _dashRefilling;
	private Vector2 _lastDashDir;
	private bool _isDashAttacking;

    //Slide
    private int _slidesLeft;
    private bool _slideRefilling;
    private Vector2 _lastSlideDir;
    private bool _isSlideAttacking;


	// attacks
	public int _comboState = 1;

    #endregion

    #region INPUT PARAMETERS
    private Vector2 _moveInput;
    private Vector2 _aimDirection;
	[SerializeField] private Material iFrameMaterial;
    private Material baseMaterial;


    public float LastPressedJumpTime { get; private set; }
	public float LastPressedDashTime { get; private set; }

    public float LastPressedSlideTime { get; private set; }
    public Vector3 LastStablePosition { get; private set; }
    public bool IsGrappling { get; private set; }
    public float IFrames { get; private set; }
    public int CurrentHealth { get; private set; }
    public float StepTime { get; private set; }
    public float JumpTestTime { get; private set; }
    public float JumpSoundCoolDown { get; private set; }

    #endregion

    #region CHECK PARAMETERS
    //Set all of these up in the inspector
    [Header("Checks")] 
	[SerializeField] private Transform _groundCheckPoint;
	//Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [SerializeField] private Transform _slideCheckPoint;
    //Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
    [SerializeField] private Vector2 _slideCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
	[SerializeField] private Transform cursorAimCollider;
    [SerializeField] private Transform movementAimCollider;
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _stableGroundLayer;
    [SerializeField] private LayerMask _raycastFilter;
	#endregion

	#region LAYERS & TAGS
	[Header("Unlocks")]
	[SerializeField] public bool _attackUnlocked = true;
    [SerializeField] public bool _dashUnlocked = true;
    [SerializeField] public bool _wallJumpUnlocked = true;
    [SerializeField] public bool _wallClimbUnlocked = true;
    [SerializeField] public bool _slideUnlocked = true;
    [SerializeField] public bool _wallHoldUnlocked = true;
	[SerializeField] public int _maxHealth = 30;
    #endregion

    #region SlideHitbox
    [SerializeField] private CapsuleCollider2D defaultCollider;
	[SerializeField] private CapsuleCollider2D slideCollider;



    #endregion
    private Animator animator;
	private SpriteRenderer spriteRenderer;
	private SpriteRenderer aimSprite;
    private bool _cursorVisible;
    private float currentPullTime;
  //  [SerializeField] private float _stepCoolDown = 0.3f;
  //  [SerializeField] private float _jumpSoundCoolDown = 0.3f;
  //  [SerializeField] private float _jumpVolumeMult = 2f;
 //   [SerializeField] private float _dashVolumeMult = 0.25f;
 //   [SerializeField] private float _stepVolumeMult = 2f;
 //   [SerializeField] private float _fallVolumeMult = 2f;
 //   [SerializeField] private float _swordVolumeMult = 1f;
//	[SerializeField] private float _healSoundMult = 0.5f;
 //   [SerializeField] private float _damageVolumeMult = 1f;
 //   [SerializeField] private float _grappleVolumeMult = 1f;

    private bool slideStuck = false;


    private void Awake()
	{
		CurrentHealth = _maxHealth;

		// removes itself if duplicate is found
		if (FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).Length > 1)
		{
			Destroy(this.gameObject);
		}
		// keep character loaded between scenes
		DontDestroyOnLoad(this);

		// setup variables
		animator = GetComponent<Animator>();
		aimSprite = cursorAimCollider.GetComponent<SpriteRenderer>();
		aimSprite.color = new Color(aimSprite.color.r, aimSprite.color.g, aimSprite.color.b, 0);
		RB = GetComponent<Rigidbody2D>();
		Collider = GetComponent<CapsuleCollider2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		baseMaterial = spriteRenderer.material;
	}

	private void Start()
	{
        SetGravityScale(Data.gravityScale);
		IsFacingRight = true;
	}

	private void Update()
	{
		Console.WriteLine("Hellow?>");

        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
		LastOnWallTime -= Time.deltaTime;
		LastOnWallRightTime -= Time.deltaTime;
		LastOnWallLeftTime -= Time.deltaTime;

		LastPressedJumpTime -= Time.deltaTime;
		LastPressedDashTime -= Time.deltaTime;
        LastPressedSlideTime -= Time.deltaTime;

		AttackCooldownTime -= Time.deltaTime;
        ComboTime -= Time.deltaTime;
        currentPullTime -= Time.deltaTime;
		IFrames -= Time.deltaTime;
		StepTime -= Time.deltaTime;
        JumpSoundCoolDown -= Time.deltaTime;
        #endregion

        if (IsSliding) IFrames = 0.01f;


        #region INPUT HANDLER
        _moveInput.x = UserInput.instance.MoveInput.x;
		_moveInput.y = UserInput.instance.MoveInput.y;

		_aimDirection = UserInput.instance.AimInput;

		if (_moveInput.x != 0)
			CheckDirectionToFace(_moveInput.x > 0);


		bool stopInteraction = !IsInteracting && !IsWallClimbing && !IsWallJumping;


        if (!slideStuck && _attackUnlocked && UserInput.instance.AttackInput && AttackCooldownTime < 0 && stopInteraction)
		{
			IsWindup = true;
            IsInteracting = true;
            StartCoroutine(nameof(StartAttack));
        }

        if (!slideStuck && UserInput.instance.JumpPressed && !IsInteracting)
        {
            OnJumpInput();
        }

        if (!slideStuck && UserInput.instance.JumpReleased && !IsInteracting)
        {
            OnJumpUpInput();
        }

        if (!slideStuck && UserInput.instance.JumpPressed && !IsWindup)
        {
            OnDashInput();
        }

        if (UserInput.instance.SprintInput && !IsInteracting)
        {
            OnSlideInput();
        }
        #endregion


        #region COLLISION CHECKS
        if (!IsDashing && !IsJumping && !IsSliding)
		{
			//Ground Check
			if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping) //checks if set box overlaps with ground
			{
				LastOnGroundTime = Data.coyoteTime; //if so sets the lastGrounded to coyoteTime
            }

			RaycastHit2D hit1 = Physics2D.Raycast(_groundCheckPoint.position + new Vector3(_groundCheckSize.x * 0.5f, 0, 0), Vector2.down, _groundCheckSize.y, _stableGroundLayer);
            RaycastHit2D hit2 = Physics2D.Raycast(_groundCheckPoint.position - new Vector3(_groundCheckSize.x * 0.5f, 0, 0), Vector2.down, _groundCheckSize.y, _stableGroundLayer);

            if (hit1 && hit2 && !IsJumping) //checks if set box overlaps with ground
            {
                LastStablePosition = transform.position;
            }

            //Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
					|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
				LastOnWallRightTime = Data.coyoteTime;

			//Right Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
				LastOnWallLeftTime = Data.coyoteTime;

			//Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
			LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
		}
		#endregion

		#region JUMP CHECKS
		if (IsJumping && RB.velocity.y < 0)
		{
			IsJumping = false;

			if(!IsWallJumping)
				_isJumpFalling = true;
		}

		if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
		{
			IsWallJumping = false;
		}

		if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
			_isJumpCut = false;

			if(!IsJumping)
				_isJumpFalling = false;
		}

		if (!IsDashing)
		{
			//Jump
			if (CanJump() && LastPressedJumpTime > 0)
			{
				IsJumping = true;
				IsWallJumping = false;
                IsSliding = false;
                _isJumpCut = false;
				_isJumpFalling = false;
				Jump();
			}
			//WALL JUMP
			else if (CanWallJump() && LastPressedJumpTime > 0)
			{
				IsWallJumping = true;
				IsJumping = false;
                IsSliding = false;
                _isJumpCut = false;
				_isJumpFalling = false;

				_wallJumpStartTime = Time.time;
				_lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

				WallJump(_lastWallJumpDir);
			}
		}
		#endregion

		#region DASH CHECKS
		if (!slideStuck && CanDash() && LastPressedSlideTime > 0 && !IsWallClimbing && !IsWallJumping)
		{
			//Freeze game for split second. Adds juiciness and a bit of forgiveness over directional input
			Sleep(Data.dashSleepTime);

			//If not direction pressed, dash forward
			if (_moveInput != Vector2.zero)
				_lastDashDir = new Vector2(_moveInput.normalized.x, 0);
				//_lastSlideDir = _moveInput.x > 0 ? Vector2.right : Vector2.left;
			//_lastDashDir = _moveInput;
			else
				_lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;
				//_lastDashDir = Vector2.up;



			IsDashing = true;
			IsSliding = false;
			IsJumping = false;
			IsWallJumping = false;
			_isJumpCut = false;

			StartCoroutine(nameof(StartDash), _lastDashDir);
		}
        #endregion
		/*
        #region SLIDE CHECKS
        if (CanSlide() && LastPressedSlideTime > 0)
        {
            //Freeze game for split second. Adds juiciness and a bit of forgiveness over directional input
            Sleep(Data.slideSleepTime);

            //If not direction pressed, dash forward
            if (_moveInput != Vector2.zero)
                _lastSlideDir = _moveInput.x > 0 ? Vector2.right + Vector2.down : Vector2.left + Vector2.down;
            else
                _lastSlideDir = IsFacingRight ? Vector2.right + Vector2.down : Vector2.left + Vector2.down;



            IsSliding = true;
			IsDashing = false;
            IsJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;

            StartCoroutine(nameof(StartSlide), _lastSlideDir);
        }
        #endregion
		*/

        #region Wall Climb CHECKS
        if (!slideStuck && CanWallClimb() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
			IsWallClimbing = true;
		else
			IsWallClimbing = false;
		#endregion

		#region GRAVITY
		if (!_isDashAttacking && !_isSlideAttacking)
		{
			//Higher gravity if we've released the jump input or are falling
			if (IsWallClimbing || IsWindup)
			{
				SetGravityScale(0);
			}
			else if (IsGrappling)
			{
				SetGravityScale(1);
			}
			else if (RB.velocity.y < 0 && _moveInput.y < 0)
			{
				//Much higher gravity if holding down
				SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
				//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
			}
			else if (_isJumpCut)
			{
				//Higher gravity if jump button released
				SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
			}
			else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
			{
				SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
			}
			else if (RB.velocity.y < 0)
			{
				//Higher gravity if falling
				SetGravityScale(Data.gravityScale * Data.fallGravityMult);
				//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
			}
			else
			{
				//Default gravity if standing on a platform or moving upwards
				SetGravityScale(Data.gravityScale);
			}
		}
		else
		{
			//No gravity when dashing (returns to normal once initial dashAttack phase over)
			SetGravityScale(0);
		}
		#endregion

		// visual info

		// test if player lands on ground
	//	if (JumpTestTime < 0 && LastOnGroundTime > JumpTestTime && JumpSoundCoolDown < 0)
	//	{
		//	AudioManager.Instance.PlaySound("Fall_Land", _fallVolumeMult);
	//		JumpSoundCoolDown = _jumpSoundCoolDown;
	//	}
        JumpTestTime = LastOnGroundTime;



        animator.SetBool("IsMoving", Mathf.Abs(_moveInput.x) > 0.1f);
		animator.SetBool("IsWallSliding", IsWallClimbing && !_wallClimbUnlocked);
        animator.SetBool("IsWallClimbing", IsWallClimbing && _wallClimbUnlocked);
		animator.SetBool("IsWallJumping", IsWallJumping);
		animator.SetBool("IsSliding", IsSliding);
		animator.SetBool("IsDashing", IsDashing);
		animator.SetBool("IsJumpFalling", _isJumpFalling);
        animator.SetBool("IsFalling", RB.velocity.y < -0.01f && LastOnGroundTime < 0);
		animator.SetBool("IsInteracting", IsInteracting);
		animator.SetBool("SlideStuck", slideStuck);
		animator.SetLayerWeight(1, IFrames > 0 ? 0.5f : 0);
		spriteRenderer.material = IFrames > 0 ? iFrameMaterial : baseMaterial;
		if (Mathf.Abs(RB.velocity.y) < 0.01) animator.SetBool("Jumping", false);
		if (_isJumpFalling) animator.SetBool("Jumping", false);

		// hide the aim cursor if aim is not moving for more than x amount of time
		
		if (slideStuck)
		{
            if (!Physics2D.OverlapBox(_slideCheckPoint.position, _slideCheckSize, 0, _groundLayer))
            {
                slideStuck = false;
				defaultCollider.enabled = true;
				slideCollider.enabled = false;
            }
        }

    }

    // for some reason this doesn't work properly
    // i suspect it has something to do with the sprite already having alpha
    public IEnumerator fadeSprite(SpriteRenderer sr, float endValue, float duration)
    {
        float elapsedTime = 0;
        float startValue = sr.color.a;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, newAlpha);
            yield return null;
        }
    }

    private IEnumerator StartAttack()
    {

		if (Data.maxCombo > Data.comboInfo.Length)
		{
			Debug.LogError("Player max combo is greater than combo data");
			Data.maxCombo = Data.comboInfo.Length;
		}

		// set current combo state
		if (ComboTime > 0) { _comboState++; }
		else _comboState = 1;

		// loop combo
		
		if (_comboState > Data.maxCombo) _comboState = 1;
        if (_comboState > Data.comboInfo.Length) _comboState = 0;

        // exit if no combo
        if (_comboState == 0)
		{
			Debug.LogError("Player is Missing Combo Data");
			IsWindup = false;
            IsInteracting = false;
            yield break;
		}

        animator.SetBool("IsAttack" + _comboState, true);
		RB.velocity = RB.velocity * 0.25f;
		MeleeComboInfo combo = Data.comboInfo[_comboState - 1];
		//AudioManager.Instance.PlaySound(combo.soundName, _swordVolumeMult);

		if (_aimDirection.x < 0 && IsFacingRight && _cursorVisible) Turn(true);
		else if (_aimDirection.x > 0 && !IsFacingRight && _cursorVisible) Turn(true);

        yield return new WaitForSeconds(combo.windupFrames / 60f);
        IsWindup = false;
		// get damagable entities within hitbox
		// rotates hitbox based on facing direction
		Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position + (Vector3)(IsFacingRight ? combo.hitBoxOffset : -combo.hitBoxOffset), combo.hitBoxSize, 0);
		foreach (Collider2D collider in colliders)
		{
			Damagable damageable = collider.GetComponent<Damagable>();
			if (damageable != null)
			{
				damageable.DamageDealt(combo.damage, this.gameObject, combo.knockbackForce);
			}
		}

		RB.velocity = RB.velocity * 2; 
        yield return new WaitForSeconds(combo.swingFrames / 60f);
		animator.SetBool("IsAttack" + _comboState, false);
        IsInteracting = false;
		AttackCooldownTime = Data.attackCooldown;
		ComboTime = Data.comboTime;
    }

    public Transform GetClosestTransform(List<Transform> targets)
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Transform t in targets)
        {
            float dist = Vector3.Distance(t.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }

    private void FixedUpdate()
    {

        //Handle Run
        if (!IsDashing && !IsSliding)
        {
            if (IsWallJumping)
                Run(Data.wallJumpRunLerp);
            else
                Run(1);
        }
        else if (_isSlideAttacking)
        {
            Run(Data.slideEndRunLerp);
        }
		else if (_isDashAttacking)
		{
			Run(Data.dashEndRunLerp);
		}

		//Handle Slide
		if (IsWallClimbing)
			WallClimb();
    }

    #region INPUT CALLBACKS
	//Methods which whandle input detected in Update()
    public void OnJumpInput()
	{
		LastPressedJumpTime = Data.jumpInputBufferTime;
	}

	public void OnJumpUpInput()
	{
		if (CanJumpCut() || CanWallJumpCut())
			_isJumpCut = true;
	}

	public void OnDashInput()
	{
		if (IsJumping || _isJumpFalling) LastPressedDashTime = Data.dashInputBufferTime;
	}

    public void OnSlideInput()
    {
        LastPressedSlideTime = Data.slideInputBufferTime;
    }
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}

	private void Sleep(float duration)
    {
		//Method used so we don't need to call StartCoroutine everywhere
		//nameof() notation means we don't need to input a string directly.
		//Removes chance of spelling mistakes and will improve error messages if any
		StartCoroutine(nameof(PerformSleep), duration);
    }

	private IEnumerator PerformSleep(float duration)
    {
		Time.timeScale = 0;
		yield return new WaitForSecondsRealtime(duration); //Must be Realtime since timeScale with be 0 
		Time.timeScale = 1;
	}
    #endregion

	//MOVEMENT METHODS
    #region RUN METHODS
    private void Run(float lerpAmount)
	{
		if (IsWindup || IsGrappling) return;
		//Calculate the direction we want to move in and our desired velocity
		float targetSpeed = _moveInput.x * Data.runMaxSpeed;


        //We can reduce are control using Lerp() this smooths changes to are direction and speed
        targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);


		#region Calculate AccelRate
		float accelRate;

		//Gets an acceleration value based on if we are accelerating (includes turning) 
		//or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
		if (LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
		#endregion

		#region Add Bonus Jump Apex Acceleration
		//Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
		if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
		{
			accelRate *= Data.jumpHangAccelerationMult;
			targetSpeed *= Data.jumpHangMaxSpeedMult;
		}
		#endregion

		#region Conserve Momentum
		//We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
		if(Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
		{
			//Prevent any deceleration from happening, or in other words conserve are current momentum
			//You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
			accelRate = 0; 
		}
		#endregion

		//Calculate difference between current velocity and desired velocity
		float speedDif = targetSpeed - RB.velocity.x;
		//Calculate force along x-axis to apply to thr player

		float movement = speedDif * accelRate;

		//Convert this to a vector and apply to rigidbody
		RB.AddForce(movement * Vector2.right, ForceMode2D.Force);

		/*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/

		// play step sound
	//	if (LastOnGroundTime > 0 && StepTime< 0 && Mathf.Abs(_moveInput.x) > 0.3f)
	//	{
			//AudioManager.Instance.PlaySound("Step", _stepVolumeMult);
     //       StepTime = _stepCoolDown;
	//	}
	}

	private void Turn(bool overrideInteract = false)
	{
		if (IsInteracting && !overrideInteract) return;
		//stores scale and flips the player along the x axis, 
		Vector3 scale = transform.localScale; 
		scale.x *= -1;
		transform.localScale = scale;

		IsFacingRight = !IsFacingRight;
	}
    #endregion

    #region JUMP METHODS
    private void Jump()
	{
		animator.SetBool("Jumping", true);
		//AudioManager.Instance.PlaySound("Jump", _jumpVolumeMult);

		//Ensures we can't call Jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;

		#region Perform Jump
		//We increase the force applied if we are falling
		//This means we'll always feel like we jump the same amount 
		//(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
		float force = Data.jumpForce;
		if (RB.velocity.y < 0)
			force -= RB.velocity.y;

		RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		#endregion
	}

	private void WallJump(int dir)
	{
        //AudioManager.Instance.PlaySound("Jump", _jumpVolumeMult);
        //Ensures we can't call Wall Jump multiple times from one press
        LastPressedJumpTime = 0;
		LastOnGroundTime = 0;
		LastOnWallRightTime = 0;
		LastOnWallLeftTime = 0;

		#region Perform Wall Jump
		Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
		force.x *= dir; //apply force in opposite direction of wall

		if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
			force.x -= RB.velocity.x;

		if (RB.velocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
			force.y -= RB.velocity.y;

		//Unlike in the run we want to use the Impulse mode.
		//The default mode will apply are force instantly ignoring masss
		RB.AddForce(force, ForceMode2D.Impulse);
		#endregion
	}
	#endregion

	#region DASH METHODS
	//Dash Coroutine
	private IEnumerator StartDash(Vector2 dir)
	{
        //AudioManager.Instance.PlaySound("Dash", _dashVolumeMult);
        //Overall this method of dashing aims to mimic Celeste, if you're looking for
        // a more physics-based approach try a method similar to that used in the jump

        LastOnGroundTime = 0;
		LastPressedDashTime = 0;

		float startTime = Time.time;

		_dashesLeft--;
		_isDashAttacking = true;

		SetGravityScale(0);

		//We keep the player's velocity at the dash speed during the "attack" phase (in celeste the first 0.15s)
		while (Time.time - startTime <= Data.dashAttackTime)
		{
			RB.velocity = dir.normalized * Data.dashSpeed;
			//Pauses the loop until the next frame, creating something of a Update loop. 
			//This is a cleaner implementation opposed to multiple timers and this coroutine approach is actually what is used in Celeste :D
			yield return null;
		}

		startTime = Time.time;

		_isDashAttacking = false;

		//Begins the "end" of our dash where we return some control to the player but still limit run acceleration (see Update() and Run())
		SetGravityScale(Data.gravityScale);
		RB.velocity = Data.dashEndSpeed * dir.normalized;

		while (Time.time - startTime <= Data.dashEndTime)
		{
			yield return null;
		}

		//Dash over
		IsDashing = false;
	}

	//Short period before the player is able to dash again
	private IEnumerator RefillDash(int amount)
	{
		//SHoet cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
		_dashRefilling = true;
		yield return new WaitForSeconds(Data.dashRefillTime);
		_dashRefilling = false;
		_dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
	}
    #endregion

    #region DASH METHODS
    //Dash Coroutine
    private IEnumerator StartSlide(Vector2 dir)
    {
        //AudioManager.Instance.PlaySound("Slide", _dashVolumeMult);
        //Overall this method of dashing aims to mimic Celeste, if you're looking for
        // a more physics-based approach try a method similar to that used in the jump

        LastOnGroundTime = 0;
        LastPressedSlideTime = 0;

        float startTime = Time.time;

        _slidesLeft--;
		_isSlideAttacking = true;

        SetGravityScale(0);

		
		slideCollider.enabled = true;
        defaultCollider.enabled = false;

        //We keep the player's velocity at the dash speed during the "attack" phase (in celeste the first 0.15s)
        while (Time.time - startTime <= Data.slideAttackTime)
        {
            RB.velocity = dir.normalized * Data.slideSpeed;
            //Pauses the loop until the next frame, creating something of a Update loop. 
            //This is a cleaner implementation opposed to multiple timers and this coroutine approach is actually what is used in Celeste :D
            yield return null;
        }

        startTime = Time.time;

        _isSlideAttacking = false;

        //Begins the "end" of our dash where we return some control to the player but still limit run acceleration (see Update() and Run())
        SetGravityScale(Data.gravityScale);
        RB.velocity = Data.slideEndSpeed * dir.normalized;

        while (Time.time - startTime <= Data.slideEndTime)
        {
            yield return null;
        }

		Collider2D coll = Physics2D.OverlapBox(_slideCheckPoint.position, _slideCheckSize, 0, _groundLayer);
		bool isStuck = false;
		if (coll != null)
		{
			isStuck = !coll.isTrigger;
			PlatformEffector2D plat = coll.GetComponent<PlatformEffector2D>();
			if (plat && ((plat.enabled && plat.rotationalOffset == 0) || !plat.enabled))
			{
				isStuck = false;
			}
		}

		if (isStuck)
		{
			slideStuck = true;
		}
		else
		{
            defaultCollider.enabled = true;
            slideCollider.enabled = false;
        }

        //Dash over
        IsSliding = false;
    }

    //Short period before the player is able to dash again
    private IEnumerator RefillSlide(int amount)
    {
        //SHoet cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
        _slideRefilling = true;
        yield return new WaitForSeconds(Data.slideRefillTime);
        _slideRefilling = false;
        _slidesLeft = Mathf.Min(Data.slideAmount, _slidesLeft + 1);
    }
    #endregion

    #region OTHER MOVEMENT METHODS
    private void WallClimb()
	{
		//Works the same as the Run but only in the y-axis
		//THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
		float speedDif; 
		float movement;

		if (_wallClimbUnlocked)
		{
            speedDif = Data.wallClimbSpeed - RB.velocity.y;
            movement = speedDif * Data.wallClimbAccel;
        }
        else
		{
            speedDif = Data.wallSlideSpeed - RB.velocity.y;
            movement = speedDif * Data.wallSlideAccel;
        }
        
        //So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
        //The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif)  * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

		RB.AddForce(movement * Vector2.up);
		if (UserInput.instance.SprintHeld && _wallHoldUnlocked)
		{
            RB.velocity = new Vector2(RB.velocity.x, Mathf.Clamp(RB.velocity.y, 0, 0));
        } else if (!_wallClimbUnlocked)
		{
			RB.velocity = new Vector2(RB.velocity.x, Mathf.Clamp(RB.velocity.y, -Data.maxFastFallSpeed, -Data.minimumWallSlideSpeed));
		}
	}
    #endregion


    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != IsFacingRight)
			Turn();
	}

    private bool CanJump()
    {
		return LastOnGroundTime > 0 && !IsJumping;
    }

	private bool CanWallJump()
    {
		if (!_wallJumpUnlocked) return false;
		return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
			 (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
	}

	private bool CanJumpCut()
    {
		return IsJumping && RB.velocity.y > 0;
    }

	private bool CanWallJumpCut()
	{
		return IsWallJumping && RB.velocity.y > 0;
	}

	private bool CanDash()
	{
		if (!_dashUnlocked) return false;
		if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
		{
			StartCoroutine(nameof(RefillDash), 1);
		}

		return _dashesLeft > 0;
	}

    private bool CanSlide()
    {
        if (!_slideUnlocked) return false;
        if (!IsSliding && _slidesLeft < Data.slideAmount && LastOnGroundTime > 0 && !_slideRefilling)
        {
            StartCoroutine(nameof(RefillSlide), 1);
        }

        return _slidesLeft > 0 && LastOnGroundTime > 0;
    }

    public bool CanWallClimb()
    {
		if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && !IsSliding && LastOnGroundTime <= 0 && Mathf.Abs(_moveInput.x) > 0.01) 
			return true;
		else
			return false;
	}
    #endregion


    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
		Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
		Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(_slideCheckPoint.position, _slideCheckSize);
        Gizmos.color = Color.yellow;
		Gizmos.DrawLine(_groundCheckPoint.position + new Vector3(_groundCheckSize.x * 0.5f, 0, 0), _groundCheckPoint.position + new Vector3(_groundCheckSize.x * 0.5f, -_groundCheckSize.y, 0));
        Gizmos.DrawLine(_groundCheckPoint.position - new Vector3(_groundCheckSize.x * 0.5f, 0, 0), _groundCheckPoint.position - new Vector3(_groundCheckSize.x * 0.5f, _groundCheckSize.y, 0));
        

        Gizmos.color = Color.red;
		foreach (MeleeComboInfo combo in Data.comboInfo)
		{
            Gizmos.DrawWireCube(transform.position + (Vector3)(IsFacingRight ? combo.hitBoxOffset : new Vector2(-combo.hitBoxOffset.x, combo.hitBoxOffset.y)), combo.hitBoxSize);
        }
    }


	public void Respawn(int damage = 0)
	{
		// implement damage later
		transform.position = LastStablePosition;
	}



    public void DealDamage(int damage, Vector2? force = null)
	{
        if (IFrames > 0) return;
		StartCoroutine(nameof(ProcessDamage), new Tuple<int, Vector2?>(damage, force));
		if (CurrentHealth <= 0) StartCoroutine(nameof(ProcessDeath));
    }

    private IEnumerator ProcessDeath()
    {
		if (IsDying) yield break;
		IsDying = true;
		animator.SetBool("IsDying", true);
		//AudioManager.Instance.PlaySound("Death_Player");
		RB.velocity = Vector2.zero;
		PlayerInput input = GetComponent<PlayerInput>();
		input.DeactivateInput();
		yield return new WaitForSeconds(72/60f);
        animator.SetBool("IsDying", false);
		animator.enabled = false;
        yield return new WaitForSeconds(1);
        AsyncOperation op = SceneManager.LoadSceneAsync(0);
		while(!op.isDone) yield return null;
		this.transform.position = FindObjectOfType<LevelManager>().getSpawnPoint("Spawn").transform.position;
        yield return null;
		input.ActivateInput();
		CurrentHealth = _maxHealth;
        animator.enabled = true;
		IsDying = false;
    }

    public IEnumerator ProcessDamage(Tuple<int, Vector2?> args)
    {
		if (IsDying) yield break;
		//AudioManager.Instance.PlaySound("Damage_Player", _damageVolumeMult);
		animator.SetBool("IsTakingDamage", true);
		spriteRenderer.color = new Color(1f, 0.4f, 0.4f);
        IFrames = Data.damageTakenIFrames;
        Vector2 actualForce = args.Item2 == null ? Vector2.zero : (Vector2)args.Item2;
		CurrentHealth -= args.Item1;
		Utility.AddForceDirection(actualForce, RB, Data.maxFastFallSpeed, Data.dashEndRunLerp, Data.runAccelAmount);
		yield return new WaitForSeconds(12/60f);
		spriteRenderer.color = Color.white;
        animator.SetBool("IsTakingDamage", false);
    }

    public void Heal(int healAmount)
    {
		int pastHealth = CurrentHealth;
		CurrentHealth += healAmount;
		if (CurrentHealth > _maxHealth) CurrentHealth = _maxHealth;
       // if (pastHealth < CurrentHealth) AudioManager.Instance.PlaySound("Heal", _healSoundMult);
    }
}

// created by Dawnosaur :D modified to hell and back by Erin Heathcote
#endregion