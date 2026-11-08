using System.Numerics;
using SampSharp.ColAndreas.Entities.Interop;

namespace SampSharp.ColAndreas.Entities.Services;

/// <summary>
/// Default <see cref="IColAndreasService"/> implementation — thin layer over
/// the <c>CASharp_*</c> exports of <c>colandreas.dll</c>.
/// </summary>
internal sealed class ColAndreasService : IColAndreasService
{
    /// <summary>
    /// Default ray for FindZ_For2DCoord: from <c>+700</c> straight down to
    /// <c>-1000</c>. Mirrors the legacy SampSharp.ColAndreas defaults; high
    /// enough to clear LV/SF airports, low enough to catch underwater ground.
    /// </summary>
    private const float SkyTopZ = 700f;
    private const float WorldFloorZ = -1000f;

    public bool IsDataLoaded => ColAndreasInterop.IsDataLoaded() != 0;
    public bool IsInitialized => ColAndreasInterop.IsInitialized() != 0;

    public void Init() => ColAndreasInterop.Init();

    // ---- ray casts ------------------------------------------------------

    public bool FindZ_For2DCoord(Vector2 position, out float z)
    {
        var start = new Vector3(position.X, position.Y, SkyTopZ);
        var end = new Vector3(position.X, position.Y, WorldFloorZ);
        if (RayCastLine(start, end, out var hit))
        {
            z = hit.Z;
            return true;
        }
        z = 0f;
        return false;
    }

    public bool FindZ_For3DCoord(Vector3 position, out float z)
    {
        var end = new Vector3(position.X, position.Y, WorldFloorZ);
        if (RayCastLine(position, end, out var hit))
        {
            z = hit.Z;
            return true;
        }
        z = 0f;
        return false;
    }

    public unsafe bool RayCastLine(Vector3 start, Vector3 end, out Vector3 hit)
    {
        float hx, hy, hz;
        var model = ColAndreasInterop.RayCastLine(
            start.X, start.Y, start.Z, end.X, end.Y, end.Z, &hx, &hy, &hz);
        if (model == 0) { hit = default; return false; }
        hit = new Vector3(hx, hy, hz);
        return true;
    }

    public unsafe int RayCastLineModel(Vector3 start, Vector3 end, out Vector3 hit)
    {
        float hx, hy, hz;
        var model = ColAndreasInterop.RayCastLine(
            start.X, start.Y, start.Z, end.X, end.Y, end.Z, &hx, &hy, &hz);
        hit = model == 0 ? default : new Vector3(hx, hy, hz);
        return model;
    }

    public unsafe bool RayCastLineId(Vector3 start, Vector3 end, out Vector3 hit, out int colId)
    {
        float hx, hy, hz;
        var id = ColAndreasInterop.RayCastLineID(
            start.X, start.Y, start.Z, end.X, end.Y, end.Z, &hx, &hy, &hz);
        if (id == 0) { hit = default; colId = 0; return false; }
        hit = new Vector3(hx, hy, hz);
        colId = id;
        return true;
    }

    public unsafe bool RayCastLineEx(Vector3 start, Vector3 end,
        out Vector3 hit, out Quaternion rotation, out Vector3 position)
    {
        float hx, hy, hz, qx, qy, qz, qw, px, py, pz;
        var model = ColAndreasInterop.RayCastLineEx(
            start.X, start.Y, start.Z, end.X, end.Y, end.Z,
            &hx, &hy, &hz, &qx, &qy, &qz, &qw, &px, &py, &pz);
        if (model == 0)
        {
            hit = default;
            rotation = default;
            position = default;
            return false;
        }
        hit = new Vector3(hx, hy, hz);
        rotation = new Quaternion(qx, qy, qz, qw);
        position = new Vector3(px, py, pz);
        return true;
    }

    public unsafe bool RayCastLineAngle(Vector3 start, Vector3 end, out Vector3 hit, out Vector3 rotation)
    {
        float hx, hy, hz, rx, ry, rz;
        var model = ColAndreasInterop.RayCastLineAngle(
            start.X, start.Y, start.Z, end.X, end.Y, end.Z,
            &hx, &hy, &hz, &rx, &ry, &rz);
        if (model == 0) { hit = default; rotation = default; return false; }
        hit = new Vector3(hx, hy, hz);
        rotation = new Vector3(rx, ry, rz);
        return true;
    }

    public unsafe bool RayCastReflectionVector(Vector3 start, Vector3 end, out Vector3 hit, out Vector3 reflection)
    {
        float hx, hy, hz, rfx, rfy, rfz;
        var model = ColAndreasInterop.RayCastReflectionVector(
            start.X, start.Y, start.Z, end.X, end.Y, end.Z,
            &hx, &hy, &hz, &rfx, &rfy, &rfz);
        if (model == 0) { hit = default; reflection = default; return false; }
        hit = new Vector3(hx, hy, hz);
        reflection = new Vector3(rfx, rfy, rfz);
        return true;
    }

    public unsafe bool RayCastNormal(Vector3 start, Vector3 end, out Vector3 hit, out Vector3 normal)
    {
        float hx, hy, hz, nx, ny, nz;
        var model = ColAndreasInterop.RayCastLineNormal(
            start.X, start.Y, start.Z, end.X, end.Y, end.Z,
            &hx, &hy, &hz, &nx, &ny, &nz);
        if (model == 0) { hit = default; normal = default; return false; }
        hit = new Vector3(hx, hy, hz);
        normal = new Vector3(nx, ny, nz);
        return true;
    }

    // ---- objects --------------------------------------------------------

    public int CreateObject(int modelId, Vector3 position, Vector3 rotation, bool addToManager = true) =>
        ColAndreasInterop.CreateObject(
            modelId, position.X, position.Y, position.Z,
            rotation.X, rotation.Y, rotation.Z,
            addToManager ? 1 : 0);

    public bool DestroyObject(int colIndex) => ColAndreasInterop.DestroyObject(colIndex) > 0;
    public bool IsValidObject(int colIndex) => ColAndreasInterop.IsValidObject(colIndex) > 0;

    public bool SetObjectPos(int colIndex, Vector3 position) =>
        ColAndreasInterop.SetObjectPos(colIndex, position.X, position.Y, position.Z) > 0;

    public bool SetObjectRot(int colIndex, Vector3 rotation) =>
        ColAndreasInterop.SetObjectRot(colIndex, rotation.X, rotation.Y, rotation.Z) > 0;

    public bool SetObjectExtraId(int colIndex, int type, int data) =>
        ColAndreasInterop.SetObjectExtraID(colIndex, type, data) > 0;

    public int GetObjectExtraId(int colIndex, int type) =>
        ColAndreasInterop.GetObjectExtraID(colIndex, type);

    // ---- buildings ------------------------------------------------------

    public bool RemoveBuilding(int model, Vector3 position, float radius) =>
        ColAndreasInterop.RemoveBuilding(model, position.X, position.Y, position.Z, radius) > 0;

    public bool RestoreBuilding(int model, Vector3 position, float radius) =>
        ColAndreasInterop.RestoreBuilding(model, position.X, position.Y, position.Z, radius) > 0;

    // ---- queries --------------------------------------------------------

    public bool ContactTest(int modelId, Vector3 position, Vector3 rotation) =>
        ColAndreasInterop.ContactTest(modelId,
            position.X, position.Y, position.Z,
            rotation.X, rotation.Y, rotation.Z) > 0;

    public unsafe bool GetModelBoundingSphere(int modelId, out Vector3 center, out float radius)
    {
        float cx, cy, cz, r;
        if (ColAndreasInterop.GetModelBoundingSphere(modelId, &cx, &cy, &cz, &r) == 0)
        {
            center = default; radius = 0f;
            return false;
        }
        center = new Vector3(cx, cy, cz); radius = r;
        return true;
    }

    public unsafe bool GetModelBoundingBox(int modelId, out Vector3 min, out Vector3 max)
    {
        float mnx, mny, mnz, mxx, mxy, mxz;
        if (ColAndreasInterop.GetModelBoundingBox(modelId, &mnx, &mny, &mnz, &mxx, &mxy, &mxz) == 0)
        {
            min = default; max = default;
            return false;
        }
        min = new Vector3(mnx, mny, mnz);
        max = new Vector3(mxx, mxy, mxz);
        return true;
    }

    // ---- math helpers ---------------------------------------------------

    public unsafe Quaternion EulerToQuat(Vector3 rotation)
    {
        float qx, qy, qz, qw;
        ColAndreasInterop.EulerToQuat(rotation.X, rotation.Y, rotation.Z, &qx, &qy, &qz, &qw);
        return new Quaternion(qx, qy, qz, qw);
    }

    public unsafe Vector3 QuatToEuler(Quaternion rotation)
    {
        float rx, ry, rz;
        ColAndreasInterop.QuatToEuler(rotation.X, rotation.Y, rotation.Z, rotation.W, &rx, &ry, &rz);
        return new Vector3(rx, ry, rz);
    }
}
