texture Texture;
sampler2D TextureSampler = sampler_state
{
    texture = <Texture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};
bool ApplyLinear;
float4 OriginalPrimaryColor;
float4 NewPrimaryColor;
float4 OriginalSecondaryColor;
float4 NewSecondaryColor;

float4 GetColor(float2 texCoord)
{
    float4 texPoint = tex2D(TextureSampler, texCoord);
    if (distance(texPoint.rgb, OriginalPrimaryColor.rgb) < 0.05f)
    {
        return NewPrimaryColor;
    }
    if (distance(texPoint.rgb, OriginalSecondaryColor.rgb) < 0.05f)
    {
        return NewSecondaryColor;
    }
    return texPoint;
}

float4 SampleBilinear(float2 texCoord, float2 texelSize)
{
    float2 f = frac(texCoord / texelSize);
    float2 base = (floor(texCoord / texelSize)) * texelSize;

    float4 tl = GetColor(base);
    float4 tr = GetColor(base + float2(texelSize.x, 0));
    float4 bl = GetColor(base + float2(0, texelSize.y));
    float4 br = GetColor(base + texelSize);

    float4 top = lerp(tl, tr, f.x);
    float4 bot = lerp(bl, br, f.x);
    return lerp(top, bot, f.y);
}

float4 ThemeShader(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    if (!ApplyLinear)
    {
        return GetColor(texCoord) * color;
    }
    float2 texelSize = float2(ddx(texCoord.x), ddy(texCoord.y));
    float4 texLinear = SampleBilinear(texCoord, texelSize);
    return texLinear * color;
}

technique Theme
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 ThemeShader();
    }
}