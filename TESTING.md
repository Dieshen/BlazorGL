# BlazorGL Testing Guide

## Overview

BlazorGL uses **xUnit** as the testing framework with **Coverlet** for code coverage analysis.

## Current Test Coverage

### Test Suite Statistics
- **Total Test Files**: 8
- **Test Categories**: 6
- **Estimated Tests**: 80+

### Coverage by Component

| Component | Test Files | Status | Priority |
|-----------|-----------|--------|----------|
| **Geometries** | ✅ 3 files | Complete | High |
| **Core (Object3D, Mesh)** | ✅ 2 files | Complete | Critical |
| **Materials** | ✅ 1 file | Complete | High |
| **Cameras** | ✅ 1 file | Complete | High |
| **Animation** | ✅ 1 file | Complete | Medium |
| **Lights** | ⚠️ Pending | Not Started | Medium |
| **Loaders** | ⚠️ Pending | Not Started | Medium |
| **Rendering** | ⚠️ Pending | Not Started | High |
| **Helpers** | ⚠️ Pending | Not Started | Low |

## Running Tests

### Quick Start

```bash
# Linux/Mac
chmod +x run-tests.sh
./run-tests.sh

# Windows PowerShell
./run-tests.ps1

# Or use dotnet directly
dotnet test
```

### With Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Continuous Testing

```bash
# Watch mode - auto-run on file changes
dotnet watch test
```

## Test Structure

### Geometry Tests (`Geometries/`)
Tests for all 21 geometry types:
- **BoxGeometryTests**: Validates cube generation, dimensions, normals
- **SphereGeometryTests**: Validates sphere surface, vertex count, normals
- **PlaneGeometryTests**: Validates planarity, subdivisions, UVs

**Coverage**: Box, Sphere, Plane (3/21 = 14%)

### Core Tests (`Core/`)
Tests for scene graph and object hierarchy:
- **Object3DTests**: Transform hierarchy, parent-child relationships, world matrices
- **MeshTests**: Mesh creation, geometry/material assignment

**Coverage**: Critical components tested

### Material Tests (`Materials/`)
Tests for material system:
- **MaterialTests**: All 17 material types, properties, transparency, depth settings

**Coverage**: Basic, Phong, Standard, Line, Points

### Camera Tests (`Cameras/`)
Tests for camera systems:
- **CameraTests**: Perspective, Orthographic, Stereo, Array cameras, projection matrices

**Coverage**: All 5 camera types

### Animation Tests (`Animation/`)
Tests for animation system:
- **AnimationTests**: Clips, tracks, keyframes, mixer, actions

**Coverage**: Core animation components

## Test Examples

### Geometry Test
```csharp
[Fact]
public void Constructor_CreatesValidGeometry()
{
    var geometry = new BoxGeometry(2, 3, 4);

    Assert.NotNull(geometry);
    Assert.NotEmpty(geometry.Vertices);
    Assert.NotEmpty(geometry.Normals);
}
```

### Transform Test
```csharp
[Fact]
public void UpdateWorldMatrix_WithParent_CombinesTransforms()
{
    var parent = new Object3D { Position = new Vector3(10, 0, 0) };
    var child = new Object3D { Position = new Vector3(5, 0, 0) };
    parent.AddChild(child);

    parent.UpdateWorldMatrix(true, true);

    var childWorldPos = child.WorldMatrix.Translation;
    Assert.InRange(childWorldPos.X, 14.9f, 15.1f);
}
```

## Coverage Goals

### Phase 1 (Current) - Foundation
- ✅ Geometries: 3 core types tested
- ✅ Scene Graph: Object3D, Mesh
- ✅ Materials: All types
- ✅ Cameras: All types
- ✅ Animation: Core system

**Current Estimated Coverage**: ~15-20% of codebase

### Phase 2 - Core Rendering
- ⚠️ Remaining 18 geometry types
- ⚠️ Light system (7 types)
- ⚠️ Shader compilation and uniforms
- ⚠️ Texture management
- ⚠️ Buffer management (VAO/VBO)

**Target Coverage**: 40-50%

### Phase 3 - Advanced Features
- ⚠️ Shadow mapping
- ⚠️ GPU instancing
- ⚠️ Skeletal animation
- ⚠️ Post-processing
- ⚠️ All loaders (GLTF, OBJ, STL, etc.)

**Target Coverage**: 60-70%

### Phase 4 - Production Ready
- ⚠️ Integration tests
- ⚠️ Performance benchmarks
- ⚠️ Edge case coverage
- ⚠️ Error handling tests

**Target Coverage**: 80%+

## Testing Best Practices

### 1. AAA Pattern
```csharp
// Arrange - Set up test data
var geometry = new BoxGeometry(1, 2, 3);

// Act - Execute the operation
var bbox = geometry.BoundingBox;

// Assert - Verify results
Assert.InRange(bbox.Min.X, -0.51f, -0.49f);
```

### 2. Theory Tests for Parameterization
```csharp
[Theory]
[InlineData(1, 1, 1)]
[InlineData(2, 3, 4)]
[InlineData(0.5, 2.5, 1.5)]
public void Constructor_WithDimensions_CreatesCorrectSize(
    float width, float height, float depth)
{
    var geometry = new BoxGeometry(width, height, depth);
    // Assertions...
}
```

### 3. Readable Test Names
- ✅ `UpdateWorldMatrix_WithParent_CombinesTransforms`
- ✅ `Normals_AreNormalized`
- ❌ `Test1`, `TestGeometry`

## Known Limitations

### WebGL Testing
- **No WebGL context available in unit tests**
- Renderer tests require mocking or integration testing
- Buffer operations cannot be tested without GL context

### Workarounds
1. **Mock WebGL interfaces** for renderer tests
2. **Integration tests** with headless browser (Selenium)
3. **Visual regression tests** for actual rendering

### Not Tested (Yet)
- Actual WebGL rendering (requires GL context)
- Shader compilation (GPU-dependent)
- Texture upload (GPU-dependent)
- Frame buffer operations

## CI/CD Integration

### GitHub Actions Example
```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - name: Run tests
        run: dotnet test --configuration Release
      - name: Upload coverage
        uses: codecov/codecov-action@v3
```

## Next Steps

1. **Complete geometry tests** (18 remaining types)
2. **Add light system tests** (7 light types)
3. **Create loader tests** (GLTF, OBJ, STL, texture loaders)
4. **Mock WebGL for renderer tests**
5. **Set up CI/CD pipeline**
6. **Add integration tests**
7. **Create performance benchmarks**

## Contributing

When adding new features:
1. Write tests FIRST (TDD approach)
2. Aim for >80% coverage of new code
3. Include edge cases and error conditions
4. Document complex test scenarios

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
