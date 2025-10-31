# BlazorGL 100% API Coverage Implementation Plan

Goal: Achieve complete Three.js API parity for BlazorGL.

## Phase 1: Core Rendering Objects (High Priority)

### 1.1 Line Rendering
**Files to create:**
- `src/BlazorGL.Core/Core/Line.cs` - Base line class
- `src/BlazorGL.Core/Core/LineSegments.cs` - Disconnected line segments
- `src/BlazorGL.Core/Core/LineLoop.cs` - Closed loop
- `src/BlazorGL.Core/Materials/LineBasicMaterial.cs` - Solid lines
- `src/BlazorGL.Core/Materials/LineDashedMaterial.cs` - Dashed lines
- `src/BlazorGL.Core/Shaders/LineShaders.cs` - GLSL shaders for lines

**Implementation details:**
- Use GL_LINES, GL_LINE_STRIP, GL_LINE_LOOP primitives
- Support line width (platform-dependent)
- Line dash pattern for LineDashedMaterial
- Vertex colors support

### 1.2 Point Rendering
**Files to create:**
- `src/BlazorGL.Core/Core/Points.cs` - Point cloud object
- `src/BlazorGL.Core/Materials/PointsMaterial.cs` - Point rendering material

**Implementation details:**
- Use GL_POINTS primitive
- Point size control
- Vertex colors
- Texture splatting

### 1.3 Sprite Rendering
**Files to create:**
- `src/BlazorGL.Core/Core/Sprite.cs` - 2D billboard
- `src/BlazorGL.Core/Materials/SpriteMaterial.cs` - Sprite material

**Implementation details:**
- Always face camera
- 2D texture mapping
- Transparency support
- Scale in screen space

---

## Phase 2: Geometries (15 Missing Types)

### 2.1 Simple Primitives
**Files to create:**
- `src/BlazorGL.Core/Geometries/ConeGeometry.cs` - Cone shape
- `src/BlazorGL.Core/Geometries/CapsuleGeometry.cs` - Pill/capsule
- `src/BlazorGL.Core/Geometries/CircleGeometry.cs` - Flat disc
- `src/BlazorGL.Core/Geometries/RingGeometry.cs` - Flat ring/annulus

### 2.2 Complex Primitives
**Files to create:**
- `src/BlazorGL.Core/Geometries/TorusKnotGeometry.cs` - Torus knot
- `src/BlazorGL.Core/Geometries/TubeGeometry.cs` - Path extrusion
- `src/BlazorGL.Core/Geometries/LatheGeometry.cs` - Rotational geometry

### 2.3 Shape Extrusion
**Files to create:**
- `src/BlazorGL.Core/Geometries/ExtrudeGeometry.cs` - 2D to 3D extrusion
- `src/BlazorGL.Core/Geometries/ShapeGeometry.cs` - 2D shapes
- `src/BlazorGL.Core/Math/Shape.cs` - 2D shape definition
- `src/BlazorGL.Core/Math/Path.cs` - 2D path
- `src/BlazorGL.Core/Math/Curve.cs` - Curve utilities

### 2.4 Platonic Solids
**Files to create:**
- `src/BlazorGL.Core/Geometries/PolyhedronGeometry.cs` - Base polyhedron
- `src/BlazorGL.Core/Geometries/TetrahedronGeometry.cs` - 4 faces
- `src/BlazorGL.Core/Geometries/OctahedronGeometry.cs` - 8 faces
- `src/BlazorGL.Core/Geometries/DodecahedronGeometry.cs` - 12 faces
- `src/BlazorGL.Core/Geometries/IcosahedronGeometry.cs` - 20 faces

### 2.5 Utility Geometries
**Files to create:**
- `src/BlazorGL.Core/Geometries/EdgesGeometry.cs` - Extract edges
- `src/BlazorGL.Core/Geometries/WireframeGeometry.cs` - Wireframe conversion

---

## Phase 3: Materials (12 Missing Types)

### 3.1 Advanced PBR
**Files to create:**
- `src/BlazorGL.Core/Materials/PhysicalMaterial.cs` - Advanced PBR (clearcoat, transmission, sheen)
- `src/BlazorGL.Core/Materials/LambertMaterial.cs` - Matte diffuse

### 3.2 Stylized Materials
**Files to create:**
- `src/BlazorGL.Core/Materials/ToonMaterial.cs` - Cel shading
- `src/BlazorGL.Core/Materials/MatcapMaterial.cs` - Matcap texture

### 3.3 Debug Materials
**Files to create:**
- `src/BlazorGL.Core/Materials/NormalMaterial.cs` - Visualize normals
- `src/BlazorGL.Core/Materials/DepthMaterial.cs` - Depth visualization
- `src/BlazorGL.Core/Materials/DistanceMaterial.cs` - Distance from point

### 3.4 Specialized Materials
**Files to create:**
- `src/BlazorGL.Core/Materials/ShadowMaterial.cs` - Receive shadows only
- `src/BlazorGL.Core/Materials/RawShaderMaterial.cs` - No built-in uniforms

**Already implemented in Phase 1:**
- LineBasicMaterial, LineDashedMaterial, PointsMaterial, SpriteMaterial

### 3.5 Update Shader Library
**Files to modify:**
- `src/BlazorGL.Core/Shaders/ShaderLibrary.cs` - Add all new shader types

---

## Phase 4: Lights (3 Missing Types)

**Files to create:**
- `src/BlazorGL.Core/Lights/HemisphereLight.cs` - Sky/ground lighting
- `src/BlazorGL.Core/Lights/RectAreaLight.cs` - Area light (requires LTC lookups)
- `src/BlazorGL.Core/Lights/LightProbe.cs` - IBL probe

**Files to modify:**
- `src/BlazorGL.Core/Rendering/Renderer.cs` - Add hemisphere light uniforms

---

## Phase 5: Shadow System

**Files to create:**
- `src/BlazorGL.Core/Lights/LightShadow.cs` - Base shadow class
- `src/BlazorGL.Core/Lights/DirectionalLightShadow.cs` - Directional shadows
- `src/BlazorGL.Core/Lights/PointLightShadow.cs` - Point light shadows (cubemap)
- `src/BlazorGL.Core/Lights/SpotLightShadow.cs` - Spotlight shadows
- `src/BlazorGL.Core/Rendering/ShadowMapPass.cs` - Shadow map rendering
- `src/BlazorGL.Core/Shaders/ShadowShaders.cs` - Depth shaders

**Files to modify:**
- `src/BlazorGL.Core/Lights/DirectionalLight.cs` - Add Shadow property
- `src/BlazorGL.Core/Lights/PointLight.cs` - Add Shadow property
- `src/BlazorGL.Core/Lights/SpotLight.cs` - Add Shadow property
- `src/BlazorGL.Core/Core/Mesh.cs` - Already has CastShadow/ReceiveShadow
- `src/BlazorGL.Core/Rendering/Renderer.cs` - Integrate shadow passes

---

## Phase 6: Instancing & Performance

**Files to create:**
- `src/BlazorGL.Core/Core/InstancedMesh.cs` - GPU instancing
- `src/BlazorGL.Core/Rendering/InstancedBuffers.cs` - Instance buffer management

**Implementation details:**
- Use instanced rendering (glDrawElementsInstanced)
- Per-instance matrices
- Per-instance colors
- Instance count management

---

## Phase 7: Skeletal Animation

**Files to create:**
- `src/BlazorGL.Core/Core/Bone.cs` - Bone object
- `src/BlazorGL.Core/Core/Skeleton.cs` - Bone hierarchy
- `src/BlazorGL.Core/Core/SkinnedMesh.cs` - Mesh with skeleton
- `src/BlazorGL.Core/Animation/AnimationUtils.cs` - Bone utilities
- `src/BlazorGL.Core/Shaders/SkinningShaders.cs` - Skinning vertex shaders

**Files to modify:**
- `src/BlazorGL.Loaders/GLTFLoader.cs` - Parse skin data
- `src/BlazorGL.Extensions/Animation/AnimationMixer.cs` - Support bone tracks

---

## Phase 8: Cameras (3 Missing Types)

**Files to create:**
- `src/BlazorGL.Core/Cameras/CubeCamera.cs` - Cube map capture
- `src/BlazorGL.Core/Cameras/StereoCamera.cs` - VR stereoscopic
- `src/BlazorGL.Core/Cameras/ArrayCamera.cs` - Multi-camera rendering

---

## Phase 9: Helpers (12 Missing Types)

**Files to create:**
- `src/BlazorGL.Extensions/Helpers/AxesHelper.cs` - XYZ axes visualization
- `src/BlazorGL.Extensions/Helpers/GridHelper.cs` - Ground grid
- `src/BlazorGL.Extensions/Helpers/PolarGridHelper.cs` - Polar grid
- `src/BlazorGL.Extensions/Helpers/BoxHelper.cs` - Bounding box wireframe
- `src/BlazorGL.Extensions/Helpers/Box3Helper.cs` - AABB helper
- `src/BlazorGL.Extensions/Helpers/ArrowHelper.cs` - Direction arrows
- `src/BlazorGL.Extensions/Helpers/PlaneHelper.cs` - Plane visualization
- `src/BlazorGL.Extensions/Helpers/CameraHelper.cs` - Camera frustum
- `src/BlazorGL.Extensions/Helpers/DirectionalLightHelper.cs` - Light direction
- `src/BlazorGL.Extensions/Helpers/PointLightHelper.cs` - Point light sphere
- `src/BlazorGL.Extensions/Helpers/SpotLightHelper.cs` - Spotlight cone
- `src/BlazorGL.Extensions/Helpers/HemisphereLightHelper.cs` - Hemisphere visual
- `src/BlazorGL.Extensions/Helpers/SkeletonHelper.cs` - Bone visualization

---

## Phase 10: Loaders & Asset Management

**Files to create:**
- `src/BlazorGL.Loaders/LoadingManager.cs` - Progress tracking
- `src/BlazorGL.Loaders/CubeTextureLoader.cs` - Skybox loading
- `src/BlazorGL.Loaders/CompressedTextureLoader.cs` - KTX, DDS, etc.
- `src/BlazorGL.Loaders/DataTextureLoader.cs` - Procedural textures
- `src/BlazorGL.Loaders/FileLoader.cs` - Generic file loading
- `src/BlazorGL.Loaders/MaterialLoader.cs` - JSON materials
- `src/BlazorGL.Loaders/ObjectLoader.cs` - Three.js JSON format
- `src/BlazorGL.Loaders/AnimationLoader.cs` - Separate animation files
- `src/BlazorGL.Loaders/ImageBitmapLoader.cs` - Bitmap loading

**Files to modify:**
- All existing loaders to use LoadingManager

---

## Phase 11: Advanced Features

### 11.1 Morph Targets
**Files to create:**
- `src/BlazorGL.Extensions/Animation/MorphTargets.cs` - Morph animation

**Files to modify:**
- `src/BlazorGL.Core/Core/Mesh.cs` - Add morph target support
- `src/BlazorGL.Loaders/GLTFLoader.cs` - Parse morph targets

### 11.2 Clipping Planes
**Files to create:**
- `src/BlazorGL.Core/Math/Plane.cs` - Plane math
- `src/BlazorGL.Core/Rendering/ClippingPlanes.cs` - Clipping support

### 11.3 Render Layers
**Files to modify:**
- `src/BlazorGL.Core/Core/Object3D.cs` - Add Layers property
- `src/BlazorGL.Core/Cameras/Camera.cs` - Add Layers property
- `src/BlazorGL.Core/Rendering/Renderer.cs` - Filter by layers

### 11.4 Additional Math Utilities
**Files to create:**
- `src/BlazorGL.Core/Math/Euler.cs` - Euler angles
- `src/BlazorGL.Core/Math/Spherical.cs` - Spherical coordinates
- `src/BlazorGL.Core/Math/Cylindrical.cs` - Cylindrical coordinates
- `src/BlazorGL.Core/Math/Frustum.cs` - View frustum
- `src/BlazorGL.Core/Math/Triangle.cs` - Triangle utilities

---

## Implementation Order

### Week 1: Core Rendering (Phase 1)
1. Line rendering (Line, LineSegments, LineLoop)
2. Line materials (LineBasicMaterial, LineDashedMaterial)
3. Points + PointsMaterial
4. Sprite + SpriteMaterial

### Week 2: Geometries (Phase 2)
1. Simple primitives (Cone, Capsule, Circle, Ring)
2. Polyhedrons (Tetrahedron, Octahedron, Dodecahedron, Icosahedron)
3. Complex shapes (TorusKnot, Tube, Lathe)
4. Shape system (ExtrudeGeometry, ShapeGeometry)
5. Utility geometries (Edges, Wireframe)

### Week 3: Materials & Helpers (Phases 3 & 9)
1. Advanced materials (Physical, Lambert, Toon, Matcap)
2. Debug materials (Normal, Depth, Distance)
3. Essential helpers (Axes, Grid, Box, Arrow)
4. Light helpers (Directional, Point, Spot, Hemisphere)
5. Other helpers (Camera, Plane, Skeleton, PolarGrid)

### Week 4: Lights & Shadows (Phases 4 & 5)
1. HemisphereLight
2. RectAreaLight (with LTC)
3. LightProbe
4. Shadow system architecture
5. Shadow map rendering
6. Shadow integration

### Week 5: Advanced Features (Phases 6-8)
1. InstancedMesh + instanced rendering
2. Skeletal animation (Bone, Skeleton, SkinnedMesh)
3. Missing cameras (Cube, Stereo, Array)
4. Morph targets

### Week 6: Loaders & Polish (Phases 10-11)
1. LoadingManager
2. Additional loaders (CubeTexture, CompressedTexture, etc.)
3. Clipping planes
4. Render layers
5. Additional math utilities
6. Testing and validation
7. Documentation updates

---

## Testing Strategy

For each feature:
1. Create unit tests in `tests/BlazorGL.Tests/`
2. Create example in `examples/BlazorGL.Examples/`
3. Verify against Three.js behavior
4. Performance testing for critical features (instancing, shadows)

---

## Success Criteria

- ✅ All 21 geometry types implemented
- ✅ All 17 material types implemented
- ✅ All 7 light types implemented
- ✅ All 5 camera types implemented
- ✅ All 13 helper types implemented
- ✅ Core objects: Line, Points, Sprite, InstancedMesh, SkinnedMesh
- ✅ Shadow system fully functional
- ✅ Skeletal animation working
- ✅ All major loaders implemented
- ✅ Feature parity with Three.js core API
- ✅ Comprehensive examples and documentation
- ✅ All tests passing

**Target:** 100% Three.js core API coverage
