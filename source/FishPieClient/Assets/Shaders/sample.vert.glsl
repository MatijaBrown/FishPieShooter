#version 460 core

struct VertexData {
    float position[3];
    float uv[2];
};

struct ObjectData {
    mat4 model;
    uint index;
};

struct MaterialData {
    float colour[3];
};

layout(binding = 0, std430) readonly buffer vertices {
    VertexData data[];
};

layout(binding = 1, std430) readonly buffer camera {
    mat4 view;
    mat4 projection;
};

layout(binding = 2, std430) readonly buffer objects {
    ObjectData object_data[];
};

layout(binding = 3, std430) readonly buffer materials {
    MaterialData material_data[];
};

vec3 get_position(uint index) {
    return vec3(
        data[index].position[0],
        data[index].position[1],
        data[index].position[2]
    );
}

vec2 get_uv(uint index) {
    return vec2(
        data[index].uv[0],
        data[index].uv[1]
    );
}

layout(location = 0) flat out uint material_id;
layout(location = 1) out vec2 uv;

void main(void) {
    gl_Position = projection * view * object_data[gl_DrawID].model * vec4(get_position(gl_VertexID), 1.0);
    material_id = object_data[gl_DrawID].index;
    uv = get_uv(gl_VertexID);
}