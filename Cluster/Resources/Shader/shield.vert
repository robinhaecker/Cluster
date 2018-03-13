#version 420 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec2 health;
layout(location = 2) in vec4 rgba;


uniform vec3 scroll;
uniform vec3 viewport;


out Vertex {
	vec4 rgba;
	vec2 size;
	vec2 health;
	float shw;
	//vec2 texCoords;
} vex;


void main(void)
{
	vex.health = health;
	vex.rgba = rgba;
	vex.shw = 1.0 / (pos.z);
	vex.size = viewport.xy*(pos.z*scroll.z*0.55 + 5.0);
	gl_Position = vec4( viewport.xy*(pos.xy - scroll.xy)*scroll.z, -0.5, 1.0);
	
	//vex.size = vec2(viewport.xy)*pos.z*2.0;
	//vex.rgba = rgba;
	//vex.texCoords = vec2( mod(chars, 16.0)/16.0, floor(chars/16.0)/16.0 );
}