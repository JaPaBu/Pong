#version 430

layout (location = 0) in vec2 inTexCoord;

layout (location = 0) out vec2 outTexCoord;

void main()
{
	outTexCoord = inTexCoord;
	gl_Position = vec4(inTexCoord.x*2 - 1, -inTexCoord.y*2 + 1, 0, 1);
}