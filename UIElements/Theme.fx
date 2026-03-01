texture Texture;
sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
};
float4 OriginalPrimary;
float4 NewPrimary;
float4 OriginalSecondary;
float4 NewSecondary;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, texCoord);
    if (distance(color.rgb, OriginalPrimary.rgb) < 0.05f)
    {
        color = NewPrimary;
    }
    if (distance(color.rgb, OriginalSecondary.rgb) < 0.05f)
    {
        color = NewSecondary;
    }
    return color;
}

technique Theme
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}