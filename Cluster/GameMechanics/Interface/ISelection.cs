﻿using System;
using Cluster.GameMechanics.Behaviour;
using Cluster.Mathematics;

namespace Cluster.GameMechanics.Interface
{
    public interface ISelection
    {
        Target pickTarget(float x, float y);
        bool selectByPick(float x, float y);
        bool selectByBox(Vec4 box);
        void setActive(bool active);
        bool isMouseOver();
        
        Vec2 getCenterOfMass();
        void updateGui();
        void renderGui();
        string getToolTipText();
    }
}