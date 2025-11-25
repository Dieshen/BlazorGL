# BlazorGL Documentation & Examples Site Architecture

## Site Structure

```
/                           - Homepage with feature overview and quick links
/docs                       - Documentation hub
/docs/getting-started       - Installation & basic setup
/docs/architecture          - Library architecture overview
/docs/api                   - API reference (organized by namespace)

/examples                   - Examples hub/gallery
/examples/basics            - Basic concepts
  /examples/basics/hello-world      - Minimal scene setup
  /examples/basics/scene-graph      - Scene hierarchy & transforms
  /examples/basics/animation-loop   - Render loop patterns

/examples/geometry          - Geometry examples
  /examples/geometry/gallery        - Interactive gallery of all 21 geometries
  /examples/geometry/buffer         - Custom BufferGeometry
  /examples/geometry/wireframe      - Wireframe & edges

/examples/materials         - Material examples
  /examples/materials/showcase      - All materials side-by-side
  /examples/materials/standard      - PBR StandardMaterial deep dive
  /examples/materials/physical      - PhysicalMaterial (clearcoat, transmission)
  /examples/materials/toon          - ToonMaterial stylized rendering
  /examples/materials/custom        - ShaderMaterial & RawShaderMaterial

/examples/lights            - Lighting examples
  /examples/lights/types            - All light types demo
  /examples/lights/shadows          - Shadow mapping basics
  /examples/lights/shadows/comparison - (existing) Advanced shadow comparison

/examples/cameras           - Camera examples
  /examples/cameras/perspective     - PerspectiveCamera
  /examples/cameras/orthographic    - OrthographicCamera
  /examples/cameras/array           - ArrayCamera (split screen)

/examples/controls          - Control examples
  /examples/controls/orbit          - OrbitControls
  /examples/controls/trackball      - TrackballControls
  /examples/controls/drag           - DragControls
  /examples/controls/transform      - TransformControls

/examples/loaders           - Loader examples
  /examples/loaders/gltf            - GLTF/GLB model loading
  /examples/loaders/obj             - OBJ model loading
  /examples/loaders/textures        - Texture loading (PNG, JPG, DDS, KTX2, RGBE)

/examples/animation         - Animation examples
  /examples/animation/keyframe      - Keyframe animation
  /examples/animation/skeletal      - Skeletal animation (SkinnedMesh)
  /examples/animation/blending      - Animation mixing & transitions

/examples/effects           - Post-processing & effects
  /examples/effects/postprocessing  - (existing) SSAO, FXAA, Color Correction
  /examples/effects/bloom           - Bloom/glow effect
  /examples/effects/dof             - Depth of field (Bokeh)
  /examples/effects/outline         - Object outline

/examples/advanced          - Advanced topics
  /examples/advanced/instancing     - InstancedMesh for performance
  /examples/advanced/lod            - Level of Detail
  /examples/advanced/raycasting     - Picking & intersection
  /examples/advanced/particles      - Particle systems

/examples/helpers           - Debug & visualization helpers
  /examples/helpers/gallery         - All 13 helpers demonstrated
```

## Page Components

### Shared Layout
- **MainLayout.razor** - Master layout with sidebar navigation
- **NavMenu.razor** - Collapsible navigation sidebar
- **ExampleLayout.razor** - Standard example page layout

### Reusable Components
- **ParameterPanel.razor** - Adjustable parameter controls (sliders, checkboxes, dropdowns)
- **StatsOverlay.razor** - FPS, draw calls, triangles, memory
- **CodeSnippet.razor** - Syntax-highlighted code display with copy button
- **ExampleCanvas.razor** - Standard canvas wrapper with resize handling
- **CategoryCard.razor** - Card for example gallery listings

## Navigation Structure

### Sidebar Categories
1. **Getting Started** (docs)
2. **Basics** (examples)
3. **Geometry** (examples)
4. **Materials** (examples)
5. **Lights & Shadows** (examples)
6. **Cameras** (examples)
7. **Controls** (examples)
8. **Loaders** (examples)
9. **Animation** (examples)
10. **Effects** (examples)
11. **Advanced** (examples)
12. **Helpers** (examples)
13. **API Reference** (docs)

## Implementation Phases

### Phase 1: Foundation
- [x] Site architecture plan
- [ ] Shared layout with navigation
- [ ] Reusable components (ParameterPanel, StatsOverlay, CodeSnippet)
- [ ] Basic styling/CSS

### Phase 2: Documentation
- [ ] Getting Started page
- [ ] Architecture overview
- [ ] API reference structure

### Phase 3: Basic Examples
- [ ] Hello World
- [ ] Scene Graph & Transforms
- [ ] Animation Loop

### Phase 4: Core Examples
- [ ] Geometry Gallery
- [ ] Materials Showcase
- [ ] Lighting Types
- [ ] Camera Types
- [ ] Controls (Orbit, Trackball, Drag, Transform)

### Phase 5: Content Pipeline
- [ ] GLTF Loader
- [ ] OBJ Loader
- [ ] Texture Loaders

### Phase 6: Advanced
- [ ] Animation System
- [ ] Raycasting/Picking
- [ ] Particles
- [ ] Instancing
- [ ] LOD

### Phase 7: Polish
- [ ] Additional post-processing examples
- [ ] Helpers gallery
- [ ] Code snippets for all examples
- [ ] Mobile responsiveness
