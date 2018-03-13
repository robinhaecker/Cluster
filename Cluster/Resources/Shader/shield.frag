#version 420 core

//layout(binding = 0) uniform sampler2D font;


in Fragment {
	vec4 rgba;
	vec3 subCoords;
	vec2 health;
} frag;


const float PI = 3.1415926535;
out vec4 out_Color;


void main(void)
{
	float radius = length(frag.subCoords.xy);
	float fraction = (atan(-frag.subCoords.x, -frag.subCoords.y) / PI + 1.0)*0.5;
	float alpha = 1.0;
	
	if (radius <= 1.0 && radius >= 0.8) // Schildefrag.health.y
	{
		alpha = 0.45 * (1.0 - (abs(0.9-radius) * 10.0)*(abs(0.9-radius) * 10.0));//max(abs(0.97-radius), abs(0.83-radius)) *20.0;
		if (fraction > frag.health.y) {discard;}
	}
	else if (radius <= 0.8 && radius >= 0.6) // Huellefrag.health.x
	{
		alpha = 0.6 * (1.0 - (abs(0.7-radius) * 10.0)*(abs(0.7-radius) * 10.0));//max(abs(0.97-radius), abs(0.83-radius)) *20.0;
		if (fraction > frag.health.x) {discard;}
	}
	else { discard;}
	
	out_Color = vec4(frag.rgba.rgb + vec3(0.3, 0.3, 0.3) * (alpha + 0.5), frag.rgba.a * alpha);//frag.rgba;
}