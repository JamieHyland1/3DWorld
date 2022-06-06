
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class MoveState : IState
{
        PlayerSM playerSM;
        CharacterController controller;
        public string STATE_NAME = "Move State";
        Transform playerTransform;
        Transform cam;
        Vector2 move;
        Vector3 velocity;
        private PlayerControls controls;
        float walkSpeed;
        float runSpeed;

        Vector3 direction;
        Vector3 moveDirection;

        float speed = 0;

        AnimationCurve accelCurve;

        float turnSmoothVelocity;

        float turnSmoothTime;
        float accelTime = 0;
        float decelTime = 1;

        PhysicsHelper helper;
        bool jumpButtonHeld = false;

        Animator animator;
        LayerMask layer;
        bool sliding = false;
        public MoveState(PlayerSM _playerSM, Animator animator, Transform playerTransform, Transform cam, AnimationCurve accelCurve, PlayerControls controls, ref CharacterController controller, ref Vector2 move, float walkSpeed, float runSpeed, float turnSmoothVelocity, float turnSmoothTime, LayerMask layer, PhysicsHelper helper ){
            playerSM = _playerSM;
           
            this.animator = animator;
            this.playerTransform = playerTransform;
            this.cam = cam;
            this.accelCurve = accelCurve;
            // this.controls = controls;
            this.controller = controller;
            this.move = move;
            this.walkSpeed = walkSpeed;
            this.runSpeed = runSpeed;
            this.turnSmoothVelocity = turnSmoothVelocity;
            this.turnSmoothTime = turnSmoothTime;
            this.layer = layer;
            this.helper = helper;
        }
        public void Enter()
        {
            controls = new PlayerControls();
            controls.Ground_Move.Enable(); 
            velocity = new Vector2();
            move = new Vector2();
            controls.Ground_Move.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
            controls.Ground_Move.Move.canceled  += ctx => move = Vector2.zero;
            controls.Ground_Move.Jump.started += ctx => Jump();
            controls.Ground_Move.Jump.canceled += ctx => jumpButtonHeld = false;
            controls.Ground_Move.Dash.performed += ctx => InitiateDash();
            controls.Ground_Move.Slide.started += ctx => InitiateSlide();
            velocity = playerSM.getVelocity();
             
            if(playerSM.getCurrentSpeed() > walkSpeed) speed = Mathf.Clamp(playerSM.getCurrentSpeed(),0,PhysicsHelper.Instance.afterDashSpeed); else speed = walkSpeed;
           
            //Sometime when changing states, controller input can lag and not register immediately causing player to immediately decellerate
            //capture the direction of the player when leaving a certain state, then compare the magnitude to see which is greater then apply that instead
           
            move.x = playerSM.getDirection().x;
            move.y = playerSM.getDirection().z;
            controller.height = 8.9f;
            controller.center = new Vector3(0,4.3f,0);
            controller.radius = 1.5f;

         

        }


        public void FixedTick()
        {
            
        }

        public void Tick(){          
            if(!playerSM.checkForSlopes()){
                controls.Ground_Move.Disable();
                velocity = playerSM.slideDownSlope(velocity);
            }else{ 
                controls.Ground_Move.Enable();


                animator.SetBool("IsGrounded", playerSM.isGrounded);

                if(!playerSM.isGrounded)velocity = PhysicsHelper.Instance.applyGravity(velocity,jumpButtonHeld); 
                else if(playerSM.isGrounded && velocity.y < 0) velocity.y = 0;



                Vector3 currDirection = new Vector3(move.x, 0f, move.y);
                direction = currDirection;
                float xDir = playerSM.scale(0, runSpeed, 0, 2, move.x);
                float zDir = playerSM.scale(0, runSpeed, 0, 2, move.y);

                animator.SetFloat("Velocity X", move.x * xDir);
                animator.SetFloat("Velocity Z", move.y * zDir);

            if (direction.magnitude > 0) { 
                animator.SetBool("IsRunning", true);
                animator.SetBool("IsWalking", false);
                float angle = 0;
                PhysicsHelper.Instance.rotateCharacter(direction,playerTransform,cam,turnSmoothTime,turnSmoothVelocity, out moveDirection, out angle);
                if(speed < PhysicsHelper.Instance.afterDashSpeed)speed = Mathf.Lerp(PhysicsHelper.Instance.moveSpeed,PhysicsHelper.Instance.runSpeed,accelCurve.Evaluate(accelTime));  
            }
            else{
                if(Vector3.Distance(velocity,Vector3.zero) > 0.1f){
                    speed = Mathf.Lerp(0,speed,accelCurve.Evaluate(decelTime)); 
                    decelTime -= Time.deltaTime/6;
                }
                else{
                    accelTime = 0;
                    velocity = Vector3.zero;
                    playerSM.setDirection(Vector3.zero);
                }
            }
            velocity.x = moveDirection.x * speed;
            velocity.z = moveDirection.z * speed;
            accelTime += Time.deltaTime/2;
            }
            controller.Move(velocity * Time.deltaTime);
            animator.SetFloat("Speed", speed);
            playerSM.isGrounded = helper.checkGroundCollision(playerSM.isGrounded,playerSM.groundCheck,layer);
            if(!playerSM.isGrounded)playerSM.ChangeState(playerSM.airMoveState);
        }

        public void InitiateDash(){
            playerSM.ChangeState(playerSM.dashState);
        }
        
        public void Exit()
        {
            playerSM.setDirection(direction);
            playerSM.setVelocity(velocity);
            playerSM.setCurrentSpeed(new Vector3(velocity.x,0f,velocity.y).magnitude);
            controls.Ground_Move.Disable();
        }
  

        public void Jump(){
           
             if(playerSM.isGrounded){
                velocity.y = Mathf.Sqrt(PhysicsHelper.Instance.jumpVelocity *-2f * helper.gravity * 2.5f);
                playerSM.setVelocity(velocity);
                playerSM.ChangeState(playerSM.airMoveState);
            }
             jumpButtonHeld = true;
            
        }
        public void InitiateSlide(){
            if(playerSM.isGrounded){
                playerSM.setCurrentSpeed(velocity.magnitude);
                playerSM.setVelocity(velocity);
                playerSM.setDirection(direction);
                playerSM.ChangeState(playerSM.slideState);
            }
        }

        public void PrintStateName(){
            Debug.Log(STATE_NAME);
        }

        public void EventTrigger(){}
    }

