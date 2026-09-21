#version 330

in vec2 pass_uv;
in vec4 pass_colour;

layout(location = 0) out vec4 out_colour;

uniform sampler2D texture_sampler;

void main(void) {
    out_colour = pass_colour * texture(texture_sampler, pass_uv);
}