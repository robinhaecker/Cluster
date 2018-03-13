using Cluster.Mathematics;

namespace Cluster.GameMechanics.Interface
{
    public interface ISelection
    {
        Vec2 getCenterOfMass();
        void updateGui();
        void renderGui();
    }
}