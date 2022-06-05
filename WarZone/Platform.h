#pragma once
#define _CRT_SECURE_NO_DEPRECATE
#include "glm/glm.hpp"
#include <windows.h>

#include <vector>
#include <map>


using namespace std;

enum ChunkType
{
	ChunkType_Undefined = 0,
	ChunkType_Zone = 1,
	ChunkType_NIF = 2,
	ChunkType_Fixture = 3,
	ChunkType_Terrain = 4,
	ChunkType_Collision = 5,
	ChunkType_BSP = 6,
	ChunkType_Region = 7,
	ChunkType_Water = 8,
	ChunkType_Count
};

enum class OcclusionResult
{
	NotLoaded = -1,
	NotOccluded = 0,
	OccludedByGeometry = 1,
	OccludedByTerrain = 2,
	OccludedByWater = 3,
	OccludedByLava = 4,
	OccludedByDynamicObject = 5,
	OccludedByClosedDoor = 6
};

struct OcclussionInfo
{
	OcclusionResult Result;
	float HitX;
	float HitY;
	float HitZ;
	float SafeX;
	float SafeY;
	float SafeZ;
	int FixtureID;
	int SurfaceType;
	float WaterDepth;
};

struct FixtureInfo
{
	float X1;
	float Y1;
	float Z1;
	float X2;
	float Y2;
	float Z2;
	int SurfaceType;
	int UniqueID;
};

struct Vector3
{
	float X;
	float Y;
	float Z;
};

struct Triangle
{
	int i0;
	int i1;
	int i2;
	int uniqueID;
};

struct Fixture
{
	int SurfaceType;
	bool Visible;
	int TriangleStartIndex;
	int TriangleCount;
	int ID;
	Vector3 P1;
	Vector3 P2;
};


