#ifndef KD_TREE_CPU_H
#define KD_TREE_CPU_H

#include <limits>
#include "KDTreeNode.h"
#include <mmintrin.h>

////////////////////////////////////////////////////
// Constants.
////////////////////////////////////////////////////


const bool USE_TIGHT_FITTING_BOUNDING_BOXES = false;

////////////////////////////////////////////////////
// KDTreeCPU.
////////////////////////////////////////////////////
class KDTreeCPU
{
public:
	KDTreeCPU( int num_tris, glm::vec3 *tris, int num_verts, glm::vec3 *verts, int *triangleIDs, int NUM_TRIS_PER_NODE);
	~KDTreeCPU( void );

	// Public traversal method that begins recursive search.
	int intersect( const glm::vec3 &ray_o, const glm::vec3 &ray_dir, float &t, glm::vec3 &hit_point, glm::vec3 &normal ) const;
	bool singleRayStacklessIntersect( const glm::vec3 &ray_o, const glm::vec3 &ray_dir, float &t, glm::vec3 &hit_point, glm::vec3 &normal ) const;

	void buildRopeStructure( void );
	// kd-tree getters.
	KDTreeNode* getRootNode( void ) const;
	int getNumLevels( void ) const;
	int getNumLeaves( void ) const;
	int getNumNodes( void ) const;

	// Input mesh getters.
	int getMeshNumVerts( void ) const;
	int getMeshNumTris( void ) const;
	glm::vec3* getMeshVerts( void ) const;
	glm::vec3* getMeshTris( void ) const;
	void SetTriangleVisible(int index, bool visible);
	// Debug methods.
	void printNumTrianglesInEachNode( KDTreeNode *curr_node, int curr_depth=1 );
	void printNodeIdsAndBounds( KDTreeNode *curr_node );
	BoundingBox computeTightFittingBoundingBox( int num_verts, glm::vec3 *verts );
private:
	// kd-tree variables.
	KDTreeNode *root;
	int num_levels, num_leaves, num_nodes;
    int NUM_TRIS_PER_NODE;
	// Input mesh variables.
	int num_verts, num_tris;
	glm::vec3 *verts, *tris;
	bool* visible;
	int* triangleIDs;
	KDTreeNode* constructTreeMedianSpaceSplit( int num_tris, int *tri_indices, BoundingBox bounds, int curr_depth );

	// Private recursive traversal method.
	int intersect( KDTreeNode *curr_node, const glm::vec3 &ray_o, const glm::vec3 &ray_dir, float &t, glm::vec3 &normal ) const;
	bool singleRayStacklessIntersect( KDTreeNode *curr_node, const glm::vec3 &ray_o, const glm::vec3 &ray_dir, float &t_entry, float &t_exit, glm::vec3 &normal ) const;

	// Rope construction.
	void buildRopeStructure( KDTreeNode *curr_node, KDTreeNode *ropes[], bool is_single_ray_case=false );
	void optimizeRopes( KDTreeNode *ropes[], BoundingBox bbox );

	// Bounding box getters.
	SplitAxis getLongestBoundingBoxSide( glm::vec3 min, glm::vec3 max );
	
	BoundingBox computeTightFittingBoundingBox( int num_tris, int *tri_indices );

	// Triangle getters.
	float getMinTriValue( int tri_index, SplitAxis axis );
	float getMaxTriValue( int tri_index, SplitAxis axis );
};

#endif