#version 420 core

//layout(binding = 0) uniform sampler2D font;


in Fragment {
	vec4 rgba;
} frag;


out vec4 out_Color;


void main(void)
{
	out_Color = frag.rgba;
}