
using System.Runtime.InteropServices;
using SlateDb;
using SlateDb.Interop;

//using var db = new SlateDb.SlateDb("./db");
unsafe
{
    var objectStoreBuilderConfig = NativeMethods.slatedb_object_store_builder_config_new();
    
    
    NativeMethods.slatedb_object_store_builder_config_set(
        objectStoreBuilderConfig,
        "local_path".ToPtr(),
        "db".ToPtr()
    );

    var confArray = NativeMethods.slatedb_object_store_builder_config_get(objectStoreBuilderConfig);

    int len = (int)confArray.len;
    var result = new Dictionary<string, string>();

    for (int i = 0; i < len; i++)
    {
        IntPtr itemPtr = IntPtr.Add((IntPtr)confArray.data, i * Marshal.SizeOf<ObjectStoreConfigItem>());
        var item = Marshal.PtrToStructure<ObjectStoreConfigItem>(itemPtr);
        string key = Marshal.PtrToStringAnsi((IntPtr)item.key);
        string value = Marshal.PtrToStringAnsi((IntPtr)item.value);
        if(key != null)
            result.Add(key, value);
    } 
    
    NativeMethods.slatedb_object_store_builder_config_get_free(confArray);



    var objectStoreBuilder = NativeMethods.slatedb_object_store_builder_new(ObjectStoreType.InMemory, objectStoreBuilderConfig);

    var db = NativeMethods.slatedb_open_with_object_builder(
        "test".ToPtr(), objectStoreBuilder);
    
    NativeMethods.slatedb_object_store_builder_config_free(objectStoreBuilderConfig);
    
}
