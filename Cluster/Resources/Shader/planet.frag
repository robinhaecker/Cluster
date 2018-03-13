#version 420 core


uniform float pos_x;
uniform float pos_y;
uniform vec3 rgb;
uniform float size;

in vec2 pass_height;
flat in vec3 pass_color;



out vec4 out_Color;


void main(void)
{
	vec3 tercol = pass_color;
	if (pass_height.x != pass_height.y) // Ist nur bei Wasser der Fall --> Dunkel-Hell Farbverlauf
	{
		tercol.xy *= 0.0125;
		tercol += vec3(0.015*(pass_height.y-18.75*size),0.015*(pass_height.y-18.75*size),0.0);
	}
	float blend = sqrt(max( 0.0, min(1.0, (pass_height.x - 19.75*size)) ));//*0.5+0.5;
	//if (pass_height.x < 19.75*size) blend=0.0;
	float bright= 1.0 - max( 0.0, min(1.0, pass_height.y / (20.0*size)))*0.95;
	//bright = 0.35*bright;//*bright;
	
	vec3 fractions = vec3(1.0, 0.0, 0.0);
	if (pass_height.y <= 12.5*size && pass_height.y >= 4.5*size) {fractions.x = (pass_height.y/size-4.5)/(12.5-4.5); fractions.y = 1.0-fractions.x;}
	if (pass_height.y <= 4.5*size && pass_height.y >= 1.5*size) {fractions.x = 0.0; fractions.z = 1.0 - (pass_height.y/size-1.5)/(4.5-1.5); fractions.y = 1.0-fractions.z;}
	if (pass_height.y <= 1.5*size) {fractions = vec3(0.0, 0.0, 1.0);}
	bright = bright * 0.5;//*(pass_height.y/size-17.5)/(20.0-17.5);
	fractions = vec3(0.41, 0.25, 0.0)*fractions.x + vec3(0.61, 0.35, 0.02)*fractions.y + vec3(0.81, 0.45, 0.02)*fractions.z;
	//vec3 inside = vec3(0.41, 0.25, 0.0) * (1.0-bright) + rgb*(bright);
	vec3 inside = fractions * (1.0-bright) + rgb*(bright);
	if (pass_height.x > 19.5*size && pass_height.x != pass_height.y) {inside *= 1.5;}
	if (pass_height.x >= 19.0*size && pass_height.x == pass_height.y)
	{
		float blendy = (pass_height.x/size-19.0) * 0.7;
		inside = inside * (1.0-blendy) + pass_color * blendy;
	}
	
	//if (pass_height.y < 12.5*size) {inside = vec3(0.61, 0.35, 0.02) * (1.0-bright) + rgb*(bright);}
	//if (pass_height.y < 4.5*size) {inside = vec3(0.81, 0.45, 0.02) * (1.0-bright) + rgb*(bright);}
	
	out_Color = vec4(tercol*blend + inside *(1.0-blend) , 0.3+(blend + bright));
}