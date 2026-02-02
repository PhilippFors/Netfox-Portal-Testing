#class_name gd_PortalTraveller_ extends Node
#
#signal onPortalEnter
#signal onPortalExit
#
#var isTravelling: bool
#@export var camera: Node3D
#
#func _ready():
	#pass
#
#func travel(entryPortal: Portal, exitPortal: Portal):
	#var body = NodeHelper.find_parent_node(self, PhysicsBody3D)
	#if !body:
		#print("No physics body attached")
	#var transform_rel_to_this_portal = entryPortal.global_transform.affine_inverse() * body.global_transform
	#var moved_to_other_portal = exitPortal.global_transform * transform_rel_to_this_portal
	#body.global_transform = moved_to_other_portal
