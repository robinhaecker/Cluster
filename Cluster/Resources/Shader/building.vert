#version 420 core

layout(location = 0) in vec2  in_Position;
layout(location = 1) in vec3  in_Colour;

uniform vec3 scroll;
uniform vec3 viewport;

uniform float pos_x;
uniform float pos_y;
uniform float size;

uniform vec3 rgb;
uniform vec3 info;

/*
out VS_OUT {
	float pass_height;
	vec3  pass_color;
	} vs_out;
*/
out float pass_height;
out vec3 pass_color;


void main(void)
{

	//Um den Planeten biegen
	//float alpha0 = info.x / size * 2.0 * 3.1415265358979323;
	//float alpha1 = 1.0 / size * 3.1415265358979323;
	
	float alpha = (info.x + 0.5-in_Position.x*0.9) / size * 2.0 * 3.1415265358979323;
	float height = 20.0*size + max(-info.y, min(in_Position.y, info.y)) * 180.0;
	vec2 vertex = vec2(cos(alpha)*height, sin(alpha)*height);
	
	//vertex = in_Position*50.0;
	
	//vec2 vertex = in_Position*50.0 + vec2(20.0*size, 0.0);
	
	gl_Position = vec4( viewport.xy * (vertex + vec2(pos_x-scroll.x, pos_y-scroll.y))*scroll.z , 0.0, 1.0);
	//gl_Position = vec4( in_Position, 0.0, 1.0);//*0.25*scroll.z , 0.0, 1.0);
	
	pass_height = in_Position.y;
	pass_color = in_Colour;
	/*
	vs_out.pass_height = in_Position.y;
	vs_out.pass_color = in_Colour;*/
	
}