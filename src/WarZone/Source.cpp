#include "Platform.h"
#include "ZoneManager.h"
#include <Windows.h>

extern "C" { int _afxForceUSRDLL; }

static ZoneManager* _manager = nullptr;
extern "C"
{
	__declspec(dllexport) int __stdcall Pin(int zoneID, int x, int y)
	{
		if (_manager == nullptr)
		{
			printf("WarZone -- Error Must call InitZones() first!");
			return -1;
		}

		return _manager->Pin(zoneID, x, y);
	}

	__declspec(dllexport) void __stdcall InitZones(const char* path, int triCount)
	{
		if (_manager != nullptr)
			delete _manager;

		_manager = new ZoneManager(path, triCount);
	}

	__declspec(dllexport) void __stdcall LoadZone(int zoneID)
	{
		if (_manager == nullptr)
		{
			printf("WarZone -- Error Must call InitZones() first!");
			return;
		}

		_manager->LoadZone(zoneID);
		/*FILE* f = fopen("D:\\bin\\Debug\\logs\\leaks"+zoneID+".txt", "a+t");
		_CrtSetReportMode( _CRT_WARN, _CRTDBG_MODE_FILE );
		_CrtSetReportFile( _CRT_WARN, f );
		_CrtDumpMemoryLeaks();
		fclose(f);
		*/
	}

	__declspec(dllexport) void __stdcall UnLoadZone(int zoneID)
	{
		if (_manager == nullptr)
			return;

		_manager->UnloadZone(zoneID);
	}

	__declspec(dllexport) OcclusionResult __stdcall SegmentIntersect(int zoneIDA, int zoneIDB,
		float originX, float originY, float originZ,
		float targetX, float targetY, float targetZ,
		bool terrain, bool normalTest, int triCount, OcclussionInfo* result)
	{
		if (_manager == nullptr)
			return OcclusionResult::NotLoaded;

		return _manager->SegmentIntersect(zoneIDA, zoneIDB, originX, originY, originZ,
			targetX, targetY, targetZ, terrain, normalTest, triCount, result);
	}

	__declspec(dllexport) bool __stdcall TerrainIntersect(int zoneIDA, int zoneIDB,
		float originX, float originY, float originZ,
		float targetX, float targetY, float targetZ, int triCount, OcclussionInfo* result)
	{
		if (_manager == nullptr)
			return false;

		return _manager->TerrainIntersect(zoneIDA, zoneIDB, originX, originY, originZ,
			targetX, targetY, targetZ, triCount, result);
	}

	__declspec(dllexport) bool __stdcall SetFixtureVisible(int zoneID, uint32_t uniqueID, uint8_t instanceID, bool visible)
	{
		if (_manager == nullptr)
			return false;

		return _manager->SetFixtureVisible(zoneID, uniqueID, instanceID, visible);
	}

	__declspec(dllexport) bool __stdcall GetFixtureVisible(int zoneID, uint32_t uniqueID, uint8_t instanceID)
	{
		if (_manager == nullptr)
			return false;

		return _manager->GetFixtureVisible(zoneID, uniqueID, instanceID);
	}

	__declspec(dllexport) int __stdcall GetFixtureCount(int zoneID)
	{
		if (_manager == nullptr)
			return 0;

		return _manager->GetFixtureCount(zoneID);
	}

	__declspec(dllexport) bool __stdcall GetFixtureInfo(int zoneID, int index, FixtureInfo* info)
	{
		if (_manager == nullptr)
			return false;

		return _manager->GetFixtureInfo(zoneID, index, info);
	}
}

BOOL WINAPI DllMain(HANDLE hInst, ULONG ul_reason, LPVOID lpReserved)
{
	switch (ul_reason) {
	case DLL_PROCESS_ATTACH:
		break;
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

int main()
{
	//DEBUG build will create exe allowing to debug and single step through code.
	//RELEASE build, main() is ignored and DllMain is used as entry point

	//InitZones("C:\\Users\\Administrator\\Documents\\Visual Studio 2012\\Projects\\WarZoneLib-master\\Run", 125);
	//LoadZone(100);
	//_manager.InitPath("C:\\Users\\Administrator\\Documents\\Visual Studio 2012\\Projects\\WarZoneLib-master\\Run");
	//_manager.LoadZone(132, 190);
	//OcclussionInfo info;
	//SetFixtureVisible(100, 11645, 1, false);
	//SegmentIntersect(100, 100, 849900, 824970, 8019+100 , 850324, 826137, 7930+12, true, true,190,  &info);
	//_manager.SegmentIntersect(132,484261,358614,0xFFFF,479125,370464,0, true, true,250,  &info);
	//char c;
	//cin >> c;
	return 0;
}