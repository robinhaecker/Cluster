#version 420 core

in vec2 space;
in vec2 space1;
in vec2 space2;
in vec2 space3;

out vec4 out_Color;

layout(binding = 0) uniform sampler2D stars;
layout(binding = 1) uniform sampler2D star_brightness;
layout(binding = 2) uniform sampler2D nebulae1;
layout(binding = 3) uniform sampler2D nebulae2;
layout(binding = 4) uniform sampler2D dominanceMap;

const float DOMINANCE_MAP_SCALING = 1.0/ (25.0);

void main(void)
{

    vec4 dominance = texture2D(dominanceMap, (space * DOMINANCE_MAP_SCALING + 0.5));
	out_Color = (0.02+dominance) * (
	              texture2D(nebulae1, space * 0.036).r *0.5
	            + texture2D(star_brightness, space2 * 0.016).r *0.25
	            + texture2D(nebulae2, space1 * 0.016).r * 0.4
	            + 0.1);
	
	float starLayer0 = texture2D(stars, space).r * texture2D(star_brightness, space * 0.096).r * 2.85;
	float starLayer1 = texture2D(stars, space1).r * texture2D(star_brightness, space1 * 0.12).r * 2.85;
	float starLayer2 = texture2D(stars, space2).r * texture2D(star_brightness, space2 * 0.35).r * 2.0;
	float starLayer3 = texture2D(stars, space3).r * texture2D(star_brightness, space3 * 0.23).r * 1.8;
	
	out_Color = out_Color + starLayer1 + starLayer2 + starLayer3;
}
