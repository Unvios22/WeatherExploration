using Godot;
using WeatherExploration.Source.Input;

namespace WeatherExploration.Source.Controllers;

public partial class CameraController : Node {
    [Export] private Camera3D _camera;
    [Export] private Node3D _orbitingTarget;
    
    [Export] private float _initialOrbitDistance;
    [Export] private float _maxOrbitDistance;
    [Export] private float _minOrbitDistance;

    [Export] private float _orbitDistanceChangeSpeed;

    [Export] private Vector2 _initialOrbitRotation;
    [Export] private float _orbitRotationChangeSpeed;
    [Export] private float _orbitXRotationAngleLimit;
    
    private float _currentOrbitDistance;
    
    private Vector2 _currentOrbitRotation;
    private Vector2 _currentOrbitRotationChangeSpeed;
    
    private float _inputOrbitingDistanceChange;
    private Vector2 _inputOrbitRotationChange;
    private bool _isDraggingInProgress;
    
    private Vector2 _mousePosThisFrame;
    private Vector2 _mousePosLastFrame;
    private Vector2 _mouseDeltaThisFrame;
    
    public override void _Ready() {
        InitializeValues();
        ApplyCameraOrientation();
    }

    private void InitializeValues() {
        _currentOrbitDistance = _initialOrbitDistance;
        _currentOrbitRotation = _initialOrbitRotation;
    }

    public override void _Process(double delta) {
        if (_isDraggingInProgress) {
            CameraOrbitTraverse();
        }
        
        CalculateOrbitRotation();
        CalculateOrbitDistance();
        ClearInputCache();
        ApplyCameraOrientation();
    }

    private void CameraOrbitTraverse() {
        //screen mouse x is left-right; y is up-down
        //orbit rotation x is considered pitch (perspective up-down), y is considered yaw (orbiting left-right)
        _inputOrbitRotationChange = new Vector2(_mouseDeltaThisFrame.Y, _mouseDeltaThisFrame.X);
    }
    
    private void CalculateOrbitRotation() {
        _currentOrbitRotation += _inputOrbitRotationChange * _orbitRotationChangeSpeed;

        var desiredCameraXRot = _currentOrbitRotation.X;
        var clampedCameraXRot = Mathf.Clamp(desiredCameraXRot, -_orbitXRotationAngleLimit, _orbitXRotationAngleLimit);
        _currentOrbitRotation.X = clampedCameraXRot;
        
        //clearing input value since it's been applied; the camera would keep rotating otherwise
        _inputOrbitRotationChange = Vector2.Zero;
    }

    private void CalculateOrbitDistance() {
        _currentOrbitDistance += _inputOrbitingDistanceChange * _orbitDistanceChangeSpeed;
        _currentOrbitDistance = Mathf.Clamp(_currentOrbitDistance, _minOrbitDistance, _maxOrbitDistance);
    }

    private void ClearInputCache() {
        //clearing input values since it's been applied; otherwise the camera would keep moving when the user stops updating input
        _inputOrbitRotationChange = Vector2.Zero;
        _inputOrbitingDistanceChange = 0f;
    }

    private void ApplyCameraOrientation() {
        var targetPosition = _orbitingTarget.Position;

        //unity's default 0,0,0 world rotation is oriented directly towards world z+, so I also treat object's z+ as default (0,0) rotation around the object
        //todo: upper comment is redundant from before porting. Currently changed the z+ to z- since they differ in Godot. Refactor the comment and logic
        //todo: make sure Node.Transform.Basis.Z is actaully equivalent of Unity Transform.Forward; add some wrapper
        var targetToZeroPositionVector = targetPosition + _orbitingTarget.Transform.Basis.Z * (1 * _currentOrbitDistance);

        _camera.Position = targetPosition + targetToZeroPositionVector;
        _camera.Quaternion = Quaternion.Identity;

        var currentOrbitRotRad = new Vector2(Mathf.DegToRad(_currentOrbitRotation.X),Mathf.DegToRad(_currentOrbitRotation.Y));
        
        //rotation around target is considered as a position on a unit sphere; x is pitch, y is yaw
        //todo: refactor
        var cameraTransform = _camera.GlobalTransform;
        var rotatedCamBasis = cameraTransform.Basis.Rotated(Vector3.Right, currentOrbitRotRad.X).Rotated(Vector3.Up, currentOrbitRotRad.Y);
        var rotatedCamOrigin = targetPosition + (cameraTransform.Origin - targetPosition).Rotated(Vector3.Right, currentOrbitRotRad.X).Rotated(Vector3.Up, currentOrbitRotRad.Y);
        _camera.GlobalTransform = new Transform3D(rotatedCamBasis, rotatedCamOrigin);
    }

    public override void _Input(InputEvent @event) {
        //todo: refactor into some dedicated InputManager with DI and event callbacks
        
        if (@event is InputEventMouseMotion eventMouseMotion) {
            _mousePosLastFrame = _mousePosThisFrame;
            _mousePosThisFrame = eventMouseMotion.Position;
            _mouseDeltaThisFrame = _mousePosThisFrame - _mousePosLastFrame;
        }
        
        if (@event.IsAction(InputActions.CAMERA_DRAG)) {
            //todo: can be reworked into isActionPressed and isActionReleased
            _isDraggingInProgress = !_isDraggingInProgress;
        }

        if (@event.IsAction(InputActions.CAMERA_ZOOM_IN)) {
            _inputOrbitingDistanceChange += _orbitDistanceChangeSpeed;
        }

        if (@event.IsAction(InputActions.CAMERA_ZOOM_OUT)) {
            _inputOrbitingDistanceChange -= _orbitDistanceChangeSpeed;
        }
    }
}