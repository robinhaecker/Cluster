#version 420 core

layout(location = 0) in vec2 pos;
layout(location = 1) in vec4 col;


uniform vec3 scroll;
uniform vec3 viewport;


out vec4 color;


void main(void)
{
	color = col;
	
	gl_Position = vec4(viewport.xy*(pos.xy - scroll.xy)*scroll.z, 0.0, 1.0);
}