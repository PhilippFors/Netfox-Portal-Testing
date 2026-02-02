#extends Area3D
#
#@export var colliders: StaticBody3D
#@onready var raycast: RayCast3D = $RayCast3D
#@onready var portal: Portal = $".."
#
#var tracked_bodies = []
#
#func _physics_process(delta: float) -> void:
	#for body in tracked_bodies:
		#checkVelocityRelativeToPortal(body)
	#pass
#
#func _process(delta: float) -> void:
	##temp checking in process for angles and whatever
	#if global_transform.basis.y.dot(Vector3.UP) != 1:
		#$"../RigidBody3D/CollisionShape3D6".disabled = true
	#else:
		#$"../RigidBody3D/CollisionShape3D6".disabled = false
#
#func checkVelocityRelativeToPortal(body):
	#var portalDot = portal.global_basis.z.dot(Vector3.UP)
	#if portalDot > 0.5:
		#return
	#var traveller = NodeHelper.find_child_node(body, PortalTraveller) as PortalTraveller
	#if traveller.isTravelling:
		#return
	#var velocity: Vector3 = body.velocity.normalized()
	#var goldgdtbody = body as GoldGdtBody
	#var input = goldgdtbody.input as GoldGdtControls
	#var moveDir = input.MoveDir.normalized()
#
	#raycast.clear_exceptions()
	#raycast.add_exception(body)
	#raycast.add_exception($"..")
	#raycast.global_position = body.global_position
	#raycast.global_rotation = body.global_rotation
	#var target: Vector3 = velocity * 2
	#if velocity == Vector3.ZERO:
		#target = moveDir * 2;
		#
	#print(target)
		#
	#raycast.target_position = target
	#raycast.force_raycast_update()
	#var collider = raycast.get_collider() as Node3D
	#var hitPortal: bool = false
	#if raycast.is_colliding():
		#print(collider.name)
	#
	#if raycast.is_colliding() && collider.name.contains("Portal"):
		#hitPortal = true
		#
	#var portalDirection: Vector3 = $"..".global_basis.z
	#var dot = velocity.dot(portalDirection)
	#if velocity == Vector3.ZERO:
		#dot = moveDir.dot(portalDirection)
	#print(dot)
	#if dot < -0.05 && hitPortal:
		#setCollisions(body)
	#else:
		#unSetCollision(body)
	#return
#
#func setCollisions(body):
	#body.set_collision_layer_value(1, false)
	#body.set_collision_mask_value(1, false)
	#body.set_collision_layer_value(3, true)
	#body.set_collision_mask_value(3, true)
#
#func unSetCollision(body):
	#body.set_collision_layer_value(1, true)
	#body.set_collision_mask_value(1, true)
	#body.set_collision_layer_value(3, false)
	#body.set_collision_mask_value(3, false)
#
#func _on_body_entered(body):
	#print("body in environment range of " + $"..".name)
	#tracked_bodies.append(body)
	#setCollisions(body)
#
#func _on_body_exited(body):
	#print("body out of environment range of " + $"..".name)
	#unSetCollision(body)
	#for i in len(tracked_bodies):
		#if(tracked_bodies[i] == body):
			#tracked_bodies.remove_at(i)
#
#func _check_shapecast_collision(body):
	#$PortalEnvShapeCast.force_shapecast_update()
	#for i in $PortalEnvShapeCast.get_collision_count():
		#if $PortalEnvShapeCast.get_collider(i) == body:
			#return true	
	#return false
