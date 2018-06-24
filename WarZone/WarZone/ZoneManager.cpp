#include "ZoneManager.h"

ZoneManager::ZoneManager(const string& path, int triCount)
	: _zonesFolder(path)
	, _triCount(triCount)
{
	for (int i = 0; i < 500; i++)
		_zones[i] = nullptr;

	Log("Initialized");
}

bool ZoneManager::LoadZone(int zoneID)
{
	if (_zones[zoneID] != nullptr)
		return true;

	string path = Util::PathAppend(_zonesFolder, ::to_string(zoneID) + ".bin");

	if (Util::FileSize(path) <= 0)
	{
		Log("Error missing zone file " + path);
		return false;
	}

	FILE* f = fopen(path.c_str(), "rb+");

	if (f == nullptr)
	{
		Log("Error opening zone file " + path);
		return false;
	}

	int64_t size = 0;
	_fseeki64(f, 0, SEEK_END);
	size = _ftelli64(f);
	_fseeki64(f, 0, SEEK_SET);
	char fileCode[3];
	fread(fileCode, 3, 1, f);

	if (fileCode[0] == 'O' && fileCode[1] == 'C' && fileCode[2] == 'C')
	{
		char version;
		char headerSize;
		fread(&version, 1, 1, f);
		fread(&headerSize, 1, 1, f);
		_fseeki64(f, headerSize, SEEK_SET);

		Zone* zone = new Zone();
		while (_ftelli64(f) < size)
		{
			ChunkType chunkType;
			uint32_t chunkSize;
			fread(&chunkType, 4, 1, f);
			fread(&chunkSize, 4, 1, f);
			uint64_t nextChunk = _ftelli64(f) + chunkSize;

			switch (chunkType)
			{
				case ChunkType::ChunkType_Collision:
					LoadCollisionChunk(f, _triCount);
					break;
				case ChunkType::ChunkType_Region:
					LoadRegionChunk(f);
					break;
				case ChunkType::ChunkType_Terrain:
					LoadTerrainChunk(f);
					break;
				case ChunkType::ChunkType_Water:
					LoadWaterCollisionChunk(f, _triCount);
					break;
			}
			_fseeki64(f, nextChunk, SEEK_SET);
		}
	}
	else
	{
		Log("Invalid zone file header. " + path);
		return false;
	}
	fclose(f);

	return 0;
}

OcclusionResult ZoneManager::SegmentIntersect(int zoneIDA, int zoneIDB,
		float originX, float originY, float originZ,
		float targetX, float targetY, float targetZ,
		bool terrain, bool normalTest, int triCount, OcclussionInfo* result)
{

	if (!LoadZone(zoneIDA) || !LoadZone(zoneIDB))
		return OcclusionResult::NotLoaded;

	result->Result = OcclusionResult::NotOccluded;
	bool terrainHit = false;
	result->FixtureID = -1;

	if (terrain)
		terrainHit = TerrainIntersect(zoneIDA, zoneIDB, originX, originY, originZ, targetX, targetY, targetZ, triCount, result);

	glm::vec3 hit_point, normal;
	glm::vec3 from, target;

	from[0] = 0xFFFF - (originX - _zones[zoneIDA]->OffsetX);
	from[1] = originY - _zones[zoneIDA]->OffsetY;
	from[2] = originZ;

	target[0] = 0xFFFF - (targetX - _zones[zoneIDA]->OffsetX);
	target[1] = targetY - _zones[zoneIDA]->OffsetY;
	target[2] = targetZ;

	glm::vec3 dir = glm::normalize(target - from);
	double distance = Util::GetDistance(from, target);
	float t = 0;
	int count = 0;

	int hit = _zones[zoneIDA]->kd_tree->intersect(from, dir, t, hit_point, normal);

	if (hit && t <= distance && hit_point.z)
	{
		if (normalTest && normal.z < 0 && hit <= 0xFFFF)
		{
			while (hit && normal.z < 0 && count < 10)
			{

				glm::vec3 newp = Util::Lerp(hit_point, target, 0.001);
				hit = _zones[zoneIDA]->kd_tree->intersect(newp, dir, t, hit_point, normal);
				count++;
			}
		}

		result->HitX = (0xFFFF - hit_point[0]) + _zones[zoneIDA]->OffsetX;
		result->HitY = hit_point[1] + _zones[zoneIDA]->OffsetY;
		result->HitZ = hit_point[2];
		result->FixtureID = hit & 0xFFFFFF;
		result->SurfaceType = hit >> 24;
		result->WaterDepth = 0;
		if (result->SurfaceType == 0)
			result->SurfaceType = 27;

		result->Result = OcclusionResult::OccludedByGeometry;
		return  OcclusionResult::OccludedByGeometry;
	}

	if (terrain && terrainHit)
		result->Result = OcclusionResult::OccludedByTerrain;

	return result->Result;
}

int ZoneManager::GetFixtureCount(int zoneID)
{
	if (zoneID < 500 && _zones[zoneID] != 0)
	{
		return (int)_zones[zoneID]->FixtureList.size();
	}
	return 0;
}

bool ZoneManager::GetFixtureInfo(int zoneID, int index, FixtureInfo* info)
{
	if (zoneID < 500 && _zones[zoneID] != 0 && index >= 0 && index < _zones[zoneID]->FixtureList.size())
	{
		info->X1 = 0xFFFF - _zones[zoneID]->FixtureList[index]->P2.X;
		info->Y1 = _zones[zoneID]->FixtureList[index]->P1.Y;
		info->Z1 = _zones[zoneID]->FixtureList[index]->P1.Z;

		info->X2 = 0xFFFF - _zones[zoneID]->FixtureList[index]->P1.X;
		info->Y2 = _zones[zoneID]->FixtureList[index]->P2.Y;
		info->Z2 = _zones[zoneID]->FixtureList[index]->P2.Z;

		info->SurfaceType = _zones[zoneID]->FixtureList[index]->SurfaceType;
		info->UniqueID = _zones[zoneID]->FixtureList[index]->ID;
		return true;
	}
	return false;
}
bool ZoneManager::GetFixtureVisible(int zoneID, uint32_t uniqueID, uint8_t instanceID)
{
	if (!LoadZone(zoneID))
		return false;

	int id = (instanceID << 24) | uniqueID;

	if (_zones[zoneID]->Fixtures.find(id) != _zones[zoneID]->Fixtures.end())
	{
		return _zones[zoneID]->Fixtures[id]->Visible;
	}
	return false;
}

bool ZoneManager::SetFixtureVisible(int zoneID, uint32_t uniqueID, uint8_t instanceID, bool visible)
{
	int doorData = (((uniqueID >> 14 & 0xFF) << 30) | (zoneID << 20) | ((uniqueID & 0x3FFF) << 6) | 0x28 + instanceID);
	//Log("setting los (" + ::to_string(visible) + ") for doorID " + ::to_string(doorData));

	if (!LoadZone(zoneID))
		return false;

	int id = (instanceID << 24) | uniqueID;

	if (_zones[zoneID]->Fixtures.find(id) != _zones[zoneID]->Fixtures.end())
	{
		_zones[zoneID]->Fixtures[id]->Visible = visible;
		for (int i = _zones[zoneID]->Fixtures[id]->TriangleStartIndex;
			i < _zones[zoneID]->Fixtures[id]->TriangleStartIndex + _zones[zoneID]->Fixtures[id]->TriangleCount;
			i++)
		{
			_zones[zoneID]->kd_tree->SetTriangleVisible(i, visible);
		}
		return true;
	}
	else
	{
		Log("Zone " + ::to_string(zoneID)
			+ " does not contain fixtureID=" + ::to_string(uniqueID)
			+ " instanceID=" + ::to_string(instanceID));
	}
	return false;
}

int ZoneManager::Pin(int zoneID, int xLoc, int yLoc, int triCount)
{
	if (!LoadZone(zoneID))
		return 0;

	float x = (float)xLoc;
	float y = (float)yLoc;

	int xcoord = (int)floor(x / 256.0f);
	int ycoord = (int)floor(y / 256.0f);

	if (xcoord < 0)
		xcoord = 0;
	if (xcoord > 255)
		xcoord = 255;

	if (ycoord < 0)
		ycoord = 0;
	if (ycoord > 255)
		ycoord = 255;

	if (_zones[zoneID]->Holemap[(ycoord * _zones[zoneID]->HolemapWidth) + xcoord] == 0)
	{
		return -1;
	}


	xcoord = (int)floor(x / 64.0f);
	ycoord = (int)floor(y / 64.0f);

	if (xcoord < 0)
		xcoord = 0;
	if (xcoord > 1023)
		xcoord = 1023;

	if (ycoord < 0)
		ycoord = 0;
	if (ycoord > 1023)
		ycoord = 1023;


	float  z1 = _zones[zoneID]->Terrain[(ycoord * _zones[zoneID]->TerrainWidth) + xcoord];
	float  z2 = _zones[zoneID]->Terrain[(ycoord * _zones[zoneID]->TerrainWidth) + (xcoord + 1)];
	float  z3 = _zones[zoneID]->Terrain[((ycoord + 1) * _zones[zoneID]->TerrainWidth) + (xcoord + 1)];
	float  z4 = _zones[zoneID]->Terrain[((ycoord + 1) * _zones[zoneID]->TerrainWidth) + (xcoord)];

	float ave = (z1 + z2 + z3 + z4) / 4;
	float ave3 = (ave + z4) / 2;
	glm::vec3 ray_o, ray1, ray2, ray3, hitpoint, ray_target;

	float d = 0;

	ray_o[0] = (float)xLoc;
	ray_o[1] = (float)yLoc;
	ray_o[2] = 0xFFFF;

	ray_target[0] = (float)xLoc;
	ray_target[1] = (float)yLoc;
	ray_target[2] = 0;

	glm::vec3 dir = glm::normalize(ray_target - ray_o);

	ray1[0] = ((float)xcoord * 64);
	ray1[1] = ((float)ycoord * 64);
	ray1[2] = z1;

	ray2[0] = ((float)xcoord * 64 + 64);
	ray2[1] = ((float)ycoord * 64);
	ray2[2] = z2;

	ray3[0] = ((float)xcoord * 64);
	ray3[1] = ((float)ycoord * 64 + 64);
	ray3[2] = z4;


	if (Intersections::triIntersect(ray_o, dir, ray1, ray2, ray3, d, hitpoint))
	{
		return (int)floor((float)0xFFFF - d);
	}

	ray1[0] = ((float)xcoord * 64 + 64);
	ray1[1] = ((float)ycoord * 64);
	ray1[2] = z2;

	ray2[0] = ((float)xcoord * 64 + 64);
	ray2[1] = ((float)ycoord * 64 + 64);
	ray2[2] = z3;

	ray3[0] = ((float)xcoord * 64);
	ray3[1] = ((float)ycoord * 64 + 64);
	ray3[2] = z4;


	if (Intersections::triIntersect(ray_o, dir, ray1, ray2, ray3, d, hitpoint))
	{
		return (int)floor((float)0xFFFF - d);
	}

	return (int)ave3;
}

bool ZoneManager::TerrainIntersect(int zoneIDA, int zoneIDB, float originX, float originY, float originZ,
	float destX, float destY, float destZ, int triCount, OcclussionInfo* result)
{
	if (!LoadZone(zoneIDA) || !LoadZone(zoneIDB))
		return false;

	glm::vec3 from, target;

	from[0] =  (originX - _zones[zoneIDA]->OffsetX);
	from[1] = originY - _zones[zoneIDA]->OffsetY;
	from[2] = originZ;

	target[0] =  (destX - _zones[zoneIDA]->OffsetX);
	target[1] = destY - _zones[zoneIDA]->OffsetY;
	target[2] = destZ;

	glm::vec3 hit_point, normal;
	glm::vec3 dir = glm::normalize(target - from);

	float t;
	result->FixtureID = -1;

	int waterHit = 0;
	bool terrainHit = false;

	//simple height test, no need to do line of sight test
	if (from[0] == target[0] && from[1] == target[1])
	{

		waterHit = _zones[zoneIDA]->kd_tree_water->intersect(from, dir, t, hit_point, normal);
		t = 0xFFFF - t;
		int height = Pin(zoneIDA, (int)originX - (int)_zones[zoneIDA]->OffsetX, (int)originY - (int)_zones[zoneIDA]->OffsetY, triCount);
		terrainHit = true;

		if (waterHit && t > height)
		{
			//waterHit = true;
		}
		else
		{
			waterHit = 0;
		}

		if ((target[2] < from[2] && height > target[2]) || (target[2] > from[2] && height < target[2]))
		{
			result->HitX = destX;
			result->HitY = destY;
			result->HitZ = (float)height;
		}
	}

	if (!terrainHit)
	{
		glm::vec3 current = from;
		double distance = Util::GetDistance(from, target);
		double currentDistance = 0;
		glm::vec3 pointA = from;
		glm::vec3 pointB = target;

		int incr = (int)(distance / 12.0);

		if (incr == 0)
			incr = 1;
		int height = Pin(zoneIDA, (int)current.x, (int)current.y, triCount);
		int height2 = Pin(zoneIDA, (int)target.x, (int)target.y, triCount);

		//perform terrain pin only if terrain is under from and destination
		if (height < current.z && height2 < target.z)
		{

			while (currentDistance < distance)
			{
				double t = currentDistance / distance;
				current = Util::Lerp(pointA, pointB, currentDistance / distance);

				height = Pin(zoneIDA, (int)current.x, (int)current.y, triCount);
				printf("%d\r\n", height);
				if (height < 0)
				{
					terrainHit = false;
					break;
				}
				if (height >= 0 && height > current.z)
				{
					result->HitX = current.x + _zones[zoneIDA]->OffsetX;
					result->HitY = current.y + _zones[zoneIDA]->OffsetY;
					result->HitZ = current.z;
					terrainHit = true;
				}
				currentDistance += incr;

			}
		}
	}
	if (waterHit || terrainHit)
	{
		//water was hit that is above terrain
		if (waterHit && hit_point.z > result->HitZ)
		{
			result->WaterDepth = hit_point.z - result->HitZ;
			result->FixtureID = (waterHit & 0xFFFFFF) - 0xFFFF;
			result->SurfaceType = waterHit >> 24;
			result->HitZ = hit_point.z;
		}
		else
		{
			result->SurfaceType = 28;
			result->FixtureID = 0;
			result->WaterDepth = 0;
		}

		return true;
	}
	return false;
}

void ZoneManager::UnloadZone(int zoneID)
{
	if (_zones[zoneID] != nullptr)
	{
		delete _zones[zoneID]->kd_tree;

		if (_zones[zoneID]->Terrain != 0)
			delete[] _zones[zoneID]->Terrain;

		_zones[zoneID] = nullptr;
	}
}

void ZoneManager::LoadTerrainChunk(FILE* f)
{
	uint32_t regionID = 0;
	uint32_t zoneID = 0;

	fread(&regionID, 4, 1, f);
	fread(&zoneID, 4, 1, f);

	if (_zones[zoneID] == 0)
		_zones[zoneID] = new Zone();

	fread(&_zones[zoneID]->TerrainWidth, 4, 1, f);
	fread(&_zones[zoneID]->TerrainHeight, 4, 1, f);

	fread(&_zones[zoneID]->HolemapWidth, 4, 1, f);
	fread(&_zones[zoneID]->HolemapHeight, 4, 1, f);

	_zones[zoneID]->Terrain = new uint16_t[_zones[zoneID]->TerrainWidth * _zones[zoneID]->TerrainHeight];
	_zones[zoneID]->Holemap = new unsigned char[_zones[zoneID]->HolemapWidth * _zones[zoneID]->HolemapHeight];

	fread(_zones[zoneID]->Terrain, _zones[zoneID]->TerrainWidth * _zones[zoneID]->TerrainHeight * 2, 1, f);
	fread(_zones[zoneID]->Holemap, _zones[zoneID]->HolemapWidth * _zones[zoneID]->HolemapHeight, 1, f);
}

void ZoneManager::LoadRegionChunk(FILE* f)
{
	int32_t regionID = 0;
	int32_t zoneCount = 0;
	fread(&regionID, 4, 1, f);
	fread(&zoneCount, 4, 1, f);

	for (int i = 0; i < zoneCount; i++)
	{
		uint32_t zoneID = 0;
		uint32_t offsetX = 0;
		uint32_t offsetY = 0;
		uint32_t nifCount = 0;
		uint32_t ficturesCount = 0;

		fread(&zoneID, 4, 1, f);
		fread(&offsetX, 4, 1, f);
		fread(&offsetY, 4, 1, f);
		fread(&nifCount, 4, 1, f);
		fread(&ficturesCount, 4, 1, f);

		if (_zones[zoneID] == 0)
			_zones[zoneID] = new Zone();

		_zones[zoneID]->ZoneID = zoneID;
		_zones[zoneID]->OffsetX = offsetX;
		_zones[zoneID]->OffsetY = offsetY;
		_zones[zoneID]->RegionID = regionID;
	}
}

void ZoneManager::LoadCollisionChunk(FILE* f, int triCount)
{
	uint32_t regionID = 0;
	uint32_t zoneID = 0;

	fread(&regionID, 4, 1, f);
	fread(&zoneID, 4, 1, f);

	if (_zones[zoneID] == 0)
		_zones[zoneID] = new Zone();

	Zone* zone = _zones[zoneID];

	zone->RegionID = regionID;
	zone->ZoneID = zoneID;

	fread(&zone->VertexCount, 4, 1, f);

	auto vertices = new glm::vec3[zone->VertexCount];
	memset(vertices, 0, zone->VertexCount * sizeof(glm::vec3));
	fread(vertices, zone->VertexCount * sizeof(glm::vec3), 1, f);

	fread(&zone->FixtureCount, 4, 1, f);
	fread(&zone->TriangleCount, 4, 1, f);
	fread(&zone->IndexSize, 4, 1, f);

	auto trianglesT = new Triangle[zone->TriangleCount];
	memset(trianglesT, 0, zone->TriangleCount * sizeof(Triangle));
	fread(trianglesT, zone->TriangleCount * sizeof(Triangle), 1, f);

	glm::vec3* triangles = new glm::vec3[zone->TriangleCount];

	Triangle* t = 0;
	int lastUniqueID = trianglesT[0].uniqueID;
	zone->Fixtures[lastUniqueID] = new Fixture();
	zone->FixtureList.push_back(zone->Fixtures[lastUniqueID]);
	zone->Fixtures[lastUniqueID]->TriangleStartIndex = 0;
	zone->Fixtures[lastUniqueID]->TriangleCount = 0;
	zone->Fixtures[lastUniqueID]->SurfaceType = trianglesT[0].uniqueID >> 24;
	zone->Fixtures[lastUniqueID]->ID = trianglesT[0].uniqueID & 0xFFFFFF;
	int* triangleIDs = new int[zone->TriangleCount];

	for (int i = 0; i< zone->TriangleCount; i++)
	{
		t = &trianglesT[i];
		if ((t->uniqueID) != lastUniqueID)
		{
			lastUniqueID = trianglesT[i].uniqueID;
			zone->Fixtures[lastUniqueID] = new Fixture();
			zone->FixtureList.push_back(zone->Fixtures[lastUniqueID]);
			zone->Fixtures[lastUniqueID]->TriangleCount = 0;
			zone->Fixtures[lastUniqueID]->TriangleStartIndex = i;
			zone->Fixtures[lastUniqueID]->SurfaceType = trianglesT[i].uniqueID >> 24;
			zone->Fixtures[lastUniqueID]->ID = trianglesT[i].uniqueID & 0xFFFFFF;
		}

		triangles[i][0] = (float)t->i0;
		triangles[i][1] = (float)t->i1;
		triangles[i][2] = (float)t->i2;
		triangleIDs[i] = t->uniqueID;

		zone->Fixtures[t->uniqueID]->TriangleCount++;
	}

	for (const auto k : zone->Fixtures)
	{
		auto fixture = zone->Fixtures[k.first];
		float minX = (float)0xFFFFFFF, minY = (float)0xFFFFFFF, minZ = (float)0xFFFFFFF, maxX = 0, maxY = 0, maxZ = 0;
		for (int i = fixture->TriangleStartIndex; i<fixture->TriangleStartIndex + fixture->TriangleCount; i++)
		{
			auto v1 = vertices[trianglesT[i].i0];
			auto v2 = vertices[trianglesT[i].i1];
			auto v3 = vertices[trianglesT[i].i2];

			if (v1.x < minX)
				minX = v1.x;
			if (v1.y < minY)
				minY = v1.y;
			if (v1.z < minZ)
				minZ = v1.z;

			if (v1.x > maxX)
				maxX = v1.x;
			if (v1.y > maxY)
				maxY = v1.y;
			if (v1.z > maxZ)
				maxZ = v1.z;

			if (v2.x < minX)
				minX = v2.x;
			if (v2.y < minY)
				minY = v2.y;
			if (v2.z < minZ)
				minZ = v2.z;

			if (v2.x > maxX)
				maxX = v2.x;
			if (v2.y > maxY)
				maxY = v2.y;
			if (v2.z > maxZ)
				maxZ = v2.z;

			if (v3.x < minX)
				minX = v3.x;
			if (v3.y < minY)
				minY = v3.y;
			if (v3.z < minZ)
				minZ = v3.z;

			if (v3.x > maxX)
				maxX = v3.x;
			if (v3.y > maxY)
				maxY = v3.y;
			if (v3.z > maxZ)
				maxZ = v3.z;
		}

		fixture->P1.X = minX;
		fixture->P1.Y = minY;
		fixture->P1.Z = minZ;

		fixture->P2.X = maxX;
		fixture->P2.Y = maxY;
		fixture->P2.Z = maxZ;
	}

	StopWatch s;
	s.Start();

	zone->kd_tree = new KDTreeCPU(
		zone->TriangleCount,
		triangles,
		zone->VertexCount,
		vertices,
		triangleIDs,
		triCount);

	delete[] triangles;
	delete[] trianglesT;
	delete[] vertices;
	delete[] triangleIDs;

	s.Stop();

	if (zone->ZoneID < 500)
	{
		_zones[zone->ZoneID] = zone;
		Log("Loaded zone " + ::to_string(zone->ZoneID) + " in " + ::to_string( s.MilliSeconds()) + "ms");
		Util::PrintMemoryInfo();
	}
}

void ZoneManager::LoadWaterCollisionChunk(FILE* f, int triCount)
{
	uint32_t regionID = 0;
	uint32_t zoneID = 0;

	fread(&regionID, 4, 1, f);
	fread(&zoneID, 4, 1, f);

	if (_zones[zoneID] == 0)
		_zones[zoneID] = new Zone();

	Zone* zone = _zones[zoneID];

	zone->RegionID = regionID;
	zone->ZoneID = zoneID;

	fread(&zone->VertexCount, 4, 1, f);

	auto vertices = new glm::vec3[zone->VertexCount];
	memset(vertices, 0, zone->VertexCount * sizeof(glm::vec3));
	fread(vertices, zone->VertexCount * sizeof(glm::vec3), 1, f);

	fread(&zone->FixtureCount, 4, 1, f);
	fread(&zone->TriangleCount, 4, 1, f);
	fread(&zone->IndexSize, 4, 1, f);

	auto trianglesT = new Triangle[zone->TriangleCount];
	memset(trianglesT, 0, zone->TriangleCount * sizeof(Triangle));
	fread(trianglesT, zone->TriangleCount * sizeof(Triangle), 1, f);

	glm::vec3* triangles = new glm::vec3[zone->TriangleCount];

	Triangle* t = 0;

	int lastUniqueID = trianglesT[0].uniqueID;
	zone->Fixtures[lastUniqueID] = new Fixture();
	zone->Fixtures[lastUniqueID]->TriangleStartIndex = 0;
	zone->Fixtures[lastUniqueID]->TriangleCount = 0;
	zone->Fixtures[lastUniqueID]->SurfaceType = trianglesT[0].uniqueID >> 24;
	zone->Fixtures[lastUniqueID]->ID = trianglesT[0].uniqueID & 0xFFFFFF;
	int* triangleIDs = new int[zone->TriangleCount];

	for (int i = 0; i< zone->TriangleCount; i++)
	{
		t = &trianglesT[i];
		triangleIDs[i] = t->uniqueID;
		if ((t->uniqueID) != lastUniqueID)
		{
			lastUniqueID = trianglesT[i].uniqueID;
			zone->Fixtures[lastUniqueID] = new Fixture();
			zone->Fixtures[lastUniqueID]->TriangleCount = 0;
			zone->Fixtures[lastUniqueID]->TriangleStartIndex = i;
			zone->Fixtures[lastUniqueID]->SurfaceType = trianglesT[i].uniqueID >> 24;
			zone->Fixtures[lastUniqueID]->ID = trianglesT[i].uniqueID & 0xFFFFFF;
		}

		triangles[i][0] = (float)t->i0;
		triangles[i][1] = (float)t->i1;
		triangles[i][2] = (float)t->i2;

		zone->Fixtures[t->uniqueID]->TriangleCount++;
	}

	zone->kd_tree_water = new KDTreeCPU(
		zone->TriangleCount,
		triangles,
		zone->VertexCount,
		vertices,
		triangleIDs,
		triCount);

	delete[] triangles;
	delete[] trianglesT;
	delete[] vertices;
	delete[] triangleIDs;
}

void ZoneManager::Log(const string& msg)
{
	printf("WarZone -- %s\r\n", msg.c_str());
}

ZoneManager::~ZoneManager()
{
	for (int i = 0; i < 500; i++)
	{
		if (_zones[i] != nullptr)
		{
			UnloadZone(i);
		}
	}
}