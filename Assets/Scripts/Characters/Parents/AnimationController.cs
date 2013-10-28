using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class AnimationStructure
{
    public string animationtitle; //This is what we use to reference it
    public string animation; //This is the actual animation

}

public class AnimationController : MonoBehaviour
{

    #region Inspector Variables
    //List of animations
    public List<AnimationStructure> Animations; //Holds a list of all the animations
    public string CurrentAnimation;
    public int CurrentIndex;
    
    #endregion

    #region Public Variables
    public exSpriteAnimation SpriteAnimation { get; set; }
    public exSprite Sprite { get; set; }
    #endregion

    #region Private Variables

    #endregion

    #region Unity Methods
    void Awake()
    {
        //Set the sprite animation component
        SpriteAnimation = GetComponent<exSpriteAnimation>();
        Sprite = GetComponent<exSprite>();

        if (Sprite == null)
        {
            Debug.LogError("No ex 2d sprite");
        }
        if (SpriteAnimation == null)
        {
            //If the sprite animation does not exist
            Debug.LogError("No Ex 2D Sprite Animation");
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        //Always update index
        UpdateIndex();
    }
    #endregion

    #region Custom Methods
    //This function changes the current animation we are in
    public void SetAnimation(string animation, int index) //Set animation plus index
    {
        if (SpriteAnimation != null) //As long as sprite has an exSpriteAnimation component
        {
            //Filter through the animation to find what we are looking for
            foreach (AnimationStructure anim in Animations)
            {
                if (anim.animationtitle == animation) //Found the animation title
                {
                    //Change the animation
                    //Debug.Log("Animation Change" + anim.animation + "::" + anim.animationtitle );
                    CurrentAnimation = animation;
                    CurrentIndex = index;
                    SpriteAnimation.SetFrame(anim.animation, index);
                    SpriteAnimation.Play(anim.animation);
                }
            }
        }
    }

    public void UpdateIndex()
    {
        //This function always make sure that this index is equal to the animation index
        if (SpriteAnimation != null)
        {
            CurrentIndex = SpriteAnimation.GetCurFrameIndex();
        }
    }

    public void SetAnimationDirection(int direction)
    {
        //Set the direction we are facing
        if (Sprite != null)
        {
            
            if (direction == -1)
            {
                if (Sprite.scale.x > 0)
                {
                    //Sprite.scale = new Vector2(-Sprite.scale.x, Sprite.scale.y);
                    Sprite.HFlip();
                }
            }
            else if(direction == 1)
            {
                if (Sprite.scale.x < 0)
                {
                    //Sprite.scale = new Vector2(Sprite.scale.x, Sprite.scale.y);
                    Sprite.HFlip();
                }
            }

            //Debug.Log("Sprite Scale: " + Sprite.scale.x + "   Direction: " + direction);
        }
    }
    #endregion

    #region Interface Methods
    #endregion

}

