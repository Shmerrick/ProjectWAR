#pragma once

#include "Platform.h"
#include "Util.h"
#include <string>
#include "Zone.h"
#include "Stopwatch.h"
#include "Intersections.h"

using namespace std;

struct Zone
{
	int RegionID;
	int ZoneID;
	int VertexCount;
	int OffsetX;
	int OffsetY;
	uint16_t* Terrain;
	unsigned char* Holemap;
	uint32_t TerrainWidth;
	uint32_t TerrainHeight;
	uint32_t HolemapWidth;
	uint32_t HolemapHeight;
	int TriangleCount;
	int FixtureCount;
	int IndexSize;
	map<int, Fixture*> Fixtures;
	vector<Fixture*> FixtureList;
	KDTreeCPU *kd_tree;
	KDTreeCPU *kd_tree_water;
public:
	Zone() 
		: Terrain(0){}
};

class ZoneManager
{
public:
	ZoneManager(const string& path, int triCount);
	~ZoneManager();
	OcclusionResult SegmentIntersect(int zoneIDA, int zoneIDB, float originX, float originY, float originZ, float targetX, float targetY, float targetZ, bool terrain, bool normalTest, int triCount, OcclussionInfo* result);
	int GetFixtureCount(int zoneID);
	bool GetFixtureInfo(int zoneID, int index, FixtureInfo* info);
	bool SetFixtureVisible(int zoneID, uint32_t uniqueID, uint8_t instanceID, bool visible);
	bool GetFixtureVisible(int zoneID, uint32_t uniqueID, uint8_t instanceID);
	int Pin(int zoneID, int xLoc, int yLoc, int triCount);
	bool TerrainIntersect(int zoneIDA, int zoneIDB, float originX, float originY, float originZ, float destX, float destY, float destZ, int triCount, OcclussionInfo* result);
	void UnloadZone(int zoneID);
	bool LoadZone(int zoneID);
private:
	void Log(const string& msg);
	void LoadTerrainChunk(FILE* f);
	void LoadRegionChunk(FILE* f);
	void LoadCollisionChunk(FILE* f, int triCount);
	void LoadWaterCollisionChunk(FILE* f, int triCount);
private:
	int _triCount;
	string _zonesFolder;
	Zone* _zones[500];
};
