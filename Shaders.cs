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
            ShaderHandle_NormalPaletted
        ;

        private void LoadShader(ref int handle, string src) {

            handle = GL.CreateProgram();
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, src);
            GL.CompileShader(fragmentShader);
            GL.AttachShader(handle, fragmentShader);
            GL.LinkProgram(handle);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
        }

        private void LoadShaders()
        {
            LoadShader(ref ShaderHandle_StijnVisionDifficulty, @"
out vec4 outputColor;
uniform sampler2D texture0;

void main() {
    vec4 pixel = texture2D(texture0, gl_TexCoord[0].xy);
    float grayscale = dot(pixel.rgb, vec3(0.299, 0.587, 0.114));
    outputColor = vec4((gl_Color * grayscale).rgb, pixel.a);
}
");
        }
    }
}
