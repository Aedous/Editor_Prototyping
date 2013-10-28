using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

public class Hero : Character
{

    #region Inspector Variables
    public GameObject projectileObject; //The Projectile object.
    public float WallJumpSpeed; //How far the jump moves
    public float WallJumpTime; //How long before controls are given back to user.
    public float WallClingDrag; //How much friction on the wall
    public float WallClingTime; //How much time has to pass before player slowly slides down.
    public float WallJumpHeight; //Wall Jump Height
    public float DashTime; //How long the dash last's for
    public float DashSpeed; //How far the dash moves
    public float JumpIncrease; //This variable holds the amount we add to the jump until it's at max
    public float JumpPressDownResponseSpeed; //Dampning the jump button pressed.


    
    #endregion

    #region Public Variables
    public bool WallCling { get; set; } //Used to find out if we are wall clinging or not.
    public bool WallJump { get; set; }
    public float LastMovement { get; set; }
    public bool IsDashing { get; set; } //Dashing parameter to switch dashing off and on 

    //Button variables for input
    public bool JumpPressed { get; set; }
    public float InterpolatingJumpHeight { get; set; } //This variable is used to track variable jumping
    //This variable alters how fast information is taking for the variable jumping, the count is abit too quick without this
    //public float JumpPressDownResponseSpeed { get; set; } 
    #endregion

    #region Private Variables

    #endregion

    #region Unity Methods
    public void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        base.Start();
        WallJump = false;
        JumpPressed = false;
        InterpolatingJumpHeight = 0;// Start's at 0 and increase until we reach max jump height

        //Control the position of the projectile start point
        ControlProjectilePosition();
    }

    public void FixedUpdate()
    {
        base.FixedUpdate();

        //Do wall jump update
        if (WallJump)
        {
            Vector3 moveDirection = new Vector3(LastMovement * (MovementSpeed * -WallJumpSpeed), 0, 0);
            rigidbody.AddForce(moveDirection, ForceMode.Acceleration);
            CharacterJump(WallJumpHeight, ForceMode.VelocityChange);
            UseCustomGravity = false;
            rigidbody.drag = DefaultDrag;
        }

        //Apply dashing in fixed update
        Dash(DashSpeed, -Direction, ForceMode.VelocityChange);
    }

    public void Update()
    {
        base.Update();

        PlayerInput(); //Activate player input /also handles if we allowed to move

       
    }
    #endregion

    #region Custom Methods

    public void ControlProjectilePosition()
    {
        int currentDirection = Direction;

        Transform bulletPositionObject = transform.FindChild("ProjectilePositionObject");

        //Set the tweaked position to zero
        Vector3 tweakedPosition = Vector3.zero;

        //Get the local position of the object, so that it is relevant to the parent
        Vector3 updatedposition = bulletPositionObject.transform.localPosition;
        updatedposition = new Vector3(updatedposition.x * Direction, updatedposition.y, updatedposition.z);

        Debug.Log("Updated Local Position: " + updatedposition);
        Debug.Log("Updated World Position: " + bulletPositionObject.transform.position);

        
        
        if (Movement > 0)
        {
            
            //Get Vector 3 and log to console
            Debug.Log("Current Position: " + updatedposition);
            //Set the direction to be to the right of the object
            if (bulletPositionObject.transform.localPosition.x < 0)
            {
                //Change Position to match the updated position
                bulletPositionObject.transform.localPosition = updatedposition;
            }
            
        }
        else if (Movement < 0)
        {
            //Set the direction to be to the right of the object
            if (bulletPositionObject.transform.localPosition.x > 0)
            {
                //Change Position to match the updated position
                bulletPositionObject.transform.localPosition = updatedposition;
            }

            //bulletPositionObject.transform.position = new Vector3(updatedposition.x, updatedposition.y, updatedposition.z);
        }
    }

    public void PlayerInput()
    {

        //This script handles the player input.
        Movement = Input.GetAxisRaw("Horizontal");

        ControlProjectilePosition();

        if (MovementAllowed)
        {
            //Movement = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump"))
            {
                //If we are allowed to jump, set the jump press to true.
                if (OnFloor) //Aslong as we are on the floor
                {
                    JumpPressed = true;
                }
            }
            if (Input.GetButton("Jump"))
            {
                if (OnFloor)
                {
                    if (InterpolatingJumpHeight < JumpHeight)
                    {
                        JumpPressed = true; //Aslong as we are holding down the button
                        //UseCustomGravity = false;
                    }
                }
                //If we are holding down the jump button and we have started the jump
                if (JumpPressed)
                {
                    OnFloor = false; //We have to be off the floor
                    //Set minimum jump height to stop character from being stuck on the floor.
                    if (InterpolatingJumpHeight < 1.5f)
                    {
                        InterpolatingJumpHeight = 1.5f;
                    }
                    //Start adding to the jump height
                    if (InterpolatingJumpHeight < JumpHeight)
                    {
                        float nextValue = InterpolatingJumpHeight + (JumpIncrease * (Time.deltaTime * JumpPressDownResponseSpeed));
                        if (nextValue >= JumpHeight)
                        {
                            InterpolatingJumpHeight = JumpHeight; //Set back to 0 because we are at max height
                            //UseCustomGravity = true;
                        }
                        else
                        {
                            InterpolatingJumpHeight = nextValue;
                        }
                    }
                    else if (InterpolatingJumpHeight >= JumpHeight)
                    {
                        JumpPressed = false;
                        InterpolatingJumpHeight = 0;
                        //UseCustomGravity = true;
                    }
                }
            }
            if (Input.GetButtonUp("Jump"))
            {
                //Once we release remove the jump press variable *even though it's reset once the character jumps.
                JumpPressed = false;
                InterpolatingJumpHeight = 0; //Reset the interpolation
            }

            //Dashing input
            if (Input.GetButtonDown("Fire1"))
            {
                SetDashingAttributes(Direction);
                JumpPressed = false;
                InterpolatingJumpHeight = 0;
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, 0);         
            }

            //Shooting Input
            if (Input.GetButtonDown("Fire2"))
            {
                //Make sure we have a projectile attached
                if (projectileObject != null)
                {
                    //Get the shooting position (Probably only need to do this once
                    Transform bulletPositionObject = transform.FindChild("ProjectilePositionObject");

                    //Get the position of the object aslong as it exists
                    if (bulletPositionObject != null)
                    {
                        Vector3 newposition = bulletPositionObject.transform.localPosition;
                        Vector3 worldposition = transform.position + newposition;
                        //Create the projectile object and fire it in the direction we are facing
                        ShootProjectile(Direction, newposition);
                    }
                    else
                    {
                        //The child object ProjectilePositionObject does not exist
                        Debug.Log("ProjectilePositionObject does not exist");
                    }
                }
            }
        }
    }

    public void ActivateWallCling()
    {
        //If there is a collision on the left or right, but no collision on the bottom
        //And character has to be falling
        //movement = Input.GetAxisRaw("Horizontal");

        if (Input.GetButton("Horizontal"))
        {
            if (Movement > 0) //If moving to right
            {
                if (CollisionRight && !CollisionBottom && currentState == CharacterState.Falling)
                {
                    //Init variables for wall cling
                    SetWallClingAttributes(1); //Face the left
                }
                else
                {
                    //Remove wall cling if there are no collisions to the side we are looking
                    WallCling = false;

                    //Restore Gravity so that player does not slide in the air because of short cling to wall
                    UseCustomGravity = true;

                }

            }
            else if (Movement < 0) //If moving to the left
            {
                if (CollisionLeft && !CollisionBottom && currentState == CharacterState.Falling)
                {
                    //Init variables for wall clinging
                    SetWallClingAttributes(-1); //Face the right
                }
                else
                {
                    //Remove wall cling if there are no collisions to the side we are looking
                    WallCling = false;

                    //Restore Gravity so that player does not slide in the air because of short cling to wall
                    UseCustomGravity = true;
                }
            }
        }
        else
        {
            //Stop wall clinging
            WallCling = false;
        }

        if (WallCling)
        {
            rigidbody.drag = WallClingDrag;
        }
        else
        {
            rigidbody.drag = DefaultDrag;
        }
    }
     
    //This functoin will set all the attributes to start a wall cling on the hero, It simply set's the variables
    public void SetWallClingAttributes(int direction)
    {
        currentState = CharacterState.WallCling;
        WallCling = true;
        //Deactivate Gravity for a couple milliseconds to simulate small cling
        UseCustomGravity = false;

        //Start Coroutine to turn the gravity back on, so that the character slowly slides down again
        StartCoroutine(WallClingTimer(WallClingTime));

        //Activate wall jump by button input
        if (Input.GetButtonDown("Jump"))
        {
            //Vector3 moveDirection = new Vector3(movement * (MovementSpeed * -0.3f), 0, 0);
            //rigidbody.AddForce(moveDirection, ForceMode.VelocityChange);
            //CharacterJump(WallJumpHeight);
            WallJump = true;
            AnimationControls.SetAnimationDirection(direction); //Makes sure the sprite is facing the correct direction when jumping
            Direction = direction; //Set the direction on our character to the direction we are jumping
            LastMovement = Movement;
            MovementAllowed = false;
            StartCoroutine(WallJumpTimer(WallJumpTime));

        }
    }

    //Initialize the dashing attributes to start the dash
    public void SetDashingAttributes(int direction)
    {
        IsDashing = true;
        //Deactivate Gravity for a couple milliseconds to simulate small cling
        UseCustomGravity = false;
        MovementAllowed = false; //Stop Movement
        AnimationControls.SetAnimationDirection(direction);
        LastMovement = Movement;
        currentState = CharacterState.Dashing;

        //Start Coroutine to turn the gravity back on, so that the character drops down, and turn dashing parameter to off to stop dash
        StartCoroutine(DashTimer(DashTime));

    }

    public void Dash(float dashspeed,int direction, ForceMode mode)
    {
        if (IsDashing)
        {
            //Debug.Log("Dashing");
            //Dash in the direction we are facing
            UseCustomGravity = false;
            Vector3 moveDirection = new Vector3(dashspeed * direction, 0, 0);
            rigidbody.AddForce(moveDirection, mode);
            //OnFloor = false;
            //currentState = CharacterState.Jumping;
        }
    }

    public void ShootProjectile(int currentdirection, Vector3 position)
    {
        //This creates the projectile in the position and applies the direction correctly

        //Tweak the direction sent in so that it all fits with the projectile correctly
        //int tweakeddirection = currentdirection * -1;

        //Create the projectile, and apply the direction
        //position = new Vector3(position.x * tweakeddirection, position.y, position.z);
        Vector3 createdPosition = position + transform.position;
        GameObject createdProjectile = (GameObject)Instantiate(projectileObject, createdPosition, Quaternion.identity);
        
        //Set the direction of the created projecitle with the script
        /*Projectile projectilescript = createdProjectile.GetComponent<Projectile>(); //Get the script
        if (projectilescript)
        {
            //projectilescript.SetProjectileDirection(tweakeddirection); // Apply the direction
        }*/
    }

    public override void CharacterMovement()
    {
        Vector3 moveDirection = new Vector3(Movement * MovementSpeed, 0, 0);

 
        rigidbody.AddForce(moveDirection, ForceMode.Acceleration);
        

        //Change Direction
        if (Movement > 0) //Moving to the right
        {
                Direction = -1; //Character Animation faces right
                AnimationControls.SetAnimationDirection(Direction);

            if (currentState != CharacterState.Jumping && currentState != CharacterState.Falling && currentState != CharacterState.WallCling)
            {
                currentState = CharacterState.Moving;
            }
        }
        else if (Movement < 0)
        {
                Direction = 1; //Character Animation faces left
                AnimationControls.SetAnimationDirection(Direction);

            if (currentState != CharacterState.Jumping && currentState != CharacterState.Falling && currentState != CharacterState.WallCling)
            {
                currentState = CharacterState.Moving;
            }
        }
        else
        {
            //Change animation if we are not moving
            //AnimationControls.SetAnimation("Walk", 0);
            if (currentState != CharacterState.Jumping && currentState != CharacterState.Falling)
            {
                if (OnFloor) //If we are not moving but are on the floor
                {
                    currentState = CharacterState.Idle;
                }
            }
        }

        if (JumpPressed)
        {
            //Variable jumping, and also input should not be in fixed update
            if (InterpolatingJumpHeight < JumpHeight)
            {
                CharacterJump(InterpolatingJumpHeight, ForceMode.VelocityChange);
                
            }
            else
            {
                JumpPressed = false; //Stop the character from flying.
            }

            //JumpPressed = false;
        }

        //Wall Clinging
        ActivateWallCling();

        

    }

    public override void ApplyGravity()
    {
        if (UseCustomGravity)
        {
            //If the character is not on the floor, apply gravity
            if (!OnFloor)
            {
                //Apply Gravity
                //rigidbody.velocity = new Vector3(rigidbody.velocity.x, -Gravity, 0);

                Vector3 moveDirection = new Vector3(0, -Gravity, 0);

                if (rigidbody.velocity.y < 0.8f) //In falling state, apply gravity.
                {
                    rigidbody.AddForce(moveDirection, ForceMode.VelocityChange);
                    currentState = CharacterState.Falling;
                }
            }
            else
            {

            }
        }

    }

    public override void AnimationHandler()
    {
        base.AnimationHandler();
        //This function handles the animation states.
        switch (currentState)
        {
            case CharacterState.Dashing:
                if (AnimationControls.CurrentAnimation != "Dashing")
                {
                    AnimationControls.SetAnimation("Dashing", 0);
                }
                break;
            case CharacterState.WallCling:
                if (AnimationControls.CurrentAnimation != "WallCling")
                {
                    AnimationControls.SetAnimation("WallCling", 0);
                }
                break;
        }
    }

    //This timer is used to track how long a wall jump occurs and then to return movement to the user.
    public IEnumerator WallJumpTimer(float time)
    {
        yield return new WaitForSeconds(time);

        //After timer is over. check which direction we are facing especially if it's the opposite direction via input,
        //to stop the flip animation bug in when giving control back to player
        int correctDirection = Direction * -1; //Flip the current direction to compare movement properly
        if (Movement != correctDirection)
        {
            //Our input direction is different, change the direction immediately
            AnimationControls.SetAnimationDirection((int)Movement);
            Direction = (int)Movement;
        }
        
        //Restore states
        MovementAllowed = true;
        WallJump = false;
        UseCustomGravity = true;

    }

    //This function will give the character a slight cling on to the wall before it start's to slide down,
    //Gravity is switched off for a short while and then switched back on.
    public IEnumerator WallClingTimer(float time)
    {
        LastMovement = LastMovement * -1;
        yield return new WaitForSeconds(time);
        UseCustomGravity = true;
    }

    //This function controls how long the dash last's for
    public IEnumerator DashTimer(float time)
    {
        yield return new WaitForSeconds(time);
        MovementAllowed = true;
        UseCustomGravity = true;
        IsDashing = false;

        //Need to put if statement..
        ResetState();
    }

    #endregion

    #region Interface Methods
    #endregion

}

