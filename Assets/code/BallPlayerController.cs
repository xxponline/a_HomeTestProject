using System.Collections;
using UnityEngine;

namespace code
{
    public class BallPlayerController : MonoBehaviour
    {
        public bool IsAcceptControl { get; private set; }

        private bool _isActing = false;
        
        public IEnumerator DoPlayerAction()
        {
            IsAcceptControl = true;
            _isActing = true;
            return new WaitUntil(() => _isActing == false);
        }


        public void PreviewMoveAction(Vector3 aimPoint)
        {
            
        }

        public void DoMove(Vector3 aimPoint)
        {
            IsAcceptControl = false;
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
