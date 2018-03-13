#version 420 core



layout(points) in;
layout(line_strip, max_vertices = 2) out;

in Vertex {
	vec3 rgb;
	vec2 alphas;
	vec2 p0;
	vec2 p1;
} vex[1];

out Fragment {
	vec4 rgba;
} frag;


void main(void)
{
	//Farbe bleibt für alle Vertices gleich.
	frag.rgba = vec4(vex[0].rgb, vex[0].alphas.x);
	
	gl_Position = vec4(vex[0].p0, 0.0, 1.0);
	EmitVertex();
	
	frag.rgba.a = vex[0].alphas.y;
	gl_Position = vec4(vex[0].p1, 0.0, 1.0);
	EmitVertex();
	
	
	/*
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
	*/
	
	EndPrimitive();
}