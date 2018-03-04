using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Cluster.math;


namespace Cluster.Rendering.Draw3D.Collisions
{
	class Collision
	{

		public static bool SphereSphere(vec4 s1, vec4 s2)
		{
			float dist = (new vec3(s1) - new vec3(s2)).length();
			return (dist > (s1.w + s2.w) * (s1.w + s2.w));
		}

		public static bool RaySphere(Ray r, vec4 s)
		{
			vec3 oh = new vec3(s) - r.o;
			float a = r.d * r.d;
			float b = r.d * oh;
			float c = oh * oh;

			if (b*b - a * (c - s.w*s.w) < 0.0f) return false;
			return true;
		}


		// Nicht optimiert, gibt evtl einiges, was man da machen kann. Einfacher Marching Cubes-Ähnlicher Algorithmus.
		public static vec3 RayTerrain(Ray r)
		{
			for (r.t = r.min_t; r.t < 100.0f; r.t += 0.05f)
			{
				vec3 pos = r.o + r.t * r.d;
				if (pos.x * pos.x + pos.z * pos.z > Terrain.getSize() * Terrain.getSize()) continue;
				if (Terrain.getHeight(pos.x, pos.z) >= pos.y) return pos;
			}
			return null; // Null if there is no intersection
		}


	}
}
/*

Type Collision
	
	
	Function FindInstanceInOctree(shoot:Ray, oct:Node, picked:Instance Var)
		For Local inst:Instance=EachIn oct.inside
			If inst.hidden Then Continue
			If RayModel(shoot, inst.from, inst) Then
				picked=inst
			EndIf
		Next
		
		For Local sub:Node=EachIn oct.child
			If RayNode(shoot, sub) Then FindInstanceInOctree(shoot, sub, picked)
			'Wird stattdessen eine RaySphere-Kollision gemacht, so können aus irgend einem Grund nicht alle Objekte gepickt werden.
			'Außerdem ist nicheinmal sicher, dass es dann schneller wäre (und wenn, dann nur unwesentlich).
			'If RaySphere(shoot, vec4.Create(sub.pos.v[0], sub.pos.v[1], sub.pos.v[2], sub.size)) Then FindInstanceInOctree(shoot, sub, picked)
		Next
	End Function
	
	
	
	Function RayNode:Byte(ray:Ray, node:Node)
		Local ro:vec3=vec3.Create(ray.o.v[0]-node.pos.v[0], ray.o.v[1]-node.pos.v[1], ray.o.v[2]-node.pos.v[2])
		Local colx:Byte, coly:Byte
		
		If ray.d.v[0]<>0 Then
			Local lambda1:Float=-(node.size+ro.v[0])/ray.d.v[0]
			If lambda1>0.0 Then
				colx=True
			Else
				Local lambda2:Float=(node.size-ro.v[0])/ray.d.v[0]
				If lambda2>0.0 Then colx=True
			EndIf
		ElseIf Abs(ro.v[0])<node.size Then
			colx=True
		EndIf
		If colx=False Then Return False
		
		If ray.d.v[1]<>0 Then
			Local my1:Float=-(node.size+ro.v[1])/ray.d.v[1]
			If my1>0.0 Then
				coly=True
			Else
				Local my2:Float=(node.size-ro.v[1])/ray.d.v[1]
				If my2>0.0 Then coly=True
			EndIf
		ElseIf Abs(ro.v[1])<node.size Then
			coly=True
		EndIf
		Return colx*coly
	End Function
	
	
	
	Function RayBox:Byte(ray:Ray, center:vec3, size:vec3)
		Local ro:vec3=vec3.Create(ray.o.v[0]-center.v[0], ray.o.v[1]-center.v[1], ray.o.v[2]-center.v[2])
		Local colx:Byte, coly:Byte
		
		If ray.d.v[0]<>0 Then
			Local lambda1:Float=-(size.v[0]+ro.v[0])/ray.d.v[0]
			If lambda1>0.0 Then
				colx=True
			Else
				Local lambda2:Float=(size.v[0]-ro.v[0])/ray.d.v[0]
				If lambda2>0.0 Then colx=True
			EndIf
		ElseIf Abs(ro.v[0])<size.v[0] Then
			colx=True
		EndIf
		If colx=False Then Return False
		
		If ray.d.v[1]<>0 Then
			Local my1:Float=-(size.v[1]+ro.v[1])/ray.d.v[1]
			If my1>0.0 Then
				coly=True
			Else
				Local my2:Float=(size.v[1]-ro.v[1])/ray.d.v[1]
				If my2>0.0 Then coly=True
			EndIf
		ElseIf Abs(ro.v[1])<size.v[1] Then
			coly=True
		EndIf
		Return colx*coly
	End Function
	
	
	
	Function RayTriangle:Byte(ray:Ray, tri:vec3[], frontfacing_only:Byte=True)
		Local vec_dst:vec3=vec3.Create(ray.o.v[0]-tri[0].v[0], ray.o.v[1]-tri[0].v[1], ray.o.v[2]-tri[0].v[2])
		Local vec_r1:vec3=vec3.Create(tri[1].v[0]-tri[0].v[0], tri[1].v[1]-tri[0].v[1], tri[1].v[2]-tri[0].v[2])
		Local vec_r2:vec3=vec3.Create(tri[2].v[0]-tri[0].v[0], tri[2].v[1]-tri[0].v[1], tri[2].v[2]-tri[0].v[2])
		Local minus_d:vec3=vec3.Create(-ray.d.v[0], -ray.d.v[1], -ray.d.v[2])
		
		Local det:Float=vec3.Det(minus_d, vec_r1, vec_r2)
		If det=0 Or (frontfacing_only And det>0) Then Return False
		
		Local t:Float=vec3.Det(vec_dst, vec_r1, vec_r2)/det
		If t<ray.min_t Then Return False
		
		Local lambda:Float=vec3.Det(minus_d, vec_dst, vec_r2)/det
		If lambda<0 Or lambda>1 Then Return False
		
		Local my:Float=vec3.Det(minus_d, vec_r1, vec_dst)/det
		If my<0 Or my+lambda>1 Then Return False
		
		If t<ray.t Then
			ray.t=t
			Return 2
		EndIf
		Return 1
	End Function
	
	
	
	
	Function RayMesh:Byte(ray:Ray, mesh:Mesh, mat:mat4, ani:Animation)
		Local k:Byte=False
		For Local I:Int=0 Until mesh.num_triangles
			If ani=Null Then
				If RayTriangle(ray, [vec3.Cast(mat.Transform(vec4.Cast(mesh.vertices[mesh.triangles[I*3]].pos))), vec3.Cast(mat.Transform(vec4.Cast(mesh.vertices[mesh.triangles[I*3+1]].pos))), vec3.Cast(mat.Transform(vec4.Cast(mesh.vertices[mesh.triangles[I*3+2]].pos)))])=2 Then k=True
			Else
				If RayTriangle(ray, [vec3.Cast(mat.Transform(ani.__vertex(mesh.triangles[I*3]))), vec3.Cast(mat.Transform(ani.__vertex(mesh.triangles[I*3+1]))), vec3.Cast(mat.Transform(ani.__vertex(mesh.triangles[I*3+2])))])=2 Then k=True
			EndIf
		Next
		Return k
	End Function
	
	
	Function RayModel:Byte(ray:Ray, model:Model, inst:Instance)
		Local mat:mat4=inst.__matrix()
		If model.bounding_box_size=Null Then model.__update_bounding_box()
		Local sphere:vec4=vec4.Create(mat.m[3,0], mat.m[3,1], mat.m[3,2], mat.Transform(vec4.Cast(model.bounding_box_size, 0.0)).Length(False))
		If Not RaySphere(ray, sphere) Then Return False
		For Local mesh:Mesh=EachIn model.meshes
			Local ani:Animation
			If mesh.anim<>Null Then ani=inst.GetAnimation(mesh)
			If RayMesh(ray, mesh, mat, ani) Then Return True
		Next
		Return False
	End Function
	
	
End Type


*/