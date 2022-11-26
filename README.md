# MagnetsGiant_Unity-DOTS-Based-DotsEffect
### A giant boss made of magnets that can't be destoryed!!!
This project using Unity 2022.2.0b16 & ECS package v1.0 to realize the unique visual effect of the boss battle scene.

**Overall the system works as follow:**
1. Voxelize given skinned mesh to small dark cubes, i.e dots.

2. Calculate the data needed for each dot, e.g Assigin each dot to closest bone (Not by parenting but calculate local to world matrix, since we need control each dot precisely, due to the update order in Job System parenting is not suitable in our case).

3. Initialize all systems:
    - Bullet entity system.

    - Bone entity system (Copied from skeleton of the skinned mesh to ECS Scene).

    - Dot entity system.

    - Bullet shooter (MonoBehaviour).

    - Boss(Ghost) controller (MonoBehaviour).

4. Run update in parallel through Unity Job System:
    1. Update bullet entities movement.
    
    2. Update bone entities movement by copy the transform from the original Skeletal Animation, and calculate local to world matrix for assigned dots.

    3. Update dot entities movement: 

        - follow the bone entities local transform -> been hit fly away -> wait for a while -> bcak to follow the bone entities local transform.

5. Other visual & sound effects triggered by event, e.g begin to attack and foot land on ground.

## Assets
- [Character model and its animations](https://www.mixamo.com/)
- [Old West Shotgun](https://sketchfab.com/3d-models/old-west-shotgun-bd3ca4e9d3ce473a85c9f6630cee27c5)
- [Heavy Stomp   Free Sound Effect](https://www.youtube.com/watch?v=7gVxObDQI5Y)
- Voxelization modified from: [unity-voxel](https://github.com/mattatz/unity-voxel)
- FPS Controller Modified from: [Demo FPS Controller](https://sharpcoderblog.com/blog/unity-3d-fps-controller)