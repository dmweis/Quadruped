using System.Numerics;

namespace Quadruped.WebInterface.VideoStreaming
{
    public interface ICameraController
    {
        void CenterView();
        void StartMove(Vector2 direction);
        void StopMove();
    }
}
