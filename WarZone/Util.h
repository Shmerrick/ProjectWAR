#include <mmsystem.h>
#include <psapi.h>
#include "Platform.h"

class Util
{
public:
	static inline double GetDistance(glm::vec3 locA, glm::vec3 locB)
	{
		double deltaX = (int)locB.x - (int)locA.x;
		double deltaY = (int)locB.y - (int)locA.y;
		double deltaZ = (int)locB.z - (int)locA.z;

		return sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
	}

	static inline glm::vec3 Lerp(glm::vec3 a, glm::vec3 b, double t)
	{
		return glm::vec3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
	}

	static std::string Util::PathAppend(const std::string& p1, const std::string& p2)
	{
		char sep = '/';
		std::string tmp = p1;

#ifdef _WIN32
		sep = '\\';
#endif

		if (p1[p1.length()] != sep) { // Need to add a
			tmp += sep;                // path separator
			return(tmp + p2);
		}
		else
			return(p1 + p2);
	}

	static __int64 Util::FileSize(std::string name)
	{
		std::wstring wc(name.begin(), name.end());
		auto result = FileSize(wc.c_str());
		return result;
	}

	static __int64  Util::FileSize(const wchar_t* name)
	{
		HANDLE hFile = CreateFile(name, GENERIC_READ,
			FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING,
			FILE_ATTRIBUTE_NORMAL, NULL);
		if (hFile == INVALID_HANDLE_VALUE)
			return -1; // error condition, could call GetLastError to find out more

		LARGE_INTEGER size;
		if (!GetFileSizeEx(hFile, &size))
		{
			CloseHandle(hFile);
			return -1; // error condition, could call GetLastError to find out more
		}

		CloseHandle(hFile);
		return size.QuadPart;
	}

	static void PrintMemoryInfo()
	{
		PrintMemoryInfo(GetCurrentProcessId());
	}

	static void PrintMemoryInfo(DWORD processID)
	{
		HANDLE hProcess;
		PROCESS_MEMORY_COUNTERS pmc;

		// Print the process identifier.

		printf("\nProcess ID: %u\n", processID);

		// Print information about the memory usage of the process.

		hProcess = OpenProcess(PROCESS_QUERY_INFORMATION |
			PROCESS_VM_READ,
			FALSE, processID);
		if (NULL == hProcess)
			return;

		if (GetProcessMemoryInfo(hProcess, &pmc, sizeof(pmc)))
		{
			printf("\tWorkingSetSize: %dMB\r\n", (int)(pmc.WorkingSetSize / 1024 / 1024));

		}

		CloseHandle(hProcess);
	}
};