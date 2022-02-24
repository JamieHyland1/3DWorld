using System;
using System.Collections.Generic;
using UnityEngine;



    public class WallSlideState : IState
    {
        PhysicsHelper helper;
        PlayerSM playerSM;
        Transform playerTransform;
        Transform groundCheck;
        Transform wallCheck;
        PlayerControls controls;
       
        CharacterController controller;
        LayerMask layer;

        Vector3 velocity;
        Animator animator;
        bool onWall;
        Vector3 normal;
        float timer = 1;
        float counter = 0;
        public WallSlideState(PlayerSM playerSM, Transform playerTransform, Transform groundCheck, Transform wallCheck, PlayerControls controls, ref CharacterController controller, LayerMask layer, Animator animator, PhysicsHelper helper){
            this.playerSM = playerSM;
            this.playerTransform = playerTransform;
            this.groundCheck = groundCheck;
            this.wallCheck = wallCheck;
            // this.controls = controls;
            this.controller = controller;
            this.layer = layer;
            this.animator = animator;
            this.helper = helper;
        }

        public void Enter(){
          controls = new PlayerControls();
            normal = new Vector3();
            velocity = playerSM.getVelocity();
            controls.Enable();
            controls.WallJump.Jump.performed += ctx => wallJump();
            counter = timer;
            
        }

       
        public void FixedTick(){
        }

        public void Tick(){
            onWall   = PhysicsHelper.Instance.checkPos(wallCheck.position, 2f, layer);
            
            playerSM.isGrounded = PhysicsHelper.Instance.checkGroundCollision(playerSM.isGrounded,groundCheck,layer);
            RaycastHit hit = PhysicsHelper.Instance.getHitInfo(wallCheck.position,playerTransform.forward,5f,layer);
            if(hit.collider != null)normal = hit.normal;
            
            
            velocity = PhysicsHelper.Instance.applyGravity(velocity,true);
            Debug.DrawLine(wallCheck.position,wallCheck.position+(playerTransform.forward*2),Color.black,0.2f);
    
            controller.Move(velocity * Time.deltaTime);


            // if(!onWall && !playerSM.isGrounded)playerSM.ChangeState(playerSM.airMoveState);
            if(playerSM.isGrounded == true)playerSM.ChangeState(playerSM.moveState);
        }


         public void Exit(){
            controls.WallJump.Disable();
            playerSM.setVelocity(Vector3.zero);
            playerSM.setCurrentSpeed(0);
            playerSM.setDirection(Vector3.zero);
        }


        public void wallJump(){
            if(onWall && !playerSM.isGrounded){
                animator.SetTrigger("Jump");
                velocity =  (Vector3.up * 45) + (normal * 55);
                 Debug.Log("Wall jump " + velocity );
                Debug.Log("Normal " + normal );
                playerTransform.rotation = Quaternion.FromToRotation(playerTransform.forward,normal) * playerTransform.rotation;
            }
        }
    }

