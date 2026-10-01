using FishPieClient.Graphics.Buffers;
using FishPieClient.Utils;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Materials;

public sealed class MaterialManager : IDisposable
{

    private readonly FlatMap<MaterialKey, MaterialData> _materialDataCpu;
    private readonly GL _gl;
    
    private MultiBuffer<PersistentBuffer<MaterialData>, MaterialData> _materialDataGpu;
    private uint _keyNum = 0;

    public Span<MaterialData> Data => _materialDataCpu.ValuesView;

    public uint Handle => _materialDataGpu.Handle;
    
    public MaterialManager(GL gl)
    {
        _gl = gl;
        _keyNum = 0;
        
        _materialDataCpu = new  FlatMap<MaterialKey, MaterialData>();
        _materialDataGpu = PersistentBuffer<MaterialData>.CreateMultiBuffer(1, "material_manager_buffer", _gl);
    }
    
    public MaterialKey Add(MaterialData materialData)
    {
        var key = new MaterialKey(_keyNum++);

        _materialDataCpu.Add(key, materialData);
        _materialDataGpu = Utils.ResizeGpuBuffer(Data, _materialDataGpu, _gl);
        
        return key;
    }

    public MaterialData this[MaterialKey materialKey]
    {
        get
        {
            Errors.Expect(_materialDataCpu.TryGetValue(materialKey, out var element),
                $"key {materialKey} does not exist");
            return element;
        }
        set => _materialDataCpu[materialKey] = value;
    }
    
    public void Remove(MaterialKey materialKey)
    {
        _materialDataCpu.Remove(materialKey);
    }

    public void Sync()
    {
        _materialDataGpu.Write(Data, 0);
    }
    
    public uint Index(MaterialKey materialKey)
    {
        int index = _materialDataCpu.IndexOfKey(materialKey);
        Errors.Expect(index != -1, $"could not find key: {materialKey}");
        return (uint)index;
    }

    public void Advance()
    {
        _materialDataGpu.Advance();
    }

    public void Dispose()
    {
        _materialDataGpu.Dispose();
    }
    
}