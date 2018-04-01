#version 430

layout (location = 0) in vec2 inTexCoord;

layout (location = 0) out vec4 outColor;

layout (location = 0) uniform dvec2 paddleSize;
layout (location = 1) uniform dvec2 paddle1Pos;
layout (location = 2) uniform dvec2 paddle2Pos;
layout (location = 3) uniform dvec3 ballPos;

bool isPaddle(vec2 pixelPos, dvec2 paddlePos)
{
	return pixelPos.x >= paddlePos.x && pixelPos.x <= paddlePos.x + paddleSize.x &&
		   pixelPos.y >= paddlePos.y && pixelPos.y <= paddlePos.y + paddleSize.y;
}

bool isBall(vec2 pixelPos)
{
	return distance(ballPos.xy, pixelPos) <= ballPos.z;
}

vec3 getColor()
{
	vec2 pixelPos = vec2(inTexCoord.x * 1280, inTexCoord.y * 720);

	if(isBall(pixelPos))
		return vec3(0, 1, 0);
	else if (isPaddle(pixelPos, paddle1Pos))
		return vec3(1, 0, 0);
	else if(isPaddle(pixelPos, paddle2Pos))
		return vec3(0, 0, 1);

	return vec3(0, 0, 0);
}

void main()
{
	outColor = vec4(getColor(), 1);
}