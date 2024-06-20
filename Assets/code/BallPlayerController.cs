using System;
using System.Collections;
using UnityEngine;

namespace code
{
    public class BallPlayerController : MonoBehaviour
    {
        public bool IsAcceptControl { get; private set; }

        private bool _isActing = false;

        private GameObject _aimBase;
        private GameObject _aimArrow;
        private Transform _frontSight;

        private void Awake()
        {
            _aimBase = transform.Find("Display/Aim").gameObject;
            _aimArrow = transform.Find("Display/AimArrowAnchor").gameObject;
            _frontSight = _aimArrow.transform.Find("AimArrow");
            _aimBase.SetActive(false);
            _aimArrow.SetActive(false);
            
        }

        public IEnumerator DoPlayerAction()
        {
            IsAcceptControl = true;
            _isActing = true;
            return new WaitUntil(() => _isActing == false);
        }

        public void StartAim()
        {
            _aimBase.SetActive(true);
        }

        public void PreviewMoveAction(Vector3 aimPoint)
        {
            var aimDir = aimPoint - transform.position;
            if (aimDir.magnitude > 0.1f)
            {
                _aimArrow.SetActive(true);
                _aimArrow.transform.rotation = Quaternion.FromToRotation(Vector3.forward, aimDir);
                _frontSight.position = aimPoint;
            }
            else
            {
                _aimArrow.SetActive(false);
            }
        }

        public void DoMove(Vector3 aimPoint)
        {
            IsAcceptControl = false;
            _aimBase.SetActive(false);
            _aimArrow.SetActive(false);
            
            GetComponent<BallMovement>().DoMove(aimPoint);
            StartCoroutine(WaitMoveOver());
        }

        private IEnumerator WaitMoveOver()
        {
            var movement = GetComponent<BallMovement>();
            yield return new WaitUntil(() => !movement.IsMoving);
            _isActing = false;
        }
    }
}
