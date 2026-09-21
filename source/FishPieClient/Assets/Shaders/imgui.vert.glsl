#version 410 core

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 uv;
layout(location = 2) in vec4 colour;

out vec2 pass_uv;
out vec4 pass_colour;

uniform mat4 projection;

void main(void) {
    pass_uv = uv;
    pass_colour = colour;
    gl_Position = projection * vec4(position, 0, 1);
}