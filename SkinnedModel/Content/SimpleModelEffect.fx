#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix WorldViewProjection;
float3 CameraPos;

float3 SunOrientation;
float Texture1Level;
Texture2D Texture1;
sampler TextureSampler1 = sampler_state
{
    Texture = <Texture1>;
};

struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float2 Uv : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float2 Uv : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.Normal = mul(input.Normal, World);
	output.Uv = input.Uv;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float cosTheta = clamp((dot( input.Normal, SunOrientation) / 2) + 0.5, 0, 1);
	float4 textureColor = (tex2D(TextureSampler1, input.Uv) * 1);// +(input.Color * (1 - Texture1Level)) * cosTheta;
	textureColor.a = 1;
	return textureColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};