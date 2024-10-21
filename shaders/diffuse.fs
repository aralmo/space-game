#version 330 core

in vec4 fragColor; // Received color from vertex shader
out vec4 finalColor; // Output color of the pixel

void main()
{
    finalColor = fragColor; // Set the pixel color to the calculated diffuse color
}