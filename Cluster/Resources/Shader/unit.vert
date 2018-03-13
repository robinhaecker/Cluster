#version 420 core

layout(location = 0) in vec2  in_Position;
layout(location = 1) in vec3  in_Colour;


uniform vec3 pos;
uniform vec4 col;
uniform vec3 scale;
//uniform vec3 shields;


out vec4 pass_color;


void main(void)
{
	pass_color = vec4(in_Colour * col.xyz * 0.75 + 0.25, col.w);
	
	vec2 vertex = vec2(sin(pos.z)*in_Position.x + cos(pos.z)*in_Position.y, sin(pos.z)*in_Position.y - cos(pos.z)*in_Position.x)*scale.x;
	
	gl_Position = vec4( scale.yz * (vertex + pos.xy) , -0.4, 1.0);
}