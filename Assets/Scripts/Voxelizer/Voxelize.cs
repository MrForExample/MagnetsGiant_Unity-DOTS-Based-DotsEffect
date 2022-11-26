using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelSystem
{
    public class Voxelize : MonoBehaviour
    {
        public Mesh targetMesh;
        public int resolution = 30;
        List<Voxel_t> voxels;
        float unit;

        // Start is called before the first frame update
        void Start()
        {
            // Voxelize target mesh with CPU Voxelizer
            CPUVoxelizer.Voxelize(
                targetMesh,   // a target mesh
                resolution,  // # of voxels for largest AABB bounds
                out voxels,
                out unit
            );
            Debug.Log("Voxels Count: " + voxels.Count);
        }
        void OnDrawGizmos() 
        {
            if (voxels != null)
            {
                foreach (var v in voxels)
                {
                    Gizmos.DrawCube(v.position, new Vector3(unit, unit, unit));
                }
            }
        }
    }
}
