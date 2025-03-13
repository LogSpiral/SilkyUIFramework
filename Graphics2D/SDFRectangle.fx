// --------------------------------
// 常量缓冲区定义（按逻辑分组）
// --------------------------------
sampler uImage0 : register(s0);  // 纹理采样器（虽然未使用，但保留以备扩展）

cbuffer MatrixBuffer : register(b0)
{
    float4x4 uTransformMatrix; // 顶点变换矩阵（需确保外部传入行主序）
    float4 uBackgroundColor; // 背景色
    float4 uBorderColor; // 边框颜色
    float4 uTransparencyColor; // 透明色（替换硬编码的0）
    float2 uSmoothstepRange; // 过渡范围（原uTransition，更名以提高可读性）
    float uBorder; // 边框宽度
    float uShadowSize; // 阴影大小
    float uInnerShrinkage; // 内缩量（用于形状收缩）
};

struct VSInput
{
    float2 Position : POSITION0;
    float2 Coord : TEXCOORD0;
    float Rounded : COLOR0;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 Coord : TEXCOORD0;
    float Rounded : COLOR0;
};

// --------------------------------
// 顶点着色器（所有通道共用）
// --------------------------------
PSInput VS_PCR(VSInput input)
{
    PSInput output;
    output.Position = mul(float4(input.Position, 0, 1), uTransformMatrix);
    output.Coord = input.Coord;
    output.Rounded = input.Rounded;
    return output;
}

float CalculateDistance(float2 q, float rounded)
{
    // 公式解释：
    // 1. min(max(q.x, q.y), 0) -> 矩形内部区域的负距离
    // 2. length(max(q, 0))     -> 矩形外部区域的正距离
    // 3. - rounded             -> 圆角半径修正
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - rounded;
}

/*float sdRoundedRectangle(float2 pos, float2 sizeOver2, float rounded)
{
    float2 q = pos - sizeOver2 + rounded;
    return min(max(q.x, q.y), 0) + length(max(q, 0)) - rounded;
}*/

// --------------------------------
// 像素着色器 - 带边框版本
// --------------------------------
float4 HasBorder(PSInput input) : SV_Target
{
    // 计算距离场（应用内缩）
    float distance = CalculateDistance(input.Coord, input.Rounded) + uInnerShrinkage;

    // 核心混合逻辑：
    // 1. 先混合背景色与边框色
    float4 color = lerp(
        uBackgroundColor,
        uBorderColor,
        smoothstep(uSmoothstepRange.x, uSmoothstepRange.y, distance + uBorder)
    );

    // 2. 再混合透明色
    return lerp(
        color,
        uTransparencyColor,
        smoothstep(uSmoothstepRange.x, uSmoothstepRange.y, distance)
    );
}

// --------------------------------
// 像素着色器 - 无边框版本
// --------------------------------
float4 NoBorder(PSInput input) : SV_Target
{
    float distance = CalculateDistance(input.Coord, input.Rounded) + uInnerShrinkage;
    return lerp(
        uBackgroundColor,
        uTransparencyColor,
        smoothstep(uSmoothstepRange.x, uSmoothstepRange.y, distance)
    );
}

// --------------------------------
// 像素着色器 - 阴影版本
// --------------------------------
float4 Shadow(PSInput input) : SV_Target
{
    // 注意：阴影不应用内缩(uInnerShrinkage)
    float distance = CalculateDistance(input.Coord, input.Rounded);

    // 扩展过渡范围以包含阴影
    float2 shadowRange = float2(
        uSmoothstepRange.x - uShadowSize,
        uSmoothstepRange.y
    );

    return lerp(
        uBackgroundColor,
        uTransparencyColor,
        smoothstep(shadowRange.x, shadowRange.y, distance)
    );
}

technique T1
{
    pass HasBorder
    {
        VertexShader = compile vs_3_0 VS_PCR();
        PixelShader = compile ps_3_0 HasBorder();
    }

    pass NoBorder
    {
        VertexShader = compile vs_3_0 VS_PCR();
        PixelShader = compile ps_3_0 NoBorder();
    }

    pass Shadow
    {
        VertexShader = compile vs_3_0 VS_PCR();
        PixelShader = compile ps_3_0 Shadow();
    }
}