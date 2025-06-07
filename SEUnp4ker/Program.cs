using unp4k;

Globals.Arguments = [.. args];

EntryPoint.PreInit();
if (EntryPoint.Init())
{
    if (EntryPoint.PostInit())
    {
        Worker.Process_P4K();
        Worker.Extract();
    }
}