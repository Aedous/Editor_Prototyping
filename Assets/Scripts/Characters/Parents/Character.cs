using UnityEngine;
using System.Collections;

/*This class is the parent of every character in the game, it has most of the basic functionality for each
 *character built into it. 
 *Each individual character that inherits from this, can also create their own individual patterns.
 */

public class Character : MonoBehaviour
{
    #region Enums and States
    public enum CharacterState
    { 
        Idle, Attacking, Hit, Dead, Stunned, Moving, Jumping, Falling, Dashing, WallCling
    }
    #endregion

    #region Variables
    //Inspector variables
    public int DefaultDrag; //Drag amount which relates to friction on the character.
    public string CharacterName;
    public float Health, AttackTime, Damage, Defence, Gravity, MovementSpeed, JumpHeight, SlideAmount;
    public bool UseCustomGravity;
    public CharacterState currentState;
    public float wallCastOffset;

    //Raycast direction and length
    public float down_cast, downleft_cast, downright_cast, up_cast, left_cast, right_cast, leftanglecast_gap, rightanglecast_gap; //Line lengths for the raycast
    
    
    //CurrentWeakness - Implement later when a structure is figured out for weakness

    //Public variables
    public float Movement { get; set; } //This variable controls the movement horizontal
    public bool MovementAllowed { get; set; }//Allows user control
    public bool CanInteract { get; set; } //This variable is used to make sure the character can carry out an action
    public bool OnFloor { get; set; }
    public Character Instance { get; set; }
    public int Direction { get; set; } //Character Direction : 1 - Left, -1 - Right

    //This is to help the collision for platform physics.
    /*public Ray BottomRay { get; set; }
    public Ray TopRay { get; set; }
    public Ray LeftRay { get; set; }
    public Ray RightRay { get; set; }*/

    //Boolean variables for top bottom left and right.
    public bool CollisionBottom { get; set; }
    public bool CollisionTop { get; set; }
    public bool CollisionRight { get; set; }
    public bool CollisionLeft { get; set; }
    public bool CollisionBottomLeft { get; set; }
    public bool CollisionBottomRight { get; set; }

    //Collider variables
    public BoxCollider Collision { get; set; } //Collision for the character
    public ContactPoint Contact { get; set; }
    public string CollisionInformation { get; set; }

    //Animation Variables
    public AnimationController AnimationControls;

    //Private variables

    #endregion


    #region MonoBehaviours
    //Debug Gizmos
    void OnDrawGizmosSelected()
    {
        Vector3 collisionShape = gameObject.collider.bounds.size; //Get the size of the collision box and store in a variable

        //Get the width and height of the collision box
        float collisionShapeWidth = collisionShape.x;
        float collisionShapeHeight = collisionShape.y;

        //Cast ray downwards
        //Draw 3 lines at bottom of sprite all facing down to check for floor collisions.
        Vector3 endPosition = CalculateVerticalLineOfSightRays(collisionShapeHeight, down_cast, gameObject.collider.bounds.center, -1);

        //Draw lines to show where extra rays will be cast
        //Calculate offset for adjustment
        Vector3 downoffset = new Vector3(collisionShapeWidth/2 - downleft_cast, 0, 0); //The offset so we can change how far apart the left and right collisions are.
        Debug.DrawLine(gameObject.collider.bounds.center - downoffset, endPosition - downoffset, Color.yellow); //Cast ray downwards
        Debug.DrawLine(gameObject.collider.bounds.center, endPosition, Color.white); //Cast ray downwards

        downoffset = new Vector3(collisionShapeWidth/2 - downright_cast, 0, 0); //The offset so we can change how far apart the left and right collisions are.
        Debug.DrawLine(gameObject.collider.bounds.center + downoffset, endPosition + downoffset, Color.red); //Cast ray 

        //Cast ray upwards
        endPosition = CalculateVerticalLineOfSightRays(collisionShapeHeight, up_cast, gameObject.collider.bounds.center, 1);
        //Draw lines to show where extra rays will be cast
        Debug.DrawLine(gameObject.collider.bounds.center, endPosition, Color.green); //Cast ray upwards

        //Cast ray left
        Vector3 centerOffset = new Vector3(0, wallCastOffset, 0);
        endPosition = CalculateHorizontalLineOfSightRays(collisionShapeWidth, left_cast, gameObject.collider.bounds.center + centerOffset, -1);
        //Draw lines to show where extra rays will be cast
        Debug.DrawLine(gameObject.collider.bounds.center, endPosition, Color.blue); //Cast ray left

        //Cast ray right
        centerOffset = new Vector3(0, wallCastOffset, 0);
        endPosition = CalculateHorizontalLineOfSightRays(collisionShapeWidth, right_cast, gameObject.collider.bounds.center + centerOffset, 1);
        //Draw lines to show where extra rays will be cast
        Debug.DrawLine(gameObject.collider.bounds.center, endPosition, Color.red); //Cast ray right

        //Cast left bottom and right bottom
        endPosition = CalculateVerticalAngleLineOfSightRays(collisionShapeWidth, collisionShapeHeight, leftanglecast_gap, down_cast, gameObject.collider.bounds.center, -1, -1);
        Debug.DrawLine(gameObject.collider.bounds.center, endPosition, Color.magenta); //Cast ray left and bottom
        //Debug.Log("left position: " + endPosition);

        endPosition = CalculateVerticalAngleLineOfSightRays(collisionShapeWidth, collisionShapeHeight, rightanglecast_gap, down_cast, gameObject.collider.bounds.center, 1, -1);
        Debug.DrawLine(gameObject.collider.bounds.center, endPosition, Color.magenta); //Cast ray left and bottom
        //Debug.Log("right position: " + endPosition);
    
//downwards
    }

    // Use this for initialization
    public void Awake()
    {
        //Get Collision objects
        Collision = GetComponent<BoxCollider>();

        if (Collision == null)
        {
            Debug.LogError("Need to attach box collider to character, attaching box collider");
            gameObject.AddComponent<BoxCollider>();
        }
    }

	public void Start () 
    {
        Physics.gravity = new Vector3(0, -Gravity, 0);
        //OnFloor = true;
        CanInteract = true;
        MovementAllowed = true;
        Direction = -1; //Right * Always start facing right.

        AnimationControls = gameObject.GetComponentInChildren<AnimationController>();
        if (AnimationControls == null)
        {
            Debug.LogError("No animation component in children");
        }

        //Create Rays - For testing if a wall is up down left or right
        /*BottomRay = new Ray(transform.position, transform.TransformDirection(Vector3.down));
        TopRay = new Ray(transform.position, transform.TransformDirection(Vector3.up));
        LeftRay = new Ray(transform.position, transform.TransformDirection(Vector3.left));
        RightRay = new Ray(transform.position, transform.TransformDirection(Vector3.right));*/

        //Set Animation to idle, and set direction
        AnimationControls.SetAnimation("Idle", 0);
        AnimationControls.SetAnimationDirection(Direction);

	}

    public void FixedUpdate()
    {
        
        if (CanInteract)
        {
            ApplyGravity();
            if (MovementAllowed)
            {
                CharacterMovement();
            }
        }
    }
	
	// Update is called once per frame
	public void Update () 
    {
        UpdateRays();
        if (CanInteract)
        {
            AnimationHandler();
        }
	}

    public void OnCollisionEnter(Collision collision)
    {
        //Get the contact points when we enter a collision
        Contact = collision.contacts[0];
        if (collision.gameObject.tag == "Solid")
        {
            //Landed on the floor
            //Check if the contact point is at the bottom
            if (CollisionBottom) //We need to check if the 3 lines we created are colliding at the bottom.
            {
                OnFloor = true;
                CollisionInformation = "Collision Entered at Y";

                if (currentState == CharacterState.Jumping || currentState == CharacterState.Falling)
                {
                    //Set our state to idle.
                    currentState = CharacterState.Idle;
                }
            }
            else
            {
                //OnFloor = false;
            }
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        //Get the contact points when we enter a collision
        Contact = collision.contacts[0];
        if (collision.gameObject.tag == "Solid")
        {
            if (Contact.normal.y > 0 || (Contact.normal.x != 0)) //Contacted at bottom
            {
                //Left the floor
                CollisionInformation = "Collision Exit";
                if (!CollisionBottom)
                {
                    OnFloor = false;
                    //Debug.Log("Entered exit state:" + OnFloor.ToString());
                }

                //Debug.Log("Entered exit state outside of collision bottom:" + OnFloor.ToString());
            }
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        //Get the contact points when we enter a collision
        Contact = collision.contacts[0];
        if (collision.gameObject.tag == "Solid")
        {
            if (CollisionBottom) //We only need to check if we are actually colliding at the bottom.
            {
                OnFloor = true;
                if (currentState == CharacterState.Jumping || currentState == CharacterState.Falling)
                {
                    //Set our state to idle.
                    currentState = CharacterState.Idle;
                }

            }
            else
            {
                
            }
        }
    }

    #endregion

    #region Character Methods
    public Vector3 CalculateVerticalHorizontalLineOfSightRays(float shapeWidth, float shapeHeight, float xoffset, float yoffset, Vector3 center, int xraydirection, int yraydirection)
    {
        float totalyoffset = (shapeHeight / 2) + yoffset; //Set direction for down cast
        float totalxoffset = (shapeWidth / 2) + xoffset; //Set the direction for horizontal aswell;
        Vector3 direction = new Vector3(totalxoffset * xraydirection, totalyoffset * yraydirection, 0);
        //Vector3 startPosition = center;
        Vector3 endPosition = center + direction;

        return endPosition;
    }

    public Vector3 CalculateVerticalLineOfSightRays(float shapebound, float offset, Vector3 center, int raydirection)
    {
        //Direction we will be passing the ray with an offset for finer tuning *format's automatically with collision width * height
        float total = (shapebound / 2) + offset; //Set direction for down cast
        Vector3 direction = new Vector3(0, total * raydirection, 0);
        //Vector3 startPosition = center;
        Vector3 endPosition = center + direction;

        return endPosition;
    }

    public Vector3 CalculateVerticalAngleLineOfSightRays(float xshapebound, float yshapebound, float xoffset, float yoffset, Vector3 center, int xraydirection, int yraydirection)
    {
        //Direction we will be passing the ray with an offset for finer tuning *format's automatically with collision width * height
        float totalyoffset = (yshapebound / 2) + yoffset; //Set direction for down cast
        float totalxoffset = (xshapebound / 2) + xoffset; //Set the direction for horizontal aswell;
        Vector3 direction = new Vector3(totalxoffset * xraydirection, totalyoffset * yraydirection, 0);
        //Vector3 startPosition = center;
        Vector3 endPosition = center + direction;

        return endPosition;
    }


    public Vector3 CalculateHorizontalLineOfSightRays(float shapebound, float offset, Vector3 center, int raydirection)
    {
        //Direction we will be passing the ray with an offset for finer tuning *format's automatically with collision width * height
        float total = (shapebound / 2) + offset; //Set direction for down cast
        Vector3 direction = new Vector3(total * raydirection, 0, 0);
        //Vector3 startPosition = center;
        Vector3 endPosition = center + direction;

        return endPosition;
    }

    //This function casts a ray in a direction and returns whether it hit an object
    public bool CastRay(Vector3 direction)
    {
        RaycastHit hitInfo = new RaycastHit();
        Vector3 startPosition = gameObject.collider.bounds.center;
        if (Physics.Linecast(startPosition, transform.position + direction, out hitInfo, LayerMask.NameToLayer("Character")))
        {
            return true;
        }
        return false;
    }

    public void UpdateRays()
    {
        //Ignore the character layer and collide with everything else
        RaycastHit hitInfo = new RaycastHit();
        
        Vector3 collisionShape = gameObject.collider.bounds.size; //Get the size of the collision box and store in a variable

        //Get the width and height of the collision box
        float collisionShapeWidth = collisionShape.x;
        float collisionShapeHeight = collisionShape.y;

        Vector3 startPosition, endPosition;
        //Create center for rays to move from.
        //Vector3 startPosition = gameObject.collider.bounds.center;

        //Cast 3 rays downwards to check if we are colliding at the bottom, if there is no collision at the bottom
        //then we can not be on the floor.

        endPosition = CalculateVerticalLineOfSightRays(collisionShapeHeight, down_cast, gameObject.collider.bounds.center, -1);
        //Draw lines to show where extra rays will be cast
        Vector3 downleftoffset = new Vector3(collisionShapeWidth / 2 - downleft_cast, 0, 0); //Offset to change left line cast
        Vector3 downrightoffset = new Vector3(collisionShapeWidth / 2 - downright_cast, 0, 0); //The offset to change right line cast

        //If any of the lines we have created are colliding, then we are definately on the floor otherwise we are not
        if (Physics.Linecast(gameObject.collider.bounds.center - downleftoffset, endPosition - downleftoffset, out hitInfo))
        {
            if (hitInfo.collider.gameObject.tag == "Solid")
            {
                CollisionBottom = true;
                //Debug.Log("Collision at bottom? : " + hitInfo.collider.gameObject.name);
            }
        }
        else if(Physics.Linecast(gameObject.collider.bounds.center, endPosition, out hitInfo))
        {
            if (hitInfo.collider.gameObject.tag == "Solid")
            {
                CollisionBottom = true;
                //Debug.Log("Collision at bottom? : " + hitInfo.collider.gameObject.name);
            }
        }
        else if (Physics.Linecast(gameObject.collider.bounds.center + downrightoffset, endPosition + downrightoffset, out hitInfo))
        {
            if (hitInfo.collider.gameObject.tag == "Solid")
            {
                CollisionBottom = true;
                //Debug.Log("Collision at bottom? : " + hitInfo.collider.gameObject.name);
            }
        }
        else
        {
            CollisionBottom = false;
            OnFloor = false; //Cant be on the floor if none of the 3 lines are actually colliding.
        }



        //Set Start Position to center of object
        startPosition = gameObject.collider.bounds.center;

        //Cast ray upwards
        endPosition = CalculateVerticalLineOfSightRays(collisionShapeHeight, up_cast, gameObject.collider.bounds.center, 1);

        if (Physics.Linecast(startPosition, endPosition, out hitInfo, LayerMask.NameToLayer("Character")))
        {
            CollisionTop = true;
        }
        else
        {
            CollisionTop = false;
        }

        //Cast ray left
        Vector3 centerOffset = new Vector3(0, wallCastOffset, 0);
        endPosition = CalculateHorizontalLineOfSightRays(collisionShapeWidth, left_cast, gameObject.collider.bounds.center + centerOffset, -1);

        if (Physics.Linecast(startPosition, endPosition, out hitInfo, LayerMask.NameToLayer("Character")))
        {
            CollisionLeft = true;
        }
        else
        {
            CollisionLeft = false;
        }


        //Cast ray right
        centerOffset = new Vector3(0, wallCastOffset, 0);
        endPosition = CalculateHorizontalLineOfSightRays(collisionShapeWidth, right_cast, gameObject.collider.bounds.center + centerOffset, 1);

        if (Physics.Linecast(startPosition, endPosition, out hitInfo, LayerMask.NameToLayer("Character")))
        {
            CollisionRight = true;
        }
        else
        {
            CollisionRight = false;
        }

        //Cast ray bottom left
        endPosition = CalculateVerticalAngleLineOfSightRays(collisionShapeWidth, collisionShapeHeight, leftanglecast_gap, down_cast, gameObject.collider.bounds.center, -1, -1);

        if (Physics.Linecast(startPosition, endPosition, out hitInfo, LayerMask.NameToLayer("Character")))
        {
            CollisionBottomLeft = true;
        }
        else
        {
            CollisionBottomLeft = false;
        }

        //Cast ray bottom right
        endPosition = CalculateVerticalAngleLineOfSightRays(collisionShapeWidth, collisionShapeHeight, rightanglecast_gap, down_cast, gameObject.collider.bounds.center, 1, -1);

        if (Physics.Linecast(startPosition, endPosition, out hitInfo, LayerMask.NameToLayer("Character")))
        {
            CollisionBottomRight = true;
        }
        else
        {
            CollisionBottomRight = false;
        }

        
    }

    public virtual void CharacterMovement()
    {
        //Default move code goes here.
    }

    public virtual void CharacterJump()
    {
        //Jump Content Goes in here
        Vector3 moveDirection = new Vector3(0, JumpHeight, 0);
        rigidbody.AddForce(moveDirection, ForceMode.VelocityChange);
        OnFloor = false;
        currentState = CharacterState.Jumping;
    }

    public virtual void CharacterJump(float jumpHeight, ForceMode mode)
    {
        //Jump Content Goes in here with altered jump height
        Vector3 moveDirection = new Vector3(0, jumpHeight, 0);
        rigidbody.AddForce(moveDirection, mode);
        OnFloor = false;
        currentState = CharacterState.Jumping;
    }

    public void ResetState()
    {
        //Finds out and resets the state after dashing,etc.
        switch (currentState)
        {
            case CharacterState.Dashing:
                if (!OnFloor)
                {
                    currentState = CharacterState.Falling;
                }
                break;

            case CharacterState.Falling:
                break;

            case CharacterState.Idle:
                break;

            case CharacterState.Moving:
                break;
        }
    }

    public virtual void ApplyGravity()
    {
        if (UseCustomGravity)
        {
            //If the character is not on the floor, apply gravity
            if (!OnFloor)
            {
                //Apply Gravity
                Vector3 moveDirection = new Vector3(0, -Gravity, 0);

                if (rigidbody.velocity.y < 0.8f) //In falling state, apply gravity.
                {
                    rigidbody.AddForce(moveDirection, ForceMode.VelocityChange);
                    if (currentState != CharacterState.WallCling)
                    {
                        currentState = CharacterState.Falling;
                    }
                }
            }
            else
            {
                
            }
        }
    }

    public virtual void AnimationHandler()
    {
        //This function handles the animation states.
        switch (currentState)
        {
            case CharacterState.Idle:
                if (AnimationControls.CurrentAnimation != "Idle")
                {
                    AnimationControls.SetAnimation("Idle", 0);
                }
                break;
            case CharacterState.Moving:
                if (AnimationControls.CurrentAnimation != "Walk")
                {
                    AnimationControls.SetAnimation("Walk", 0);
                }
                break;
            case CharacterState.Jumping:
                if (AnimationControls.CurrentAnimation != "Jump")
                {
                    AnimationControls.SetAnimation("Jump", 0);
                }
                break;
            case CharacterState.Falling:
                if (AnimationControls.CurrentAnimation != "Fall")
                {
                    AnimationControls.SetAnimation("Fall", 0);
                }
                break;
        }
    }

    //This section holds the character methods not passed in the ICharacter Interface
    public void AddCharacterToManager()
    {
        //This method gets called once everything has been created
        //Add yourself to the gamemanager
        //Manager.Game.AddCharacter(this);
    }

    public void ActivateCharacter() //This function is called to give the character life
    {
        CanInteract = true;
    }

    public void DeactivateCharacter() //This function is called to pause the character or stop.
    {
        CanInteract = false;
    }

    #endregion

    #region ICharacter Methods
    public virtual void Attack(Character character)
    {
        //Debug.Log("Attack for:" + Damage);
        if (character != null)
        {
            character.TakeDamage(Damage);
        }
    }

    public virtual void TakeDamage(float amount)
    {
        //Debug.Log("Hit : " + amount);
        Health -= amount;

        if (Health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        //Debug.Log("Dead!");
        //Manager.Game.RemoveCharacter(this);
        Destroy(gameObject);
    }

    public virtual void Heal(float amount)
    {
        Debug.Log("Healed :" + amount);
    }

    public virtual void Stun(float time)
    {
        Debug.Log("Stunned for :" + time.ToString() + " seconds.");
    }

    public virtual float GetHealth()
    {
        Debug.Log("Returning health");
        return Health;
    }

    
    #endregion
}
