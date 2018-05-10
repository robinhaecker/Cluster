#version 420 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec2 health;
layout(location = 2) in vec4 rgba;

uniform vec3 scroll;
uniform vec3 viewport;
uniform float pos_x;
uniform float pos_y;
uniform vec3 rgb;
uniform float size;

out Vertex {
	vec2 size;
} vex;


void main(void)
{
	vex.size = viewport.xy * (size * 20.0f + 400.0f) *scroll.z;
	gl_Position = vec4( viewport.xy*(vec2(pos_x, pos_y)-scroll.xy)*scroll.z, -0.65, 1.0);
}