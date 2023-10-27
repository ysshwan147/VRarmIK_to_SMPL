﻿using UnityEngine;
using UnityEngine.UI;

namespace VRArmIKtoSMPL
{
    public class Shoulder : MonoBehaviour
    {
        public UpperBodyTransform upperBody;
        public Transform target;
        public Transform elbow, wrist;

        public bool isLeft = false;

        public float offsetAngle = 15.0f;

        public float xBias = 30.0f;
        public float yBias = 120.0f;
        public float zBias = 65.0f;

        public float yWeight = -60.0f;
        public float zWeight = 260.0f;
        public float zWeightTop = 260.0f;
        public float zWeightBottom = -100.0f;
        public float zStartDistance = 0.5f;
        public float xWeight = -50.0f;
        public float xStartDistance = 0.1f;

        public float minAngle = 13.0f;
        public float maxAngle = 175.0f;

        public bool correctElbowOutside = true;
        public float weight1 = 0.5f;
        public float startBelowZ = 0.4f;
        public float startAboveY = 0.1f;

        public bool useFixedElbowWhenNearShoulder = true;
        public float startBelowDistance = 0.5f;
        public float startBelowY = 0.1f;
        public float weight2 = 2.0f;
        public Vector3 localElbowPos = new Vector3(0.3f, -1.0f, -2.0f);

        public bool useWristRotation = true;
        public bool rotationWithHandRight = true;
        public bool rotationWithHandForward = true;
        public float handDeltaPow = 1.5f, handDeltaFactor = -.3f, handDeltaOffset = 45f;
        // todo fix rotateElbowWithHandForward with factor != 1 -> horrible jumps. good value would be between [0.4, 0.6]
        public float handDeltaForwardPow = 2f, handDeltaForwardFactor = 1f, handDeltaForwardOffset = 0f, handDeltaForwardDeadzone = .3f;
        public float rotateElbowWithHandDelay = .08f;

        float interpolatedDeltaElbow;
        float interpolatedDeltaElbowForward;

        Quaternion startRotation;
        Vector3 armDirection => isLeft ? Vector3.left : Vector3.right;
        Quaternion shoulderRotation => transform.rotation * Quaternion.Inverse(startRotation);

        void setRotation(Quaternion rotation) => transform.rotation = rotation * startRotation;
        void setLocalRotation(Quaternion rotation) => transform.rotation = transform.parent.rotation * rotation * startRotation;


        void Awake()
        {
            startRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            rotateShoulder();
            correctElbowRotation();
            correctElbowAfterPositioning();
            rotateElbowWithHandRight();
            rotateElbowWithHandFoward();
        }

        void rotateShoulder()
        {
            Vector3 targetShoulderDirection = (target.position - transform.position).normalized;
            float targetAngleAboudElbowAngle = calcAngleAboutElbowAngle();

            Quaternion toTargetRotation = Quaternion.FromToRotation(armDirection, targetShoulderDirection);
            setRotation(toTargetRotation);

            // modify: elbow.rotation
            transform.rotation = Quaternion.AngleAxis(targetAngleAboudElbowAngle, elbow.rotation * Vector3.up) * transform.rotation;

            float targetAngleAboutHandShoulderAxis = calcAngleAboutHandShoulderAxis();

            rotateWithAngle(targetAngleAboutHandShoulderAxis);
        }

        void rotateWithAngle(float angle)
        {
            Vector3 shoulderWristDirection = (transform.position - wrist.position).normalized;

            Quaternion rotation = Quaternion.AngleAxis(angle, shoulderWristDirection);
            setRotation(rotation * shoulderRotation);
        }

        void correctElbowRotation()
        {
            Vector3 localTargetPos = transform.InverseTransformPoint(target.position) / upperBody.armLength;
            float elbowOutsideFactor = Mathf.Clamp01(
                                     Mathf.Clamp01((startBelowZ - localTargetPos.z) /
                                                   Mathf.Abs(startBelowZ) * .5f) *
                                     Mathf.Clamp01((localTargetPos.y - startAboveY) /
                                                   Mathf.Abs(startAboveY)) *
                                     Mathf.Clamp01(1f - localTargetPos.x * (isLeft ? -1f : 1f))
                                 ) * weight1;

            Vector3 shoulderHandDirection = (transform.position - wrist.position).normalized;
            Vector3 targetDir = transform.rotation * (Vector3.up + (correctElbowOutside ? (armDirection + Vector3.forward * -.2f) * elbowOutsideFactor : Vector3.zero));
            Vector3 cross = Vector3.Cross(shoulderHandDirection, targetDir * 1000f);

            Vector3 upperArmUp = shoulderRotation * Vector3.up;

            float elbowTargetUp = Vector3.Dot(upperArmUp, targetDir);
            float elbowAngle = Vector3.Angle(cross, upperArmUp) + (isLeft ? 180f : 0f);
            Quaternion rotation = Quaternion.AngleAxis(elbowAngle * Mathf.Sign(elbowTargetUp), shoulderHandDirection);
            transform.rotation = rotation * transform.rotation;
        }

        void correctElbowAfterPositioning()
        {
            Vector3 localTargetPos = transform.InverseTransformPoint(target.position) / upperBody.armLength;
            Vector3 shoulderHandDirection = (transform.position - wrist.position).normalized;
            Vector3 elbowPos = localElbowPos;

            if (isLeft)
                elbowPos.x *= -1f;

            Vector3 targetDir = transform.rotation * elbowPos.normalized;
            Vector3 cross = Vector3.Cross(shoulderHandDirection, targetDir);

            Vector3 upperArmUp = shoulderRotation * Vector3.up;


            Vector3 distance = target.position - transform.position;
            distance = distance.magnitude * transform.InverseTransformDirection(distance / distance.magnitude);

            float weight = Mathf.Clamp01(Mathf.Clamp01((startBelowDistance - distance.xz().magnitude / upperBody.armLength) /
                           startBelowDistance) * weight2 + Mathf.Clamp01((-distance.z + .1f) * 3)) *
                           Mathf.Clamp01((startBelowY - localTargetPos.y) /
                                         startBelowY);

            float elbowTargetUp = Vector3.Dot(upperArmUp, targetDir);
            float elbowAngle2 = Vector3.Angle(cross, upperArmUp) + (isLeft ? 180f : 0f);
            Quaternion rotation = Quaternion.AngleAxis((elbowAngle2 * Mathf.Sign(elbowTargetUp)).toSignedEulerAngle() * Mathf.Clamp(weight, 0, 1f), shoulderHandDirection);
            transform.rotation = rotation * transform.rotation;
        }

        void rotateElbowWithHandRight()
        {
            Vector3 handUpVec = target.rotation * Vector3.up;
            float forwardAngle = VectorHelpers.getAngleBetween(elbow.rotation * Vector3.right, target.rotation * Vector3.right,
                elbow.rotation * Vector3.up, elbow.rotation * Vector3.forward);

            // todo reduce influence if hand local forward rotation is high (hand tilted inside)
            Quaternion handForwardRotation = Quaternion.AngleAxis(-forwardAngle, elbow.rotation * Vector3.forward);
            handUpVec = handForwardRotation * handUpVec;

            float elbowTargetAngle = VectorHelpers.getAngleBetween(elbow.rotation * Vector3.up, handUpVec,
                elbow.rotation * Vector3.forward, elbow.rotation * armDirection);

            float deltaElbow = (elbowTargetAngle + (isLeft ? -handDeltaOffset : handDeltaOffset)) / 180f;

            deltaElbow = Mathf.Sign(deltaElbow) * Mathf.Pow(Mathf.Abs(deltaElbow), handDeltaPow) * 180f * handDeltaFactor;
            interpolatedDeltaElbow =
                Mathf.LerpAngle(interpolatedDeltaElbow, deltaElbow, Time.deltaTime / rotateElbowWithHandDelay);

            rotateWithAngle(interpolatedDeltaElbow);
        }

        void rotateElbowWithHandFoward()
        {
            Vector3 handRightVec = target.rotation * armDirection;

            float elbowTargetAngleForward = VectorHelpers.getAngleBetween(elbow.rotation * armDirection, handRightVec,
                elbow.rotation * Vector3.up, elbow.rotation * Vector3.forward);

            float deltaElbowForward = (elbowTargetAngleForward + (isLeft ? -handDeltaForwardOffset : handDeltaForwardOffset)) / 180f;

            if (Mathf.Abs(deltaElbowForward) < handDeltaForwardDeadzone)
                deltaElbowForward = 0f;
            else
            {
                deltaElbowForward = (deltaElbowForward - Mathf.Sign(deltaElbowForward) * handDeltaForwardDeadzone) / (1f - handDeltaForwardDeadzone);
            }

            deltaElbowForward = Mathf.Sign(deltaElbowForward) * Mathf.Pow(Mathf.Abs(deltaElbowForward), handDeltaForwardPow) * 180f;
            interpolatedDeltaElbowForward = Mathf.LerpAngle(interpolatedDeltaElbowForward, deltaElbowForward, Time.deltaTime / rotateElbowWithHandDelay);

            float signedInterpolated = interpolatedDeltaElbowForward.toSignedEulerAngle();

            rotateWithAngle(signedInterpolated * handDeltaForwardFactor);
        }

        private float calcAngleAboutHandShoulderAxis()
        {
            // modify: hand.position
            Vector3 localHandPosNormalized = transform.InverseTransformDirection(target.position);// / upperBody.armLength;
            float angle = offsetAngle;

            // angle from Y
            angle += yWeight * localHandPosNormalized.y;

            // angle from Z
            if (localHandPosNormalized.y > 0)
                angle += zWeightTop * (Mathf.Max(zStartDistance - localHandPosNormalized.z, 0f) * Mathf.Max(localHandPosNormalized.y, 0f));
            else
                angle += zWeightBottom * (Mathf.Max(zStartDistance - localHandPosNormalized.z, 0f) * Mathf.Max(-localHandPosNormalized.y, 0f));

            // angle from X
            angle += xWeight * Mathf.Max(localHandPosNormalized.x * (isLeft ? 1.0f : -1.0f) + xStartDistance, 0f);

            // clamp angle
            angle = Mathf.Clamp(angle, minAngle, maxAngle);

            return angle * (isLeft? 1.0f : -1.0f);
        }

        private float calcAngleAboutElbowAngle()
        {
            float angle = 0.0f;
            Vector3 targetShoulderDirection = (target.position - transform.position).normalized;
            float targetShoulderDistance = (target.position - transform.position).magnitude;

            angle = (isLeft ? -1.0f : 1.0f) *
                Mathf.Acos(Mathf.Clamp((Mathf.Pow(targetShoulderDistance, 2.0f) + Mathf.Pow(upperBody.upperArmLength, 2.0f) -
                            Mathf.Pow(upperBody.lowerArmLength, 2.0f)) / (2.0f * targetShoulderDistance * upperBody.upperArmLength), -1.0f, 1.0f)) * Mathf.Rad2Deg;
            if (float.IsNaN(angle))
                angle = 0.0f;

            return angle;
        }
    }
}
