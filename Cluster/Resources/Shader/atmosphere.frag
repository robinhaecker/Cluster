#version 420 core

uniform vec3 scroll;
uniform vec3 viewport;
uniform float pos_x;
uniform float pos_y;
uniform vec3 rgb;
uniform float size;

in Fragment {
	vec2 texCoords;
} frag;

out vec4 out_Color;


void main(void)
{
	float radius = (length(frag.texCoords.xy) * (size * 20.0f + 400.0f) - (size * 20.0f + 50.0f)) / 350.0f;
	float radius2 = (length(frag.texCoords.xy) * (size * 20.0f + 400.0f) - (size * 20.0f)) / 400.0f;
	
	if (radius > 1.0) {discard;}
	
	float blend = 1.0 - 0.5 * (radius + radius2);
	vec3 color = (rgb * 0.25 + 0.75) * 0.5;
	out_Color = vec4(color, blend * blend * 0.7);
}