using BlazorGL.Core.Textures;

namespace BlazorGL.Loaders.Textures;

/// <summary>
/// Loader for DirectDraw Surface (DDS) files
/// Supports BC1-BC7 compressed formats and cubemaps
/// </summary>
public class DDSLoader
{
    private readonly HttpClient _httpClient;

    // DDS constants
    private const uint DDS_MAGIC = 0x20534444; // "DDS " in ASCII
    private const uint DDSD_MIPMAPCOUNT = 0x20000;
    private const uint DDPF_FOURCC = 0x4;
    private const uint DDSCAPS2_CUBEMAP = 0x200;

    // FourCC codes
    private const uint FOURCC_DXT1 = 0x31545844; // "DXT1"
    private const uint FOURCC_DXT3 = 0x33545844; // "DXT3"
    private const uint FOURCC_DXT5 = 0x35545844; // "DXT5"
    private const uint FOURCC_ATI1 = 0x31495441; // "ATI1"
    private const uint FOURCC_ATI2 = 0x32495441; // "ATI2"
    private const uint FOURCC_DX10 = 0x30315844; // "DX10"

    /// <summary>
    /// Create DDS loader
    /// </summary>
    public DDSLoader(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Load DDS texture from URL
    /// </summary>
    public async Task<CompressedTexture> LoadAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        byte[] data = await _httpClient.GetByteArrayAsync(url);

        // Parse DDS file
        var ddsData = ParseDDS(data);

        // Create compressed texture
        var texture = new CompressedTexture(ddsData.Mipmaps, ddsData.Format)
        {
            Width = ddsData.Width,
            Height = ddsData.Height,
            IsCubemap = ddsData.IsCubemap,
            GenerateMipmaps = false, // DDS includes mipmaps
            Name = Path.GetFileName(url)
        };

        return texture;
    }

    private DDSData ParseDDS(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        // Read magic number
        uint magic = reader.ReadUInt32();
        if (magic != DDS_MAGIC)
            throw new FormatException($"Not a valid DDS file. Magic: 0x{magic:X}");

        // Read DDS_HEADER (124 bytes)
        var header = ReadDDSHeader(reader);

        // Determine compression format
        var format = DetermineFormat(header);

        // Check for DX10 extended header
        bool hasDX10Header = header.PixelFormat.FourCC == FOURCC_DX10;
        if (hasDX10Header)
        {
            // Read DDS_HEADER_DXT10 (20 bytes)
            var dx10Header = ReadDX10Header(reader);
            format = DetermineDX10Format(dx10Header);
        }

        // Read mipmap data
        var mipmaps = ReadMipmaps(reader, header, format);

        return new DDSData
        {
            Width = (int)header.Width,
            Height = (int)header.Height,
            Format = format,
            Mipmaps = mipmaps,
            IsCubemap = (header.Caps2 & DDSCAPS2_CUBEMAP) != 0
        };
    }

    private DDSHeader ReadDDSHeader(BinaryReader reader)
    {
        var header = new DDSHeader
        {
            Size = reader.ReadUInt32(),
            Flags = reader.ReadUInt32(),
            Height = reader.ReadUInt32(),
            Width = reader.ReadUInt32(),
            PitchOrLinearSize = reader.ReadUInt32(),
            Depth = reader.ReadUInt32(),
            MipmapCount = reader.ReadUInt32()
        };

        // Skip reserved[11]
        reader.ReadBytes(11 * 4);

        // Read DDPIXELFORMAT
        header.PixelFormat = new DDSPixelFormat
        {
            Size = reader.ReadUInt32(),
            Flags = reader.ReadUInt32(),
            FourCC = reader.ReadUInt32(),
            RGBBitCount = reader.ReadUInt32(),
            RBitMask = reader.ReadUInt32(),
            GBitMask = reader.ReadUInt32(),
            BBitMask = reader.ReadUInt32(),
            ABitMask = reader.ReadUInt32()
        };

        // Read caps
        header.Caps = reader.ReadUInt32();
        header.Caps2 = reader.ReadUInt32();
        header.Caps3 = reader.ReadUInt32();
        header.Caps4 = reader.ReadUInt32();

        // Skip reserved2
        reader.ReadUInt32();

        return header;
    }

    private DX10Header ReadDX10Header(BinaryReader reader)
    {
        return new DX10Header
        {
            DXGIFormat = reader.ReadUInt32(),
            ResourceDimension = reader.ReadUInt32(),
            MiscFlag = reader.ReadUInt32(),
            ArraySize = reader.ReadUInt32(),
            MiscFlags2 = reader.ReadUInt32()
        };
    }

    private CompressedTextureFormat DetermineFormat(DDSHeader header)
    {
        var pf = header.PixelFormat;

        if ((pf.Flags & DDPF_FOURCC) != 0)
        {
            return pf.FourCC switch
            {
                FOURCC_DXT1 => CompressedTextureFormat.BC1,
                FOURCC_DXT3 => CompressedTextureFormat.BC2,
                FOURCC_DXT5 => CompressedTextureFormat.BC3,
                FOURCC_ATI1 => CompressedTextureFormat.BC4,
                FOURCC_ATI2 => CompressedTextureFormat.BC5,
                _ => throw new FormatException($"Unsupported DDS FourCC: 0x{pf.FourCC:X}")
            };
        }

        throw new FormatException("Unsupported DDS format: No FourCC code");
    }

    private CompressedTextureFormat DetermineDX10Format(DX10Header dx10Header)
    {
        // DXGI_FORMAT enum values
        return dx10Header.DXGIFormat switch
        {
            71 => CompressedTextureFormat.BC1,  // DXGI_FORMAT_BC1_UNORM
            74 => CompressedTextureFormat.BC2,  // DXGI_FORMAT_BC2_UNORM
            77 => CompressedTextureFormat.BC3,  // DXGI_FORMAT_BC3_UNORM
            80 => CompressedTextureFormat.BC4,  // DXGI_FORMAT_BC4_UNORM
            83 => CompressedTextureFormat.BC5,  // DXGI_FORMAT_BC5_UNORM
            95 => CompressedTextureFormat.BC6H, // DXGI_FORMAT_BC6H_UF16
            98 => CompressedTextureFormat.BC7,  // DXGI_FORMAT_BC7_UNORM
            _ => throw new FormatException($"Unsupported DXGI format: {dx10Header.DXGIFormat}")
        };
    }

    private List<MipmapData> ReadMipmaps(BinaryReader reader, DDSHeader header, CompressedTextureFormat format)
    {
        var mipmaps = new List<MipmapData>();
        int width = (int)header.Width;
        int height = (int)header.Height;
        int mipCount = (header.Flags & DDSD_MIPMAPCOUNT) != 0 ? (int)Math.Max(1, header.MipmapCount) : 1;

        for (int level = 0; level < mipCount; level++)
        {
            int dataSize = CalculateCompressedSize(width, height, format);
            byte[] mipData = reader.ReadBytes(dataSize);

            mipmaps.Add(new MipmapData
            {
                Data = mipData,
                Width = width,
                Height = height,
                Level = level
            });

            // Calculate next mip level size
            width = Math.Max(1, width / 2);
            height = Math.Max(1, height / 2);
        }

        return mipmaps;
    }

    private int CalculateCompressedSize(int width, int height, CompressedTextureFormat format)
    {
        // Block size in bytes for each format
        int blockSize = format switch
        {
            CompressedTextureFormat.BC1 => 8,   // 64 bits per 4x4 block
            CompressedTextureFormat.BC2 => 16,  // 128 bits per 4x4 block
            CompressedTextureFormat.BC3 => 16,  // 128 bits per 4x4 block
            CompressedTextureFormat.BC4 => 8,   // 64 bits per 4x4 block
            CompressedTextureFormat.BC5 => 16,  // 128 bits per 4x4 block
            CompressedTextureFormat.BC6H => 16, // 128 bits per 4x4 block
            CompressedTextureFormat.BC7 => 16,  // 128 bits per 4x4 block
            _ => throw new ArgumentException($"Unknown format: {format}")
        };

        // Calculate number of blocks (round up for non-multiple dimensions)
        int blocksWide = Math.Max(1, (width + 3) / 4);
        int blocksHigh = Math.Max(1, (height + 3) / 4);

        return blocksWide * blocksHigh * blockSize;
    }
}

/// <summary>
/// Parsed DDS file data
/// </summary>
internal class DDSData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public CompressedTextureFormat Format { get; set; }
    public List<MipmapData> Mipmaps { get; set; } = new();
    public bool IsCubemap { get; set; }
}

/// <summary>
/// DDS header structure
/// </summary>
internal class DDSHeader
{
    public uint Size { get; set; }
    public uint Flags { get; set; }
    public uint Height { get; set; }
    public uint Width { get; set; }
    public uint PitchOrLinearSize { get; set; }
    public uint Depth { get; set; }
    public uint MipmapCount { get; set; }
    public DDSPixelFormat PixelFormat { get; set; } = new();
    public uint Caps { get; set; }
    public uint Caps2 { get; set; }
    public uint Caps3 { get; set; }
    public uint Caps4 { get; set; }
}

/// <summary>
/// DDS pixel format structure
/// </summary>
internal class DDSPixelFormat
{
    public uint Size { get; set; }
    public uint Flags { get; set; }
    public uint FourCC { get; set; }
    public uint RGBBitCount { get; set; }
    public uint RBitMask { get; set; }
    public uint GBitMask { get; set; }
    public uint BBitMask { get; set; }
    public uint ABitMask { get; set; }
}

/// <summary>
/// DX10 extended header
/// </summary>
internal class DX10Header
{
    public uint DXGIFormat { get; set; }
    public uint ResourceDimension { get; set; }
    public uint MiscFlag { get; set; }
    public uint ArraySize { get; set; }
    public uint MiscFlags2 { get; set; }
}
