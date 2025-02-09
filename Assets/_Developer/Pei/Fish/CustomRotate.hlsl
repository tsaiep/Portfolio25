void CustomRotate_float(float3 In, float3 Axis, float Rotation, out float3 Out, out float3x3 RotateMatrix)
{
    #ifdef SHADERGRAPH_PREVIEW
    Out = float3(1,1,1);
    RotateMatrix = 0;
    #else
    Rotation = radians(Rotation);
    float s = sin(Rotation);
    float c = cos(Rotation);
    float one_minus_c = 1.0 - c;
    Axis = normalize(Axis);
    float3x3 rot_mat =
    {   one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
        one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
        one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
    };
    Out = mul(rot_mat,  In);
    RotateMatrix = rot_mat;
    #endif
}