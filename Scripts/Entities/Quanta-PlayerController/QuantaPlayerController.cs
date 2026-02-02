using System;
using Godot;
using Godot.Collections;
using Netfox_Test.Scripts.Extensions;
using Netfox;
using Netfox.Logging;
using QuantaDM.quanta_game.Scripts.Entities.Player_Controller;
using QuantaDM.quanta_game.Scripts.Extensions;

namespace QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

[GlobalClass]
public partial class QuantaPlayerController : CharacterBody3D
{
    [ExportGroup("Player View")] public float offset = 0.711f; // Current offset from player's origin.

    [ExportGroup("Collision")] [Export] public CollisionShape3D
        collisionHull; //# Player collision shape/hull, make sure it's a box unless you edit the script to use otherwise!

    [Export] public float
        minTraceMarginAmount = -0.065f; //# How much the step detection shape should shrink on the X and Z axes.

    [Export]
    public float maxTraceMarginAmount = 0.1f; //# How much the step detection shape should shrink on the X and Z axes.

    [Export] public CameraController localController;

    [Export] public Timer
        duckTimer; //# Timer used for ducking animation and collision hull swapping. Time is set in [method _duck] to 1 second.

    [Export(PropertyHint.Range, "-89,89")] public float
        StartViewPitch = 0; //# How the vertical view of the pawn should be rotated on ready. The default value is 0.

    [Export] public float
        StartViewYaw = 0; //# How the horizontal view of the pawn should be rotated on ready. The default values is 0.


    [Export] private QuantaInputController InputController;
    [Export] private PlayerParams PlayerParams;

    private float traceMargin = 0.01f;
    private float traceDirAdd = 1;
    private BoxShape3D bboxStanding = new BoxShape3D(); // Cached hull for standing.
    private BoxShape3D bboxDucking = new BoxShape3D(); // Cached hull for ducking.
    private BoxShape3D bboxStep = new BoxShape3D(); // Cached hull for step detection.
    private BoxShape3D bboxNearWall = new BoxShape3D(); // Cached hull for nearby wall detection.
    private BoxShape3D bboxIntoWall = new BoxShape3D(); // Cached hull for wall collision.

    private NetfoxLogger logger = NetfoxLogger.ForNetfox("NetworkRollback");

    public bool loggerFlag;
    public long logTickStart;
    private int logTicks = 10;

    [Export]
    public Vector3 ProxyPosition
    {
        get
        {
            if (loggerFlag)
                logger.LogWarning("get: " + Position.ToString());
            return Position;
        }

        set
        {
            if (loggerFlag)
                logger.LogWarning("set: " + value.ToString());
            Position = value;
        }
    }

    private enum WallCollision
    {
        NONE,
        NEAR,
        ON
    }

    private Vector3 wallNormal = Vector3.Zero;
    [Export] public bool ducked; // True if you are fully ducked.
    [Export] private bool ducking;

    private QuantaMovementController movementController;
    private QuantaViewController viewController;

    public override void _Ready()
    {
        localController.horizontalView.SetTransformY(offset);

        Input.MouseMode = Input.MouseModeEnum.Captured;
        OverrideViewRotation(new Vector2(Mathf.DegToRad(StartViewYaw), Mathf.DegToRad(StartViewPitch)));
        movementController = new QuantaMovementController(this, PlayerParams);
        viewController = new QuantaViewController(this, PlayerParams, localController);
        SetShapeBounds(bboxStanding, PlayerParams.HULL_STANDING_BOUNDS);
        SetShapeBounds(bboxDucking, PlayerParams.HULL_DUCKING_BOUNDS);
        collisionHull.Shape = bboxStanding;
        localController.horizontalView.SetTransformY(offset);
    }

    private void OverrideViewRotation(Vector2 rotation)
    {
        localController.Rotate(rotation);

        // Camera.GlobalRotation = View.CameraMount.GlobalRotation;
        // Camera.Orthonormalize();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!InputController.IsMultiplayerAuthority())
            return;

        viewController.CameraBob();
    }

    public override void _Process(double delta)
    {
        if (!InputController.IsMultiplayerAuthority())
            return;
        viewController.HandleLocalCameraInput(InputController.mouseInputBuffer);
        Rpc(nameof(BroadcastRotation),
            new Vector2(localController.verticalView.Rotation.X, localController.horizontalView.Rotation.Y));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void BroadcastRotation(Vector2 rot)
    {
        var vertRot = localController.verticalView.Rotation;
        localController.verticalView.Rotation = new Vector3(rot.X, vertRot.Y, vertRot.Z);
        var horRot = localController.horizontalView.Rotation;
        localController.horizontalView.Rotation = new Vector3(horRot.X, rot.Y, horRot.Z);
    }

    private void _force_update_physics_transform()
    {
        PhysicsServer3D.BodySetMode(GetRid(), PhysicsServer3D.BodyMode.Static);
        PhysicsServer3D.BodySetState(GetRid(), PhysicsServer3D.BodyState.Transform, GlobalTransform);
        PhysicsServer3D.BodySetMode(GetRid(), PhysicsServer3D.BodyMode.Kinematic);
    }

    // executed by netfox rollbacksynchronizer
    public void _rollback_tick(double delta, long tick, bool isFresh)
    {
        _force_update_physics_transform();
        if (InputController.teleport)
        {
            var tempVel = Velocity;
            Velocity = Vector3.Zero;
            logger.LogWarning("Origin: " + GlobalPosition);
            var pos = Position;
            var forward = localController.cameraMount.GlobalTransform.Basis.Z;
            pos -= forward * 3;
            Position = pos;
            this.FindChildByType<TickInterpolator>()?.Teleport();
            loggerFlag = true;
            logTickStart = tick;
            MoveBody();
            Velocity = tempVel;
            return;
        }

        if (loggerFlag && logTicks + logTickStart == tick)
            loggerFlag = false;
        Move(delta, tick, isFresh);
    }

    private void Move(double delta, long tick, bool isFresh)
    {
        // Check if we are on ground
        ForceFloorCheck();
        bool isOnFloor = IsOnFloor();
        if (isOnFloor)
        {
            if (InputController.jumpOn)
            {
                // Not running friction on ground if you press jump fast enough allows you to preserve all speed.
                movementController.Jump(delta);
                // NOTE: This is sort of a band-aid to make bunny-hopping on walkable slopes feel a lot nicer.
                movementController.AirAccelerate(delta, InputController.moveDir.Normalized(),
                    InputController.moveDir.Length(), PlayerParams.AIR_ACCELERATION);
            }
            else
            {
                movementController.Friction(delta, 1.0);
                movementController.Accelerate(delta, InputController.moveDir.Normalized(),
                    InputController.moveDir.Length(), PlayerParams.ACCELERATION);
            }
        }
        else
        {
            movementController.AirAccelerate(delta, InputController.moveDir.Normalized(),
                InputController.moveDir.Length(), PlayerParams.AIR_ACCELERATION);
        }

        if (!isOnFloor)
            this.SetVelY(Velocity.Y - PlayerParams.GRAVITY * (float)delta);

        HandleStepTraceValues();

        var shape = (BoxShape3D)collisionHull.Shape;
        SetShapeBounds(bboxStep, shape.Size + new Vector3(traceMargin, 0, traceMargin));
        Velocity *= (float)NetworkTime.PhysicsFactor;
        CheckForStep();
        MoveBody();
        Velocity /= (float)NetworkTime.PhysicsFactor;
        Duck(InputController.duckOn, isOnFloor);

    }

    private void ForceFloorCheck()
    {
        Vector3 oldVel = Velocity;
        Velocity = Vector3.Zero;
        MoveAndSlide();
        Velocity = oldVel;
    }

    private void SetShapeBounds(BoxShape3D shape, Vector3 size) => shape.Size = size;

    private WallCollision WallCheck()
    {
        SetShapeBounds(bboxNearWall,
            ((BoxShape3D)collisionHull.Shape).Size + new Vector3(0.3f, -PlayerParams.STEP_HEIGHT, 0.25f));
        SetShapeBounds(bboxIntoWall,
            ((BoxShape3D)collisionHull.Shape).Size + new Vector3(0.05f, -PlayerParams.STEP_HEIGHT, 0.05f));

        var near = CastStaticTrace(this, bboxNearWall, GlobalPosition + Vector3.Up * (PlayerParams.STEP_HEIGHT / 2));
        var on = CastStaticTrace(this, bboxIntoWall, GlobalPosition + Vector3.Up * (PlayerParams.STEP_HEIGHT / 2));

        if (near == null && on == null)
        {
            wallNormal = Vector3.Up;
            return WallCollision.NONE;
        }

        if (on != null)
        {
            wallNormal = on.normal;
            return WallCollision.ON;
        }

        if (near != null)
        {
            wallNormal = near.normal;
            return WallCollision.NEAR;
        }

        return WallCollision.NONE;
    }

    // Use a test move_and_collide to check if we should try stepping up something.
    private void CheckForStep()
    {
        var delta = GetPhysicsProcessDeltaTime();
        KinematicCollision3D collision = MoveAndCollide(Velocity * (float)delta, true);
        if (collision != null)
        {
            var normal = collision.GetNormal();

            if (IsOnFloor() && normal.Y < 0.7)
            {
                MoveStep();
            }
        }
    }

    // Deforms step trace info based on wall proximity
    private void HandleStepTraceValues()
    {
        switch (WallCheck())
        {
            case WallCollision.NONE:
                traceDirAdd = 1.0f;
                traceMargin = maxTraceMarginAmount;
                break;
            case WallCollision.NEAR:
                traceDirAdd = 1.05f;
                traceMargin = 0.05f;
                break;
            case WallCollision.ON:
                traceDirAdd = 1.45f;
                traceMargin = minTraceMarginAmount;
                break;
        }
    }

    // Hacks move_and_slide() to make slopes behave a little more like GoldSrc.
    public void MoveBody()
    {
        var collided = MoveAndSlide();
        var normal = GetFloorNormal();
        if (collided && normal.Length() == 0)
        {
            var slideDirection = GetLastSlideCollision().GetNormal();
            Velocity = Velocity.Slide(slideDirection);
            FloorBlockOnWall = false;
        }
        else
        {
            // Hacky McHack to restore wallstrafing behaviour which doesn't work unless 'floor_block_on_wall' is true
            FloorBlockOnWall = true;
        }

        FloorStopOnSlope = Velocity.Length() < 0.001f;
    }

    // Handles crouching logic.
    public void Duck(bool duckOn, bool isOnFloor)
    {
        // If we aren't ducking, but are holding the "pm_duck" input...
        if (duckOn)
        {
            float time;
            float frac;
            float crouchDist = PlayerParams.HULL_DUCKING_BOUNDS.Y / 2;

            if (!ducked && !ducking)
            {
                ducking = true;
                duckTimer.Start(0.1f);
            }

            time = Mathf.Max(0, (1.0f - (float)duckTimer.TimeLeft));

            if (ducking)
            {
                if (duckTimer.TimeLeft <= 0.05f || !isOnFloor)
                {
                    // Set the collision hull and view offset to the ducking counterpart.
                    collisionHull.Shape = bboxDucking;
                    offset = PlayerParams.DUCK_VIEW_OFFSET;
                    ducked = true;
                    ducking = false;
                    if (isOnFloor)
                    {
                        // Get half of standing box height and introduce a little margin to prevent clipping.
                        var pos = Position.Y;
                        Position = new Vector3(Position.X, pos - crouchDist - 0.001f, Position.Z);
                        // this.SetYPos(Position.Y - crouchDist - 0.001f);
                    }
                }
                // Move our character down in order to stop them from "falling" after crouching, but ONLY on the ground.
                else
                {
                    frac = SplineFraction(time, 2.5f);
                    offset = (PlayerParams.DUCK_VIEW_OFFSET - crouchDist) +
                             (PlayerParams.VIEW_OFFSET);
                }
            }
        }

        if (!duckOn && (ducking || ducked))
            Unduck();
    }

    private void Unduck()
    {
        var crouchDist = PlayerParams.HULL_DUCKING_BOUNDS.Y / 2f;

        if (UnduckTrace(Position + Vector3.Up * (crouchDist + 0.005f), bboxStanding, this))
        {
            //If there is a ceiling above the player that would cause us to clip into it when unducking, stay ducking.
            ducked = true;
        }
        else
        {
            //Otherwise, unduck.ducked = false
            ducked = false;
            ducking = false;
            if (IsOnFloor())
            {
                var posY = Position.Y;
                Position = new Vector3(Position.X, posY + crouchDist + 0.001f, Position.Z);
            }

            collisionHull.Shape = bboxStanding;
            offset = PlayerParams.VIEW_OFFSET;
        }
    }

    private bool UnduckTrace(Vector3 origin, Shape3D shape, CharacterBody3D e)
    {
        var physicsQueryParams = new PhysicsShapeQueryParameters3D();
        physicsQueryParams.SetShape(shape);
        var transform = physicsQueryParams.Transform;
        transform.Origin = origin;
        physicsQueryParams.Transform = transform;
        physicsQueryParams.CollideWithBodies = true;
        physicsQueryParams.Exclude = new Array<Rid>() { e.GetRid() };
        physicsQueryParams.SetCollisionMask(0b00000000_00000000_00000000_00000011);
        var spaceState = GetWorld3D().DirectSpaceState;
        var results = spaceState.CollideShape(physicsQueryParams, 8);
        return results.Count > 0;
    }

    // Creates a smooth interpolation fraction.
    private float SplineFraction(float val, float scale)
    {
        val = scale * val;
        float valueSquared = val * val;
        return 3 * valueSquared - 2 * valueSquared * val;
    }

    // Casts a collision shape trace at a specific position with no motion, then stores relevant collision data.
    private Trace CastStaticTrace(CollisionObject3D what, Shape3D shape, Vector3 origin)
    {
        var shapeQueryParams = new PhysicsShapeQueryParameters3D();
        shapeQueryParams.SetShape(shape);
        var trans = shapeQueryParams.Transform;
        trans.Origin = origin;
        shapeQueryParams.Transform = trans;
        shapeQueryParams.CollideWithBodies = true;
        shapeQueryParams.Exclude = new Array<Rid>() { what.GetRid() };
        var spaceState = what.GetWorld3D().DirectSpaceState;
        var results = spaceState.CollideShape(shapeQueryParams);

        var rest = spaceState.GetRestInfo(shapeQueryParams);
        Vector3 norm;
        if (rest.TryGetValue("normal", out var res))
        {
            norm = res.AsVector3();
        }
        else
        {
            norm = Vector3.Up;
        }

        return results.Count > 0 ? new Trace(origin, 1, norm) : null;
    }

    private Trace CastTrace(CollisionObject3D what, Shape3D shape, Vector3 from, Vector3 to)
    {
        Trace trace;
        var motion = to - from;
        var shapeQueryParams = new PhysicsShapeQueryParameters3D();
        shapeQueryParams.SetShape(shape);
        var trans = shapeQueryParams.Transform;
        trans.Origin = from;
        shapeQueryParams.Transform = trans;
        shapeQueryParams.CollideWithBodies = true;
        shapeQueryParams.SetMotion(motion);
        shapeQueryParams.Exclude = new Array<Rid>() { what.GetRid() };
        //#params.set_collision_mask(1)
        var spaceState = what.GetWorld3D().DirectSpaceState;
        var results = spaceState.CastMotion(shapeQueryParams);

        if (Math.Abs(results[0] - 1) < 0.001f)
        {
            trace = new Trace().SetEndPos(to);
            return trace;
        }

        var endPos = from + motion * results[1];
        trans = shapeQueryParams.Transform;
        trans.Origin = endPos;
        shapeQueryParams.Transform = trans;

        var rest = spaceState.GetRestInfo(shapeQueryParams);
        Vector3 norm;
        if (rest.TryGetValue("normal", out var res))
        {
            norm = res.AsVector3();
        }
        else
        {
            norm = Vector3.Up;
        }

        trace = new Trace(endPos, results[0], norm);
        return trace;
    }

    // Casts traces to detect steps to climb.
    private bool MoveStep()
    {
        Vector3 dest;
        Vector3 down;
        Trace trace;

        // # Get destination position that is one step-size above the intended move
        var originalPos = GlobalTransform.Origin;
        var vel = (Velocity.Normalized() * traceDirAdd).Slide(wallNormal);
        var speed = PlayerParams.MAX_SPEED * traceDirAdd;
        var dir = Velocity.LengthSquared() < speed * speed ? vel * speed : (Velocity * traceDirAdd).Slide(wallNormal);

        var delta = (float)GetPhysicsProcessDeltaTime();
        dest = originalPos;
        dest[0] += dir[0] * delta;
        dest[1] += PlayerParams.STEP_HEIGHT;
        dest[2] += dir[2] * delta;

        // # 1st Trace: Check for collisions one stepsize above the original position
        // # and along the intended destination
        trace = CastTrace(this, bboxStep, originalPos + Vector3.Up * PlayerParams.STEP_HEIGHT, dest);
        // # 2nd Trace: Check for collisions below the stepsize until
        // # level with original position
        down = new Vector3(trace.endPos[0], originalPos[1], trace.endPos[2]);
        trace = CastTrace(this, bboxStep, trace.endPos, down);
        // # Move to trace collision position if step is higher than original position
        // # and not steep
        if (trace.endPos[1] > originalPos[1] && trace.normal[1] >= 0.7f)
        {
            var trans = GlobalTransform;
            trans.Origin = trace.endPos;
            GlobalTransform = trans;
            // Velocity = Velocity.Slide(trace.normal)
            return true;
        }

        return false;
    }
}