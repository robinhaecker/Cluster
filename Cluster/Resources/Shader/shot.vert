#version 420 core

layout(location = 0) in vec4 pos;
layout(location = 1) in vec3 rgb;
layout(location = 2) in vec2 alphas;


uniform vec3 scroll;
uniform vec3 viewport;


out Vertex {
	vec3 rgb;
	vec2 alphas;
	vec2 p0;
	vec2 p1;
} vex;


void main(void)
{
	vex.rgb = rgb;
	vex.alphas = alphas;
	vex.p1 = viewport.xy*(pos.xy - scroll.xy)*scroll.z;
	vex.p0 = viewport.xy*(pos.zw - scroll.xy)*scroll.z;
	
	gl_Position = vec4(vex.p0, 0.0, 1.0);
	//vex.size = vec2(viewport.xy)*pos.z*2.0;
	//vex.rgba = rgba;
	//vex.texCoords = vec2( mod(chars, 16.0)/16.0, floor(chars/16.0)/16.0 );
}