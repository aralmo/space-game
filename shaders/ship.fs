#version 330

// Input vertex attributes (from vertex shader)
in vec3 fragPosition;
in vec2 fragTexCoord;
in vec4 fragColor; // Use vertex color instead of texture
in vec3 fragNormal;

// Input uniform values
// Removed texture0 as it's no longer needed
uniform vec4 colDiffuse;
// Output fragment color
out vec4 finalColor;

void main()
{
    // Use vertex color instead of texel color
    vec4 vertexColor = fragColor;
    vec3 lightDot = vec3(0.0);
    vec3 normal = normalize(fragNormal);
    vec3 specular = vec3(0.0);
    bool isWindow = (fragColor.r == 1.0 && fragColor.g == 1.0 && fragColor.b == 0.0);
    // NOTE: Implement here your fragment shader code
    if (isWindow)
    {
        finalColor = fragColor;
    }
    else{
        vec3 light = vec3(0.0);
        light = normalize(-fragPosition);     
        float NdotL = max(dot(normal, light), 0.0);
        lightDot += vec3(.3) * NdotL;
        finalColor = (vertexColor+ (vertexColor *  vec4(lightDot, 1.0)))*.8;       
    }
}
