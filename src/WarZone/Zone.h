//#pragma once
//#include "KDTreeCPU.h"
//#include <vector>
//#include <map>
//#include <vector>
//
//using namespace std;
//
//struct Vector3
//{
//	float X;
//	float Y;
//	float Z;
//};
//
//struct Triangle
//{
//	int i0;
//	int i1;
//	int i2;
//	int uniqueID;
//};
//
//struct Fixture
//{
//	int SurfaceType;
//	bool Visible;
//	int TriangleStartIndex;
//	int TriangleCount;
//	int ID;
//    Vector3 P1;
//	Vector3 P2;
//};
//
//struct Zone
//{
//	int RegionID;
//	int ZoneID;
//	int VertexCount;
//	int OffsetX;
//	int OffsetY;
//	uint16_t* Terrain;
//	unsigned char* Holemap;
//	uint32_t TerrainWidth;
//	uint32_t TerrainHeight;
//	uint32_t HolemapWidth;
//	uint32_t HolemapHeight;
//	int TriangleCount;
//	int FixtureCount;
//	int IndexSize;
//	map<int, Fixture*> Fixtures;
//	vector<Fixture*> FixtureList;
//	KDTreeCPU *kd_tree;
//	KDTreeCPU *kd_tree_water;
//public:
//	Zone():Terrain(0)
//	{
//	}
//};