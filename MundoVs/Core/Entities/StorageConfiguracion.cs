namespace MundoVs.Core.Entities;

public class StorageConfiguracionGlobal : BaseEntity
{
    public StorageProviderEnum StorageProvider { get; set; } = StorageProviderEnum.Local;
    public string? BasePath { get; set; }
    public string PathTemplate { get; set; } = StoragePathTemplates.Default;
}

public class EmpresaStorageConfiguracion : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public bool UsaConfiguracionGlobal { get; set; } = true;
    public StorageProviderEnum StorageProvider { get; set; } = StorageProviderEnum.Local;
    public string? BasePath { get; set; }
    public string PathTemplate { get; set; } = StoragePathTemplates.Default;
}

public enum StorageProviderEnum
{
    Local = 0
}

public static class StoragePathTemplates
{
    public const string Default = "{empresa}/{modulo}/{cliente}/{entidad}/{documento}/{anio}/{mes}";
}
