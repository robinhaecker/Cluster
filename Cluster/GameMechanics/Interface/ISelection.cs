using Cluster.Mathematics;

namespace Cluster.GameMechanics.Interface
{
    public interface ISelection
    {
        bool selectByPick(float x, float y);
        bool selectByBox(Vec4 box);
        
        Vec2 getCenterOfMass();
        void updateGui();
        void renderGui();
    }
}