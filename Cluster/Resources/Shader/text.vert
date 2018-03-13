#version 420 core

layout(location = 0) in vec4 rgba;
layout(location = 1) in float chars;
layout(location = 2) in vec3 pos;


uniform vec3 viewport;


out Vertex {
	vec4 rgba;
	vec2 size;
	vec2 texCoords;
} vex;


void main(void)
{
	gl_Position = vec4( -1.0+viewport.x*pos.x*2.0, 1.0+viewport.y*(1.0-pos.y)*2.0, -0.5, 1.0);
	
	vex.size = vec2(viewport.xy)*pos.z*2.0;
	vex.rgba = rgba;
	vex.texCoords = vec2( mod(chars, 16.0)/16.0, floor(chars/16.0)/16.0 );
}