#version 460 core

layout(location = 0) in vec3 in_colour;

layout(location = 0) out vec4 out_colour;

void main(void) {
    out_colour = vec4(in_colour, 1.0);
}