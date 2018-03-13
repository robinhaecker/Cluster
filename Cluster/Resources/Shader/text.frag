#version 420 core

layout(binding = 0) uniform sampler2D font;


in Fragment {
	vec4 rgba;
	vec2 texCoords;
} frag;



out vec4 out_Color;


void main(void)
{
	out_Color = frag.rgba * texture2D(font, frag.texCoords);// * frag.rgba;
	if(out_Color.a < 0.05) discard;
	//out_Color = vec4(frag.texCoords, 1.0, 1.0) * frag.rgba;
}