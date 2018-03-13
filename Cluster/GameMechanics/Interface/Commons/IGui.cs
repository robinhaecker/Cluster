namespace Cluster.GameMechanics.Interface.Commons
{
    public interface IGui
    {
        void setPosition(float x, float y);
        void updateState();
        bool isMouseOver();
        bool isActive();
        void render();
    }
}