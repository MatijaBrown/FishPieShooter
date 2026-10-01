using System.Numerics;
using System.Runtime.InteropServices;
using FishPieClient.Graphics.Shaders;
using FishPieClient.Utils;
using Hexa.NET.ImGui;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using Shader = FishPieClient.Graphics.Shaders.Shader;

namespace FishPieClient.Graphics.UI.ImGuiImpl;

public static class ImGuiImplOpenGl
{

    private sealed class ImGuiImplOpenGlData
    {
        public int GlVersion;
        public required GL Gl;

        public int GlProfileMask;
        public int MaxTextureSize;
        
        public ShaderProgram? ShaderProgramme;
        
        public int UniformLocationTex;
        public int UniformLocationProjMat;
        public int AttribLocationVtxPos;
        public int AttribLocationVtxUv;
        public int AttribLocationVtxColour;

        public uint VboId;
        public uint EboId;

        public uint VertexBufferSize;
        public uint IndexBufferSize;
    }
    

    private static unsafe ImGuiImplOpenGlData? GetBackendData()
    {
        if (ImGui.GetCurrentContext().Handle == null) return null;
        var handle = GCHandle.FromIntPtr((nint)ImGui.GetIO().BackendRendererUserData);
        return (ImGuiImplOpenGlData?)handle.Target;
    }
    

    public static unsafe bool Init(GL gl)
    {
        var io = ImGui.GetIO();
        Errors.Ensure(io.BackendRendererUserData == null, "Already initialized a renderer backend!");

        var bd = new ImGuiImplOpenGlData()
        {
            Gl = gl
        };
        io.BackendRendererUserData = (void*)GCHandle.ToIntPtr(GCHandle.Alloc(bd));
        io.BackendRendererName = (byte*)SilkMarshal.StringToPtr("imgui_renderer_opengl");

        int major = gl.GetInteger(GetPName.MajorVersion);
        int minor = gl.GetInteger(GetPName.MinorVersion);
        bd.GlVersion = major * 100 + minor * 10;
        bd.MaxTextureSize = gl.GetInteger(GetPName.MaxTextureSize);

        string versionString = gl.GetStringS(StringName.Version);
        Log.Information(
            "GlVersion = {Version}, \"{VersionString}\"" + Environment.NewLine
                                                         + "GlProfileMask = {ProfileMask}" + Environment.NewLine
                                                         + "GL_VENDOR = {Vendor}" + Environment.NewLine
                                                         + "GL_RENDERER = {Renderer}",
            bd.GlVersion, versionString, bd.GlProfileMask, gl.GetStringS(StringName.Vendor),
            gl.GetStringS(StringName.Renderer));

        if (bd.GlVersion >= 320)
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasTextures;
        
        var platformIo = ImGui.GetPlatformIO();
        platformIo.RendererTextureMaxWidth = platformIo.RendererTextureMaxHeight = bd.MaxTextureSize;
        
        return true;
    }

    public static unsafe void Shutdown()
    {
        var bd = GetBackendData();
        Errors.Ensure(bd != null, "No renderer backend to shutdown, or already shutdown?");
        
        var io = ImGui.GetIO();
        var platformIo = ImGui.GetPlatformIO();
        
        DestroyDeviceObjects();
        
        io.BackendRendererName = null;
        
        var handle = GCHandle.FromIntPtr((nint)io.BackendRendererUserData);
        handle.Free();
        io.BackendRendererUserData = null;
        
        io.BackendFlags &= ~(ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.RendererHasTextures);
        platformIo.ClearRendererHandlers();
    }

    public static void NewFrame()
    {
        var bd = GetBackendData();
        Errors.Ensure(bd != null, "Context or backend not initialized! Did you call ImGuiImplOpenGl.Int()?");

        if (bd!.ShaderProgramme == null)
            CreateDeviceObjects();
    }
    
    private static unsafe void SetupRenderState(ImDrawDataPtr drawData, int fbWidth, int fbHeight, uint vao)
    {
        var bd = GetBackendData()!;
        var gl = bd.Gl;

        gl.Enable(EnableCap.Blend);
        gl.Enable(EnableCap.ScissorTest);
        gl.BlendEquation(BlendEquationModeEXT.FuncAdd);
        gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.One,
            BlendingFactor.OneMinusSrcAlpha);
        gl.Disable(EnableCap.CullFace);
        gl.Disable(EnableCap.DepthTest);
        gl.Disable(EnableCap.StencilTest);
        gl.Disable(EnableCap.PrimitiveRestart);

        gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

        bool clipOriginLowerLeft = true;
        var currentClipOrigin = (GLEnum)gl.GetInteger(GLEnum.ClipOrigin);
        if (currentClipOrigin == GLEnum.UpperLeft)
            clipOriginLowerLeft = false;

        gl.Viewport(0, 0, (uint)fbWidth, (uint)fbHeight);
        
        float L = drawData.DisplayPos.X;
        float R = drawData.DisplayPos.X + drawData.DisplaySize.X;
        float T = drawData.DisplayPos.Y;
        float B = drawData.DisplayPos.Y + drawData.DisplaySize.Y;

        if (!clipOriginLowerLeft)
        {
            (T, B) = (B, T);
        }

        var orthoProjection = new Matrix4x4(
            2.0f / (R - L),     0.0f,               0.0f,       0.0f,
            0.0f,               2.0f / (T - B),     0.0f,       0.0f,
            0.0f,               0.0f,              -1.0f,       0.0f,
            (R + L) / (L - R),  (T + B) / (B - T),  0.0f,       1.0f
        );
        bd.ShaderProgramme!.Use();
        gl.Uniform1(bd.UniformLocationTex, 0);
        gl.UniformMatrix4(bd.UniformLocationProjMat, 1, false, (float*)&orthoProjection);

        gl.BindSampler(0, 0);

        gl.BindVertexArray(vao);

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, bd.VboId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, bd.EboId);
        
        gl.EnableVertexAttribArray((uint)bd.AttribLocationVtxPos);
        gl.EnableVertexAttribArray((uint)bd.AttribLocationVtxUv);
        gl.EnableVertexAttribArray((uint)bd.AttribLocationVtxColour);

        gl.VertexAttribPointer((uint)bd.AttribLocationVtxPos, 2, VertexAttribPointerType.Float, false,
            (uint)sizeof(ImDrawVert), (void*)Marshal.OffsetOf<ImDrawVert>("Pos"));
        gl.VertexAttribPointer((uint)bd.AttribLocationVtxUv, 2, VertexAttribPointerType.Float, false,
            (uint)sizeof(ImDrawVert), (void*)Marshal.OffsetOf<ImDrawVert>("Uv"));
        gl.VertexAttribPointer((uint)bd.AttribLocationVtxColour, 4, VertexAttribPointerType.UnsignedByte, true,
            (uint)sizeof(ImDrawVert), (void*)Marshal.OffsetOf<ImDrawVert>("Col"));
    }

    public static unsafe void RenderDrawData(ImDrawDataPtr drawData)
    {
        int fbWidth = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
        int fbHeight = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
        if (fbWidth < 0 || fbHeight < 0)
            return;

        var bd = GetBackendData()!;
        var gl = bd.Gl;
        
        if (drawData.Textures.Data != null)
        {
            for (int i = 0; i < drawData.Textures.Size; i++)
            {
                var tex = drawData.Textures[i];
                if (tex.Status != ImTextureStatus.Ok)
                    UpdateTexture(tex, gl);
            }
        }
        
        // Backup GL state
        var lastActiveTexture = (GLEnum)gl.GetInteger(GetPName.ActiveTexture);
        var lastProgramme = (uint)gl.GetInteger(GetPName.CurrentProgram);
        var lastTexture = (uint)gl.GetInteger(GetPName.TextureBinding2D);
        var lastSampler = (uint)gl.GetInteger(GetPName.SamplerBinding);
        var lastArrayBuffer = (uint)gl.GetInteger(GetPName.ArrayBufferBinding);
        var lastVertexArrayObject = (uint)gl.GetInteger(GetPName.VertexArrayBinding);

        Span<int> lastPolygonMode = stackalloc int[2];
        gl.GetInteger(GetPName.PolygonMode, lastPolygonMode);

        Span<int> lastViewport = stackalloc int[4];
        gl.GetInteger(GetPName.Viewport, lastViewport);

        Span<int> lastScissorBox = stackalloc int[4];
        gl.GetInteger(GetPName.ScissorBox, lastScissorBox);

        var lastBlendSrcRgb = (GLEnum)gl.GetInteger(GetPName.BlendSrcRgb);
        var lastBlendDstRgb = (GLEnum)gl.GetInteger(GetPName.BlendDstRgb);
        var lastBlendSrcAlpha = (GLEnum)gl.GetInteger(GetPName.BlendSrcAlpha);
        var lastBlendDstAlpha = (GLEnum)gl.GetInteger(GetPName.BlendDstAlpha);
        var lastBlendEquationRgb = (GLEnum)gl.GetInteger(GetPName.BlendEquationRgb);
        var lastBlendEquationAlpha = (GLEnum)gl.GetInteger(GetPName.BlendEquationAlpha);

        bool lastEnableBlend = gl.IsEnabled(EnableCap.Blend);
        bool lastEnableCullFace = gl.IsEnabled(EnableCap.CullFace);
        bool lastEnableDepthTest = gl.IsEnabled(EnableCap.DepthTest);
        bool lastEnableStencilTest = gl.IsEnabled(EnableCap.StencilTest);
        bool lastEnableScissorTest = gl.IsEnabled(EnableCap.ScissorTest);
        
        bool lastEnablePrimitiveRestart = gl.IsEnabled(EnableCap.PrimitiveRestart);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        Errors.CheckGlError("before render", gl);
        
        uint vao = gl.GenVertexArray();
        Errors.CheckGlError("gen vao", gl);

        SetupRenderState(drawData, fbWidth, fbHeight, vao);
        Errors.CheckGlError("setup state", gl);

        var clipOff = drawData.DisplayPos;
        var clipScale = drawData.FramebufferScale;

        for (int n = 0; n < drawData.CmdLists.Size; n++)
        {
            var drawList = drawData.CmdLists[n];

            uint vtxBufferSize = (uint)drawList.VtxBuffer.Size * (uint)sizeof(ImDrawVert);
            uint idxBufferSize = (uint)drawList.IdxBuffer.Size * sizeof(ushort);

            if (bd.VertexBufferSize < vtxBufferSize)
            {
                bd.VertexBufferSize = vtxBufferSize;
                gl.BufferData(BufferTargetARB.ArrayBuffer, bd.VertexBufferSize, null, BufferUsageARB.StreamDraw);
                Errors.CheckGlError("resize vertex buffer", gl);
            }

            if (bd.IndexBufferSize < idxBufferSize)
            {
                bd.IndexBufferSize = idxBufferSize;
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, bd.IndexBufferSize, null, BufferUsageARB.StreamDraw);
                Errors.CheckGlError("resize index buffer", gl);
            }

            gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, vtxBufferSize, drawList.VtxBuffer.Data);
            Errors.CheckGlError("update vertex  buffer", gl);
            gl.BufferSubData(BufferTargetARB.ElementArrayBuffer, 0, idxBufferSize, drawList.IdxBuffer.Data);
            Errors.CheckGlError("update index buffer", gl);

            for (int cmdIdx = 0; cmdIdx < drawList.CmdBuffer.Size; cmdIdx++)
            {
                var pcmd = drawList.CmdBuffer[cmdIdx];

                if (pcmd.UserCallback != null)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    var clipMin = new Vector2((pcmd.ClipRect.X - clipOff.X) * clipScale.X,
                        (pcmd.ClipRect.Y - clipOff.Y) * clipScale.Y);
                    var clipMax = new Vector2((pcmd.ClipRect.Z - clipOff.X) * clipScale.X,
                        (pcmd.ClipRect.W - clipOff.Y) * clipScale.Y);

                    if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                        continue;

                    gl.Scissor((int)clipMin.X, (int)((float)fbHeight - clipMax.Y), (uint)(clipMax.X - clipMin.X),
                        (uint)(clipMax.Y - clipMin.Y));
                    Errors.CheckGlError("setup scissor", gl);
                    
                    gl.BindTexture(TextureTarget.Texture2D, (uint)pcmd.GetTexID());
                    Errors.CheckGlError("bind texture", gl);
                    gl.DrawElementsBaseVertex(PrimitiveType.Triangles, (uint)pcmd.ElemCount,
                        DrawElementsType.UnsignedShort, (void*)(pcmd.IdxOffset * sizeof(ushort)), (int)pcmd.VtxOffset);
                    Errors.CheckGlError("render", gl);
                }
            }
        }
     
        gl.DeleteVertexArray(vao);
        Errors.CheckGlError("delete vao", gl);
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        // Restore modified GL state
        if (lastProgramme == 0 || gl.IsProgram(lastProgramme)) gl.UseProgram(lastProgramme);
        
        gl.BindTexture(TextureTarget.Texture2D, lastTexture);
        gl.BindSampler(0, lastSampler);
        gl.ActiveTexture(lastActiveTexture);
        
        gl.BindVertexArray(lastVertexArrayObject);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, lastArrayBuffer);
        
        gl.BlendEquationSeparate(lastBlendEquationRgb, lastBlendEquationAlpha);
        gl.BlendFuncSeparate(lastBlendSrcRgb, lastBlendDstRgb, lastBlendSrcAlpha, lastBlendDstAlpha);
        
        if (lastEnableBlend) gl.Enable(EnableCap.Blend);
        else gl.Disable(EnableCap.Blend);
        
        if (lastEnableCullFace) gl.Enable(EnableCap.CullFace);
        else gl.Disable(EnableCap.CullFace);
        
        if (lastEnableDepthTest) gl.Enable(EnableCap.DepthTest);
        else gl.Disable(EnableCap.DepthTest);
        
        if (lastEnableStencilTest) gl.Enable(EnableCap.StencilTest);
        else gl.Disable(EnableCap.StencilTest);
        
        if (lastEnableScissorTest) gl.Enable(EnableCap.ScissorTest);
        else gl.Disable(EnableCap.ScissorTest);
        
        if (lastEnablePrimitiveRestart) gl.Enable(EnableCap.PrimitiveRestart);
        else gl.Disable(EnableCap.PrimitiveRestart);
        
        gl.PolygonMode(TriangleFace.FrontAndBack, (GLEnum)lastPolygonMode[0]);
        gl.Viewport(lastViewport[0], lastViewport[1], (uint)lastViewport[2], (uint)lastViewport[3]);
        gl.Scissor(lastScissorBox[0], lastScissorBox[1], (uint)lastScissorBox[2], (uint)lastScissorBox[3]);
    }

    private static void DestroyTexture(ImTextureDataPtr tex, GL gl)
    {
        uint glTexId = (uint)tex.TexID;
        gl.DeleteTexture(glTexId);
        
        tex.SetTexID(ImTextureID.Null);
        tex.SetStatus(ImTextureStatus.Destroyed);
    }

    private static unsafe void UpdateTexture(ImTextureDataPtr tex, GL gl)
    {
        if (tex.Status == ImTextureStatus.WantCreate)
        {
            Errors.Ensure(tex.TexID.Handle == 0 && tex.BackendUserData == null, "Texture already created!");
            Errors.Ensure(tex.Format == ImTextureFormat.Rgba32, "Faulty texture format!");

            void* pixels = tex.GetPixels();
            uint textureId = 0;

            int lastTexture = gl.GetInteger(GetPName.TextureBinding2D);
            
            textureId = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, textureId);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

            gl.PixelStore(PixelStoreParameter.UnpackRowLength, 0);

            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)tex.Width, (uint)tex.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            
            tex.SetTexID(new ImTextureID((ulong)textureId));
            tex.SetStatus(ImTextureStatus.Ok);

            gl.BindTexture(TextureTarget.Texture2D, (uint)lastTexture);
        }
        else if (tex.Status == ImTextureStatus.WantUpdates)
        {
            int lastTexture = gl.GetInteger(GetPName.TextureBinding2D);

            uint texId = (uint)tex.TexID;
            gl.BindTexture(TextureTarget.Texture2D, texId);
            
            gl.PixelStore(PixelStoreParameter.UnpackRowLength, tex.Width);
            for (int i = 0; i < tex.Updates.Size; i++)
            {
                var r = tex.Updates[i];

                gl.TexSubImage2D(TextureTarget.Texture2D, 0, r.X, r.Y, r.W, r.H, PixelFormat.Rgba,
                    PixelType.UnsignedByte, tex.GetPixelsAt(r.X, r.Y));
            }
            gl.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            
            tex.SetStatus(ImTextureStatus.Ok);
            gl.BindTexture(TextureTarget.Texture2D, (uint)lastTexture);
        }
        else if (tex.Status == ImTextureStatus.WantDestroy && tex.UnusedFrames > 0)
        {
            DestroyTexture(tex, gl);
        }
    }
    
    private static void CreateDeviceObjects()
    {
        var bd = GetBackendData();
        Errors.Ensure(bd != null, "Context or backend not initialized! Did you call ImGuiImplOpenGl.Int()?");

        var gl = bd!.Gl;
        
        // Backup GL state
        int lastTexture = gl.GetInteger(GetPName.TextureBinding2D);
        int lastArrayBuffer = gl.GetInteger(GetPName.ArrayBufferBinding);

        int lastPixelUnpackBuffer = gl.GetInteger(GetPName.PixelUnpackBufferBinding);
        gl.BindBuffer(BufferTargetARB.PixelUnpackBuffer, 0);
        
        int lastVertexArray = gl.GetInteger(GetPName.VertexArrayBinding);
        
        // create shaders
        var vertexShader = new Shader("imgui.vert", Shader.Type.Vertex, "imgui_vertex_shader", gl);
        var fragmentShader = new Shader("imgui.frag", Shader.Type.Fragment, "imgui_fragment_shader", gl);

        bd.ShaderProgramme = new ShaderProgram(vertexShader, fragmentShader, "imgui_shader", gl);

        vertexShader.Dispose();
        fragmentShader.Dispose();

        bd.UniformLocationTex = bd.ShaderProgramme.GetUniformLocation("texture_sampler");
        bd.UniformLocationProjMat = bd.ShaderProgramme.GetUniformLocation("projection");
        bd.AttribLocationVtxPos = bd.ShaderProgramme.GetAttribLocation("position");
        bd.AttribLocationVtxUv = bd.ShaderProgramme.GetAttribLocation("uv");
        bd.AttribLocationVtxColour = bd.ShaderProgramme.GetAttribLocation("colour");

        // Create buffers
        bd.VboId = gl.GenBuffer();
        bd.EboId = gl.GenBuffer();
        
        // Restore modified GL state
        gl.BindTexture(TextureTarget.Texture2D, (uint)lastTexture);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, (uint)lastArrayBuffer);
        gl.BindBuffer(BufferTargetARB.PixelUnpackBuffer, (uint)lastPixelUnpackBuffer);
        gl.BindVertexArray((uint)lastVertexArray);
    }

    private static void DestroyDeviceObjects()
    {
        var bd = GetBackendData();
        Errors.Ensure(bd != null, "Context or backend not initialized! Did you call ImGuiImplOpenGl.Int()?");

        var gl = bd!.Gl;
        
        if (bd.VboId != 0)
        {
            gl.DeleteBuffer(bd.VboId);
            bd.VboId = 0;
        }

        if (bd.EboId != 0)
        {
            gl.DeleteBuffer(bd.EboId);
            bd.EboId = 0;
        }

        if (bd.ShaderProgramme != null)
        {
            bd.ShaderProgramme.Dispose();
            bd.ShaderProgramme = null;
        }

        var textures = ImGui.GetPlatformIO().Textures;
        for (int i = 0; i < textures.Size; i++)
        {
            var tex = textures[i];
            if (tex.RefCount == 1)
                DestroyTexture(tex, gl);
        }
    }
    
}