using System;
using System.Collections;

namespace Code.Core.Level.Element
{
    using UnityEngine;

    public class SimpleArcJump : MonoBehaviour
    {
        [Header("Jump Settings")] 
        [SerializeField] private float _minJumpHeight = 1f;
        [SerializeField] private float _maxJumpHeight = 4f;
        [SerializeField] private float _jumpDuration = 1.4f;
        [SerializeField] private float _delayAnimation;
        [SerializeField] private bool _yMovementIsFixed = false;
        [SerializeField] private float _simpleMovementSpeed = 5f;

        [Header("Rotation")] 
        [SerializeField] private  Vector3 _rotationOffset = new Vector3(0f, 0f, 90f);

        Quaternion currentRotationAtStart;
        private bool isJumping = false;
        private Vector3 startPosition;
        private Vector3 previousPosition;
        private float startTime;

        private Vector3 currentTarget;
        private bool currentIsLocalPos;

        [HideInInspector] public Transform parentAtStartOfJump;

        public void SimpleMove(Vector3 targetPos, bool isLocalPos, int index, Action callback = null)
        {
            var delay = index * _delayAnimation * _jumpDuration;
            StartCoroutine(JumpToTarget(targetPos, isLocalPos, delay, callback));
        }

        IEnumerator JumpToTarget(Vector3 target, bool isLocalPos = false, float delay = 0f, Action callback = null)
        {
            yield return new WaitForSeconds(delay);
            
            isJumping = true;

            Quaternion rotationAtStart = transform.rotation;

            float realWorldFixedPosOnY = 0f;

            if (_yMovementIsFixed)
                target = new Vector3(target.x, realWorldFixedPosOnY, target.z);

            startPosition = isLocalPos ? transform.localPosition : transform.position;
            previousPosition = startPosition;

            float horizontalDistance = Vector3.Distance(new Vector3(startPosition.x, 0, startPosition.z),
                new Vector3(target.x, 0, target.z));
            float verticalDistance = Mathf.Abs(target.y - startPosition.y);

            float jumpHeight = Mathf.Lerp(_minJumpHeight, _maxJumpHeight, horizontalDistance / 10);
            jumpHeight = Mathf.Max(jumpHeight, verticalDistance * 0.5f);


            startTime = Time.time;

            currentTarget = target;
            currentRotationAtStart = rotationAtStart;
            currentIsLocalPos = isLocalPos;



            while (_yMovementIsFixed && Mathf.Abs(transform.position.y - realWorldFixedPosOnY) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                    new Vector3(transform.position.x, realWorldFixedPosOnY, transform.position.z),
                    _simpleMovementSpeed * Time.deltaTime);
                yield return null;
            }


            while (Time.time - startTime < _jumpDuration)
            {
                float t = (Time.time - startTime) / _jumpDuration;
                Vector3 jumpArc = CalculateJumpArc(startPosition, target, jumpHeight, t);


                if (isLocalPos)
                    transform.localPosition = jumpArc;
                else
                    transform.position = jumpArc;


         
                    // Update orientation to always face forward along the jump arc
                    Vector3 direction = jumpArc - previousPosition;
                    if (direction != Vector3.zero)
                    {
                        Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
                        transform.rotation = rot * Quaternion.Euler(_rotationOffset);
                    }
                

                previousPosition = jumpArc;

                yield return null;
            }

            // Snap to the exact target position and finish the jump
            transform.position = target;
            FinishJumping();
            
            callback?.Invoke();
        }
        

        public void FinishJumping()
        {
            StopAllCoroutines();

            transform.position = currentTarget; // Ensure exact positioning at the end


            transform.rotation = currentRotationAtStart;

            if (currentIsLocalPos) transform.localPosition = currentTarget;

            CheckIfStackHasMatchingColors();

            ReCheckNeighbours();

            isJumping = false;
            Debug.Log("Finished Jumping....");

        }

        public bool IsJumping()
        {
            return isJumping;
        }

        void CheckIfStackHasMatchingColors()
        {
            if (transform.parent != null && transform.parent.childCount - 1 == transform.GetSiblingIndex())
            {
                if (transform.parent != null)
                {
                    // transform.parent.GetComponentInParent<CheckNeighbours>().StartCheckMatchingColorsInStack();
                }
            }
        }

        void ReCheckNeighbours()
        {
            if (transform.parent == null) return;

            if ((transform.parent.childCount - 1) == transform.GetSiblingIndex())
            {
                if (parentAtStartOfJump != null)
                {
                }

                if (transform.parent != null)
                {
                }
            }
        }

        private Vector3 CalculateJumpArc(Vector3 start, Vector3 target, float height, float time)
        {
            float x = Mathf.Lerp(start.x, target.x, time);
            float y = Mathf.Lerp(start.y, target.y, time) + Mathf.Sin(time * Mathf.PI) * height;
            float z = Mathf.Lerp(start.z, target.z, time);

            return new Vector3(x, y, z);
        }
    }

}