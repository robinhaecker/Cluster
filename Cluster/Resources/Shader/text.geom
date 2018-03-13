#version 420 core

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

in Vertex {
	vec4 rgba;
	vec2 size;
	vec2 texCoords;
} vex[1];

out Fragment {
	vec4 rgba;
	vec2 texCoords;
} frag;


void main(void)
{
	//Farbe bleibt für alle Vertices gleich.
	frag.rgba = vex[0].rgba;
	
	//Oben links
	frag.texCoords = vex[0].texCoords;
	gl_Position = gl_in[0].gl_Position;
	EmitVertex();
	
	//unten links
	frag.texCoords = vex[0].texCoords + vec2(0.0, 1.0/16.5);
	gl_Position = gl_in[0].gl_Position + vec4(0.0, -vex[0].size.y, 0.0, 0.0);
	EmitVertex();
	
	//oben rechts
	frag.texCoords = vex[0].texCoords + vec2(1.0/16.5, 0.0);
	gl_Position = gl_in[0].gl_Position + vec4(vex[0].size.x, 0.0, 0.0, 0.0);
	EmitVertex();
	
	//unten rechts
	frag.texCoords = vex[0].texCoords + vec2(1.0/16.5, 1.0/16.5);
	gl_Position = gl_in[0].gl_Position + vec4(vex[0].size.x, -vex[0].size.y, 0.0, 0.0);
	EmitVertex();
	EndPrimitive();
}