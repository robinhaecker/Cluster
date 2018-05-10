#version 420 core

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

uniform vec3 scroll;
uniform vec3 viewport;
uniform float pos_x;
uniform float pos_y;
uniform vec3 rgb;
uniform float size;

in Vertex {
	vec2 size;
} vex[1];

out Fragment {
	vec2 texCoords;
} frag;

void main(void)
{
	//Farbe bleibt für alle Vertices gleich.
	
	//oben links
	frag.texCoords = vec2(-1.0, +1.0);
	gl_Position = gl_in[0].gl_Position + vec4(-vex[0].size.x, +vex[0].size.y, 0.0, 0.0);
	EmitVertex();
	
	//unten links
	frag.texCoords = vec2(-1.0, -1.0);
	gl_Position = gl_in[0].gl_Position + vec4(-vex[0].size.x, -vex[0].size.y, 0.0, 0.0);
	EmitVertex();
	
	//oben rechts
	frag.texCoords = vec2(+1.0, +1.0);
	gl_Position = gl_in[0].gl_Position +  + vec4(+vex[0].size.x, +vex[0].size.y, 0.0, 0.0);
	EmitVertex();
	
	//unten rechts
	frag.texCoords = vec2(+1.0, -1.0);
	gl_Position = gl_in[0].gl_Position + vec4(+vex[0].size.x, -vex[0].size.y, 0.0, 0.0);
	EmitVertex();
	
	EndPrimitive();
}