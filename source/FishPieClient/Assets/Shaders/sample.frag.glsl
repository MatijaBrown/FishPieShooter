#version 460 core

struct MaterialData {
    float colour[3];
};

layout(binding = 3, std430) readonly buffer materials {
    MaterialData material_data[];
};

vec3 get_colour(uint index) {
    return vec3(
        material_data[index].colour[0],
        material_data[index].colour[1],
        material_data[index].colour[2]
    );
}

layout(location = 0) flat in uint material_id;

layout(location = 0) out vec4 out_colour;

void main(void) {
    out_colour = vec4(get_colour(material_id), 1.0);
}