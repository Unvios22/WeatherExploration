using Godot;

namespace WeatherExploration.Source.Autoload;

public partial class RayCasterNode3D : Node3D {
    //helper class to get custom raycasts from code
    //the API exposes direct calls, but these are wholly inefficient, see
    //https://sampruden.github.io/posts/godot-is-not-the-new-unity/#godot
    //as of 05.09.24 the API hasn't been changed still: (4.2 docs)
    //https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html
    
    //the class instantiates an autoload RaycastNode3D and then dynamically changes its world pos to query the raycasts

    private RayCast3D _rayCast3DNode;

    public bool RayCastInDirection(Vector3 origin, Vector3 direction, out HitInfo hitInfo) {
        var target = origin + direction;
        return RayCastInternal(origin, target, out hitInfo);
    }

    public bool RayCastToTarget(Vector3 origin, Vector3 target, out HitInfo hitInfo) {
        return RayCastInternal(origin, target, out hitInfo);
    }
    
    private bool RayCastInternal(Vector3 origin, Vector3 target, out HitInfo hitInfo) {
        _rayCast3DNode.Position = origin;
        _rayCast3DNode.TargetPosition = target;
        _rayCast3DNode.ForceRaycastUpdate();

        var isCollisionFound = _rayCast3DNode.IsColliding();

        if (isCollisionFound) {
            var collisionPoint = _rayCast3DNode.GetCollisionPoint();
            var collisionDistance = (collisionPoint - origin).Length();
        
            hitInfo = new HitInfo() {
                CollisionPoint = collisionPoint,
                CollisionDistance = collisionDistance,
                CollisionNormal = _rayCast3DNode.GetCollisionNormal(),
                CollisionObject = _rayCast3DNode.GetCollider()
            };
        }
        else {
            hitInfo = new HitInfo();
        }
        
        return isCollisionFound;
    }
    
    public override void _EnterTree() {
        InstantiateRayCastNodeAsChild();
    }

    private void InstantiateRayCastNodeAsChild() {
        _rayCast3DNode = new RayCast3D();
        _rayCast3DNode.Name = "RayCast3D";
        AddChild(_rayCast3DNode);
    }
}

public record struct HitInfo {
    public Vector3 CollisionPoint;
    public float CollisionDistance;
    public Vector3 CollisionNormal;
    public GodotObject CollisionObject;
}