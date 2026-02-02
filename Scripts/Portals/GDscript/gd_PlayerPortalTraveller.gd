#class_name gd_PlayerPortalTraveller_ extends PortalTraveller_
#
#var goldGdtCam
#var horizontalView: Node3D
#var verticalView: Node3D
#var body:PhysicsBody3D
#
#func _ready():
	#goldGdtCam = $"../../Interpolated Camera"
	#horizontalView = $"../Horizontal View"
	#verticalView = $"../Horizontal View/Vertical View"
	#body = $".."
#
#func travel(entryPortal: Portal, exitPortal: Portal):
	#var transform_rel_to_this_portal = entryPortal.global_transform.affine_inverse() * body.global_transform
	#var trans = exitPortal.global_transform * transform_rel_to_this_portal.rotated(Vector3.UP, PI)
	#
	##var moved_to_other_portal = exitPortal.global_transform * transform_rel_to_this_portal
	#var r = exitPortal.global_transform.basis.get_euler() - entryPortal.global_transform.basis.get_euler()
	#var transform = entryPortal.real_to_exit_transform(camera.global_transform)
	#var r1 = transform.basis.get_euler()
	#
	#goldGdtCam.hasTravelled = true
#
	#horizontalView.rotate_object_local(Vector3.DOWN, horizontalView.transform.basis.get_euler().y - r1.y)
	#verticalView.rotate_object_local(Vector3.LEFT, verticalView.transform.basis.get_euler().x - r1.x)
	#var newPos = entryPortal.real_to_exit_transform(body.global_transform).origin
	#body.global_position = newPos
	#var mountPos = $"../Horizontal View/Vertical View/Camera Mount".global_position
	#var diff = (mountPos - newPos) * exitPortal.transform.basis.z
	#body.global_position -= diff
	#var oldVel =  body.velocity \
	#.rotated(Vector3(1, 0, 0), r.x) \
	#.rotated(Vector3(0, 1, 0), r.y + PI) \
	#.rotated(Vector3(0, 0, 1), r.z).normalized()
	#var newDir = exitPortal.transform.basis.z
	#
	#if body.velocity.y >= -2 && body.velocity.y < 5:
		#newDir *= 0
	#else:
		#oldVel *= 0.3
	#var newVel = (newDir + oldVel).normalized() * body.velocity.length()
	#body.velocity = newVel
	
