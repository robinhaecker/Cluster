﻿using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class MeshButton : Button
    {
        private Mesh mesh;
        private Mesh background;
        
        internal MeshButton(Mesh mesh, Mesh background = null, float x = 0, float y = 0, float size = Properties.BUTTON_SIZE_DEFAULT) : base(x, y, size)
        {
            this.mesh = mesh;
            this.background = background;
        }

        public override void render()
        {
            base.render();
            mesh.deferred_drawFit(x, y, width, height);
            background?.deferred_drawFit(x, y, width, height);
        }
    }
}