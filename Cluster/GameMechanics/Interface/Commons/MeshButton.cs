using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class MeshButton : Button
    {
        private Mesh _mesh;
        
        internal MeshButton(Mesh mesh, float x, float y, float size = Properties.BUTTON_SIZE_DEFAULT) : base(x, y, size)
        {
            _mesh = mesh;
        }

        public new void render()
        {
            base.render();
            _mesh.deferred_drawFit(x, y, width, height);
        }
    }
}