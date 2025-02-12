// 世界视图投影矩阵
float4x4 MatrixTransform;

// 顶点着色器输入
struct VertexInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

// 像素着色器输入
struct PixelInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

PixelInput VS(VertexInput input)
{
    PixelInput output;
    output.Position = mul(input.Position, MatrixTransform);
    output.Color = input.Color;
    return output;
}

float4 PS(PixelInput input) : SV_TARGET
{
    return input.Color;
}

technique Basic
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}