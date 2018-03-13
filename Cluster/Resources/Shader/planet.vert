#version 420 core

layout(location = 0) in vec2  in_Position;
layout(location = 1) in float in_Terrain;
layout(location = 2) in vec3  in_Colour;

uniform vec3 scroll;
uniform vec3 viewport;

uniform float pos_x;
uniform float pos_y;
uniform vec3 rgb;
uniform float size;


out vec2 pass_height;
flat out vec3 pass_color;




void main(void)
{	
	
	//viewport.xy * + vec2(pos_x-scroll.x, pos_y-scroll.y))*scroll.z
	vec2 vertex = in_Position;
	bool isWater = false;
	float temp_waterdepth = 0.0;
	float wave = 0.0;
	if ( in_Terrain > -0.5 && in_Terrain < 0.5)
	{
		temp_waterdepth = 20.0*size - length(vertex);
		vec2 delta = in_Position / length(in_Position);
		wave = abs(sin(viewport.z* (sin(in_Position.x)+1.1) ) + cos(viewport.z * (cos(in_Position.y)+1.2) ));
		vertex = vertex / length(vertex)* 19.75*size + wave*delta *3.0;
		isWater = true;
	}
	
	gl_Position = vec4( viewport.xy * (vertex + vec2(pos_x-scroll.x, pos_y-scroll.y))*scroll.z , 0.0, 1.0);
	
	pass_height = vec2(length(vertex), length(vertex));//min( 1.0 , max( 0.0, length(in_Position) - 20.0*size) );
	if ( length(in_Position) < 1.0 ) pass_height = vec2(0.0, 0.0);
	if (isWater == true) pass_height += vec2(temp_waterdepth * 2.0 + wave, temp_waterdepth * 0.75);
	pass_color = in_Colour;
	
}