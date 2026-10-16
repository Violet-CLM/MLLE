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
        internal int
            ShaderHandle_StijnVisionDifficulty,
            ShaderHandle_NormalPaletted,
            ShaderHandle_PalShift
        ;

        private void LoadShader(ref int handle, string src, bool standardUniforms = true) {

            handle = GL.CreateProgram();
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            if (standardUniforms)
                src = @"
uniform sampler2D currentTexture;
uniform sampler2D palette;
" + src;
            src = @"  #version 130
out vec4 outputColor;
" + src;
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

        private void UseShaderHandle(Layer layer) {
            bool spriteModeUsesParam = false;
            int handleToUse = ShaderHandle_NormalPaletted; //default

            switch (layer.SpriteMode) {
                case 18:
                    handleToUse = ShaderHandle_PalShift;
                    spriteModeUsesParam = true;
                    break;
            }

            GL.UseProgram(handleToUse);
            if (spriteModeUsesParam)
                GL.Uniform1(GL.GetUniformLocation(handleToUse, "param"), layer.SpriteParam / 256.0f);
        }

        private void LoadShaders()
        {
            LoadShader(ref ShaderHandle_StijnVisionDifficulty, @"
uniform sampler2D currentTexture;

void main() {
    vec4 pixel = texture2D(currentTexture, gl_TexCoord[0].xy);
    float grayscale = dot(pixel.rgb, vec3(0.299, 0.587, 0.114));
    outputColor = vec4((gl_Color * grayscale).rgb, pixel.a);
}
", false);
            LoadShader(ref ShaderHandle_NormalPaletted, @"
		
void main() {
	vec4 index = texture2D(currentTexture, gl_TexCoord[0].xy);
    if (index.a == 0) {
        outputColor = index;
        return;
    }
	vec4 pixel = texture2D(palette, vec2(index.r, 0));
	outputColor = vec4(pixel.r, pixel.g, pixel.b, index.a * gl_Color.a);
}
");
            LoadShader(ref ShaderHandle_PalShift, @"
uniform float param;
		
void main() {
	vec4 index = texture2D(currentTexture, gl_TexCoord[0].xy);
    if (index.a == 0) {
        outputColor = index;
        return;
    }
	vec4 pixel = texture2D(palette, vec2(mod(index.r + param, 1.0), 0));
	outputColor = vec4(pixel.r, pixel.g, pixel.b, index.a * gl_Color.a);
}
");
        }
    }
}
