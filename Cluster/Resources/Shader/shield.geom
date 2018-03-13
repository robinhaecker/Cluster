#version 420 core



layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

in Vertex {
	vec4 rgba;
	vec2 size;
	vec2 health;
	float shw;
	//vec2 texCoords;
} vex[1];

out Fragment {
	vec4 rgba;
	vec3 subCoords;
	vec2 health;
} frag;


void main(void)
{
	//Farbe bleibt für alle Vertices gleich.
	frag.rgba = vex[0].rgba;
	frag.health = vex[0].health;
	//frag.health = vec2(0.4, 0.85);
	
	//oben links
	frag.subCoords = vec3(-1.0, +1.0, 1.0/vex[0].shw);
	gl_Position = gl_in[0].gl_Position +  + vec4(-vex[0].size.x, +vex[0].size.y, 0.0, 0.0);
	EmitVertex();
	
	//unten links
	frag.subCoords = vec3(-1.0, -1.0, 1.0/vex[0].shw);
	gl_Position = gl_in[0].gl_Position + vec4(-vex[0].size.x, -vex[0].size.y, 0.0, 0.0);
	EmitVertex();
	
	//oben rechts
	frag.subCoords = vec3(+1.0, +1.0, 1.0/vex[0].shw);
	gl_Position = gl_in[0].gl_Position +  + vec4(+vex[0].size.x, +vex[0].size.y, 0.0, 0.0);
	EmitVertex();
	
	//unten rechts
	frag.subCoords = vec3(+1.0, -1.0, 1.0/vex[0].shw);
	gl_Position = gl_in[0].gl_Position + vec4(+vex[0].size.x, -vex[0].size.y, 0.0, 0.0);
	EmitVertex();
	
	
	EndPrimitive();
}