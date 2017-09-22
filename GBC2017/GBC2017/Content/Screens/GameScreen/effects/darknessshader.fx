// This sampler is set automatically based on the render texture source
sampler SpriteBatchTexture : register(s0);
sampler DisplacementTexture : register(s1);
sampler DisplacedWorld : register(s2);

float ViewerX = .5;
float ViewerY = .5;
float BlurStrength = 0.036;
float FocusArea = .25f;

float DisplacementStart = 0;
float CameraHeight = 0;
float CameraTop = 0;

float DisplacementTextureOffset = 0;

float4 DisplacementFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float2 spriteBatchTextureCoord = texCoord;
	float pixelY = CameraTop - (CameraHeight * texCoord.y);

	if (pixelY < DisplacementStart)
	{
		texCoord.y -= CameraTop / CameraHeight - DisplacementTextureOffset;

		texCoord.y -= trunc(texCoord.y);

		float4 color = tex2D(DisplacementTexture, texCoord);

		spriteBatchTextureCoord.y += -.01 + (color.y * .02);
	}
	return tex2D(SpriteBatchTexture, spriteBatchTextureCoord);
}

float4 DistanceBlurFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float ratioX = abs(texCoord.x - ViewerX);
	float ratioY = abs(texCoord.y - ViewerY);

	float blurRatio = max(0, max(ratioX, ratioY) - FocusArea) * BlurStrength;

	float4 color = 0;

	int samples = 4;
	float samplesSquared = 16;
	float blurOver2 = blurRatio / 2;
	float blurOverSamples = blurRatio / samples;

	for (int x = 0; x < samples; x++)
	{
		for (int y = 0; y < samples; y++)
		{
			float2 coord = texCoord;
			coord.x = coord.x - blurOver2 + (blurOverSamples * x);
			coord.y = coord.y - blurOver2 + (blurOverSamples * y);

			color += tex2D(DisplacedWorld, coord) / samplesSquared;
		}
	}
	return color;
}

technique DistanceBlurTechnique
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 DisplacementFunction();
	}

	pass Pass2
	{
		PixelShader = compile ps_2_0 DistanceBlurFunction();
	}

}