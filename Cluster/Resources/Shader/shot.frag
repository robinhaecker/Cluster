#version 420 core

in Fragment {
	vec4 rgba;
} frag;


out vec4 out_Color;


void main(void)
{
	out_Color = frag.rgba;
}