attribute vec3 fuVertex;
attribute vec3 fuNormal;
attribute vec2 fuUV;
varying vec3 normal;
varying vec3 viewpos;
varying vec2 uv;
uniform mat4 FUSEE_MVP;
uniform mat4 FUSEE_MV;
uniform mat4 FUSEE_ITMV;


void main()
{
	uv = fuUV;
	normal = mat3(FUSEE_ITMV[0].xyz, FUSEE_ITMV[1].xyz, FUSEE_ITMV[2].xyz) * fuNormal;
    normal = normalize(normal);
    viewpos = (FUSEE_MV * vec4(fuVertex, 1.0)).xyz;
    gl_Position = FUSEE_MVP * vec4(fuVertex, 1.0);
}