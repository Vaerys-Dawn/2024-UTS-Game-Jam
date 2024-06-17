using System;
using UnityEngine;

public class Utility 
{
    [Serializable]
    public enum PowerUpType
    {
        DASH,
        SLIDE,
        WALL_CLIMB,
        WALL_JUMP,
        GRAPPLE,
        WALL_HOLD,
        ATTACK
    }
    public static void AddForceDirection(Vector2 direction, Rigidbody2D rigid, float maxSpeed, float lerpAmount, float accelRate)
    {
        direction = direction.normalized;
        //Calculate the direction we want to move in and our desired velocity
        float targetSpeedX = direction.x * maxSpeed;
        float targetSpeedY = direction.y * maxSpeed;

        //We can reduce are control using Lerp() this smooths changes to are direction and speed
        targetSpeedX = Mathf.Lerp(rigid.velocity.x, targetSpeedX, lerpAmount);
        targetSpeedY = Mathf.Lerp(rigid.velocity.y, targetSpeedY, lerpAmount);

        //Calculate difference between current velocity and desired velocity
        float speedDifX = targetSpeedX - rigid.velocity.x;
        float speedDifY = targetSpeedY - rigid.velocity.y;
        //Calculate force along x-axis to apply to thr player

        float moveX = speedDifX * accelRate;
        float moveY = speedDifY * accelRate;

        Vector2 movement = Vector2.ClampMagnitude(new Vector2(moveX, moveY), maxSpeed);

        //Convert this to a vector and apply to rigidbody
        rigid.AddForce(movement, ForceMode2D.Force);
    }
}
