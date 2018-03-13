#version 420 core

layout(location = 0) in vec2 pos;

uniform vec3 viewport;
uniform vec3 scroll;

out vec2 space;
out vec2 space1;
out vec2 space2;
out vec2 space3;

void main(void)
{
	gl_Position = vec4(pos, 0.6, 1.0);
	
	space = ((viewport.xy * pos / scroll.z) + scroll.xy )*0.001;
	space1 = ((viewport.xy * pos / scroll.z) + scroll.xy * 0.75 )*0.001;
	space2 = -((viewport.xy * pos / scroll.z) + scroll.xy * 0.5 )*0.001;
	space3 = ((viewport.xy * pos / scroll.z) + scroll.xy * 0.25 )*0.001;
	
}