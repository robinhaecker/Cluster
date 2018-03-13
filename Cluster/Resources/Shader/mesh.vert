#version 420 core

layout(location = 0) in vec2  in_Position;
layout(location = 1) in vec3  in_Colour;


uniform vec2 pos;
uniform vec3 scale;
uniform vec4 col;

out vec4 pass_color;


void main(void)
{
	pass_color = vec4(in_Colour * col.xyz, col.w);
	float py = max(min(in_Position.y, scale.z), -scale.z);
	gl_Position = vec4(in_Position.x * scale.x *2.0 + pos.x, py * scale.y*2.0 + (1.0-pos.y), -0.7, 1.0);
}