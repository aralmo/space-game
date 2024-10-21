#version 330

// Input vertex attributes
in vec3 vertexPosition;
in vec3 vertexNormal;
in vec4 vertexColor;

// Input uniform values
uniform mat4 mvp;


// Output vertex attributes (to fragment shader)
out vec4 fragColor;


// NOTE: Add here your custom variables
vec4 norm(vec3 normals)
{
    if (abs(normals.x) > 0.)
    {
        return vec4(.6,.6,.6,1);
    }
    if (abs(normals.y) > 0.)
    {
        return vec4(.8,.8,.8,1);
    }
    if (abs(normals.z) > 0.)
    {
        return vec4(1,1,1,1);
    }
    return vec4(0.,0.,0.,1.);
}
void main()
{
    // Send vertex attributes to fragment shader
    fragColor = vertexColor * norm(vertexNormal);
    // Calculate final vertex position
    gl_Position = mvp*vec4(vertexPosition, 1.0);
}
