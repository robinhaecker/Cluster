#version 420 core


layout(binding = 0) uniform sampler2D tex;

in vec4 pass_color;
in vec2 texcoord;

out vec4 out_Color;

void main(void)
{
	out_Color = pass_color * texture2D(tex, texcoord);
}
