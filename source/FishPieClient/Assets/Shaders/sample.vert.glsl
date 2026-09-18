#version 460 core

struct VertexData {
    float position[3];
    float colour[3];
};

struct ObjectData {
    mat4 model;
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

vec3 get_position(int index) {
    return vec3(
        data[index].position[0],
        data[index].position[1],
        data[index].position[2]
    );
}

vec3 get_colour(int index) {
    return vec3(
        data[index].colour[0],
        data[index].colour[1],
        data[index].colour[2]
    );
}

layout(location = 0) out vec3 out_colour;

void main(void) {
    gl_Position = projection * view * object_data[gl_DrawID].model * vec4(get_position(gl_VertexID), 1.0);
    out_colour = get_colour(gl_VertexID);
}