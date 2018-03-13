#version 420 core

layout(location = 0) in vec3  in_Position;
layout(location = 1) in vec4  in_Colour;
layout(location = 2) in vec2  in_TexCoords;


uniform vec3 viewport;

out vec4 pass_color;
out vec2 texcoord;


void main(void)
{
	gl_Position = vec4( -1.0+viewport.x*in_Position.x*2.0, 1.0+viewport.y*(1.0-in_Position.y)*2.0, in_Position.z, 1.0);
	pass_color = in_Colour;
	texcoord = in_TexCoords;
}