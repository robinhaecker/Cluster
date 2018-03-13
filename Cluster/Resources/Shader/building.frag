#version 420 core


uniform vec3 rgb;
uniform float size;
uniform vec3 info;

//

in float pass_height;
in vec3 pass_color;

out vec4 out_Color;

void main(void)
{
	//if ( pass_height > info.y) discard;
	out_Color = vec4(pass_color*rgb, info.z);
}
/*
in GS_OUT {
	float pass_height;
	vec3  pass_color;
} gs_out;


out vec4 out_Color;


void main(void)
{
	if ( gs_out.pass_height > info.y) discard;
	out_Color = vec4(gs_out.pass_color*rgb, info.z);
}*/