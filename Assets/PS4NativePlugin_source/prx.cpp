
#include <kernel.h>
#include <perf.h>

#define PRX_EXPORT extern "C" __declspec (dllexport)

extern "C" int module_start(size_t sz, const void* arg)
{
	return 0;
}

PRX_EXPORT int plugin_sceRazorCpuPushMarker(const char* label)
{
	return sceRazorCpuPushMarker(label, 0xffffffff /* color */, 1 /* flags */);
}

PRX_EXPORT int plugin_sceRazorCpuPopMarker()
{
	return sceRazorCpuPopMarker();
}
