using System.Numerics;
using System.Runtime.InteropServices;
using FishPieClient.Graphics.Shaders;
using ImGuiNET;
using Serilog;
using Silk.NET.OpenGL;
using Shader = FishPieClient.Graphics.Shaders.Shader;

namespace FishPieClient.Graphics.UI.ImGuiImpl;

public sealed class ImGuiImplOpenGl : IDisposable
{

    private readonly uint _version;
    private readonly string _versionString;

    private readonly int _profileMask;
    private readonly int _maxTextureSize;

    private readonly uint _vaoId;
    
    private readonly IntPtr _context;
    private readonly GL _gl;

    private uint _fontTexture;
    private ShaderProgram? _shader;

    private int _uniformLocationTex;
    private int _uniformLocationProjMtx;
    private uint _attribLocationPos;
    private uint _attribLocationUV;
    private uint _attribLocationColour;
    
    private uint _vboHandle;
    private uint _eboHandle;
    
    private uint _vertexBufferSize;
    private uint _indexBufferSize;

    public ImGuiImplOpenGl(IntPtr context, GL gl)
    {
        _context = context;
        _gl = gl;
        
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();

        string glVersionStr = _gl.GetStringS(StringName.Version);
        int major = _gl.GetInteger(GetPName.MajorVersion);
        int minor = _gl.GetInteger(GetPName.MinorVersion);
        if (major == 0 && minor == 0)
            glVersionStr = $"{major}.{minor}";

        _version = (uint)(major * 100 + minor * 10);
        _gl.GetInteger(GetPName.MaxTextureSize, out _maxTextureSize);
        
        Log.Information($"GlVersion = {_version} \"{_versionString}\"\n" +
                        $"\tGL_VENDOR = '{_gl.GetStringS(StringName.Vendor)}'\n" +
                        $"\tGL_RENDERER = '{_gl.GetStringS(StringName.Renderer)}'");

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        _vaoId = _gl.GenVertexArray();
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }

    public void NewFrame()
    {
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        if (_shader == null)
        {
            CreateDeviceObjects();
        }

        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }

    private unsafe void SetupRenderState(ImDrawDataPtr drawData, int fbWidth, int fbHeight)
    {
        _gl.Enable(EnableCap.Blend);
        _gl.BlendEquation(BlendEquationModeEXT.FuncAdd);
        _gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.One,
            BlendingFactor.OneMinusSrcAlpha);
        _gl.Disable(EnableCap.CullFace);
        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.StencilTest);
        _gl.Enable(EnableCap.ScissorTest);
        _gl.Disable(EnableCap.PrimitiveRestart);

        _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

        _gl.Viewport(0, 0, (uint)fbWidth, (uint)fbHeight);
        float L = drawData.DisplayPos.X;
        float R = drawData.DisplayPos.X + drawData.DisplaySize.X;
        float T = drawData.DisplayPos.Y;
        float B = drawData.DisplayPos.Y + drawData.DisplaySize.Y;

        Span<float> orthoProjection =
        [
            2.0f / (R - L),     0.0f,               0.0f,   0.0f,
            0.0f,               2.0f / (T - B),     0.0f,   0.0f,
            0.0f,               0.0f,              -1.0f,   0.0f,
            (R + L) / (L - R),  (T + B) / (B - T),  0.0f,   1.0f
        ];
        _shader!.Use();
        _gl.Uniform1(_uniformLocationTex, 0);
        _gl.UniformMatrix4(_uniformLocationProjMtx, 1, false, orthoProjection);

        _gl.BindSampler(0, 0);

        _gl.BindVertexArray(_vaoId);
        
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboHandle);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _eboHandle);
        _gl.EnableVertexAttribArray(_attribLocationPos);
        _gl.EnableVertexAttribArray(_attribLocationUV);
        _gl.EnableVertexAttribArray(_attribLocationColour);
        _gl.VertexAttribPointer(_attribLocationPos, 2, VertexAttribPointerType.Float, false, (uint)sizeof(ImDrawVert),
            0);
        _gl.VertexAttribPointer(_attribLocationUV, 2, VertexAttribPointerType.Float, false, (uint)sizeof(ImDrawVert),
            8);
        _gl.VertexAttribPointer(_attribLocationColour, 4, GLEnum.UnsignedByte, true, (uint)sizeof(ImDrawVert), 16);
    }
    
    public unsafe void RenderDrawData(ImDrawDataPtr drawData)
    {
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        int fbWidth = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
        int fbHeight = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
        if (fbWidth < 0 || fbHeight < 0) return;
        
        // backup GL states
        var lastActiveTexture = (GLEnum)_gl.GetInteger(GetPName.ActiveTexture);
        _gl.ActiveTexture(TextureUnit.Texture0);
        var lastProgram = (uint)_gl.GetInteger(GetPName.CurrentProgram);
        var lastTexture = (uint)_gl.GetInteger(GetPName.TextureBinding2D);
        var lastSampler = (uint)_gl.GetInteger(GetPName.SamplerBinding);
        var lastArrayBuffer = (uint)_gl.GetInteger(GetPName.ArrayBufferBinding);
        var lastVao = (uint)_gl.GetInteger(GetPName.VertexArrayBinding);
        
        Span<int> lastPolygonMode = stackalloc int[2];
        _gl.GetInteger(GetPName.PolygonMode, lastPolygonMode);

        Span<int> lastViewport = stackalloc int[4];
        _gl.GetInteger(GetPName.Viewport, lastViewport);

        Span<int> lastScissorBox = stackalloc int[4];
        _gl.GetInteger(GetPName.ScissorBox, lastScissorBox);

        var lastBlendSrcRgb = (GLEnum)_gl.GetInteger(GetPName.BlendSrcRgb);
        var lastBlendDstRgb = (GLEnum)_gl.GetInteger(GetPName.BlendDstRgb);
        var lastBlendSrcAlpha = (GLEnum)_gl.GetInteger(GetPName.BlendSrcAlpha);
        var lastBlendDstAlpha = (GLEnum)_gl.GetInteger(GetPName.BlendDstAlpha);
        var lastBlendEquationRgb = (GLEnum)_gl.GetInteger(GetPName.BlendEquationRgb);
        var lastBlendEquationAlpha = (GLEnum)_gl.GetInteger(GetPName.BlendEquationAlpha);

        bool lastEnableBlend = _gl.IsEnabled(EnableCap.Blend);
        bool lastEnableCullFace = _gl.IsEnabled(EnableCap.CullFace);
        bool lastEnableDepthTest = _gl.IsEnabled(EnableCap.DepthTest);
        bool lastEnableStencilTest = _gl.IsEnabled(EnableCap.StencilTest);
        bool lastEnableScissorTest = _gl.IsEnabled(EnableCap.ScissorTest);
        bool lastEnablePrimitiveRestart = _gl.IsEnabled(EnableCap.PrimitiveRestart);
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        SetupRenderState(drawData, fbWidth, fbHeight);

        var clipOff = drawData.DisplayPos;
        var clipScale = drawData.FramebufferScale;

        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdLists[n];

            // Upload vertex / index buffers
            uint vertexBufferSize = (uint)cmdList.VtxBuffer.Size * (uint)sizeof(ImDrawVert);
            uint indexBufferSize = (uint)cmdList.IdxBuffer.Size * sizeof(ushort);

            if (_vertexBufferSize < vertexBufferSize)
            {
                _vertexBufferSize = vertexBufferSize;
                _gl.BufferData(BufferTargetARB.ArrayBuffer, _vertexBufferSize, null, BufferUsageARB.StreamDraw);
            }

            if (_indexBufferSize < indexBufferSize)
            {
                _indexBufferSize = indexBufferSize;
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, _indexBufferSize, null, BufferUsageARB.StreamDraw);
            }

            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, vertexBufferSize, cmdList.VtxBuffer.Data.ToPointer());
            _gl.BufferSubData(BufferTargetARB.ElementArrayBuffer, 0, indexBufferSize,
                cmdList.IdxBuffer.Data.ToPointer());

            for (int cmdIdx = 0; cmdIdx < cmdList.CmdBuffer.Size; cmdIdx++)
            {
                var cmd = cmdList.CmdBuffer[cmdIdx];

                // Project scissor / clipping rectangle into framebuffer space
                var clipMin = new Vector2(
                    (cmd.ClipRect.X - clipOff.X) * clipScale.X,
                    (cmd.ClipRect.Y - clipOff.Y) * clipScale.Y
                );
                var clipMax = new Vector2(
                    (cmd.ClipRect.Z - clipOff.X) * clipScale.X,
                    (cmd.ClipRect.W - clipOff.Y) * clipScale.Y
                );

                if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                {
                    continue;
                }

                // note y is inverted in OpenGL
                _gl.Scissor((int)clipMin.X, (int)((float)fbHeight - clipMax.Y), (uint)(clipMax.X - clipMin.X),
                    (uint)(clipMax.Y - clipMin.Y));
                
                // bind texture
                _gl.BindTexture(TextureTarget.Texture2D, (uint)cmd.GetTexID());
                
                // Draw
                _gl.DrawElementsBaseVertex(
                    mode: PrimitiveType.Triangles,
                    count: cmd.ElemCount,
                    type: DrawElementsType.UnsignedShort,
                    (void*)(cmd.IdxOffset * sizeof(ushort)),
                    (int)cmd.VtxOffset
                );
            }
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        // Restore GL states
        if (lastProgram == 0 || _gl.IsProgram(lastProgram))
            _gl.UseProgram(lastProgram);
        _gl.BindTexture(TextureTarget.Texture2D, lastTexture);
        _gl.BindSampler(0, lastSampler);
        _gl.ActiveTexture(lastActiveTexture);
        _gl.BindVertexArray(lastVao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, lastArrayBuffer);

        _gl.BlendEquationSeparate(lastBlendEquationRgb, lastBlendEquationAlpha);
        _gl.BlendFuncSeparate(lastBlendSrcRgb, lastBlendDstRgb, lastBlendSrcAlpha, lastBlendDstAlpha);
        
        if (lastEnableBlend) _gl.Enable(EnableCap.Blend);
        else _gl.Disable(EnableCap.Blend);
        if (lastEnableCullFace) _gl.Enable(EnableCap.CullFace);
        else _gl.Disable(EnableCap.CullFace);
        if (lastEnableDepthTest) _gl.Enable(EnableCap.DepthTest);
        else _gl.Disable(EnableCap.DepthTest);
        if (lastEnableStencilTest) _gl.Enable(EnableCap.StencilTest);
        else _gl.Disable(EnableCap.StencilTest);
        if (lastEnableScissorTest) _gl.Enable(EnableCap.ScissorTest);
        else _gl.Disable(EnableCap.ScissorTest);
        if (lastEnablePrimitiveRestart) _gl.Enable(EnableCap.PrimitiveRestart);
        else _gl.Disable(EnableCap.PrimitiveRestart);
        
        _gl.PolygonMode(TriangleFace.FrontAndBack, (PolygonMode)lastPolygonMode[0]);
        
        _gl.Viewport(lastViewport[0], lastViewport[1], (uint)lastViewport[2], (uint)lastViewport[3]);
        _gl.Scissor(lastScissorBox[0], lastScissorBox[1], (uint)lastScissorBox[2], (uint)lastScissorBox[3]);
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }
    
    public void Dispose()
    {
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();

        DestroyDeviceObjects();
        io.BackendFlags &= ~ImGuiBackendFlags.RendererHasVtxOffset;

        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }

    private unsafe void CreateFontsTexture()
    {
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();

        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height);

        // save current state
        int lastTexture = _gl.GetInteger(GetPName.TextureBinding2D);
        
        _fontTexture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _fontTexture);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
        
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, pixels);

        io.Fonts.SetTexID((int)_fontTexture);

        // restore state
        _gl.BindTexture(TextureTarget.Texture2D, (uint)lastTexture);
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }

    private void DestroyFontsTexture()
    {
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();

        if (_fontTexture != 0)
        {
            _gl.DeleteTexture(_fontTexture);
            io.Fonts.SetTexID(0);
            _fontTexture = 0;
        }
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }

    private void CreateDeviceObjects()
    {
        // save state
        int lastTexture = _gl.GetInteger(GetPName.TextureBinding2D);
        int lastArrayBuffer = _gl.GetInteger(GetPName.ArrayBufferBinding);

        var vertexShader = new Shader("imgui.vert", Shader.Type.Vertex, "imgui_vertex_shader", _gl);
        var fragmentShader = new Shader("imgui.frag", Shader.Type.Fragment, "imgui_fragment_shader", _gl);

        _shader = new ShaderProgram(vertexShader, fragmentShader, "imgui_shader", _gl);
        
        vertexShader.Dispose();
        fragmentShader.Dispose();

        _uniformLocationTex = _shader.GetUniformLocation("texture_sampler");
        _uniformLocationProjMtx = _shader.GetUniformLocation("projection");
        _attribLocationPos = (uint)_shader.GetAttribLocation("position");
        _attribLocationUV = (uint)_shader.GetAttribLocation("uv");
        _attribLocationColour = (uint)_shader.GetAttribLocation("colour");

        _vboHandle = _gl.GenBuffer();
        _eboHandle = _gl.GenBuffer();

        CreateFontsTexture();
        
        // restore state
        _gl.BindTexture(TextureTarget.Texture2D, (uint)lastTexture);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, (uint)lastArrayBuffer);
    }
    
    private void DestroyDeviceObjects()
    {
        if (_vboHandle != 0)
        {
            _gl.DeleteBuffer(_vboHandle);
            _vboHandle = 0;
        }

        if (_eboHandle != 0)
        {
            _gl.DeleteBuffer(_eboHandle);
            _eboHandle = 0;
        }

        if (_vaoId != 0)
        {
            _gl.DeleteVertexArray(_vaoId);
        }

        if (_shader != null)
        {
            _shader.Dispose();
            _shader = null;
        }
        
        DestroyFontsTexture();
    }
    
}