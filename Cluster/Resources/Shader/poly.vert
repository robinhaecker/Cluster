#version 420 core

layout(location = 0) in vec3  in_Position;
layout(location = 1) in vec4  in_Colour;


uniform vec3 viewport;

out vec4 pass_color;


void main(void)
{
	gl_Position = vec4( -1.0+viewport.x*in_Position.x*2.0, 1.0+viewport.y*(1.0-in_Position.y)*2.0, in_Position.z, 1.0);
	pass_color = in_Colour;
}