#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

matrix World;
matrix WorldViewProjection;
Texture2D Texture1;
float3 SunOrientation;
float4x4 gBonesOffsets[50];

sampler TextureSampler1 = 
sampler_state
{
    Texture = <Texture1>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float2 Uv : TEXCOORD0;
	float4 blendIndices : BLENDINDICES; 
	float4 blendWeights : BLENDWEIGHT;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float2 Uv : TEXCOORD0;
	float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4 skinnedPosition = float4(0.0, 0.0, 0.0, 0.0);
	float4 skinnedNormal = float4(0.0, 0.0, 0.0, 0.0);
	
	int index = input.blendIndices[0];
	skinnedPosition += mul(input.Position, gBonesOffsets[index]) * input.blendWeights[0];
	
	index = input.blendIndices[1];
	skinnedPosition += mul(input.Position, gBonesOffsets[index]) * input.blendWeights[1];
	
	index = input.blendIndices[2];
	skinnedPosition += mul(input.Position, gBonesOffsets[index]) * input.blendWeights[2];
	
	index = input.blendIndices[3];
	skinnedPosition += mul(input.Position, gBonesOffsets[index]) * input.blendWeights[3];
	
	//skinnedNormal += mul(float4(input.Normal, 0.0), gBonesOffsets[index]) * weight;

	
	output.Position = mul(skinnedPosition, WorldViewProjection);
	output.Normal = mul(input.Normal, World);
	output.Uv = input.Uv;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float cosTheta = clamp(dot( input.Normal, SunOrientation) + 1, 0, 1);
	float4 textureColor = tex2D(TextureSampler1, input.Uv);// input.Color * cosTheta;
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