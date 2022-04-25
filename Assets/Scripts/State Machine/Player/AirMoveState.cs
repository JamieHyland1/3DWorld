

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


    public class AirMoveState : IState
    {
        PlayerSM playerSM;
        CharacterController controller;
        
        Transform playerTransform;
        Transform wallCheck;
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

        PhysicsHelper helper;
        bool jumpButtonHeld = false;

        Animator animator;
        LayerMask layer;
        bool sliding = false;
        public AirMoveState(PlayerSM _playerSM, Animator animator, Transform playerTransform, Transform cam, Transform wallCheck, AnimationCurve accelCurve, PlayerControls controls, ref CharacterController controller, ref Vector2 move, float walkSpeed, float runSpeed, float turnSmoothVelocity, float turnSmoothTime, LayerMask layer, PhysicsHelper helper ){
            playerSM = _playerSM;
           
            this.animator = animator;
            this.playerTransform = playerTransform;
            this.cam = cam;
            this.wallCheck = wallCheck;
            this.accelCurve = accelCurve;
            this.controls = controls;
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
            Debug.Log("In Air Move state");
            controls = new PlayerControls();
            controls.Air_Move.Enable();
            controls.Air_Move.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
            controls.Air_Move.Move.canceled  += ctx => move = Vector2.zero;
            controls.Air_Move.Dash.performed += ctx => InitiateDash();
            //decide on wether you should have a floaty jump depending on holding the jump button or not
            // controls.Air_Move.Jump.started += ctx => jumpButtonHeld = false;
            // controls.Air_Move.Jump.canceled += ctx => jumpButtonHeld = true;
            //if(playerSM.getCurrentSpeed() > walkSpeed) speed = playerSM.getCurrentSpeed(); else speed = walkSpeed;
            //Sometime when changing states, controller input can lag and not register immediately causing player to immediately decellerate
            //capture the direction of the player when leaving a certain state, then compare the magnitude to see which is greater then apply that instead
            // move.x = playerSM.getDirection().x;
            // move.y = playerSM.getDirection().z;
            velocity = playerSM.getVelocity();
            speed = playerSM.getCurrentSpeed();

            controller.height = 5.5f;
            controller.center = new Vector3(0,5.8f,0.5f);
            controller.radius = 1.5f;
           
            Debug.Log("AirMove velocity " + velocity);
            Debug.Log("AirMove speed " + speed);
            Debug.Log("AirMove direction " + direction);
            animator.SetBool("IsGrounded", false);

        }

      

        public void FixedTick()
        {
            
        }

        public void Tick(){
            if(!playerSM.checkForSlopes()){
                controls.Air_Move.Disable();
                velocity = playerSM.slideDownSlope(velocity);
            }
            else {
                controls.Air_Move.Enable();
                velocity = PhysicsHelper.Instance.applyGravity(velocity,true); 


                if(PhysicsHelper.Instance.checkPos(wallCheck.position,1f,layer) && playerSM.isGrounded == false)playerSM.ChangeState(playerSM.wallSlide);

                Vector3 currDirection = new Vector3(move.x, 0f, move.y);
                Debug.Log("Air Move " + direction.magnitude);
                direction = currDirection;


                if (direction.magnitude > 0) { 
                    float angle = 0;
                    PhysicsHelper.Instance.rotateCharacter(direction,playerTransform,cam,turnSmoothTime,turnSmoothVelocity, out moveDirection, out angle);
                    if(speed < PhysicsHelper.Instance.afterDashSpeed)speed = Mathf.Lerp(PhysicsHelper.Instance.moveSpeed,PhysicsHelper.Instance.runSpeed,accelCurve.Evaluate(accelTime));  
                }


                velocity.x = moveDirection.x * speed * direction.magnitude;
                velocity.z = moveDirection.z * speed * direction.magnitude;
                accelTime += Time.deltaTime/2;
            }
            controller.Move(velocity * Time.deltaTime);

            playerSM.isGrounded = helper.checkGroundCollision(playerSM.isGrounded,playerSM.groundCheck,layer);

            if(playerSM.isGrounded)playerSM.ChangeState(playerSM.moveState);
            
        }
        public void InitiateDash(){
            Debug.Log("Dash called from airMove state");
            playerSM.setVelocity(velocity);
            playerSM.ChangeState(playerSM.dashState);
        }
           public void Exit()
        {
            jumpButtonHeld = false;
            playerSM.setVelocity(Vector3.zero);
            playerSM.setCurrentSpeed(speed);
            playerSM.setDirection(Vector3.zero);
            controls.Air_Move.Disable();
         
        }
          
   
    }

