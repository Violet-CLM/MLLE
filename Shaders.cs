using System.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MLLE
{
    public partial class Mainframe : Form
    {
        private bool LastShaderTileWas32Bit;
        private struct ShaderHandlePair
        {
            internal int Paletted, TrueColor;
        }

        static private ShaderHandlePair
            Normal, PalShift, Shadow
        ;

        private int
            ShaderHandle_StijnVisionDifficulty
        ;

        private void LoadShader(ref int handle, string src, bool standardUniforms = true) {

            handle = GL.CreateProgram();
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            if (standardUniforms)
            {
                src = @"
uniform sampler2D currentTexture;
uniform sampler2D palette;
uniform float param;
		
void main() {
"
                + src;
            }
            src = @"  #version 130
out vec4 outputColor;
" + src;
            Console.Write(src);
            GL.ShaderSource(fragmentShader, src);
            GL.CompileShader(fragmentShader);
            GL.AttachShader(handle, fragmentShader);
            GL.LinkProgram(handle);
            if (standardUniforms)
            {
                GL.UseProgram(handle);
                GL.Uniform1(GL.GetUniformLocation(handle, "currentTexture"), 0);
                GL.Uniform1(GL.GetUniformLocation(handle, "palette"), 1);
            }
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
        }
        private void LoadShaderPair(ref ShaderHandlePair handles, string palIndexToColor = null, string trueColorToColor = null)
        {
            if (palIndexToColor == null)
                palIndexToColor = @"
	vec4 colorWithPaletteApplied = texture2D(palette, vec2(colorInTileTexture.r, 0));";
            if (trueColorToColor == null)
                trueColorToColor = @"
	outputColor = vec4(colorWithPaletteApplied.rgb, tilePixelAlpha);";
            trueColorToColor += @"
}";

            LoadShader(ref handles.Paletted, @"
	vec4 colorInTileTexture = texture2D(currentTexture, gl_TexCoord[0].xy);
    float tilePixelAlpha = colorInTileTexture.a;
    if (tilePixelAlpha == 0) {
        outputColor = colorInTileTexture;
        return;
    }
    tilePixelAlpha *= gl_Color.a;
"
            + palIndexToColor + trueColorToColor);
            LoadShader(ref handles.TrueColor, @"
    vec4 colorWithPaletteApplied = texture2D(currentTexture, gl_TexCoord[0].xy);
    float tilePixelAlpha = colorWithPaletteApplied.a * gl_Color.a;" + trueColorToColor
            );
        }

        private ShaderHandlePair CurrentLayerShaderHandles;
        private void UseShaderHandle(Layer layer) {
            bool usesParam8Bit = false;
            bool usesParam16Bit = false;
            CurrentLayerShaderHandles = Normal; //default

            switch (layer.SpriteMode) {
                case 18:
                    CurrentLayerShaderHandles = PalShift;
                    usesParam8Bit = true;
                    break;
                case 20:
                    CurrentLayerShaderHandles = Shadow;
                    break;
            }

            GL.UseProgram(CurrentLayerShaderHandles.Paletted); //safe default
            LastShaderTileWas32Bit = false; //safe default
            if (usesParam8Bit)
                GL.Uniform1(GL.GetUniformLocation(CurrentLayerShaderHandles.Paletted, "param"), layer.SpriteParam / 256.0f);
            if (usesParam16Bit)
                GL.Uniform1(GL.GetUniformLocation(CurrentLayerShaderHandles.TrueColor, "param"), layer.SpriteParam / 256.0f);
        }

        private void LoadShaders()
        {
            //sprite modes:
            LoadShaderPair(ref Normal);
            LoadShaderPair(ref PalShift, @"
	vec4 colorWithPaletteApplied = texture2D(palette, vec2(mod(colorInTileTexture.r + param, 1.0), 0));");
            LoadShaderPair(ref Shadow, "", @"
	outputColor = vec4(0,0,0, tilePixelAlpha * 0.5);");

            //more irregular stuff:
            LoadShader(ref ShaderHandle_StijnVisionDifficulty, @"
uniform sampler2D currentTexture;

void main() {
    vec4 pixel = texture2D(currentTexture, gl_TexCoord[0].xy);
    float grayscale = dot(pixel.rgb, vec3(0.299, 0.587, 0.114));
    outputColor = vec4((gl_Color * grayscale).rgb, pixel.a);
}
", false);
        }
    }
}
