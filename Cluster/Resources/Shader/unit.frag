#version 420 core

in vec4 pass_color;

out vec4 out_Color;

void main(void)
{
	out_Color = pass_color;
	//out_Color = vec4(1.0, 1.0, 1.0, 1.0);
}
