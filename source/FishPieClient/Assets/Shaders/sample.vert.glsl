#version 460 core

struct VertexData {
    float position[3];
};

layout(binding = 0, std430) readonly buffer vertices {
    VertexData data[];
};

vec3 get_position(int index) {
    return vec3(
        data[index].position[0],
        data[index].position[1],
        data[index].position[2]
    );
}

void main(void) {
    gl_Position = vec4(get_position(gl_VertexID), 1.0);
}