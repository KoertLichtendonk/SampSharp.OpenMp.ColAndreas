using System.Numerics;

namespace SampSharp.ColAndreas.Entities.Services;

/// <summary>
/// Managed front for the ColAndreas Bullet-physics plugin. Surfaces ray casts
/// against the static GTA: SA collision world plus user-managed col-objects /
/// building removals.
///
/// Method names follow the legacy x86 SampSharp.ColAndreas binding so
/// gamemode code can move over without a rename pass.
/// </summary>
public interface IColAndreasService
{
    // ---- diagnostics ----------------------------------------------------

    /// <summary>True iff the .cadb collision database loaded successfully.</summary>
    bool IsDataLoaded { get; }

    /// <summary>True after <see cref="Init"/> has built the dynamics world.</summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Forces world initialization. The plugin auto-builds the world on first
    /// access these days, so calling this is optional — kept for source
    /// compatibility with the legacy x86 binding.
    /// </summary>
    void Init();

    // ---- ray casts ------------------------------------------------------

    /// <summary>Casts a ray and reports the surface Z at <paramref name="position"/> (700 .. -1000).</summary>
    bool FindZ_For2DCoord(Vector2 position, out float z);

    /// <summary>Casts a ray straight down from <paramref name="position"/>'s Z and reports the first hit's Z.</summary>
    bool FindZ_For3DCoord(Vector3 position, out float z);

    /// <summary>Single-hit ray cast. Returns false on miss.</summary>
    bool RayCastLine(Vector3 start, Vector3 end, out Vector3 hit);

    /// <summary>Single-hit ray cast that returns the hit model id (0 on miss), like the legacy CA_RayCastLine. Water is model 20000.</summary>
    int RayCastLineModel(Vector3 start, Vector3 end, out Vector3 hit);

    /// <summary>Single-hit ray cast that also returns the hit col-object index (col-id).</summary>
    bool RayCastLineId(Vector3 start, Vector3 end, out Vector3 hit, out int colId);

    /// <summary>Single-hit ray cast that also returns the hit object's full transform (rotation quat + position).</summary>
    bool RayCastLineEx(Vector3 start, Vector3 end, out Vector3 hit, out Quaternion rotation, out Vector3 position);

    /// <summary>Single-hit ray cast that also returns the hit object's Euler rotation.</summary>
    bool RayCastLineAngle(Vector3 start, Vector3 end, out Vector3 hit, out Vector3 rotation);

    /// <summary>Single-hit ray cast plus reflection vector (incident reflected about hit normal).</summary>
    bool RayCastReflectionVector(Vector3 start, Vector3 end, out Vector3 hit, out Vector3 reflection);

    /// <summary>Single-hit ray cast plus surface normal at hit.</summary>
    bool RayCastNormal(Vector3 start, Vector3 end, out Vector3 hit, out Vector3 normal);

    // ---- objects --------------------------------------------------------

    /// <summary>Creates a managed col-object. Returns the col-index, or -1 on failure (data not loaded / model has no collision).</summary>
    int CreateObject(int modelId, Vector3 position, Vector3 rotation, bool addToManager = true);

    /// <summary>Destroys a col-object created via <see cref="CreateObject"/>. Returns true on success.</summary>
    bool DestroyObject(int colIndex);

    /// <summary>True if the col-object slot is currently in use.</summary>
    bool IsValidObject(int colIndex);

    /// <summary>Repositions a col-object.</summary>
    bool SetObjectPos(int colIndex, Vector3 position);

    /// <summary>Re-rotates a col-object (Euler).</summary>
    bool SetObjectRot(int colIndex, Vector3 rotation);

    /// <summary>Stamps an extra integer onto a col-object slot under the given <paramref name="type"/> bucket.</summary>
    bool SetObjectExtraId(int colIndex, int type, int data);

    /// <summary>Reads back a previously stamped extra integer.</summary>
    int GetObjectExtraId(int colIndex, int type);

    // ---- buildings (must be called BEFORE Init) -------------------------

    /// <summary>
    /// Removes a default-map building from the collision world. Must be
    /// called before <see cref="Init"/> — once the world is built, removals
    /// are no-ops.
    /// </summary>
    bool RemoveBuilding(int model, Vector3 position, float radius);

    /// <summary>
    /// Restores a previously-removed default-map building. Requires the
    /// world to already be initialized (mirror of <see cref="RemoveBuilding"/>).
    /// </summary>
    bool RestoreBuilding(int model, Vector3 position, float radius);

    // ---- queries --------------------------------------------------------

    /// <summary>True iff a hypothetical instance of <paramref name="modelId"/> at the given pose would collide with anything in the world.</summary>
    bool ContactTest(int modelId, Vector3 position, Vector3 rotation);

    /// <summary>Gets the bounding sphere of a model's collision mesh.</summary>
    bool GetModelBoundingSphere(int modelId, out Vector3 center, out float radius);

    /// <summary>Gets the AABB of a model's collision mesh.</summary>
    bool GetModelBoundingBox(int modelId, out Vector3 min, out Vector3 max);

    // ---- math helpers ---------------------------------------------------

    /// <summary>Converts GTA Euler rotation to a quaternion using ColAndreas's exact same logic.</summary>
    Quaternion EulerToQuat(Vector3 rotation);

    /// <summary>Converts a quaternion to GTA Euler rotation using ColAndreas's exact same logic.</summary>
    Vector3 QuatToEuler(Quaternion rotation);
}
