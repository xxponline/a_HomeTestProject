using UnityEngine;
using UnityEngine.EventSystems;

namespace code.UI
{
    public class UIControllerPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private bool _controlling;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_controlling)
            {
                if (BallGameDirector.GetInstance().BallPlayerController.IsAcceptControl)
                {
                    var checkRay = Camera.main!.ScreenPointToRay(eventData.position);
                    var playerBall = BallGameDirector.GetInstance().BallPlayerController.GetComponent<Ball>();
                    if (GamePhysicsUtils.CheckIntersect(checkRay, playerBall))
                    {
                        playerBall.GetComponent<BallPlayerController>().StartAim();
                        _controlling = true;
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_controlling)
            {
                // control
                var checkRay = Camera.main!.ScreenPointToRay(eventData.position);
                var gameYard = BallGameDirector.GetInstance().gameYard;
                var playerBall = BallGameDirector.GetInstance().BallPlayerController.GetComponent<BallPlayerController>();
                var controlAimPoint = GamePhysicsUtils.RayCastGameYard(checkRay, gameYard);
                playerBall.DoMove(controlAimPoint);
                _controlling = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_controlling)
            {
                var checkRay = Camera.main!.ScreenPointToRay(eventData.position);
                var gameYard = BallGameDirector.GetInstance().gameYard;
                var playerBall = BallGameDirector.GetInstance().BallPlayerController.GetComponent<BallPlayerController>();
                var controlAimPoint = GamePhysicsUtils.RayCastGameYard(checkRay, gameYard);
                playerBall.PreviewMoveAction(controlAimPoint);
            }
        }
    }
}
