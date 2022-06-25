using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateManager : MonoBehaviour
{
    public Animator animator;
    int speedCounter = 0;
    int dashCounter = 0;

    // Subscribe to events
    public void Start(){
        EventManager.current.OnPlayerGroundedUpdated += SetPlayerGrounded;
        EventManager.current.OnPlayerDashingUpdated += SetPlayerDashing;
        EventManager.current.OnPlayerSlidingUpdated += SetPlayerSliding;
        EventManager.current.OnPlayerSpeedUpdated += SetPlayerSpeed;
        EventManager.current.OnPlayerXDirectionUpdated += SetPlayerXDir;
        EventManager.current.OnPlayerYDirectionUpdated += SetPlayerYDir;
        
    }   

    // Unsubscribe to events
    public void OnDestroy(){
        EventManager.current.OnPlayerGroundedUpdated -= SetPlayerGrounded;
        EventManager.current.OnPlayerDashingUpdated -= SetPlayerDashing;
        EventManager.current.OnPlayerSlidingUpdated -= SetPlayerSliding;
        EventManager.current.OnPlayerSpeedUpdated -= SetPlayerSpeed;
        EventManager.current.OnPlayerXDirectionUpdated -= SetPlayerXDir;
        EventManager.current.OnPlayerYDirectionUpdated -= SetPlayerYDir;
    }


    private void SetPlayerGrounded(Boolean isGrounded){
        animator.SetBool("IsGrounded", isGrounded);
    }
    private void SetPlayerDashing(Boolean isDashing){
        animator.SetBool("IsDashing",isDashing);
    }
    private void SetPlayerSliding(Boolean isSliding){
       animator.SetBool("IsSliding", isSliding);
       if(!isSliding)animator.SetTrigger("ExitSlide"); 
    }
    private void SetPlayerSpeed(float speed){
        animator.SetFloat("Speed", speed);
    }
    private void SetPlayerXDir(float xDir){
        animator.SetFloat("Velocity X", xDir);
    }
    private void SetPlayerYDir(float yDir){
        animator.SetFloat("Velocity Z", yDir);
    }
}
